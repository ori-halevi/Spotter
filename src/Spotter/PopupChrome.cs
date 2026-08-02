using System.Windows;
using System.Windows.Controls;
using Spotter.Localization;

namespace Spotter;

/// <summary>
/// Gives tooltips and context menus the two things they cannot inherit: the current reading
/// direction and the current <see cref="UiScale"/>.
///
/// They need telling because each is its own top-level window rather than part of the window's
/// visual tree. Neither <c>FlowDirection</c> nor a <c>LayoutTransform</c> crosses that boundary —
/// in Hebrew that left a tooltip whose body was right-aligned sitting under a heading still flush
/// left, and at 130% it left every menu drawn at the one size in the app that had not grown.
///
/// Two approaches that look right and are not:
///
/// 1. An implicit <c>Style</c> in Application.Resources. Written as
///    <c>BasedOn="{StaticResource {x:Type ToolTip}}"</c> it appears to extend the theme, but WPF-UI
///    registers no implicit style under that key — so the lookup falls through to the *system*
///    theme and every tooltip silently reverts to stock light-grey WPF chrome inside a dark Fluent
///    window.
/// 2. A class handler on <c>FrameworkElement.LoadedEvent</c>. It never runs: WPF raises Loaded
///    through its own broadcast helper rather than the routed-event path, so class handlers are not
///    invoked. This failed completely silently — the direction simply never changed.
///
/// What works is the *opening* events, which are ordinary routed events raised on the owner before
/// the popup is shown, so the placement is computed from the direction and the size we set rather
/// than corrected after the fact.
/// </summary>
internal static class PopupChrome
{
    /// <summary>
    /// Whether this popup should follow the UI language. Default true; set False on a popup whose
    /// content is machine data — the indexing tooltip is a table of volume names, percentages and
    /// byte counts, and is left-to-right because of what it is, not because of the language.
    ///
    /// The scale is not optional in the same way. There is no such thing as a popup that should be
    /// drawn at a size the rest of the app is not.
    /// </summary>
    public static readonly DependencyProperty FollowProperty =
        DependencyProperty.RegisterAttached(
            "Follow", typeof(bool), typeof(PopupChrome), new PropertyMetadata(true));

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
    /// Assigns both every time rather than only when unset, so a language or scale change taken
    /// mid-session reaches popups that have already been opened once — which, being shared
    /// resources, is most of them.
    /// </summary>
    private static void Apply(FrameworkElement popup)
    {
        if (GetFollow(popup))
        {
            popup.FlowDirection = Lang.Instance.FlowDirection;
        }

        // The shared instance, not a copy: a scale changed while the menu is open then follows.
        popup.LayoutTransform = UiScale.Transform;
    }

    /// <summary>
    /// Scales a combo box's drop-down, which is a popup of the theme's making rather than one of
    /// ours and so is reached through the template.
    ///
    /// Hooked to <c>DropDownOpened</c> rather than done once, because the template is not applied
    /// until the box is first measured, and because the popup's child is replaced when the theme
    /// changes. Silently does nothing if the theme names its popup something else — an unscaled
    /// drop-down is a blemish, not a reason to fail.
    /// </summary>
    public static void ScaleDropDown(ComboBox box)
    {
        if (box == null)
        {
            return;
        }

        box.DropDownOpened += (_, __) =>
        {
            box.ApplyTemplate();
            if (box.Template?.FindName("PART_Popup", box)
                    is System.Windows.Controls.Primitives.Popup popup
                && popup.Child is FrameworkElement child)
            {
                child.LayoutTransform = UiScale.Transform;
            }
        };
    }
}
