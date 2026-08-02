using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;

namespace Spotter;

/// <summary>
/// One monitor, described the way a window needs it: how much of it is usable, and how many
/// device pixels it puts into a device-independent unit.
/// </summary>
internal readonly struct Display
{
    public Display(Rect workArea, double scale, bool isPrimary)
    {
        WorkArea = workArea;
        Scale = scale;
        IsPrimary = isPrimary;
    }

    /// <summary>
    /// The usable area — the monitor minus the taskbar and any other appbar — in the same units a
    /// WPF <see cref="Window"/> expresses Left, Top, Width and Height in, so the two can be
    /// compared and mixed directly.
    ///
    /// Windows hands this out in device pixels on the virtual desktop; it is divided here by the
    /// monitor's own scale, which is exactly the mapping WPF applies to a window sitting on that
    /// monitor. On a machine where every monitor runs at the same scaling that makes one
    /// consistent coordinate space; where they differ it is piecewise, one piece per monitor —
    /// and so is WPF's, which is the point.
    /// </summary>
    public Rect WorkArea { get; }

    /// <summary>Device pixels per unit: 1.0 at 96 dpi, 1.5 where Windows is set to 150%.</summary>
    public double Scale { get; }

    public bool IsPrimary { get; }

    /// <summary>
    /// The diagonal of the usable area, in units. This is the app's stand-in for "how big is this
    /// screen": a unit is 1/96 inch by definition, so unlike a pixel count it does not change when
    /// the same physical panel is driven at a higher resolution and scaled back up by Windows.
    /// </summary>
    public double Diagonal =>
        Math.Sqrt(WorkArea.Width * WorkArea.Width + WorkArea.Height * WorkArea.Height);
}

/// <summary>
/// The monitors attached right now.
///
/// Deliberately never cached. Monitors are plugged in, unplugged, and rearranged while the app is
/// running, and the whole point of the callers — deciding where a window may open and whether a
/// remembered position is still reachable — is to be right about the machine as it is at that
/// moment. Enumerating is three cheap Win32 calls per monitor.
/// </summary>
internal static class Displays
{
    private const uint MONITORINFOF_PRIMARY = 1;

    /// <summary>MDT_EFFECTIVE_DPI — the scaling the user actually chose for that monitor.</summary>
    private const int MDT_EFFECTIVE_DPI = 0;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    private delegate bool MonitorEnumProc(IntPtr monitor, IntPtr hdc, IntPtr clip, IntPtr data);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr clip,
        MonitorEnumProc callback, IntPtr data);

    [DllImport("user32.dll", EntryPoint = "GetMonitorInfoW")]
    private static extern bool GetMonitorInfo(IntPtr monitor, ref MONITORINFO info);

    /// <summary>Windows 8.1 and later. Absent on anything older, which is why every call is guarded.</summary>
    [DllImport("Shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr monitor, int dpiType,
        out uint dpiX, out uint dpiY);

    [DllImport("user32.dll")]
    private static extern uint GetDpiForSystem();

    /// <summary>
    /// Every attached monitor. Never empty: if Windows will not say, the answer is one screen the
    /// size of whatever WPF believes the primary work area to be, which is enough for every caller
    /// here to produce a sane window rather than refuse to open one.
    /// </summary>
    public static IReadOnlyList<Display> All
    {
        get
        {
            List<Display> found = new List<Display>();
            try
            {
                // Held in a local so the delegate cannot be collected while the callback is still
                // being invoked — the call is synchronous, but the argument alone is not a rooted
                // reference the GC is obliged to honour.
                MonitorEnumProc callback = (monitor, _, __, ___) =>
                {
                    Describe(monitor, found);
                    return true;
                };
                EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);
                GC.KeepAlive(callback);
            }
            catch (Exception ex)
            {
                Log.Write("could not enumerate monitors: " + ex.Message);
            }

            if (found.Count == 0)
            {
                found.Add(new Display(SystemParameters.WorkArea, SystemScale(), true));
            }
            return found;
        }
    }

    private static void Describe(IntPtr monitor, List<Display> into)
    {
        MONITORINFO info = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
        if (!GetMonitorInfo(monitor, ref info))
        {
            return;
        }

        double scale = ScaleOf(monitor);
        double width = (info.rcWork.Right - info.rcWork.Left) / scale;
        double height = (info.rcWork.Bottom - info.rcWork.Top) / scale;
        if (width < 1 || height < 1)
        {
            return;
        }

        into.Add(new Display(
            new Rect(info.rcWork.Left / scale, info.rcWork.Top / scale, width, height),
            scale,
            (info.dwFlags & MONITORINFOF_PRIMARY) != 0));
    }

    private static double ScaleOf(IntPtr monitor)
    {
        try
        {
            if (GetDpiForMonitor(monitor, MDT_EFFECTIVE_DPI, out uint dpiX, out _) == 0 && dpiX > 0)
            {
                return dpiX / 96.0;
            }
        }
        catch (Exception ex)
        {
            // Shcore or the export is missing, i.e. Windows 8 or older. The system-wide DPI is the
            // right answer there, because per-monitor DPI did not exist yet.
            Log.Write("per-monitor dpi unavailable, using the system dpi: " + ex.Message);
        }
        return SystemScale();
    }

    private static double SystemScale()
    {
        try
        {
            uint dpi = GetDpiForSystem();
            if (dpi > 0)
            {
                return dpi / 96.0;
            }
        }
        catch
        {
            // Pre-1607. 96 dpi is the only assumption left, and it is the common case anyway.
        }
        return 1.0;
    }

    public static Display Primary
    {
        get
        {
            IReadOnlyList<Display> all = All;
            foreach (Display display in all)
            {
                if (display.IsPrimary)
                {
                    return display;
                }
            }
            return all[0];
        }
    }

    /// <summary>
    /// The monitor a rectangle mostly sits on, or the primary one if it sits on none — which is
    /// what happens when a position was remembered from a monitor that is no longer attached.
    /// </summary>
    public static Display Containing(Rect bounds)
    {
        Display best = default;
        double bestOverlap = 0;
        foreach (Display display in All)
        {
            Rect shared = Rect.Intersect(bounds, display.WorkArea);
            double overlap = shared.IsEmpty ? 0 : shared.Width * shared.Height;
            if (overlap > bestOverlap)
            {
                bestOverlap = overlap;
                best = display;
            }
        }
        return bestOverlap > 0 ? best : Primary;
    }

    /// <summary>
    /// The monitor a window is on. Falls back to the primary one before the window has a position,
    /// which is the case while it is still being constructed.
    /// </summary>
    public static Display For(Window window)
    {
        if (window == null)
        {
            return Primary;
        }

        double left = window.Left;
        double top = window.Top;
        double width = window.ActualWidth > 0 ? window.ActualWidth : window.Width;
        double height = window.ActualHeight > 0 ? window.ActualHeight : window.Height;

        if (double.IsNaN(left) || double.IsNaN(top) || double.IsNaN(width) || double.IsNaN(height)
            || width <= 0 || height <= 0)
        {
            return Primary;
        }
        return Containing(new Rect(left, top, width, height));
    }
}
