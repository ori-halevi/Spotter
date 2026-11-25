using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using Wpf.Ui.Appearance;

namespace Spotter;

public partial class App : Application
{
    private Mutex _singleInstance;

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_RESTORE = 9;

    [DllImport("user32.dll")]
    private static extern bool AllowSetForegroundWindow(int dwProcessId);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private const int VK_SHIFT = 0x10;

    /// <summary>
    /// Shift held while the app is launched, which Windows itself treats as "a new window of
    /// this program" — shift-clicking a taskbar button is the shell's own gesture for it, and so
    /// is middle-clicking one. The middle click cannot be seen from here: the shell starts the
    /// executable with no arguments and the button is long released by the time a process
    /// exists, so a plain launch is a plain launch. Shift is still held, and can be.
    /// </summary>
    private static bool ShiftHeldAtLaunch() => (GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0;

    /// <summary>ASFW_ANY — any process may take the foreground from us.</summary>
    private const int ASFW_ANY = -1;

    private const string MutexName = @"Spotter.SingleInstance";
    private const string ShowEventName = @"Spotter.Show";

    /// <summary>
    /// Asks for a window of its own instead of handing over to the instance already running.
    /// Survives elevation: ElevationTask passes the command line through to the elevated copy, so
    /// a new instance started from an unelevated window is still a new instance once it comes
    /// back up with administrator rights.
    /// </summary>
    private const string NewInstanceFlag = "--new-instance";

    /// <summary>
    /// True in a process that found another instance already running and carried on anyway. It is
    /// the extra window, not the one that owns the machine-wide pieces, and three things follow
    /// from that: it does not register the restore hotkey (Windows gives a combination to one
    /// window only), it does not listen for the show signal, and it never writes the settings
    /// file — see <see cref="UiSettings.Save"/> for why that one matters most.
    /// </summary>
    internal static bool SecondaryInstance { get; private set; }

    /// <summary>
    /// This process's command line, so a new instance can be started the way this one was: a
    /// --demo window spawns another demo window, --lang he another Hebrew one.
    /// </summary>
    private static string[] _launchArgs = Array.Empty<string>();

    /// <summary>
    /// Set by a second launch to ask the running instance to come to the front. A named event
    /// rather than SetForegroundWindow on its MainWindowHandle, because an instance sitting in
    /// the tray has no main window handle at all — and that is exactly when someone re-launches.
    /// Null when this process is not the owning instance.
    /// </summary>
    internal static EventWaitHandle ShowSignal { get; private set; }

    /// <summary>
    /// The one settings instance for the process. Loaded once here: the window used to load its
    /// own second copy, so a change made through Options was invisible to whatever App had read.
    /// </summary>
    internal static UiSettings Settings { get; private set; } = new UiSettings();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        string[] args = _launchArgs = e?.Args ?? Array.Empty<string>();

        // Before TryElevate, so that the flag is part of the command line the elevated copy is
        // started with: by then the key is no longer held and the intent would be lost.
        if (ShiftHeldAtLaunch() && !HasFlag(args, NewInstanceFlag))
        {
            string[] withFlag = new string[args.Length + 1];
            Array.Copy(args, withFlag, args.Length);
            withFlag[args.Length] = NewInstanceFlag;
            args = _launchArgs = withFlag;
            Log.Write("shift held at launch: taken as --new-instance");
        }

        // Results arrive on engine worker threads, so a throw there kills the process with
        // nothing but a Windows Error Reporting entry and an exception code. Log it instead.
        AppDomain.CurrentDomain.UnhandledException += (_, unhandled) =>
            Log.Write("UNHANDLED (background thread): " + unhandled.ExceptionObject);
        DispatcherUnhandledException += (_, dispatcherArgs) =>
        {
            Log.Write("UNHANDLED (ui thread): " + dispatcherArgs.Exception);
            dispatcherArgs.Handled = true;      // keep the window alive; the log has the detail
        };

        // Settings and language before anything that can put text on screen. MainWindow's XAML asks
        // for translated strings and a flow direction while it is being constructed, and the
        // "already running" message below is shown earlier still — a window that came up in
        // English and flipped to Hebrew a frame later would be a visible stutter rather than a
        // nicety.
        Settings = UiSettings.Load();

        // --lang <code> overrides the setting for this run only and is never written back. It
        // exists so a right-to-left layout can be looked at without editing the user's real
        // settings file, which is the same reason --demo exists.
        string forced = ArgumentAfter(args, "--lang");
        Localization.Lang.Initialize(forced ?? Settings.Language);
        Log.Write($"language: {Localization.Lang.Current.Code}"
                  + (forced != null ? " (--lang)"
                      : string.IsNullOrWhiteSpace(Settings.Language)
                          ? " (following Windows, which is set to "
                            + Localization.Lang.SystemCultureName + ")"
                          : " (chosen in Options)"));

        if (TryElevate(args))
        {
            return;      // handed over to the elevated copy
        }

        // Every launch otherwise re-reads the file table of every volume. Two or three instances
        // contending for the same disks turned a 60 s index into minutes, so an ordinary second
        // launch hands over to the one already running instead of piling on — which is also what
        // the tray icon and the restore hotkey rely on, since clicking the shortcut again is how
        // most people bring a hidden window back.
        //
        // --new-instance is the deliberate exception: a second window on request, paid for with
        // that second pass over the drives. The claim is still attempted first, so the very first
        // window is the owning instance whether or not it was asked for with the flag.
        InstanceClaim claim = ClaimSingleInstance();
        if (claim == InstanceClaim.AlreadyRunning && !HasFlag(args, NewInstanceFlag))
        {
            if (!SignalExistingInstance())
            {
                // Never vanish without a word. Exiting silently here — which is what this used to
                // do for *any* failure to inspect the mutex, including one belonging to another
                // user — looks exactly like the app being broken.
                Dialogs.Info(null, Localization.Lang.Tr("Dialog.AlreadyRunning"),
                    MessageBoxImage.Information);
            }
            Shutdown();
            return;
        }
        SecondaryInstance = claim == InstanceClaim.AlreadyRunning;
        if (SecondaryInstance)
        {
            Log.Write("extra instance (--new-instance): no hotkey, no show signal, settings not saved");
        }

        // Now elevated and the only instance: pay forward the prompt the user just approved, so
        // launching from this same location never asks again.
        if (IsElevated() && !HasFlag(args, "--noelevate") && !HasFlag(args, "--demo"))
        {
            ElevationTask.RegisterIfMissing();
        }

        // Before the first window is constructed, for the same reason as the language above: the
        // whole layout is drawn through this one transform, so starting at 100% and correcting a
        // frame later would be a visible jolt on every launch.
        UiScale.Initialize(Settings);
        ShellIcons.FollowScale();

        ApplicationThemeManager.Apply(ResolveTheme(Settings.Theme));

        // After the theme, and deliberately not via a Style — a tooltip is its own window and does
        // not inherit the reading direction, but restyling it to fix that loses the Fluent look.
        PopupChrome.Install();

        InstallJumpList();
    }

    /// <summary>
    /// Puts "New instance" on the taskbar button's right-click menu, which is where Windows
    /// itself puts that command for every program that has one.
    ///
    /// It is a task rather than anything this process handles: the shell starts the executable
    /// again with the argument, exactly as a shortcut would, and the normal startup path turns
    /// that into a second window. Registered per launch rather than once, because the list is
    /// keyed to the executable's path and this one is meant to be moved around.
    /// </summary>
    private static void InstallJumpList()
    {
        try
        {
            string path = Environment.ProcessPath;
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            System.Windows.Shell.JumpList list = new System.Windows.Shell.JumpList
            {
                // Neither is ours to show: Spotter opens no documents, so the shell's recent and
                // frequent lists would be empty categories taking up the menu.
                ShowRecentCategory = false,
                ShowFrequentCategory = false,
            };
            list.JumpItems.Add(new System.Windows.Shell.JumpTask
            {
                Title = Localization.Lang.Tr("Menu.NewInstance"),
                Description = Localization.Lang.Tr("NewInstance.Tip"),
                ApplicationPath = path,
                IconResourcePath = path,
                Arguments = NewInstanceFlag,
            });
            System.Windows.Shell.JumpList.SetJumpList(Current, list);
        }
        catch (Exception ex)
        {
            // A jump list is a convenience; nothing about the app depends on one existing.
            Log.Write("could not install the jump list: " + ex.Message);
        }
    }

    /// <summary>Maps the stored setting onto a concrete theme, following Windows when set to Auto.</summary>
    internal static ApplicationTheme ResolveTheme(string setting) => setting switch
    {
        "Dark" => ApplicationTheme.Dark,
        "Light" => ApplicationTheme.Light,
        _ => SystemUsesDarkTheme() ? ApplicationTheme.Dark : ApplicationTheme.Light,
    };

    /// <summary>
    /// Relaunches elevated and shuts this copy down. Reading the MFT needs a raw volume handle,
    /// so without administrator the app cannot do its job at all — telling the user to go and
    /// restart it themselves, which is what this used to do, is just making them do the work.
    ///
    /// The manifest stays asInvoker rather than requireAdministrator so that <c>--noelevate</c>
    /// still gives a runnable, automatable window for development.
    /// </summary>
    private bool TryElevate(string[] args)
    {
        if (IsElevated() || HasFlag(args, "--noelevate") || HasFlag(args, "--demo"))
        {
            return false;
        }

        // Preferred route: a task already registered for THIS executable's location starts the
        // elevated copy with no prompt at all. Copy the app elsewhere and that copy simply
        // prompts once and registers its own — the app stays portable.
        // Hand our foreground rights to the copy we are about to start. Without this Windows
        // refuses to let it come to the front — because the process starting it is the scheduler,
        // not the window the user just clicked — and it opens behind the folder it was launched
        // from. The 1.5.1 app solved the same problem by injecting a call to this very function
        // into the foreground process; from here it is one line, because we are the one giving
        // the permission away.
        AllowSetForegroundWindow(ASFW_ANY);

        if (ElevationTask.TryLaunchElevated(args))
        {
            Shutdown();
            return true;
        }

        try
        {
            ProcessStartInfo start = new ProcessStartInfo(Environment.ProcessPath)
            {
                UseShellExecute = true,
                Verb = "runas",
            };
            foreach (string arg in args)
            {
                start.ArgumentList.Add(arg);
            }
            Process.Start(start);
            Shutdown();
            return true;
        }
        catch (Exception ex)
        {
            // Almost always "the user said No" to the UAC prompt. Carry on unelevated; the
            // window explains what is missing and offers the button again.
            Log.Write("elevation declined or failed: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// How many other copies of Spotter are running. Used to cascade an extra window clear of
    /// the ones already on screen, so the count only has to be about right: one too few means
    /// two windows share a position, which the user then drags apart, and the alternative — a
    /// handshake between instances to hand out positions — is a great deal of machinery for a
    /// window that is about to be moved anyway.
    /// </summary>
    internal static int OtherInstanceCount()
    {
        try
        {
            using Process self = Process.GetCurrentProcess();
            Process[] all = Process.GetProcessesByName(self.ProcessName);
            try
            {
                // Only this logon session's copies. GetProcessesByName spans the machine, so on
                // a shared or remote-desktop machine another signed-in user's windows — which
                // are not on this desktop and cannot be stepped clear of — would push this one
                // further and further down the screen.
                int here = 0;
                foreach (Process p in all)
                {
                    try
                    {
                        if (p.SessionId == self.SessionId) here++;
                    }
                    catch { /* exited between the listing and the read */ }
                }
                return Math.Max(0, here - 1);
            }
            finally
            {
                foreach (Process p in all)
                {
                    p.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            // Enumerating processes can fail outright on a locked-down machine. One step down is
            // the right guess: this is only ever called by an instance that already knows
            // another one is running.
            Log.Write("could not count the running instances: " + ex.Message);
            return 1;
        }
    }

    /// <summary>
    /// Starts another copy of this executable with a window of its own. Returns false only if the
    /// process could not be started at all, which the caller reports — a request that does
    /// nothing and says nothing is indistinguishable from the app being broken.
    ///
    /// No "runas" verb and no elevation work here: a child process inherits this one's token, so
    /// started from the elevated window it is elevated too, with no second UAC prompt. Started
    /// from an unelevated one it elevates itself exactly as a fresh launch would.
    /// </summary>
    internal static bool StartAnotherInstance()
    {
        try
        {
            ProcessStartInfo start = new ProcessStartInfo(Environment.ProcessPath)
            {
                UseShellExecute = true,
                // Whatever folder the app was launched from may since have been deleted, and an
                // invalid working directory fails the launch rather than being ignored.
                WorkingDirectory = AppContext.BaseDirectory,
            };

            // This process's own switches first, so the new window is the same kind of window:
            // --demo, --noelevate and --lang all have to carry over. Any --new-instance already
            // there is dropped and re-added once, rather than accumulating one per generation.
            foreach (string arg in _launchArgs)
            {
                if (!arg.Equals(NewInstanceFlag, StringComparison.OrdinalIgnoreCase))
                {
                    start.ArgumentList.Add(arg);
                }
            }
            start.ArgumentList.Add(NewInstanceFlag);

            // The new window has to be allowed to take the foreground from us; without this it
            // opens behind this one, looking like nothing happened.
            AllowSetForegroundWindow(ASFW_ANY);
            Process.Start(start);
            return true;
        }
        catch (Exception ex)
        {
            Log.Write("could not start another instance: " + ex.Message);
            return false;
        }
    }

    private static bool HasFlag(string[] args, string flag) =>
        Array.Exists(args, a => a.Equals(flag, StringComparison.OrdinalIgnoreCase));

    private static string ArgumentAfter(string[] args, string flag)
    {
        int i = Array.FindIndex(args, a => a.Equals(flag, StringComparison.OrdinalIgnoreCase));
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }

    internal static bool IsElevated()
    {
        try
        {
            using System.Security.Principal.WindowsIdentity id =
                System.Security.Principal.WindowsIdentity.GetCurrent();
            return new System.Security.Principal.WindowsPrincipal(id)
                .IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    private enum InstanceClaim
    {
        /// <summary>This process owns the single-instance slot.</summary>
        Owner,

        /// <summary>Another instance holds it; hand over and exit.</summary>
        AlreadyRunning,

        /// <summary>Could not tell. Carry on rather than refuse to start.</summary>
        Undetermined,
    }

    /// <summary>
    /// Claims the single-instance slot.
    ///
    /// Tries the Global namespace first so an elevated instance and an ordinary one see each
    /// other — otherwise "Restart as administrator" leaves two running, both indexing. Creating a
    /// Global object needs SeCreateGlobalPrivilege, which a standard user does not hold, so it
    /// falls back to a per-session name rather than treating the failure as "already running".
    ///
    /// That conflation was the bug: any failure to *inspect* the mutex — a standard account, or a
    /// mutex owned by a different user on the same machine — was read as "another instance is
    /// running", and the app exited with no window and no message.
    /// </summary>
    private InstanceClaim ClaimSingleInstance()
    {
        InstanceClaim claim = TryClaim(@"Global\" + MutexName, out bool retry);
        if (retry)
        {
            Log.Write("global single-instance name unavailable, falling back to session scope");
            claim = TryClaim(@"Local\" + MutexName, out _);
        }

        if (claim == InstanceClaim.Owner)
        {
            try
            {
                ShowSignal = new EventWaitHandle(false, EventResetMode.AutoReset, ShowEventName);
            }
            catch (Exception ex)
            {
                Log.Write("could not create the show signal: " + ex.Message);
            }
        }
        return claim;
    }

    private InstanceClaim TryClaim(string name, out bool retryElsewhere)
    {
        retryElsewhere = false;
        try
        {
            _singleInstance = new Mutex(initiallyOwned: true, name, out bool createdNew);
            return createdNew ? InstanceClaim.Owner : InstanceClaim.AlreadyRunning;
        }
        catch (UnauthorizedAccessException)
        {
            // Either the privilege to create in this namespace is missing, or an existing object
            // of that name belongs to a session we cannot open. Both mean "try a narrower scope",
            // NOT "someone else is already running".
            retryElsewhere = true;
            return InstanceClaim.Undetermined;
        }
        catch (Exception ex)
        {
            Log.Write("single-instance check failed, starting anyway: " + ex.Message);
            return InstanceClaim.Undetermined;      // never block startup over a diagnostics detail
        }
    }

    /// <summary>
    /// Asks the running instance to show itself. Returns false if there is nothing listening.
    /// </summary>
    private static bool SignalExistingInstance()
    {
        // The event is the reliable route: it reaches an instance sitting in the tray, which by
        // definition has no window to find.
        try
        {
            if (EventWaitHandle.TryOpenExisting(ShowEventName, out EventWaitHandle signal))
            {
                using (signal)
                {
                    AllowSetForegroundWindow(ASFW_ANY);
                    signal.Set();
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Write("could not signal the running instance: " + ex.Message);
        }

        // Older build, or the event could not be created: fall back to its window if it has one.
        return ActivateExistingWindow();
    }

    private static bool ActivateExistingWindow()
    {
        try
        {
            Process self = Process.GetCurrentProcess();
            foreach (Process other in Process.GetProcessesByName(self.ProcessName))
            {
                if (other.Id == self.Id || other.MainWindowHandle == IntPtr.Zero)
                {
                    continue;
                }
                ShowWindow(other.MainWindowHandle, SW_RESTORE);
                SetForegroundWindow(other.MainWindowHandle);
                return true;
            }
        }
        catch
        {
            // Nothing to hand over to.
        }
        return false;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try { _singleInstance?.ReleaseMutex(); } catch { }
        _singleInstance?.Dispose();
        ShowSignal?.Dispose();
        base.OnExit(e);

        // Closing the window has to end the process, not just the UI.
        //
        // Two things in the 2014 engine make that not happen by itself. It starts threads without
        // IsBackground (GClass102, GClass270), and a single foreground thread keeps the CLR alive
        // long after Main returns. And its volume readers never close the handles they opened —
        // GClass156.Dispose only clears a "keep reading" flag, leaving the SafeFileHandle to a
        // finalizer that may never run.
        //
        // The result is a process nobody can see, still holding \\.\C: and every other volume
        // open. That is enough to make Windows refuse to safely remove a USB drive, which is a
        // genuinely bad thing for a file search tool to do to someone.
        //
        // So end it deliberately. Everything above has already run: settings are saved in the
        // window's Closing handler, the tray icon and hotkey are released in Closed, and the
        // mutex is released just above. Ending the process here hands every remaining handle
        // back to Windows immediately.
        Environment.Exit(0);
    }

    internal static bool SystemUsesDarkTheme()
    {
        try
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return key?.GetValue("AppsUseLightTheme") is int light && light == 0;
        }
        catch
        {
            // Match the "value is absent" answer above rather than contradicting it: Windows
            // treats a missing AppsUseLightTheme as light.
            return false;
        }
    }
}
