using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Spotter;

/// <summary>
/// Small file-type icons from the shell.
///
/// Cached by extension rather than by path, which is the whole trick: every .dll shares one
/// icon, so a 300,000-row result set needs a few dozen lookups instead of 300,000. Combined
/// with SHGFI_USEFILEATTRIBUTES — which tells the shell to answer from the extension alone and
/// never touch the disk — this stays cheap enough to resolve on demand while scrolling.
///
/// The 1.5.1 app instead queued real per-file lookups on a background thread with a LIFO stack
/// for visible rows. That is more accurate (it picks up per-file icons like .exe resources) but
/// far more machinery; extension icons are the right trade for now, and per-file icons for
/// executables can be layered on later.
/// </summary>
internal static class ShellIcons
{
    private const uint SHGFI_ICON = 0x000000100;
    private const uint SHGFI_SMALLICON = 0x000000001;
    private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
    private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
    private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;

    private static readonly Dictionary<string, ImageSource> Cache =
        new Dictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Every caller today is on the UI thread, but nothing enforces that and a plain Dictionary
    /// corrupts silently rather than throwing if that ever stops being true. The lock is
    /// uncontended in the normal case, which costs nothing worth measuring next to a shell call.
    /// </summary>
    private static readonly object Gate = new object();

    /// <summary>
    /// A whole-disk search turns up a long tail of junk extensions, and each miss would otherwise
    /// be remembered forever. Well above the few hundred extensions a real machine has.
    /// </summary>
    private const int CacheLimit = 4096;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string szTypeName;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
        ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    public static ImageSource Get(string name, bool isDirectory)
    {
        string key = isDirectory
            ? "\\folder"
            : ExtensionOf(name);

        lock (Gate)
        {
            if (Cache.TryGetValue(key, out ImageSource cached))
            {
                return cached;
            }

            ImageSource icon = Load(key, isDirectory);
            if (Cache.Count >= CacheLimit)
            {
                Cache.Clear();
            }
            Cache[key] = icon;      // cache misses too, so a failure is not retried per row
            return icon;
        }
    }

    private static string ExtensionOf(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return string.Empty;
        }
        int dot = name.LastIndexOf('.');
        return dot > 0 && dot < name.Length - 1 ? name.Substring(dot) : string.Empty;
    }

    private static ImageSource Load(string key, bool isDirectory)
    {
        SHFILEINFO info = new SHFILEINFO();
        // A name, not a path: with USEFILEATTRIBUTES the shell resolves from the extension and
        // never hits the filesystem, so this works for paths that no longer exist.
        string probe = isDirectory ? "folder" : "file" + key;
        uint attributes = isDirectory ? FILE_ATTRIBUTE_DIRECTORY : FILE_ATTRIBUTE_NORMAL;

        IntPtr result = SHGetFileInfo(probe, attributes, ref info,
            (uint)Marshal.SizeOf<SHFILEINFO>(),
            SHGFI_ICON | SHGFI_SMALLICON | SHGFI_USEFILEATTRIBUTES);

        if (result == IntPtr.Zero || info.hIcon == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            ImageSource source = Imaging.CreateBitmapSourceFromHIcon(
                info.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();      // shared across threads and rows
            return source;
        }
        catch
        {
            return null;
        }
        finally
        {
            DestroyIcon(info.hIcon);
        }
    }
}
