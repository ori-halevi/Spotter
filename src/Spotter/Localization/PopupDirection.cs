using System.Windows;
using System.Windows.Controls;

namespace Spotter.Localization;

/// <summary>
/// Gives tooltips and context menus the current reading direction.
///
/// They need telling. Each is its own top-level window rather than part of the window's visual
/// tree, so neither inherits <c>FlowDirection</c> from it — in Hebrew that left a tooltip whose body
/// was right-aligned sitting under a heading still flush left.
///
/// Two approaches that look right and are not:
///
/// 1. An implicit <c>Style</c> in Application.Resources with a FlowDirection setter. Written as
///    <c>BasedOn="{StaticResource {x:Type ToolTip}}"</c> it appears to extend the theme, but WPF-UI
///    registers no implicit style under that key — so the lookup falls through to the *system*
///    theme and every tooltip silently reverts to stock light-grey WPF chrome inside a dark Fluent
///    window.
/// 2. A class handler on <c>FrameworkElement.LoadedEvent</c>. It never runs: WPF raises Loaded
///    through its own broadcast helper rather than the routed-event path, so class handlers are not
///    invoked. This failed completely silently — the direction simply never changed.
///
/// What works is the *opening* events, which are ordinary routed events raised on the owner before
/// the popup is shown, so the placement is computed from the direction we set rather than corrected
/// after the fact.
/// </summary>
internal static class PopupDirection
{
    /// <summary>
    /// Whether this popup should follow the UI language. Default true; set False on a popup whose
    /// content is machine data — the indexing tooltip is a table of volume names, percentages and
    /// byte counts, and is left-to-right because of what it is, not because of the language.
    /// </summary>
    public static readonly DependencyProperty FollowProperty =
        DependencyProperty.RegisterAttached(
            "Follow", typeof(bool), typeof(PopupDirection), new PropertyMetadata(true));

    public static void SetFollow(DependencyObject element, bool value) =>
        element.SetValue(FollowProperty, value);

    public static bool GetFollow(DependencyObject element) =>
        (bool)element.GetValue(FollowProperty);

    /// <summary>Registers the class handlers for the process. Called once, at startup.</summary>
    public static void Install()
    {
        EventManager.RegisterClassHandler(typeof(FrameworkElement),
            ToolTipService.ToolTipOpeningEvent, new ToolTipEventHandler(OnToolTipOpening));

        EventManager.RegisterClassHandler(typeof(FrameworkElement),
            ContextMenuService.ContextMenuOpeningEvent,
            new ContextMenuEventHandler(OnContextMenuOpening));
    }

    private static void OnToolTipOpening(object sender, ToolTipEventArgs e)
    {
        if (sender is not FrameworkElement owner)
        {
            return;
        }

        object content = ToolTipService.GetToolTip(owner);
        if (content == null)
        {
            return;
        }

        if (content is ToolTip existing)
        {
            Apply(existing);
            return;
        }

        // A bare string or a panel. WPF would wrap it in a ToolTip of its own that we never get a
        // reference to, so it is wrapped here first — setting the tooltip during ToolTipOpening is
        // the supported way to supply one late, and the wrapper is reused from then on.
        ToolTip wrapper = new ToolTip { Content = content };
        Apply(wrapper);
        ToolTipService.SetToolTip(owner, wrapper);
    }

    private static void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        // Context menus are always real ContextMenu instances here — shared window resources, one
        // menu serving three text boxes — so there is nothing to wrap.
        if (sender is FrameworkElement owner
            && ContextMenuService.GetContextMenu(owner) is ContextMenu menu)
        {
            Apply(menu);
        }
    }

    /// <summary>
    /// Assigns the direction every time rather than only when unset, so a language switch taken
    /// mid-session reaches popups that have already been opened once — which, being shared
    /// resources, is most of them.
    /// </summary>
    private static void Apply(FrameworkElement popup)
    {
        if (GetFollow(popup))
        {
            popup.FlowDirection = Lang.Instance.FlowDirection;
        }
    }
}
