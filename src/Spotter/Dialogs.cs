using System.Windows;
using Spotter.Localization;

namespace Spotter;

/// <summary>
/// The app's message boxes, in one place so they are all right-to-left aware.
///
/// <see cref="MessageBox"/> is a Win32 dialog, not WPF content: it is not part of any visual tree,
/// so it inherits nothing — not the theme, and not the window's <c>FlowDirection</c>. Left to
/// itself it lays Hebrew out left-to-right and flush left, which puts the sentence's final full stop
/// at the wrong end and the buttons in the wrong order. The one thing it does understand is
/// <see cref="MessageBoxOptions.RtlReading"/>, which is what these wrappers add.
///
/// They also carry the caption, which is the product name and is the same in every language.
/// </summary>
internal static class Dialogs
{
    private const string Caption = "Spotter";

    private static MessageBoxOptions Direction =>
        Lang.Instance.IsRightToLeft
            ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
            : MessageBoxOptions.None;

    /// <summary>A statement. One button.</summary>
    public static void Info(Window owner, string text,
                            MessageBoxImage image = MessageBoxImage.None)
    {
        // The owner-less overload exists because the "already running" message is shown before
        // there is a window to own it.
        if (owner == null)
        {
            MessageBox.Show(text, Caption, MessageBoxButton.OK, image,
                MessageBoxResult.OK, Direction);
        }
        else
        {
            MessageBox.Show(owner, text, Caption, MessageBoxButton.OK, image,
                MessageBoxResult.OK, Direction);
        }
    }

    /// <summary>
    /// A question. True only for Yes — and No is the default, so dismissing the dialog with Esc or
    /// the close button never counts as consent. That matters most for the one that deletes files.
    /// </summary>
    public static bool Confirm(Window owner, string text,
                               MessageBoxImage image = MessageBoxImage.Question)
    {
        MessageBoxResult answer = owner == null
            ? MessageBox.Show(text, Caption, MessageBoxButton.YesNo, image,
                MessageBoxResult.No, Direction)
            : MessageBox.Show(owner, text, Caption, MessageBoxButton.YesNo, image,
                MessageBoxResult.No, Direction);

        return answer == MessageBoxResult.Yes;
    }
}
