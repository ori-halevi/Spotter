using System;
using System.Drawing;
using System.Windows.Forms;

namespace Spotter;

/// <summary>
/// Notification-area presence: a menu, and a way back to the window.
///
/// Still WinForms' NotifyIcon — WPF has no equivalent, and every "WPF tray icon" library wraps
/// this same control. It is the one place the new shell reaches back into WinForms.
/// </summary>
public sealed class TrayIcon : IDisposable
{
    private readonly NotifyIcon _icon;

    /// <summary>Restore requested: double-click, or "Show Spotter".</summary>
    public event Action ShowRequested;

    /// <summary>Real exit requested, as opposed to closing to the tray.</summary>
    public event Action ExitRequested;

    /// <summary>
    /// The menu and the icon, kept so Dispose can release them.
    ///
    /// NotifyIcon.Dispose() releases neither: it does not own the ContextMenuStrip it is given,
    /// and it does not own the Icon. Without these two fields the menu, its items, the bold font
    /// on "Show" and the HICON all outlived the tray icon.
    /// </summary>
    private readonly ContextMenuStrip _menu;

    /// <summary>
    /// Null when the icon came from <see cref="SystemIcons"/>, which is a shared static that must
    /// never be disposed - doing so breaks it for the rest of the process.
    /// </summary>
    private readonly Icon _ownedIcon;

    /// <summary>
    /// The bold font on "Show". ToolStripItem does not own the font it is given either, so this
    /// is a third thing Dispose has to release by hand.
    /// </summary>
    private readonly Font _boldFont;

    public TrayIcon()
    {
        ContextMenuStrip menu = _menu = new ContextMenuStrip();

        // WinForms has no equivalent of WPF's FlowDirection inheritance, so the tray menu is told
        // outright. Without it a Hebrew menu keeps its text right-aligned but its submenu arrows
        // and item padding on the left, which is the half-mirrored look that reads as a bug.
        if (Localization.Lang.Instance.IsRightToLeft)
        {
            menu.RightToLeft = RightToLeft.Yes;
        }

        ToolStripMenuItem show = new ToolStripMenuItem(Localization.Lang.Tr("Tray.Show"));
        show.Font = _boldFont = new Font(show.Font, FontStyle.Bold);   // marks it as the default action
        show.Click += (_, __) => ShowRequested?.Invoke();
        menu.Items.Add(show);
        menu.Items.Add(new ToolStripSeparator());
        ToolStripMenuItem exit = new ToolStripMenuItem(Localization.Lang.Tr("Tray.Exit"));
        exit.Click += (_, __) => ExitRequested?.Invoke();
        menu.Items.Add(exit);

        Icon icon = LoadIcon(out bool owned);
        _ownedIcon = owned ? icon : null;
        _icon = new NotifyIcon
        {
            Text = "Spotter",
            Icon = icon,
            ContextMenuStrip = menu,
            Visible = false,
        };
        _icon.DoubleClick += (_, __) => ShowRequested?.Invoke();
    }

    /// <summary>
    /// <paramref name="owned"/> is false for the <see cref="SystemIcons"/> fallback, which is
    /// shared process-wide and not ours to dispose.
    /// </summary>
    private static Icon LoadIcon(out bool owned)
    {
        try
        {
            // The app's own icon, straight out of the executable — no separate asset to keep in
            // sync, and it matches whatever the exe shows in Explorer.
            Icon extracted = Icon.ExtractAssociatedIcon(Environment.ProcessPath);
            if (extracted != null)
            {
                owned = true;
                return extracted;
            }
        }
        catch
        {
        }
        owned = false;
        return SystemIcons.Application;
    }

    public bool Visible
    {
        get => _icon.Visible;
        set => _icon.Visible = value;
    }

    /// <summary>One-time hint the first time the window disappears into the tray.</summary>
    public void NotifyHidden(string hotkeyText)
    {
        try
        {
            string tip = string.IsNullOrEmpty(hotkeyText)
                ? Localization.Lang.Tr("Tray.Hidden.NoHotkey")
                : Localization.Lang.Tr("Tray.Hidden.Hotkey", hotkeyText);
            _icon.ShowBalloonTip(4000, "Spotter", tip, ToolTipIcon.Info);
        }
        catch
        {
            // Balloon tips are suppressed by some policies; never worth failing over.
        }
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
        // After the NotifyIcon, so nothing is drawing from either of them any more. The menu
        // and the font are not owned by anything the NotifyIcon released, so they go by hand.
        _menu.Dispose();
        _boldFont.Dispose();
        _ownedIcon?.Dispose();
    }
}
