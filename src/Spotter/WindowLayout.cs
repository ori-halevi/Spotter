using System;
using System.Windows;

namespace Spotter;

/// <summary>
/// Where a window opens, how small it may get, and which of the user's own arrangements are worth
/// remembering.
///
/// The rule the app follows: a search window is something you glance at, not something you live
/// in, so it never opens filling the screen. It opens in the top-left quarter, inset from both the
/// screen's corner and the middle of the screen, at a size and a <see cref="UiScale"/> that both
/// follow the size of the screen rather than its pixel count. Everything the user then does to it
/// is kept — except the two arrangements that would make the next launch look broken.
/// </summary>
internal static class WindowLayout
{
    /// <summary>
    /// The smallest the main window's content is usable at, before <see cref="UiScale"/> is
    /// applied — the query row needs three fields side by side, and the results grid needs enough
    /// height to be a list rather than a peephole. Multiplied by the scale at the point of use, so
    /// a bigger interface gets a proportionally bigger floor.
    /// </summary>
    public static readonly Size MinimumContent = new Size(720, 420);

    /// <summary>
    /// How far the window sits inside its quarter of the screen, as a fraction of the shorter side
    /// of that quarter — so the gap grows with the screen instead of looking generous on a laptop
    /// and hairline on a television.
    /// </summary>
    private const double GapFraction = 0.05;

    private const double MinimumGap = 12;
    private const double MaximumGap = 48;

    // ---- what counts as an arrangement worth keeping ------------------------------------------

    /// <summary>
    /// The strip along the top of a window that can be grabbed to drag it. A window whose title
    /// bar is off-screen cannot be moved back with the mouse at all, which is the one way to park
    /// a window that a user cannot undo.
    /// </summary>
    private const double GrabStripHeight = 32;

    /// <summary>Enough title bar to actually hit with a pointer.</summary>
    private const double MinimumGrabWidth = 180;

    /// <summary>
    /// How much of the window has to be on a screen somewhere. A window deliberately pushed half
    /// off the edge is a real thing people do and is kept; one that is a sliver is not.
    /// </summary>
    private const double MinimumVisibleFraction = 0.3;

    /// <summary>
    /// Below this, a stored size is not a small window — it is a corrupt or hand-edited settings
    /// file. The real floor is the window's own MinWidth/MinHeight, which Windows enforces while
    /// the user drags.
    /// </summary>
    private const double AbsurdlySmall = 200;

    /// <summary>
    /// Bumped when the meaning of the stored geometry changes, so an old file is ignored once
    /// rather than restoring a position that means something else now. Version 1 was the 1200x720
    /// centred default with no scale attached to it.
    /// </summary>
    private const int CurrentVersion = 2;

    // ---- the default ---------------------------------------------------------------------------

    /// <summary>
    /// The top-left quarter of the screen, inset on all four sides.
    ///
    /// Picture the screen quartered by a cross: this is the top-left cell, pulled in from the
    /// screen's own corner and from the middle of the cross by the same gap, so it touches neither.
    /// The result is a window that is obviously a window — you can see the desktop around it — and
    /// that leaves the other three quarters clear for whatever the search is *for*.
    /// </summary>
    public static Rect Default(Display screen, double scale)
    {
        Rect area = screen.WorkArea;
        double quarterWidth = area.Width / 2;
        double quarterHeight = area.Height / 2;

        double gap = Math.Round(Math.Clamp(
            GapFraction * Math.Min(quarterWidth, quarterHeight), MinimumGap, MaximumGap));

        // The floor wins over the quarter: on a small screen a quarter is smaller than the window
        // needs to be usable, and a cramped window is worse than one that spills past the middle.
        double width = Math.Min(
            Math.Max(quarterWidth - gap * 2, MinimumContent.Width * scale), area.Width);
        double height = Math.Min(
            Math.Max(quarterHeight - gap * 2, MinimumContent.Height * scale), area.Height);

        double left = Math.Max(area.Left, Math.Min(area.Left + gap, area.Right - width));
        double top = Math.Max(area.Top, Math.Min(area.Top + gap, area.Bottom - height));

        return new Rect(left, top, width, height);
    }

    /// <summary>Puts a window back where it would have opened on a first run, straight away.</summary>
    public static void ApplyDefault(Window window)
    {
        if (window == null)
        {
            return;
        }
        window.WindowState = WindowState.Normal;

        Rect bounds = Default(Displays.For(window), UiScale.Current);
        window.Left = bounds.Left;
        window.Top = bounds.Top;
        window.Width = bounds.Width;
        window.Height = bounds.Height;
    }

    // ---- validating a remembered arrangement ----------------------------------------------------

    /// <summary>
    /// Whether a remembered rectangle is one the user could still work with on the monitors
    /// attached right now.
    ///
    /// Two things disqualify it, and only two — everything else the user did is theirs to keep:
    /// there is not enough title bar on any screen to grab, or too little of the window is on a
    /// screen at all. Both cover the same accident from different directions: the window was
    /// dragged somewhere it cannot be dragged back from, or the monitor it was on has since been
    /// unplugged.
    /// </summary>
    public static bool IsUsable(Rect bounds)
    {
        if (double.IsNaN(bounds.X) || double.IsNaN(bounds.Y)
            || double.IsNaN(bounds.Width) || double.IsNaN(bounds.Height)
            || double.IsInfinity(bounds.X) || double.IsInfinity(bounds.Y)
            || bounds.Width < AbsurdlySmall || bounds.Height < AbsurdlySmall)
        {
            return false;
        }

        Rect grabStrip = new Rect(bounds.Left, bounds.Top, bounds.Width,
            Math.Min(GrabStripHeight, bounds.Height));

        double visibleArea = 0;
        double grabbableWidth = 0;
        foreach (Display display in Displays.All)
        {
            Rect onScreen = Rect.Intersect(bounds, display.WorkArea);
            if (!onScreen.IsEmpty)
            {
                visibleArea += onScreen.Width * onScreen.Height;
            }

            // Per monitor, not summed: half a title bar on each of two screens is still half a
            // title bar to aim at, and the gap between them is not clickable.
            Rect grabbable = Rect.Intersect(grabStrip, display.WorkArea);
            if (!grabbable.IsEmpty && grabbable.Height >= GrabStripHeight / 2)
            {
                grabbableWidth = Math.Max(grabbableWidth, grabbable.Width);
            }
        }

        return grabbableWidth >= Math.Min(MinimumGrabWidth, bounds.Width)
               && visibleArea >= bounds.Width * bounds.Height * MinimumVisibleFraction;
    }

    // ---- persistence ------------------------------------------------------------------------------

    /// <summary>
    /// Positions a window from the settings, falling back to <see cref="Default"/> whenever the
    /// stored arrangement is missing, from an older build, turned off, or no longer usable.
    /// </summary>
    public static void Restore(Window window, UiSettings settings)
    {
        window.WindowStartupLocation = WindowStartupLocation.Manual;

        Rect bounds = Remembered(settings);
        if (bounds.IsEmpty)
        {
            bounds = Default(Displays.Primary, UiScale.Current);
        }

        // An extra instance restores the same remembered arrangement as the first one, which
        // means it opens exactly on top of it and hides it completely — and the whole point of a
        // second window is to see it beside the first.
        if (App.SecondaryInstance)
        {
            bounds = Cascade(bounds, App.OtherInstanceCount());
        }

        window.Left = bounds.Left;
        window.Top = bounds.Top;
        window.Width = bounds.Width;
        window.Height = bounds.Height;

        // Maximized is remembered for the window that owns the settings. An extra window that
        // came up maximized would cover the first one whatever the cascade did.
        if (settings.RememberPlacement && settings.Maximized && !App.SecondaryInstance)
        {
            window.WindowState = WindowState.Maximized;
        }
    }

    /// <summary>
    /// Steps a window down and across from the one before it, the way Windows has cascaded new
    /// windows since it had any.
    ///
    /// The full offset is tried first and then shorter ones, so a window near the bottom-right
    /// corner stacks less far instead of walking off the screen — <see cref="IsUsable"/> is the
    /// same test the remembered position has to pass, so a cascaded window is always one that
    /// can be grabbed and dragged.
    /// </summary>
    private static Rect Cascade(Rect bounds, int index)
    {
        // One title bar: enough that the window underneath is visible and clickable, little
        // enough that the two still overlap, which is what says they are the same program. It
        // follows the interface scale because the title bar it is measured in does.
        double step = 34 * UiScale.Current;

        for (int n = index; n > 0; n--)
        {
            Rect moved = new Rect(bounds.Left + n * step, bounds.Top + n * step,
                                  bounds.Width, bounds.Height);
            if (IsUsable(moved))
            {
                return moved;
            }
        }
        return bounds;
    }

    /// <summary>
    /// The stored arrangement if it still stands up, or <see cref="Rect.Empty"/> to mean "use the
    /// default". A stored size larger than the monitor it lands on is trimmed rather than thrown
    /// away: that is a screen that shrank, not a user who did something odd.
    /// </summary>
    private static Rect Remembered(UiSettings settings)
    {
        if (!settings.RememberPlacement || settings.PlacementVersion != CurrentVersion)
        {
            return Rect.Empty;
        }

        Rect bounds = new Rect(
            settings.WindowLeft, settings.WindowTop, Zeroed(settings.WindowWidth),
            Zeroed(settings.WindowHeight));

        if (!IsUsable(bounds))
        {
            if (!double.IsNaN(settings.WindowLeft))
            {
                Log.Write("the remembered window position is not reachable on the monitors "
                          + "attached now, opening in the default place instead");
            }
            return Rect.Empty;
        }

        Rect area = Displays.Containing(bounds).WorkArea;
        return new Rect(bounds.Left, bounds.Top,
            Math.Min(bounds.Width, area.Width), Math.Min(bounds.Height, area.Height));
    }

    private static double Zeroed(double value) => double.IsNaN(value) ? 0 : value;

    /// <summary>
    /// Records how the window is arranged now, for the next launch.
    ///
    /// <paramref name="maximized"/> is passed in rather than read off the window because the caller
    /// knows something this does not: a window sitting hidden in the tray reports a state that
    /// describes the tray, not the way the user left it.
    /// </summary>
    public static void Capture(Window window, UiSettings settings, bool maximized)
    {
        if (!settings.RememberPlacement)
        {
            Forget(settings);
            return;
        }

        settings.Maximized = maximized;

        // A maximized window's Left/Top/Width/Height still describe the size it will return to, so
        // there is nothing to capture in that state and overwriting the restore size with the
        // screen's would lose it.
        if (window.WindowState != WindowState.Normal)
        {
            return;
        }

        Rect bounds = new Rect(window.Left, window.Top, window.Width, window.Height);
        if (!IsUsable(bounds))
        {
            Log.Write("not remembering this window position — it would be unreachable next time");
            Forget(settings);
            return;
        }

        settings.WindowLeft = bounds.Left;
        settings.WindowTop = bounds.Top;
        settings.WindowWidth = bounds.Width;
        settings.WindowHeight = bounds.Height;
        settings.PlacementVersion = CurrentVersion;
    }

    /// <summary>Drops the stored arrangement, so the next launch starts from the default.</summary>
    public static void Forget(UiSettings settings)
    {
        settings.WindowLeft = double.NaN;
        settings.WindowTop = double.NaN;
        settings.WindowWidth = double.NaN;
        settings.WindowHeight = double.NaN;
        settings.Maximized = false;
        settings.PlacementVersion = 0;
    }

    // ---- keeping a window in step with the scale --------------------------------------------------

    /// <summary>
    /// Hangs a window's content off the shared scale transform and keeps the window's own minimum
    /// size in step with it, for as long as the window lives.
    ///
    /// The minimum has to be maintained from here because it is a property of the window frame,
    /// which sits outside the transform: at 140% the same content genuinely needs 40% more window
    /// before it stops being squashed.
    /// </summary>
    public static void Scale(Window window, FrameworkElement content, Size minimumContent)
    {
        content.LayoutTransform = UiScale.Transform;

        void Follow() => ApplyMinimum(window, minimumContent);

        Follow();
        UiScale.Changed += Follow;
        window.Closed += (_, __) => UiScale.Changed -= Follow;
    }

    /// <summary>
    /// Capped at the screen, because a minimum larger than the monitor is a window that cannot be
    /// moved or closed — Windows honours MinWidth even when it means hanging off the edge.
    /// </summary>
    private static void ApplyMinimum(Window window, Size minimumContent)
    {
        Rect area = Displays.For(window).WorkArea;
        window.MinWidth = Math.Min(minimumContent.Width * UiScale.Current, area.Width);
        window.MinHeight = Math.Min(minimumContent.Height * UiScale.Current, area.Height);
    }
}
