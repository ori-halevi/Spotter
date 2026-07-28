using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Spotter;

/// <summary>
/// Starts a file or program through the desktop shell instead of through this process.
///
/// Spotter runs elevated — reading the master file table needs a raw volume handle, and that
/// needs administrator. Any process it starts itself inherits that token. So "open this result",
/// the most ordinary action the app has, was starting documents, scripts and executables with
/// full administrator rights and no indication that it had.
///
/// Explorer runs at medium integrity as the logged-on user. Asking *it* to do the opening puts
/// the child back where it belongs. The route is the shell window's own IShellDispatch2, reached
/// through the ShellWindows collection — the documented "run as the desktop user from an elevated
/// process" technique, and the only in-box one that does not require a service or a scheduled task.
/// </summary>
internal static class ShellLaunch
{
    /// <summary>
    /// Returns true when the shell accepted the request. False means the caller should fall back;
    /// it never throws.
    /// </summary>
    public static bool TryOpenAsUser(string file, string arguments)
    {
        object shellWindows = null;
        object dispatch = null;
        object background = null;
        object folderView = null;
        object application = null;

        try
        {
            Type type = Type.GetTypeFromCLSID(ShellWindowsClsid);
            if (type == null)
            {
                return false;
            }

            shellWindows = Activator.CreateInstance(type);
            if (shellWindows == null)
            {
                return false;
            }

            // FindWindowSW(pvarLoc, pvarLocRoot, swClass, out hwnd, swfwOptions)
            // SWC_DESKTOP = 8, SWFO_NEEDDISPATCH = 1 — i.e. "the desktop window, and give me
            // something I can call methods on".
            object[] parameters = { null, null, DesktopWindowClass, 0, NeedDispatch };
            dispatch = shellWindows.GetType().InvokeMember(
                "FindWindowSW", BindingFlags.InvokeMethod, null, shellWindows, parameters);
            if (dispatch == null)
            {
                return false;
            }

            background = dispatch.GetType().InvokeMember(
                "Document", BindingFlags.GetProperty, null, dispatch, null);
            if (background == null)
            {
                return false;
            }

            folderView = background.GetType().InvokeMember(
                "Application", BindingFlags.GetProperty, null, background, null);
            application = folderView;
            if (application == null)
            {
                return false;
            }

            // ShellExecute(File, Arguments, Directory, Operation, Show)
            application.GetType().InvokeMember(
                "ShellExecute", BindingFlags.InvokeMethod, null, application,
                new object[]
                {
                    file,
                    arguments ?? string.Empty,
                    string.Empty,
                    string.Empty,   // default verb, same as a double-click
                    1,              // SW_SHOWNORMAL
                });
            return true;
        }
        catch (Exception ex)
        {
            Log.Write("shell hand-off failed, will start it directly: " + ex.Message);
            return false;
        }
        finally
        {
            Release(application);
            if (!ReferenceEquals(folderView, application))
            {
                Release(folderView);
            }
            Release(background);
            Release(dispatch);
            Release(shellWindows);
        }
    }

    private static readonly Guid ShellWindowsClsid = new Guid("9BA05972-F6A8-11CF-A442-00A0C90A8F39");

    private const int DesktopWindowClass = 8;   // SWC_DESKTOP
    private const int NeedDispatch = 1;         // SWFO_NEEDDISPATCH

    private static void Release(object comObject)
    {
        try
        {
            if (comObject != null && Marshal.IsComObject(comObject))
            {
                Marshal.ReleaseComObject(comObject);
            }
        }
        catch
        {
            // Releasing is best-effort; a leaked RCW is not worth a crash on the open path.
        }
    }
}
