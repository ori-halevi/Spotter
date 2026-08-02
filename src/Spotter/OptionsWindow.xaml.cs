using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Spotter.Localization;

namespace Spotter;

public partial class OptionsWindow
{
    private readonly UiSettings _settings;
    private ModifierKeys _modifiers;
    private Key _key;

    public OptionsWindow(UiSettings settings)
    {
        InitializeComponent();
        _settings = settings;

        // Before anything is measured. This dialog is also the one place the scale can be changed,
        // so it re-lays itself out along with the window behind it as the slider moves.
        WindowLayout.Scale(this, RootLayout, new Size(460, 420));

        FillLanguages();

        if (Hotkey.TryParse(settings.RestoreHotkey, out ModifierKeys m, out Key k))
        {
            _modifiers = m;
            _key = k;
        }
        HotkeyBox.Text = Describe(_modifiers, _key);

        LiveSearchBox.IsChecked = settings.SearchAsYouType;
        UpdateLiveSearchHint();

        ShowTrayBox.IsChecked = settings.ShowTrayIcon;
        MinimizeTrayBox.IsChecked = settings.MinimizeToTray;
        CloseTrayBox.IsChecked = settings.CloseToTray;
        UpdateTrayEnabled();

        ThemeBox.SelectedIndex = settings.Theme switch
        {
            "Dark" => 1,
            "Light" => 2,
            _ => 0,
        };

        FillScale();
        ShowStorageLocations();

        // The two drop-downs open in their own windows, so the scale transform on this window's
        // content does not reach them.
        PopupChrome.ScaleDropDown(LanguageBox);
        PopupChrome.ScaleDropDown(ThemeBox);

        // Text this window writes from code, re-rendered when the picker changes language. The
        // XAML looks after itself, because {loc:Loc} is a binding.
        Lang.Changed += Relocalize;
        Closed += (_, __) =>
        {
            Lang.Changed -= Relocalize;
            RevertLanguageUnlessSaved();
            RevertScaleUnlessSaved();
        };
    }

    /// <summary>
    /// Sizes this dialog against the screen it is about to open on rather than the one that
    /// happened to be primary when it was constructed. By here the owner is set, and WPF has not
    /// yet worked out where to centre it — so changing the size now still centres it correctly.
    /// </summary>
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        Rect area = Displays.For(Owner ?? this).WorkArea;
        double scale = UiScale.Current;

        Width = Math.Min(NominalWidth * scale, area.Width);
        Height = Math.Min(NominalHeight * scale, area.Height * 0.94);
    }

    /// <summary>The size this dialog's content was laid out at, before the interface scale.</summary>
    private const double NominalWidth = 560;

    private const double NominalHeight = 820;

    // ---- language ----------------------------------------------------------------------------

    /// <summary>
    /// One entry in the picker. <see cref="Code"/> is null for "follow Windows", which is a
    /// different thing from picking the language Windows happens to be set to today: it keeps
    /// following if that changes.
    /// </summary>
    private sealed class LanguageChoice : System.ComponentModel.INotifyPropertyChanged
    {
        private string _label;

        public LanguageChoice(string code, string label)
        {
            Code = code;
            _label = label;
        }

        public string Code { get; }

        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                PropertyChanged?.Invoke(this,
                    new System.ComponentModel.PropertyChangedEventArgs(nameof(Label)));
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }

    private readonly List<LanguageChoice> _languages = new List<LanguageChoice>();

    /// <summary>
    /// What the language was when this window opened, so Cancel can undo the live preview.
    /// </summary>
    private string _languageOnEntry;

    /// <summary>Suppresses the handler while the list is being populated.</summary>
    private bool _fillingLanguages;

    private void FillLanguages()
    {
        _fillingLanguages = true;
        _languageOnEntry = _settings.Language;

        _languages.Add(new LanguageChoice(null, SystemChoiceLabel()));
        foreach (LanguageInfo language in Lang.Available)
        {
            _languages.Add(new LanguageChoice(language.Code, language.DisplayName));
        }

        LanguageBox.ItemsSource = _languages;
        LanguageBox.SelectedIndex = _languages.FindIndex(
            c => string.Equals(c.Code ?? string.Empty, _settings.Language ?? string.Empty,
                               StringComparison.OrdinalIgnoreCase));
        if (LanguageBox.SelectedIndex < 0)
        {
            LanguageBox.SelectedIndex = 0;      // a code we no longer ship
        }

        UpdateUnsupportedNote();
        _fillingLanguages = false;
    }

    /// <summary>
    /// "Same as Windows (Deutsch)" — naming what that resolves to, because otherwise the entry
    /// says nothing about which language you are actually about to get.
    /// </summary>
    private static string SystemChoiceLabel()
    {
        LanguageInfo resolved = Lang.Find(Lang.SystemCode());
        return resolved == null
            ? Lang.Tr("Options.Language.System")
            : Lang.Tr("Options.Language.SystemDetail", resolved.NativeName);
    }

    /// <summary>
    /// Says so when Windows is set to a language that is not shipped here, and only then. Silently
    /// falling back to English looks like the setting was ignored.
    /// </summary>
    private void UpdateUnsupportedNote()
    {
        bool following = (LanguageBox.SelectedItem as LanguageChoice)?.Code == null;

        LanguageUnsupported.Visibility = following && !Lang.SystemLanguageIsAvailable
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    /// <summary>
    /// Applies the choice immediately rather than on Save.
    ///
    /// A language picker is the one setting where a preview is not a nicety: someone who has landed
    /// in a language they cannot read needs to see the change to know they have found the right
    /// entry. Cancel puts it back.
    /// </summary>
    private void OnLanguageChosen(object sender, SelectionChangedEventArgs e)
    {
        if (_fillingLanguages || LanguageBox.SelectedItem is not LanguageChoice choice)
        {
            return;
        }
        Lang.Use(choice.Code ?? Lang.SystemCode());
        UpdateUnsupportedNote();
    }

    private void Relocalize()
    {
        // Only the "Same as Windows" entry is translated; the others are their own native names,
        // which are the same in every language by definition.
        if (_languages.Count > 0)
        {
            _languages[0].Label = SystemChoiceLabel();
        }
        UpdateLiveSearchHint();
        UpdateScaleText();
        ShowStorageLocations();
    }

    // ---- interface size and window placement ---------------------------------------------------

    /// <summary>
    /// What the scale was when this window opened, so Cancel can undo the live preview — the same
    /// arrangement as the language picker, and for the same reason.
    /// </summary>
    private double _scaleOnEntry;

    /// <summary>
    /// Suppresses the slider handler while the controls are being populated — and, because it
    /// starts true, while the XAML itself is being loaded. Setting the slider's Minimum to 50
    /// coerces its value up from 0, which raises ValueChanged at a point where the label the
    /// handler writes to does not exist yet.
    /// </summary>
    private bool _fillingScale = true;

    private void FillScale()
    {
        _fillingScale = true;
        _scaleOnEntry = UiScale.Current;

        ScaleSlider.Minimum = Math.Round(UiScale.Minimum * 100);
        ScaleSlider.Maximum = Math.Round(UiScale.Maximum * 100);
        // Whether or not the box is ticked, the slider starts at what is on screen now, so
        // unticking it hands over the current size instead of jumping to some remembered one.
        ScaleSlider.Value = Math.Round(UiScale.Current * 100);

        AutoScaleBox.IsChecked = _settings.ScaleFollowsScreen;
        RememberPlacementBox.IsChecked = _settings.RememberPlacement;

        UpdateScaleEnabled();
        UpdateScaleText();
        _fillingScale = false;
    }

    private void OnAutoScaleToggled(object sender, RoutedEventArgs e)
    {
        if (_fillingScale)
        {
            return;
        }

        _fillingScale = true;
        if (AutoScaleBox.IsChecked == true)
        {
            ScaleSlider.Value = Math.Round(UiScale.Automatic * 100);
        }
        _fillingScale = false;

        UiScale.Apply(ScaleSlider.Value / 100);
        UpdateScaleEnabled();
        UpdateScaleText();
    }

    private void OnScaleSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_fillingScale)
        {
            return;
        }
        UiScale.Apply(e.NewValue / 100);
        UpdateScaleText();
    }

    /// <summary>
    /// Greys the slider and its percentage out while the scale is following the screen.
    ///
    /// The opacity is set by hand because <see cref="System.Windows.UIElement.IsEnabled"/> alone
    /// changes nothing anyone can see: WPF-UI draws a disabled Slider exactly as it draws an
    /// enabled one, so the control was live to the eye, dead to the mouse, and read as broken. The
    /// number is dimmed with it rather than hidden — it is still saying what the scale is, and it
    /// still moves when the automatic value does.
    ///
    /// Set on the row rather than through a Style, deliberately. An implicit Style for Slider here
    /// would replace WPF-UI's, which is the trap documented in <see cref="PopupChrome"/>.
    /// </summary>
    private void UpdateScaleEnabled()
    {
        bool locked = AutoScaleBox.IsChecked != true;
        ScaleRow.IsEnabled = locked;
        ScaleRow.Opacity = locked ? 1.0 : 0.4;
    }

    private void UpdateScaleText()
    {
        // The slider itself is disabled under "scale to fit", but it still tracks the value, so the
        // number beside it is right either way and moves on its own when the box is ticked.
        ScaleValueText.Text = Lang.Tr("Options.Size.Percent", Math.Round(ScaleSlider.Value));
        AutoScaleHint.Text = Lang.Tr("Options.Size.Auto.Detail",
            Math.Round(UiScale.Automatic * 100));
    }

    /// <summary>
    /// Puts the window behind this one back where a first run would have opened it, straight away
    /// and whether or not this dialog is saved — it is a button that does a thing, like Remove
    /// beside the scheduled task, not a setting waiting on Save.
    /// </summary>
    private void OnResetWindow(object sender, RoutedEventArgs e)
    {
        WindowLayout.Forget(_settings);
        WindowLayout.ApplyDefault(Owner);
    }

    /// <summary>
    /// Undoes the size preview when the dialog was dismissed rather than saved. See
    /// <see cref="RevertLanguageUnlessSaved"/> — this hangs off Closed for the same reason, so that
    /// the title bar's close button is not the one way out of here that changes something.
    /// </summary>
    private void RevertScaleUnlessSaved()
    {
        if (!Saved)
        {
            UiScale.Apply(_scaleOnEntry);
        }
    }

    // ---- what the app leaves on the machine -------------------------------------------------

    /// <summary>
    /// Beside the app's own log, under per-user data — the engine resolves a bare file name
    /// there too. Nothing Spotter writes ever lands next to the executable.
    /// </summary>
    private static string EngineLogPath =>
        System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(Log.FilePath) ?? string.Empty,
            "Spotter.Engine.log");

    private void ShowStorageLocations()
    {
        SettingsPathBox.Text = UiSettings.FilePath;
        AppLogPathBox.Text = Log.FilePath;
        EngineLogPathBox.Text = EngineLogPath + Lang.Tr("Options.Storage.EngineLog.Suffix");
        ExePathBox.Text = Environment.ProcessPath;

        System.Reflection.AssemblyName app =
            System.Reflection.Assembly.GetEntryAssembly()?.GetName();
        AboutVersionText.Text = app == null
            ? "Spotter"
            : Lang.Tr("Options.About.Version", app.Version);

        ShowTaskState();
    }

    private void ShowTaskState()
    {
        bool registered = ElevationTask.Exists();
        TaskPathBox.Text = registered
            ? Lang.Tr("Options.Storage.Task.Registered", ElevationTask.Name)
            : Lang.Tr("Options.Storage.Task.Missing");
        RemoveTaskButton.IsEnabled = registered;
    }

    /// <summary>
    /// Opens Explorer with the file selected, or the folder itself when the file has not been
    /// written yet — pointing at a path that does not exist would just look broken.
    /// </summary>
    private void OnOpenStorage(object sender, RoutedEventArgs e)
    {
        // Parenthesised: without them this reads as `Tag as (string switch ...)`, which the
        // compiler warns about because it is not what anyone means by it.
        string path = ((sender as FrameworkElement)?.Tag as string) switch
        {
            "settings" => UiSettings.FilePath,
            "applog" => Log.FilePath,
            "enginelog" => EngineLogPath,
            _ => null,
        };
        if (path == null)
        {
            return;
        }

        try
        {
            if (System.IO.File.Exists(path))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(
                    "explorer.exe", $"/select,\"{path}\"") { UseShellExecute = true });
                return;
            }
            string folder = System.IO.Path.GetDirectoryName(path);
            if (System.IO.Directory.Exists(folder))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(folder)
                {
                    UseShellExecute = true,
                });
            }
        }
        catch (Exception ex)
        {
            Dialogs.Info(this, Lang.Tr("Options.Storage.ExplorerFailed", ex.Message));
        }
    }

    private void OnRemoveElevationTask(object sender, RoutedEventArgs e)
    {
        if (!Dialogs.Confirm(this, Lang.Tr("Options.Storage.Task.Remove.Confirm", ElevationTask.Name)))
        {
            return;
        }

        ElevationTask.Remove();
        ShowTaskState();

        if (ElevationTask.Exists())
        {
            Dialogs.Info(this, Lang.Tr("Options.Storage.Task.Remove.Failed"),
                System.Windows.MessageBoxImage.Warning);
        }
    }

    /// <summary>Set when Save was pressed, so the caller knows to re-apply.</summary>
    public bool Saved { get; private set; }

    private void OnHotkeyKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;

        Key pressed = e.Key == Key.System ? e.SystemKey : e.Key;

        // Ignore the modifiers themselves - they are the prefix, never the key.
        if (pressed is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt
            or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin)
        {
            return;
        }

        ModifierKeys modifiers = Keyboard.Modifiers;
        if (modifiers == ModifierKeys.None)
        {
            // A bare key would be claimed system-wide: press "F" anywhere and the window jumps.
            HotkeyHint.Text = Lang.Tr("Options.Hotkey.NeedsModifier");
            HotkeyHint.Visibility = Visibility.Visible;
            return;
        }

        _modifiers = modifiers;
        _key = pressed;
        HotkeyBox.Text = Describe(_modifiers, _key);
        HotkeyHint.Visibility = Visibility.Collapsed;
    }

    private void OnClearHotkey(object sender, RoutedEventArgs e)
    {
        _modifiers = ModifierKeys.None;
        _key = Key.None;
        HotkeyBox.Text = string.Empty;
        HotkeyHint.Visibility = Visibility.Collapsed;
    }

    private void OnLiveSearchToggled(object sender, RoutedEventArgs e) => UpdateLiveSearchHint();

    /// <summary>
    /// Spells out what each state actually does. "Search as you type" is clear enough when it is
    /// on; when it is off the consequence — a button appears and nothing runs until it is pressed
    /// — is not something a checkbox label can carry.
    /// </summary>
    private void UpdateLiveSearchHint() =>
        LiveSearchHint.Text = Lang.Tr(LiveSearchBox.IsChecked == true
            ? "Options.SearchAsYouType.On"
            : "Options.SearchAsYouType.Off");

    private void OnTrayToggled(object sender, RoutedEventArgs e) => UpdateTrayEnabled();

    private void UpdateTrayEnabled()
    {
        // Minimize/close-to-tray are meaningless without an icon to restore from.
        bool on = ShowTrayBox.IsChecked == true;
        MinimizeTrayBox.IsEnabled = on;
        CloseTrayBox.IsEnabled = on;
    }

    private static string Describe(ModifierKeys modifiers, Key key)
    {
        if (key == Key.None)
        {
            return string.Empty;
        }
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        if (modifiers.HasFlag(ModifierKeys.Control)) sb.Append("Ctrl+");
        if (modifiers.HasFlag(ModifierKeys.Alt)) sb.Append("Alt+");
        if (modifiers.HasFlag(ModifierKeys.Shift)) sb.Append("Shift+");
        if (modifiers.HasFlag(ModifierKeys.Windows)) sb.Append("Win+");
        sb.Append(key);
        return sb.ToString();
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        // Blank for "follow Windows", which is why the setting stores a null code rather than the
        // language that resolved to today.
        _settings.Language = (LanguageBox.SelectedItem as LanguageChoice)?.Code ?? string.Empty;

        _settings.SearchAsYouType = LiveSearchBox.IsChecked == true;
        _settings.RestoreHotkey = Describe(_modifiers, _key);
        _settings.ShowTrayIcon = ShowTrayBox.IsChecked == true;
        _settings.MinimizeToTray = MinimizeTrayBox.IsChecked == true;
        _settings.CloseToTray = CloseTrayBox.IsChecked == true;
        _settings.Theme = (ThemeBox.SelectedIndex switch
        {
            1 => "Dark",
            2 => "Light",
            _ => "Auto",
        });

        _settings.ScaleFollowsScreen = AutoScaleBox.IsChecked == true;
        _settings.Scale = UiScale.Clamp(ScaleSlider.Value / 100);
        _settings.RememberPlacement = RememberPlacementBox.IsChecked == true;

        _settings.Save();
        Saved = true;
        Close();
    }

    private void OnCancel(object sender, RoutedEventArgs e) => Close();

    /// <summary>
    /// Undoes the language preview when the dialog was dismissed rather than saved.
    ///
    /// Everything else here is only written to <see cref="_settings"/> by Save, but the language
    /// was applied to the running app the moment it was picked. Without this, Cancel — and the
    /// title bar's close button, which is why this hangs off Closed rather than off the Cancel
    /// handler — would be the one way out of this window that changed something.
    /// </summary>
    private void RevertLanguageUnlessSaved()
    {
        if (Saved)
        {
            return;
        }
        Lang.Use(string.IsNullOrWhiteSpace(_languageOnEntry)
            ? Lang.SystemCode()
            : _languageOnEntry);
    }
}
