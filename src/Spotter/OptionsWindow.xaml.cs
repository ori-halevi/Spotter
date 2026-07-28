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

        ShowStorageLocations();

        // Text this window writes from code, re-rendered when the picker changes language. The
        // XAML looks after itself, because {loc:Loc} is a binding.
        Lang.Changed += Relocalize;
        Closed += (_, __) =>
        {
            Lang.Changed -= Relocalize;
            RevertLanguageUnlessSaved();
        };
    }

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
        ShowStorageLocations();
    }

    // ---- what the app leaves on the machine -------------------------------------------------

    private static string EngineLogPath =>
        System.IO.Path.Combine(AppContext.BaseDirectory, "SpotterLog.txt");

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
