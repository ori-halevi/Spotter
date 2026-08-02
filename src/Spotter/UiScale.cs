using System;
using System.Windows.Media;

namespace Spotter;

/// <summary>
/// How large the whole interface is drawn — text, padding, icons, row heights, the title bar, the
/// lot.
///
/// It is one <see cref="ScaleTransform"/>, shared. Every window hangs its content off this exact
/// instance as a <c>LayoutTransform</c>, so changing <see cref="Current"/> re-lays out everything
/// that is already on screen, the same way picking a language re-renders every label. A transform
/// rather than a font size because the request is for the size of *everything*: a font size scales
/// the words and leaves the padding, the icons and the scrollbars where they were.
///
/// A LayoutTransform does not reach content that WPF renders in its own top-level window —
/// tooltips, context menus and the popups this app opens by hand. Each of those is given the same
/// transform at the point it opens; see <see cref="PopupChrome"/> and MainWindow's popups.
/// </summary>
internal static class UiScale
{
    /// <summary>
    /// What the user may pick by hand. Wider than the automatic range on purpose: this is the
    /// override, and someone who wants the window tiny on a projector or huge on a television is
    /// not making a mistake.
    /// </summary>
    public const double Minimum = 0.5;

    public const double Maximum = 2.5;

    /// <summary>
    /// The bounds on the *automatic* value. Left to itself the formula below would keep going in
    /// both directions; on a small laptop that means text nobody can read, and on a wall-mounted
    /// television it means a search box the size of a hand.
    /// </summary>
    private const double AutomaticMinimum = 0.75;

    private const double AutomaticMaximum = 1.5;

    /// <summary>
    /// The screen the layout is drawn at its own size for.
    ///
    /// Set between the two shapes of ordinary desktop monitor rather than on top of either: a
    /// 1920x1080 panel measures 2184 units across the diagonal, a 2560x1440 one at 150% measures
    /// 2149, and the same panel at 125% measures 2327. All three are a normal monitor at a normal
    /// viewing distance and all three want the layout at the size it was drawn, so the reference
    /// sits in the middle of them and every one of them rounds to 100%. Landing it on 1920x1080
    /// exactly, which is where this started, put the 125% case at 105% — a difference you can see,
    /// for no reason a user could name.
    /// </summary>
    private const double ReferenceWidth = 2000;

    private const double ReferenceHeight = 1030;

    /// <summary>
    /// How hard the scale follows the screen. 1 would be proportional — twice the screen, twice
    /// the text — which is far too much: a screen that is twice the size does not want twice the
    /// type, it wants slightly bigger type and a great many more rows. At 0.3 a screen twice the
    /// diagonal gets about 23% more type and the rest of the space goes to content, and the whole
    /// ordinary-desktop band above stays flat at 100% instead of drifting across it.
    /// </summary>
    private const double Damping = 0.3;

    /// <summary>The transform every window and popup in the app shares. Never replaced.</summary>
    public static ScaleTransform Transform { get; } = new ScaleTransform(1, 1);

    public static double Current { get; private set; } = 1;

    /// <summary>
    /// What "match my screen" works out to on this machine — computed whether or not it is in use,
    /// because the options window says so next to the switch that turns it off.
    /// </summary>
    public static double Automatic { get; private set; } = 1;

    /// <summary>Raised after <see cref="Apply"/> changes the scale, for anything a transform cannot carry.</summary>
    public static event Action Changed;

    /// <summary>
    /// Works out the automatic value and applies whichever value the settings ask for. Called once,
    /// before the first window is constructed — the whole UI is laid out through this transform, so
    /// starting at 1 and correcting afterwards would be a visible jump on every launch.
    /// </summary>
    public static void Initialize(UiSettings settings)
    {
        Automatic = ComputeAutomatic(Displays.Primary);
        Apply(Resolve(settings));
        Log.Write($"ui scale {Current:0.##} "
                  + (settings != null && !settings.ScaleFollowsScreen
                      ? "(locked in options)"
                      : $"(from the screen; {Displays.Primary.WorkArea.Width:0}x"
                        + $"{Displays.Primary.WorkArea.Height:0} units usable)"));
    }

    /// <summary>The scale a settings object asks for, without applying it.</summary>
    public static double Resolve(UiSettings settings) =>
        settings == null || settings.ScaleFollowsScreen ? Automatic : Clamp(settings.Scale);

    /// <summary>
    /// Applies a scale to the whole app. Safe to call repeatedly and while windows are open — that
    /// is how the options window previews a value before it is saved.
    /// </summary>
    public static void Apply(double scale)
    {
        scale = Clamp(scale);
        if (Math.Abs(scale - Current) < 0.0001)
        {
            return;
        }
        Current = scale;
        Transform.ScaleX = scale;
        Transform.ScaleY = scale;
        Changed?.Invoke();
    }

    public static double Clamp(double scale) =>
        double.IsNaN(scale) || double.IsInfinity(scale)
            ? 1
            : Math.Clamp(scale, Minimum, Maximum);

    /// <summary>
    /// The scale that suits a screen of this size.
    ///
    /// Size, not resolution: <see cref="Display.WorkArea"/> is in units of 1/96 inch, so a 4K panel
    /// that Windows drives at 150% measures the same here as the 1080p panel of the same physical
    /// size sitting next to it — and gets the same answer, which is the entire point. Rounded to
    /// the nearest 5% so the number shown in Options is one a person would write down.
    /// </summary>
    private static double ComputeAutomatic(Display screen)
    {
        double reference = Math.Sqrt(
            ReferenceWidth * ReferenceWidth + ReferenceHeight * ReferenceHeight);
        double diagonal = screen.Diagonal;
        if (diagonal <= 0 || reference <= 0)
        {
            return 1;
        }

        double raw = Math.Pow(diagonal / reference, Damping);
        return Math.Round(Math.Clamp(raw, AutomaticMinimum, AutomaticMaximum) * 20) / 20;
    }
}
