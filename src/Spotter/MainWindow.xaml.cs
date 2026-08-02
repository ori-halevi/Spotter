using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Spotter.Localization;
using Wpf.Ui.Controls;

namespace Spotter;

public partial class MainWindow
{
    private readonly ResultsView _results = new ResultsView();
    private readonly SearchEngine _engine;
    private readonly DispatcherTimer _typingDebounce;
    private int _benchRemaining;

    public MainWindow()
    {
        InitializeComponent();

        _fieldTips[FilenameBox] = FilenameBox.ToolTip;
        _fieldTips[DirectoryBox] = DirectoryBox.ToolTip;
        _fieldTips[SizeBox] = SizeBox.ToolTip;

        _engine = new SearchEngine(_results);
        _engine.StatusChanged += OnStatusChanged;

        ResultsList.ItemsSource = _results;
        ResultsList.SelectionChanged += (_, args) => UpdateSelectionText(args);
        ResultsList.PreviewKeyDown += OnResultsKeyDown;

        // The list's own ScrollViewer is built by its template, so its viewport is reported rather
        // than measured: this is the only thing that knows how much width the columns have to
        // share, and it is raised whenever that changes — including when the vertical scrollbar
        // appears and takes some of it away.
        ResultsList.AddHandler(System.Windows.Controls.ScrollViewer.ScrollChangedEvent,
            new System.Windows.Controls.ScrollChangedEventHandler(OnResultsScrollChanged));

        // Searching on every keystroke would restart the engine mid-flight while someone is
        // still typing a word. A short idle gap is enough to feel live without the churn.
        _typingDebounce = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(180) };
        _typingDebounce.Tick += (_, __) =>
        {
            _typingDebounce.Stop();
            RunSearch();
        };

        RestoreSettings();

        // Text set from code cannot come through a Loc binding, so it is re-rendered here instead.
        // Without this, switching language updated every label in the XAML and left the status
        // line, the column headers and the options summary in the language before it.
        Lang.Changed += Relocalize;

        Loaded += OnLoaded;
        Loaded += (_, __) => SetUpTrayAndHotkey();
        Loaded += (_, __) => ListenForShowRequests();
        StateChanged += OnWindowStateChanged;
        Closing += OnWindowClosing;
        // Logoff and shutdown end the process whatever the tray settings say, but they arrive at
        // OnWindowClosing looking like an ordinary close — which close-to-tray cancels, skipping
        // the save. The cancel is ignored (WPF is shutting down regardless), so everything the
        // session changed was silently lost. Save here, while there is still time to; the flag
        // keeps the close that follows from detouring into the tray.
        Application.Current.SessionEnding += (_, __) =>
        {
            _reallyExiting = true;
            SaveSettings();
        };
        // Each independently: the engine's stop signal is the one that matters most on the way
        // out, and it should not be skipped because unregistering a hotkey threw.
        Closed += (_, __) =>
        {
            Lang.Changed -= Relocalize;
            // Both are stopped by hand: a running DispatcherTimer is held by the dispatcher's own
            // timer list, so it keeps ticking - and keeps this window reachable - after the window
            // has gone. _indexPoll is otherwise only stopped by IndexingFinished, which never
            // arrives if the app is closed while the drives are still being read.
            try { _typingDebounce.Stop(); } catch { }
            try { StopIndexProgress(); } catch { }
            try { _showWatch?.Unregister(null); } catch { }
            try { _hotkey?.Dispose(); } catch { }
            try { _tray?.Dispose(); } catch { }
            try { _engine.Dispose(); } catch { }
        };
    }

    private System.Threading.RegisteredWaitHandle _showWatch;

    /// <summary>
    /// Listens for a second launch asking this instance to come to the front.
    ///
    /// The launcher cannot just call SetForegroundWindow on our MainWindowHandle, because an
    /// instance sitting in the tray does not have one — and that is precisely the case where
    /// someone re-launches the app.
    /// </summary>
    private void ListenForShowRequests()
    {
        System.Threading.EventWaitHandle signal = App.ShowSignal;
        if (signal == null)
        {
            return;
        }
        _showWatch = System.Threading.ThreadPool.RegisterWaitForSingleObject(
            signal,
            (_, __) => Dispatcher.BeginInvoke(new Action(RestoreFromTray)),
            null,
            System.Threading.Timeout.Infinite,
            executeOnlyOnce: false);
    }

    // ---- tray icon and global hotkey --------------------------------------------------------

    private TrayIcon _tray;
    private Hotkey _hotkey;
    private bool _reallyExiting;
    private bool _toldAboutTray;

    private void SetUpTrayAndHotkey()
    {
        if (_settings.ShowTrayIcon)
        {
            _tray = new TrayIcon { Visible = true };
            _tray.ShowRequested += RestoreFromTray;
            _tray.ExitRequested += () =>
            {
                _reallyExiting = true;
                Close();
            };
        }

        if (Hotkey.TryParse(_settings.RestoreHotkey, out ModifierKeys modifiers, out Key key))
        {
            _hotkey = new Hotkey();
            _hotkey.Pressed += ToggleFromHotkey;
            if (_hotkey.Register(this, modifiers, key))
            {
                Log.Write($"global hotkey registered: {_settings.RestoreHotkey}");
            }
            else
            {
                // Silent failure here is the worst outcome: the user presses the key forever and
                // nothing happens. Say so where they will see it.
                StatusText.Text = Lang.Tr("Status.HotkeyTaken", _settings.RestoreHotkey);
            }
        }
    }

    /// <summary>
    /// Pressing the hotkey while the window is already in front hides it again, so the same
    /// keystroke both summons and dismisses.
    /// </summary>
    private void ToggleFromHotkey()
    {
        if (IsVisible && WindowState != WindowState.Minimized && IsActive)
        {
            HideToTray();
        }
        else
        {
            RestoreFromTray();
        }
    }

    private void RestoreFromTray()
    {
        Show();
        // _wasMaximized, not _settings.Maximized: the setting is only written when the window
        // closes, so during a session it still describes the *previous* run. Maximize, minimize
        // to the tray, restore — and the window came back merely Normal.
        WindowState = _wasMaximized ? WindowState.Maximized : WindowState.Normal;
        Activate();
        Topmost = true;      // nudge past the foreground lock, then let go immediately
        Topmost = false;
        FilenameBox.Focus();
        FilenameBox.SelectAll();
    }

    private void HideToTray()
    {
        if (_tray == null)
        {
            WindowState = WindowState.Minimized;
            return;
        }
        Hide();
        if (!_toldAboutTray)
        {
            _toldAboutTray = true;
            _tray.NotifyHidden(_settings.RestoreHotkey);
        }
    }

    private void OnOptions(object sender, RoutedEventArgs e)
    {
        OptionsWindow dialog = new OptionsWindow(_settings) { Owner = this };
        dialog.ShowDialog();
        if (dialog.Saved)
        {
            ApplySettingsChange();
        }
    }

    /// <summary>
    /// Re-applies whatever the options dialog changed, without a restart. Rebuilding the tray
    /// icon and hotkey from scratch is simpler and more predictable than trying to mutate them
    /// in place, and both are cheap.
    /// </summary>
    private void ApplySettingsChange()
    {
        // The options window previews the scale live, so this is usually a no-op — but it is what
        // makes Save the thing that commits it, rather than the preview having quietly done so.
        UiScale.Apply(UiScale.Resolve(_settings));

        Wpf.Ui.Appearance.ApplicationThemeManager.Apply(_settings.Theme switch
        {
            "Dark" => Wpf.Ui.Appearance.ApplicationTheme.Dark,
            "Light" => Wpf.Ui.Appearance.ApplicationTheme.Light,
            _ => App.SystemUsesDarkTheme()
                ? Wpf.Ui.Appearance.ApplicationTheme.Dark
                : Wpf.Ui.Appearance.ApplicationTheme.Light,
        });

        _hotkey?.Dispose();
        _hotkey = null;
        _tray?.Dispose();
        _tray = null;
        _toldAboutTray = false;

        SetUpTrayAndHotkey();
        ApplySearchMode();
    }

    /// <summary>
    /// Whether the window was maximized before it was minimized or hidden, so restoring puts it
    /// back the way the user left it rather than the way the last session ended.
    /// </summary>
    private bool _wasMaximized;

    private void OnWindowStateChanged(object sender, EventArgs e)
    {
        if (WindowState != WindowState.Minimized)
        {
            _wasMaximized = WindowState == WindowState.Maximized;
        }

        if (WindowState == WindowState.Minimized && _settings.MinimizeToTray && _tray != null)
        {
            HideToTray();
        }
    }

    private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!_reallyExiting && _settings.CloseToTray && _tray != null)
        {
            e.Cancel = true;
            HideToTray();
            return;
        }
        SaveSettings();
    }

    /// <summary>
    /// The process-wide settings, loaded once by <see cref="App"/>. This used to be a second
    /// independent Load(), so App and the window each held their own copy and a change saved
    /// through Options was invisible to whichever one had not made it.
    /// </summary>
    private readonly UiSettings _settings = App.Settings;

    private void RestoreSettings()
    {
        // Size and place the window before anything else can put a number on screen. Both come
        // from WindowLayout, which either restores what the user left or works out the top-left
        // quarter of the screen they are on — see the comment on that class for why a search
        // window never opens filling the display.
        WindowLayout.Scale(this, RootLayout, WindowLayout.MinimumContent);
        WindowLayout.Restore(this, _settings);
        _wasMaximized = WindowState == WindowState.Maximized;   // StateChanged skips the initial state

        _nameMatchCase = _settings.MatchCase;
        _nameRegex = _settings.Regex;
        _folderMatchCase = _settings.FolderMatchCase;
        _folderRegex = _settings.FolderRegex;
        _subfolders = _settings.Subfolders;
        DescribeActiveOptions();

        FilesToggle.IsChecked = _settings.IncludeFiles;
        FoldersToggle.IsChecked = _settings.IncludeFolders;

        _sortColumn = _settings.SortColumn;
        _sortDescending = _settings.SortDescending;

        ApplySearchMode();

        // Stored widths are only in charge once the user has taken the columns over by dragging
        // one; until then they share out whatever width the list has, which is what keeps all
        // seven of them on screen at any window size.
        if (!_settings.AutoFitColumns)
        {
            System.Windows.Controls.GridView view =
                (System.Windows.Controls.GridView)ResultsList.View;
            for (int i = 0; i < Math.Min(_settings.ColumnWidths.Count, view.Columns.Count); i++)
            {
                double w = _settings.ColumnWidths[i];
                if (w > 20 && !double.IsNaN(w))
                {
                    view.Columns[i].Width = w;
                }
            }
        }
        UpdateHeaderArrows();
    }

    private void SaveSettings()
    {
        CommitHistory();

        // While the window is hidden in the tray its WindowState no longer describes how the user
        // left it, so fall back to the tracked value.
        WindowLayout.Capture(this, _settings,
            IsVisible && WindowState != WindowState.Minimized
                ? WindowState == WindowState.Maximized
                : _wasMaximized);

        _settings.MatchCase = _nameMatchCase;
        _settings.Regex = _nameRegex;
        _settings.FolderMatchCase = _folderMatchCase;
        _settings.FolderRegex = _folderRegex;
        _settings.Subfolders = _subfolders;
        _settings.IncludeFiles = FilesToggle.IsChecked == true;
        _settings.IncludeFolders = FoldersToggle.IsChecked == true;

        _settings.SortColumn = _sortColumn;
        _settings.SortDescending = _sortDescending;

        _settings.ColumnWidths.Clear();
        foreach (System.Windows.Controls.GridViewColumn c in
                 ((System.Windows.Controls.GridView)ResultsList.View).Columns)
        {
            _settings.ColumnWidths.Add(c.ActualWidth);
        }

        _settings.Save();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Belt and braces for the foreground rules: the launcher hands over its rights, and the
        // new window still asks explicitly. Topmost is toggled rather than left on, which is the
        // standard way to jump the queue without actually becoming an always-on-top window.
        Activate();
        Topmost = true;
        Topmost = false;

        FilenameBox.Focus();

        if (TryStartDemoMode())
        {
            return;
        }

        if (!IsElevated())
        {
            _needsAdmin = true;
            _readyLine = Lang.Tr("Status.NeedsAdmin");
            RefreshStatusLine();
            BusyRing.Visibility = Visibility.Collapsed;
            ElevateButton.Visibility = Visibility.Visible;
            return;
        }

        Log.Write("elevated, indexing started");
        try
        {
            // Searching is live from this moment: a search already running keeps picking up
            // drives as they come online. Measured — a query issued half a second into a 54 s
            // index returned 855 results, the same count as the searches run after it finished.
            //
            // So a drive landing only updates the count on screen. Re-running the query here as
            // well, which is what this used to do, would clear the grid and start again once per
            // group, replacing a steady stream of results with a series of resets.
            _ready = true;
            _indexing = true;
            StatusText.Text = Lang.Tr("Status.ReadingDrives");

            // The count on screen is provisional while drives are still arriving; the progress
            // bar and its poll say how provisional, so there is nothing to do here but let it
            // run. Kept subscribed because the volume names it refreshes feed that display.
            _engine.VolumesChanged += () => UpdateIndexProgress();

            _engine.IndexingFinished += elapsed =>
            {
                _indexing = false;
                StopIndexProgress();
                BusyRing.Visibility = Visibility.Collapsed;
                Log.Write($"indexed {_engine.Volumes.Length} volume(s) in {elapsed.TotalSeconds:F1}s: "
                          + string.Join(",", _engine.Volumes));
                _indexedVolumes = _engine.Volumes;
                _readyLine = BuildReadyLine();
                RefreshStatusLine();
                _results.Refresh();      // folder totals are final now; draw them one last time
                ResyncSelectionTotal();

                // The one re-run worth paying for: a query typed just before the last drive
                // landed would otherwise be the only one that never saw it.
                RerunIfLive();
            };

            _engine.BeginIndexing();
            StartIndexProgress();

            string startupTerm = ArgumentAfter("--search");
            if (!string.IsNullOrEmpty(startupTerm))
            {
                FilenameBox.Text = startupTerm;
                Log.Write($"startup search: \"{startupTerm}\"");
                // An explicit instruction on the command line, so it runs whatever the mode is.
                _hasManualSearch = true;
            }
            if (int.TryParse(ArgumentAfter("--bench"), out int repeats))
            {
                _benchRemaining = Math.Max(0, repeats);
            }
            RerunIfLive();
        }
        catch (Exception ex)
        {
            BusyRing.Visibility = Visibility.Collapsed;
            StatusText.Text = Lang.Tr("Status.IndexFailed", ex.Message);
            Log.Write("indexing FAILED: " + ex);
        }
    }

    private bool _indexing;

    // ---- what the idle status line is made of ------------------------------------------------
    //
    // Kept as data rather than as the finished sentence, so switching language can rebuild it.
    // Holding only the rendered string meant the line that is on screen most of the time — "Ready
    // — 8 drives (C: D: …)" — stayed in the previous language until the next search.

    private bool _needsAdmin;
    private string[] _indexedVolumes;
    private long _demoRows;

    private string BuildReadyLine()
    {
        if (_demoRows > 0)
        {
            // "synthetic" belongs inside the counted phrase, not in the sentence around it: as an
            // adjective it has to agree with the noun, and it goes before the noun in English and
            // after it in Hebrew and Spanish. Keeping it outside produced "400 rows synthetic rows".
            return Lang.Tr("Status.Demo", Lang.Count("Plural.DemoRows", _demoRows));
        }
        if (_needsAdmin)
        {
            return Lang.Tr("Status.NeedsAdmin");
        }
        return _indexedVolumes == null
            ? null
            : Lang.Tr("Status.Ready",
                Lang.Count("Plural.Drives", _indexedVolumes.Length),
                string.Join(" ", _indexedVolumes));
    }

    /// <summary>
    /// Re-renders everything this window writes from code. Called when the language changes; the
    /// XAML looks after itself, because <c>{loc:Loc}</c> is a binding.
    /// </summary>
    private void Relocalize()
    {
        _readyLine = BuildReadyLine();
        _searchButtonLabel = null;      // force the Content assignment through
        UpdateSearchButton();
        DescribeActiveOptions();
        UpdateHeaderArrows();           // the headers carry translated names plus a sort arrow
        UpdateSelectionText(null);
        if (_lastStatus != null)
        {
            ShowCounts(_lastStatus.Value);
        }
        if (_indexing)
        {
            UpdateIndexProgress();
        }
        RefreshStatusLine();
    }

    // ---- indexing progress -----------------------------------------------------------------
    //
    // Reading eight drives takes the best part of a minute. A spinner over that says only "still
    // working", which is the one thing the user can already see; what they want to know is
    // whether to wait. The engine can answer that — it tracks bytes read against bytes total per
    // volume — so this polls it and shows a real percentage, rate and estimate.

    private DispatcherTimer _indexPoll;
    private readonly Stopwatch _indexClock = Stopwatch.StartNew();
    private readonly Queue<KeyValuePair<TimeSpan, long>> _indexSamples =
        new Queue<KeyValuePair<TimeSpan, long>>();

    /// <summary>The progress half of the status line, or null when not indexing.</summary>
    private string _indexStatus;

    /// <summary>The result-count half of the status line, or null before the first search.</summary>
    private string _lastCounts;

    private void StartIndexProgress()
    {
        _indexStatus = Lang.Tr("Index.Reading");
        IndexBar.Visibility = Visibility.Visible;
        IndexBar.IsIndeterminate = true;

        _indexPoll = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        _indexPoll.Tick += (_, __) => UpdateIndexProgress();
        _indexPoll.Start();
    }

    private void StopIndexProgress()
    {
        _indexPoll?.Stop();
        _indexPoll = null;
        _indexStatus = null;
        IndexBar.Visibility = Visibility.Collapsed;
    }

    private void UpdateIndexProgress()
    {
        IndexProgress progress = _engine.GetIndexProgress();
        if (progress.VolumesTotal == 0)
        {
            return;                 // the volume list is not up yet
        }

        string counted = Lang.Tr("Index.DrivesDone", progress.VolumesDone, progress.VolumesTotal);

        if (!progress.IsMeasurable)
        {
            // Sizes are not known yet, or every volume reports an unknown length. Counting
            // drives is still better than counting nothing.
            IndexBar.IsIndeterminate = true;
            _indexStatus = Lang.Tr("Index.ReadingCount", counted);
        }
        else
        {
            IndexBar.IsIndeterminate = false;
            IndexBar.Value = progress.Fraction;
            _indexStatus = Lang.Tr("Index.ReadingPercent",
                progress.Fraction.ToString("P0", CultureInfo.CurrentCulture),
                counted, RateAndEstimate(progress));
        }

        // A real ToolTip rather than a bare string, so it can opt out of following the language: the
        // content is a column of volume names, percentages and byte counts, which a right-to-left
        // paragraph direction would reorder into nonsense.
        System.Windows.Controls.ToolTip volumes = new System.Windows.Controls.ToolTip
        {
            Content = DescribeVolumes(progress),
            FlowDirection = FlowDirection.LeftToRight,
        };
        PopupChrome.SetFollow(volumes, false);
        IndexBar.ToolTip = volumes;
        RefreshStatusLine();
        RedrawGrowingFolders();
    }

    private int _folderRedrawTicks;

    /// <summary>
    /// A folder's size and contents are totals the engine builds up as it reads, so rows showing
    /// folders are out of date the moment they are drawn. Redrawing them lets those numbers
    /// climb while indexing runs — which is the whole point of showing them.
    ///
    /// Every eighth poll, so roughly every two seconds: each redraw is a Reset, and a Reset makes
    /// WPF rebuild every realized row. Often enough to watch, rare enough not to fight scrolling.
    /// </summary>
    private void RedrawGrowingFolders()
    {
        if (++_folderRedrawTicks < 8)
        {
            return;
        }
        _folderRedrawTicks = 0;
        if (_results.Count > 0)
        {
            _results.Refresh();
            ResyncSelectionTotal();
        }
    }

    /// <summary>
    /// Rate and time remaining, from a trailing three-second window rather than the average
    /// since the start — drives differ enough in speed that a running average is a poor guide to
    /// what is happening now.
    /// </summary>
    private string RateAndEstimate(IndexProgress progress)
    {
        TimeSpan now = _indexClock.Elapsed;
        _indexSamples.Enqueue(new KeyValuePair<TimeSpan, long>(now, progress.BytesRead));
        while (_indexSamples.Count > 2
               && now - _indexSamples.Peek().Key > TimeSpan.FromSeconds(3))
        {
            _indexSamples.Dequeue();
        }
        if (_indexSamples.Count < 2)
        {
            return string.Empty;
        }

        KeyValuePair<TimeSpan, long> oldest = _indexSamples.Peek();
        double seconds = (now - oldest.Key).TotalSeconds;
        long delta = progress.BytesRead - oldest.Value;
        if (seconds < 0.5 || delta <= 0)
        {
            return string.Empty;
        }

        double perSecond = delta / seconds;
        string rate = Lang.Tr("Index.Rate", Lang.FormatBytes((long)perSecond));

        long remaining = progress.BytesTotal - progress.BytesRead;
        return remaining <= 0
            ? rate
            : rate + Lang.Tr("Index.Remaining",
                FormatDuration(TimeSpan.FromSeconds(remaining / perSecond)));
    }

    private static string DescribeVolumes(IndexProgress progress)
    {
        StringBuilder sb = new StringBuilder();
        foreach (VolumeProgress volume in progress.Volumes)
        {
            if (sb.Length > 0)
            {
                sb.AppendLine();
            }
            sb.Append(volume.Name).Append("   ");
            if (volume.State != null)
            {
                // A key, not a sentence — the engine wrapper reports the state and this decides
                // what to call it, so the tooltip follows the language like everything else.
                sb.Append(Lang.Tr(volume.State));
            }
            else if (volume.BytesTotal > 0)
            {
                sb.Append(Lang.Tr("Index.Volume.Progress",
                    volume.Fraction.ToString("P0", CultureInfo.CurrentCulture),
                    Lang.FormatBytes(volume.BytesRead),
                    Lang.FormatBytes(volume.BytesTotal)));
            }
            else
            {
                sb.Append(Lang.Tr("Index.Volume.Waiting"));
            }
        }
        return sb.ToString();
    }

    private static string FormatDuration(TimeSpan span)
    {
        if (span.TotalSeconds < 60)
        {
            return Lang.Tr("Duration.Seconds", Math.Max(1, (int)span.TotalSeconds));
        }
        return span.TotalMinutes < 60
            ? Lang.Tr("Duration.MinutesSeconds", (int)span.TotalMinutes, span.Seconds)
            : Lang.Tr("Duration.HoursMinutes", (int)span.TotalHours, span.Minutes);
    }

    /// <summary>
    /// The status line is written by two things that run independently — searches finishing and
    /// drives being read — so both write their half here rather than overwriting each other.
    /// </summary>
    private void RefreshStatusLine()
    {
        // A field the engine cannot parse outranks everything else: until it is fixed, the
        // result count on screen answers a different question from the one being asked.
        if (_problemText != null)
        {
            StatusText.Text = _problemText;
            return;
        }

        string counts = _lastCounts;
        string index = _indexing ? _indexStatus : null;
        string line = counts == null ? index
            : index == null ? counts
            : $"{counts}  ·  {index}";

        // Neither half has anything to say — an empty query with the drives already read — so
        // fall back to what the app is ready to do. Never to whatever happens to be on screen:
        // falling back to the existing text left a corrected error message sitting there after
        // the field that caused it had been fixed.
        StatusText.Text = line ?? _readyLine ?? string.Empty;
    }

    /// <summary>"Ready — N drives (…)", shown whenever there is nothing more specific to say.</summary>
    private string _readyLine;

    private static string ArgumentAfter(string flag)
    {
        string[] args = Environment.GetCommandLineArgs();
        int i = Array.FindIndex(args, a => a.Equals(flag, StringComparison.OrdinalIgnoreCase));
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }

    /// <summary>
    /// Development scaffolding: <c>--demo [rows]</c> fills the grid with synthetic records so the
    /// list, icons, sorting and selection can be exercised without administrator rights.
    ///
    /// It fabricates <see cref="GClass54"/> records with no owning volume, so the Path column
    /// stays blank — path resolution walks the live index, and there isn't one. Everything else
    /// is the real rendering path.
    /// </summary>
    private bool TryStartDemoMode()
    {
        string[] args = Environment.GetCommandLineArgs();
        int flag = Array.FindIndex(args, a => a.Equals("--demo", StringComparison.OrdinalIgnoreCase));
        if (flag < 0)
        {
            return false;
        }

        int rows = 250000;
        if (flag + 1 < args.Length && int.TryParse(args[flag + 1], out int requested))
        {
            rows = Math.Max(1, requested);
        }

        string[] extensions = { ".dll", ".exe", ".txt", ".png", ".cs", ".json", ".zip", ".pdf", string.Empty };
        Random random = new Random(1234);          // fixed seed: same grid every run
        List<GClass54> batch = new List<GClass54>(rows);
        DateTime start = new DateTime(2020, 1, 1);

        for (int i = 0; i < rows; i++)
        {
            string extension = extensions[random.Next(extensions.Length)];
            bool isDirectory = extension.Length == 0;
            // All three date fields, or the columns reading dateTime_1/_2 show 1/1/0001 and the
            // demo stops being representative of what the grid actually renders.
            DateTime created = start.AddMinutes(random.Next(0, 3_000_000));
            // Folders are GClass56, the record type that carries the child counts, and they get
            // a size: the engine totals a folder's contents while indexing. Building them as
            // plain GClass54 with no size made the demo misrepresent what the grid renders.
            GClass54 record = isDirectory
                ? new GClass56 { int_0 = random.Next(1, 40_000), int_1 = random.Next(0, 900) }
                : new GClass54();

            record.string_0 = $"sample_{i:D6}{extension}";
            record.long_0 = (long)(random.NextDouble() * (isDirectory ? 40_000_000_000 : 800_000_000));
            record.dateTime_0 = created;                                    // Created
            record.dateTime_1 = created.AddMinutes(random.Next(0, 500_000)); // Modified
            record.dateTime_2 = created.AddMinutes(random.Next(0, 900_000)); // Accessed
            record.genum32_0 = isDirectory ? (GEnum32)0x10 : 0;
            batch.Add(record);
        }

        _results.AddRange(batch);
        BusyRing.Visibility = Visibility.Collapsed;
        _demoRows = rows;
        _readyLine = BuildReadyLine();
        RefreshStatusLine();
        return true;
    }

    private void OnQueryChanged(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }
        if (ReferenceEquals(sender, DirectoryBox) && !_suppressFolderSuggest)
        {
            UpdateFolderSuggestions();
        }
        if (!_settings.SearchAsYouType)
        {
            // Nothing runs until Search is pressed, so what is on screen no longer answers what
            // is typed. Say so on the button rather than leaving stale rows looking current.
            _hasManualSearch = false;
            UpdateSearchButton();
            return;
        }
        _typingDebounce.Stop();
        _typingDebounce.Start();
    }

    // ---- search-on-demand ------------------------------------------------------------------
    //
    // The 1.5.1 "Search On-Demand" option. Its tooltip described the checked state as "auto-start
    // new search on each letter typed or setting changed", which is this app's default; turning it
    // off brings back the manual Start/Stop button the old app showed in exactly that case.

    /// <summary>
    /// True once a manual search has been run for the text currently in the boxes. It is what
    /// lets a drive that finishes indexing late re-run the query — but only one the user actually
    /// asked for, never a half-typed one.
    /// </summary>
    private bool _hasManualSearch;

    private bool _searching;

    private void ApplySearchMode()
    {
        SearchButton.Visibility = _settings.SearchAsYouType
            ? Visibility.Collapsed
            : Visibility.Visible;
        _searchButtonLabel = null;      // force a re-apply after a mode change
        UpdateSearchButton();
    }

    private string _searchButtonLabel;

    /// <summary>
    /// Called on every status update — roughly eight times a second while results stream in — so
    /// it has to be free when nothing has changed. Assigning Content and a fresh SymbolIcon each
    /// time instead cost tens of milliseconds per search in re-templating the button.
    /// </summary>
    private void UpdateSearchButton()
    {
        if (_settings.SearchAsYouType)
        {
            return;
        }
        string label = Lang.Tr(_searching ? "Toolbar.Stop"
            : _hasManualSearch ? "Toolbar.SearchAgain"
            : "Toolbar.Search");
        if (label == _searchButtonLabel)
        {
            return;
        }
        _searchButtonLabel = label;
        SearchButton.Content = label;
        SearchButton.Icon = new SymbolIcon(_searching ? SymbolRegular.Stop24 : SymbolRegular.Search24);
    }

    private void OnSearchButton(object sender, RoutedEventArgs e)
    {
        if (_searching)
        {
            _engine.StopSearch();
            return;
        }
        _typingDebounce.Stop();
        CommitHistory();
        _hasManualSearch = true;
        RunSearch();
    }

    /// <summary>
    /// Re-runs the current query when a volume finishes indexing. In manual mode that has to be
    /// conditional: re-running whatever happens to be typed would quietly defeat the whole point
    /// of the mode.
    /// </summary>
    private void RerunIfLive()
    {
        if (_settings.SearchAsYouType || _hasManualSearch)
        {
            RunSearch();
        }
    }

    // ---- search history --------------------------------------------------------------------

    private string _lastCommitted;

    /// <summary>
    /// Records the current term as one the user meant.
    ///
    /// Only explicit signals get here — Enter, leaving the box, opening a result, closing the
    /// window. Recording on every keystroke instead would fill the list with the prefixes of one
    /// word, which is the failure mode that makes a history feature worse than none.
    /// </summary>
    private void CommitHistory()
    {
        string term = FilenameBox.Text?.Trim();
        if (string.IsNullOrEmpty(term)
            || string.Equals(term, _lastCommitted, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        _lastCommitted = term;
        _settings.RememberSearch(term);
    }

    private void OnQueryBoxLostFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        // Not while the popup is taking focus — the term is about to be replaced by a chosen one.
        if (!HistoryPopup.IsOpen)
        {
            CommitHistory();
        }
    }

    private void OnQueryBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down && ReferenceEquals(sender, FilenameBox))
        {
            OpenHistory();
            e.Handled = true;
            return;
        }
        if (e.Key == Key.Down && ReferenceEquals(sender, DirectoryBox) && FolderPopup.IsOpen)
        {
            // Step into the folder list, the same gesture that opens the name history.
            FolderSuggestions.SelectedIndex = 0;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (FolderSuggestions.ItemContainerGenerator.ContainerFromIndex(0)
                    is System.Windows.Controls.ListBoxItem row)
                {
                    row.Focus();
                }
            }), DispatcherPriority.Input);
            e.Handled = true;
            return;
        }
        if (e.Key == Key.Enter)
        {
            // Enter is a commitment either way: it names a term worth remembering, and it skips
            // the rest of the debounce instead of waiting out an interval the user has ended.
            _typingDebounce.Stop();
            CommitHistory();
            _hasManualSearch = true;
            RunSearch();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && sender is Wpf.Ui.Controls.TextBox box)
        {
            // The 1.5.1 rule, which is a good one: Esc clears the field you are in, and Esc
            // again — now that it is empty — resets the whole form. One keystroke never throws
            // away more than the field you were looking at.
            if (box.Text.Length > 0)
            {
                box.Clear();
            }
            else
            {
                OnClearAllFields(sender, e);
            }
            e.Handled = true;
        }
    }

    /// <summary>
    /// F5 re-runs the search, which is what F5 means everywhere and what the menu claims. The
    /// gestures printed in the menus have to be real ones.
    /// </summary>
    private void OnWindowKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            OnSearchAgain(sender, e);
            e.Handled = true;
        }
    }

    private void OnShowHistory(object sender, RoutedEventArgs e) => OpenHistory();

    private void OpenHistory()
    {
        CommitHistory();           // so the term being typed right now is in the list too
        RefreshHistoryList();
        HistoryPopup.IsOpen = true;

        if (_settings.RecentSearches.Count == 0)
        {
            return;
        }
        HistoryList.SelectedIndex = 0;

        // The container does not exist until the popup has laid out, so focusing it has to wait
        // a beat — without this the arrow keys still belong to the text box.
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (HistoryList.ItemContainerGenerator.ContainerFromIndex(0)
                is System.Windows.Controls.ListBoxItem row)
            {
                row.Focus();
            }
        }), DispatcherPriority.Input);
    }

    private void RefreshHistoryList()
    {
        bool any = _settings.RecentSearches.Count > 0;
        HistoryList.ItemsSource = new List<string>(_settings.RecentSearches);
        HistoryList.Visibility = any ? Visibility.Visible : Visibility.Collapsed;
        HistoryEmpty.Visibility = any ? Visibility.Collapsed : Visibility.Visible;
        ClearHistoryButton.IsEnabled = any;
    }

    private void OnHistoryKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            UseHistory(HistoryList.SelectedItem as string);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            HistoryPopup.IsOpen = false;
            FilenameBox.Focus();
            e.Handled = true;
        }
        else if (e.Key == Key.Delete && HistoryList.SelectedItem is string term)
        {
            int position = HistoryList.SelectedIndex;
            _settings.RecentSearches.RemoveAll(
                t => string.Equals(t, term, StringComparison.OrdinalIgnoreCase));
            if (string.Equals(term, _lastCommitted, StringComparison.OrdinalIgnoreCase))
            {
                _lastCommitted = null;    // forgetting it means it can be recorded again
            }
            _settings.Save();
            RefreshHistoryList();
            if (_settings.RecentSearches.Count == 0)
            {
                HistoryPopup.IsOpen = false;
                FilenameBox.Focus();
            }
            else
            {
                HistoryList.SelectedIndex = Math.Min(position, _settings.RecentSearches.Count - 1);
            }
            e.Handled = true;
        }
    }

    private void OnHistoryClicked(object sender, MouseButtonEventArgs e)
    {
        // Resolve the row under the pointer rather than reading SelectedItem: a click on the
        // empty space below the last entry would otherwise re-apply whatever was selected.
        if (e.OriginalSource is DependencyObject source
            && FindAncestor<System.Windows.Controls.ListBoxItem>(source)?.Content is string term)
        {
            UseHistory(term);
            e.Handled = true;
        }
    }

    private void UseHistory(string term)
    {
        if (string.IsNullOrEmpty(term))
        {
            return;
        }
        HistoryPopup.IsOpen = false;

        FilenameBox.Text = term;               // starts the debounce when searching live
        FilenameBox.CaretIndex = term.Length;
        FilenameBox.Focus();

        if (!_settings.SearchAsYouType)
        {
            _hasManualSearch = true;
            RunSearch();
        }
    }

    private void OnClearHistory(object sender, RoutedEventArgs e)
    {
        _settings.RecentSearches.Clear();
        _lastCommitted = null;
        _settings.Save();
        RefreshHistoryList();
        HistoryPopup.IsOpen = false;
        FilenameBox.Focus();
    }

    private bool _ready;

    private void RunSearch()
    {
        SearchQuery query = CurrentQuery();

        // Check before anything else, including before the engine is ready. The engine builds
        // its matchers on a worker thread and swallows what they throw, so an unparseable size
        // or a broken regular expression comes back as "0 results" — identical to a good search
        // that found nothing. Saying which field is wrong, and why, is the difference between
        // the two, and it is just as useful while the drives are still being read.
        QueryProblems problems = query.IsEmpty ? new QueryProblems() : query.Check();
        ShowProblems(problems);
        if (problems.Any)
        {
            _searching = false;
            UpdateSearchButton();
            BusyRing.Visibility = _indexing ? Visibility.Visible : Visibility.Collapsed;
            return;
        }

        // _ready rather than IsIndexed: the engine answers against whatever volumes are already
        // cached, so searching is live from the moment indexing starts. Waiting for IsIndexed
        // would mean a dead window for the best part of a minute.
        if (!_ready)
        {
            return;
        }

        _searching = true;
        UpdateSearchButton();
        _engine.Search(query);
    }

    private SearchQuery CurrentQuery() => new SearchQuery
    {
        Filename = FilenameBox.Text,
        Directory = DirectoryBox.Text,
        Size = SizeBox.Text,
        FilenameIgnoreCase = !_nameMatchCase,
        FilenameRegex = _nameRegex,
        DirectoryIgnoreCase = !_folderMatchCase,
        DirectoryRegex = _folderRegex,
        IncludeSubdirectories = _subfolders,
        IncludeFiles = FilesToggle.IsChecked == true,
        IncludeDirectories = FoldersToggle.IsChecked == true,
    };

    /// <summary>
    /// Marks the offending field and says what is wrong with it, the way the 1.5.1 app coloured
    /// the box — but in the status bar rather than a modal box, because this runs while you type.
    /// </summary>
    private void ShowProblems(QueryProblems problems)
    {
        MarkField(FilenameBox, problems.Filename);
        MarkField(DirectoryBox, problems.Directory);
        MarkField(SizeBox, problems.Size);

        _problemText = problems.Any
            ? (problems.Filename != null ? Lang.Tr("Problem.Name", problems.Filename)
               : problems.Directory != null ? Lang.Tr("Problem.Folder", problems.Directory)
               : problems.Size != null ? Lang.Tr("Problem.Size", problems.Size)
               // A key rather than a sentence, so the engine wrapper does not have to know a
               // language to say that nothing can match.
               : Lang.Tr(problems.Kinds))
            : null;
        RefreshStatusLine();
    }

    private string _problemText;

    private static readonly System.Windows.Media.Brush BadInput =
        new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(0xE8, 0x4C, 0x54));

    /// <summary>
    /// The syntax tooltips are the field's normal ones and have to come back afterwards —
    /// FrameworkElement.ToolTip and ToolTipService.ToolTip are the same property, so clearing it
    /// to remove an error message would take the syntax help with it, permanently.
    /// </summary>
    private readonly Dictionary<Wpf.Ui.Controls.TextBox, object> _fieldTips =
        new Dictionary<Wpf.Ui.Controls.TextBox, object>();

    private void MarkField(Wpf.Ui.Controls.TextBox field, string problem)
    {
        if (problem == null)
        {
            field.ClearValue(System.Windows.Controls.Control.BorderBrushProperty);
            field.ClearValue(System.Windows.Controls.Control.BorderThicknessProperty);
            if (_fieldTips.TryGetValue(field, out object original))
            {
                field.ToolTip = original;
            }
            return;
        }
        field.BorderBrush = BadInput;
        field.BorderThickness = new Thickness(1.5);
        field.ToolTip = problem;
    }

    private void OnStatusChanged(SearchStatus status)
    {
        _searching = !status.Completed;
        UpdateSearchButton();

        if (status.Completed)
        {
            // Engine time and total time separately: without both, a slow search is
            // unattributable.
            double engineMs = _engine.LastEngineElapsed.TotalMilliseconds;
            double totalMs = status.Elapsed.TotalMilliseconds;
            Log.Write($"search complete: {status.Count:N0} results — "
                      + $"engine {engineMs:F0} ms, ui +{Math.Max(0, totalMs - engineMs):F0} ms, "
                      + $"total {totalMs:F0} ms, {_engine.LastBatchCount} batches");

            if (_benchRemaining > 0 && status.Count > 0)
            {
                _benchRemaining--;
                Log.Write($"bench: repeating ({_benchRemaining} left)");
                Dispatcher.BeginInvoke(new Action(RunSearch), DispatcherPriority.Background);
            }
        }
        // Rows stream in in engine order, so anything that lands after a sort sits out of place
        // and the header arrow is briefly a lie. Re-applying once the search settles keeps the
        // chosen sort sticky across searches instead of silently dropping it.
        if (status.Completed && _sortColumn != null && status.Count > 0)
        {
            ApplySort();
        }

        // Keep spinning while drives are still loading, even between searches — the count on
        // screen is provisional until every volume is in.
        BusyRing.Visibility = status.Completed && !_indexing
            ? Visibility.Collapsed
            : Visibility.Visible;

        ShowCounts(status);
        RefreshStatusLine();
    }

    /// <summary>The last search's outcome, kept so the count can be re-rendered in a new language.</summary>
    private SearchStatus? _lastStatus;

    private void ShowCounts(SearchStatus status)
    {
        _lastStatus = status;

        // Nothing asked for is not the same as nothing found: "0 results in 0 ms" against an
        // empty box reads like a failed search, and it is the first thing shown at startup.
        bool askedForSomething = !string.IsNullOrWhiteSpace(FilenameBox.Text)
                                 || !string.IsNullOrWhiteSpace(DirectoryBox.Text)
                                 || !string.IsNullOrWhiteSpace(SizeBox.Text);

        _lastCounts = askedForSomething
            ? Lang.Tr("Status.Found",
                  Lang.Count("Plural.Results", status.Count),
                  status.Elapsed.TotalMilliseconds.ToString("N0", CultureInfo.CurrentCulture))
              + (status.Completed ? string.Empty : Lang.Tr("Status.Searching.Suffix"))
            : null;
    }

    private long _selectedBytes;

    /// <summary>
    /// Tracks the selected byte total incrementally.
    ///
    /// The obvious version — re-summing <c>SelectedItems</c> on every change — is O(n) per
    /// keystroke of Shift+Arrow, and on a virtualized list it also drags every selected row
    /// through the bounded display cache, evicting it continuously. Select-all over a
    /// 300,000-row result set froze the window. The delta from the event is all that is needed.
    /// </summary>
    private void UpdateSelectionText(System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (e != null)
        {
            foreach (object item in e.AddedItems)
            {
                if (item is FileItem f && !f.IsDirectory && f.SizeBytes > 0)
                {
                    _selectedBytes += f.SizeBytes;
                }
            }
            foreach (object item in e.RemovedItems)
            {
                if (item is FileItem f && !f.IsDirectory && f.SizeBytes > 0)
                {
                    _selectedBytes -= f.SizeBytes;
                }
            }
        }

        int n = ResultsList.SelectedItems.Count;
        if (n == 0)
        {
            _selectedBytes = 0;
            SelectionText.Text = string.Empty;
            return;
        }
        SelectionText.Text = Lang.Tr("Status.Selected",
            n.ToString("N0", CultureInfo.CurrentCulture), Lang.FormatBytes(_selectedBytes));
    }

    /// <summary>
    /// Selections small enough to add up cheaply are re-derived rather than tracked.
    ///
    /// The incremental total is only correct while every change arrives as a matched pair of
    /// AddedItems/RemovedItems. A Reset — which this app raises on every sort, every delete and
    /// every two seconds while folder totals are still growing — does not come with either, so
    /// the running figure drifts away from the truth with nothing to pull it back.
    ///
    /// Summing SelectedItems is safe to do here: it holds the FileItem objects directly, so it
    /// does not go through the bounded display cache and cannot evict the rows on screen. It is
    /// merely O(n), hence the ceiling — beyond it the tracked value is kept, because being a few
    /// megabytes out in a status line beats a visible stall.
    /// </summary>
    private const int ExactSelectionSumLimit = 4096;

    private void ResyncSelectionTotal()
    {
        System.Collections.IList selected = ResultsList.SelectedItems;
        if (selected.Count > 0 && selected.Count <= ExactSelectionSumLimit)
        {
            long total = 0;
            foreach (object entry in selected)
            {
                if (entry is FileItem file && !file.IsDirectory && file.SizeBytes > 0)
                {
                    total += file.SizeBytes;
                }
            }
            _selectedBytes = total;
        }
        UpdateSelectionText(null);
    }

    // ---- sorting -------------------------------------------------------------------------

    private string _sortColumn;
    private bool _sortDescending;

    private void OnColumnHeaderClick(object sender, RoutedEventArgs e)
    {
        // The sort key is the column's ColumnKey.Value, never its Header. The header carries a
        // translated name and a sort arrow — "Size ▼", "גודל ▲" — so reading it back as an
        // identifier finds no comparison and silently stops sorting. That failure had already
        // happened once here for the arrow alone, in English; translation would have made every
        // column dead in four languages out of five.
        if (sender is not System.Windows.Controls.GridViewColumnHeader header
            || KeyOf(header.Column) is not string column)
        {
            return;
        }

        // Clicking the same column again flips direction; a new column starts ascending, except
        // size and date which are far more useful biggest/newest first.
        if (_sortColumn == column)
        {
            _sortDescending = !_sortDescending;
        }
        else
        {
            _sortColumn = column;
            _sortDescending = column is "Size" or "Modified" or "Created" or "Accessed";
        }

        ApplySort();
    }

    /// <summary>Sorts by the current column and refreshes the header arrows.</summary>
    private void ApplySort()
    {
        if (_sortColumn == null)
        {
            return;
        }
        Comparison<GClass54> comparison = BuildComparison(_sortColumn);
        if (comparison == null)
        {
            return;
        }
        if (_sortDescending)
        {
            Comparison<GClass54> ascending = comparison;
            comparison = (a, b) => ascending(b, a);
        }

        Mouse.OverrideCursor = Cursors.Wait;
        try
        {
            _results.Sort(comparison);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
        UpdateHeaderArrows();
        ResyncSelectionTotal();      // the Reset above may have moved the selection
    }

    /// <summary>
    /// Writes each column's visible header: its translated name, plus the arrow on the one being
    /// sorted by. Built from the key every time rather than edited in place, so a language change
    /// is just another call to this.
    /// </summary>
    private void UpdateHeaderArrows()
    {
        foreach (System.Windows.Controls.GridViewColumn c in
                 ((System.Windows.Controls.GridView)ResultsList.View).Columns)
        {
            if (KeyOf(c) is not string key)
            {
                continue;
            }
            string name = Lang.Tr("Column." + key);
            // The arrows are geometric symbols with no direction of their own, so they are the
            // same in Hebrew; WPF puts them on the leading side of the text for us.
            c.Header = key == _sortColumn
                ? name + (_sortDescending ? " ▼" : " ▲")
                : name;
        }
    }

    /// <summary>The language-independent identity of a column, or null for the padding header
    /// past the last one.</summary>
    private static string KeyOf(System.Windows.Controls.GridViewColumn column) =>
        column == null ? null : ColumnKey.GetValue(column);

    private static Comparison<GClass54> BuildComparison(string column) => column switch
    {
        // GClass233/GClass75 are the engine's own natural-order comparers. Using them keeps the
        // ordering identical to the 1.5.1 app - plain string.Compare would put "file10" before
        // "file2".
        "Name" => (a, b) => GClass233.smethod_7(a.string_0, b.string_0),
        "Path" => (a, b) => GClass75.smethod_0(SafePath(a), SafePath(b)),
        "Size" => (a, b) => a.long_0.CompareTo(b.long_0),
        "Modified" => (a, b) => a.dateTime_1.CompareTo(b.dateTime_1),
        "Created" => (a, b) => a.dateTime_0.CompareTo(b.dateTime_0),
        "Accessed" => (a, b) => a.dateTime_2.CompareTo(b.dateTime_2),
        "Type" => (a, b) => string.Compare(ExtensionOf(a), ExtensionOf(b),
                                           StringComparison.OrdinalIgnoreCase),
        _ => null,
    };

    private static string ExtensionOf(GClass54 record)
    {
        string name = record.string_0;
        if (string.IsNullOrEmpty(name))
        {
            return string.Empty;
        }
        int dot = name.LastIndexOf('.');
        return dot > 0 && dot < name.Length - 1 ? name.Substring(dot + 1) : string.Empty;
    }

    private static string SafePath(GClass54 record)
    {
        try { return record.method_8() ?? string.Empty; }
        catch { return string.Empty; }
    }

    // ---- commands ------------------------------------------------------------------------

    private IEnumerable<FileItem> Selected => ResultsList.SelectedItems.OfType<FileItem>();

    private void OnResultsKeyDown(object sender, KeyEventArgs e)
    {
        // Alt+Enter has to be tested before plain Enter, or the first branch claims it and
        // Alt+Enter opens the file instead of showing its properties.
        if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Alt)
        {
            OnProperties(sender, e);
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            OnOpen(sender, e);
            e.Handled = true;
        }
        else if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
        {
            OnCopyFullPath(sender, e);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            FilenameBox.Focus();
            FilenameBox.SelectAll();
            e.Handled = true;
        }
        else if (e.Key == Key.Delete)
        {
            Delete(permanent: Keyboard.Modifiers == ModifierKeys.Shift);
            e.Handled = true;
        }
        else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
        {
            OnExportSelected(sender, e);
            e.Handled = true;
        }
    }

    private void OnResultActivated(object sender, MouseButtonEventArgs e) => OnOpen(sender, e);

    private void OnElevate(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(Environment.ProcessPath)
            {
                UseShellExecute = true,
                Verb = "runas",          // triggers the UAC prompt
            });
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            // Most likely the user dismissed the UAC prompt; stay open and unelevated.
            StatusText.Text = Lang.Tr("Status.ElevateFailed", ex.Message);
        }
    }

    private const int BulkLaunchWarnAt = 10;

    /// <summary>
    /// Confirms before opening a lot of things at once. The previous version silently capped at
    /// ten, so selecting fifty files and pressing Enter opened ten of them with no explanation of
    /// where the rest went.
    /// </summary>
    private bool ConfirmBulk(int count, string verbKey)
    {
        if (count < BulkLaunchWarnAt)
        {
            return true;
        }
        return Dialogs.Confirm(this,
            Lang.Tr("Dialog.BulkConfirm", Lang.Tr(verbKey), Lang.Count("Plural.Items", count)));
    }

    private void OnOpen(object sender, RoutedEventArgs e)
    {
        List<FileItem> items = Selected.ToList();
        if (!ConfirmBulk(items.Count, "Dialog.BulkOpen.Verb"))
        {
            return;
        }
        // Opening a result is the clearest evidence there is that the query was the right one.
        CommitHistory();
        foreach (FileItem item in items)
        {
            Launch(item.FullPath);
        }
    }

    private void OnOpenFolder(object sender, RoutedEventArgs e)
    {
        // One Explorer window per distinct folder, not per file - selecting twenty files from the
        // same directory should not open twenty identical windows.
        List<string> folders = Selected
            .Select(i => i.FullPath)
            .Where(p => !string.IsNullOrEmpty(p))
            .GroupBy(System.IO.Path.GetDirectoryName, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        if (!ConfirmBulk(folders.Count, "Dialog.BulkExplorer.Verb"))
        {
            return;
        }
        foreach (string path in folders)
        {
            Launch("explorer.exe", $"/select,\"{path}\"");
        }
    }

    private void OnCopyFullPath(object sender, RoutedEventArgs e) =>
        CopyLines(Selected.Select(i => i.FullPath));

    private void OnCopyName(object sender, RoutedEventArgs e) =>
        CopyLines(Selected.Select(i => i.Name));

    private void OnCopyFolder(object sender, RoutedEventArgs e) =>
        CopyLines(Selected.Select(i => i.Directory));

    private static void CopyLines(IEnumerable<string> lines)
    {
        StringBuilder sb = new StringBuilder();
        foreach (string line in lines)
        {
            sb.AppendLine(line);
        }
        if (sb.Length > 0)
        {
            try { Clipboard.SetText(sb.ToString()); } catch { /* clipboard can be locked */ }
        }
    }

    private void OnProperties(object sender, RoutedEventArgs e)
    {
        FileItem item = Selected.FirstOrDefault();
        if (item != null)
        {
            ShowFileProperties(item.FullPath);
        }
    }

    private void OnExportSelected(object sender, RoutedEventArgs e) => Export(Selected.ToList());

    private void OnExportAll(object sender, RoutedEventArgs e) =>
        // Fresh wrappers over a snapshot of the records, rather than the collection's indexer:
        // the indexer would drag every row through the bounded display cache and evict the ones
        // on screen, so the grid rebuilt itself as soon as the export finished.
        Export(_results.SnapshotRecords().ConvertAll(r => new FileItem(r)));

    private void Export(List<FileItem> items)
    {
        if (items.Count == 0)
        {
            Dialogs.Info(this, Lang.Tr("Dialog.NothingToExport"));
            return;
        }

        Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = Lang.Tr("Dialog.Export.Filter"),
            FileName = Lang.Tr("Dialog.Export.FileName"),
        };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        string target = dialog.FileName;
        bool plainText = target.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);

        // Resolve every string HERE, on the UI thread, before handing the work off. FileItem walks
        // the live engine index to build a path, and the index is not safe to walk from an
        // arbitrary thread. Formatting is the cheap part; writing several hundred thousand lines
        // to disk is the part that used to freeze the window, and that is what moves.
        List<string> lines = new List<string>(items.Count + 1);
        if (plainText)
        {
            // A .txt export is just the paths - that is what it gets pasted into.
            foreach (FileItem item in items)
            {
                lines.Add(item.FullPath);
            }
        }
        else
        {
            // Translated, because the header row is read by whoever opens the file rather than by
            // this program. The row *values* stay invariant — a size is parsed, not read.
            lines.Add(Lang.Tr("Dialog.Export.CsvHeader"));
            foreach (FileItem item in items)
            {
                lines.Add(string.Join(",",
                    Csv(item.Name), Csv(item.Directory),
                    item.IsDirectory
                        ? string.Empty
                        : item.SizeBytes.ToString(CultureInfo.InvariantCulture),
                    Csv(item.Modified.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))));
            }
        }

        ExportAsync(target, lines, items.Count);
    }

    private async void ExportAsync(string target, List<string> lines, int rowCount)
    {
        Mouse.OverrideCursor = Cursors.Wait;
        StatusText.Text = Lang.Tr("Status.Exporting", Lang.Count("Plural.Rows", rowCount));
        try
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                using StreamWriter writer = new StreamWriter(target, false, Encoding.UTF8);
                foreach (string line in lines)
                {
                    writer.WriteLine(line);
                }
            }).ConfigureAwait(true);

            Log.Write($"exported {rowCount:N0} rows to {target}");
            RefreshStatusLine();
            if (Dialogs.Confirm(this,
                    Lang.Tr("Dialog.Export.Done", Lang.Count("Plural.Rows", rowCount))))
            {
                Launch(target);
            }
        }
        catch (Exception ex)
        {
            RefreshStatusLine();
            Dialogs.Info(this, Lang.Tr("Dialog.Export.Failed", ex.Message));
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    private static readonly char[] CsvSpecials = { ',', '"', '\n', '\r' };

    private static string Csv(string value)
    {
        value ??= string.Empty;

        // A leading =, +, - or @ makes Excel and Calc treat the cell as a formula rather than
        // text. File names are not our text — they are whatever happened to be on the disk, so a
        // file called `=cmd|'/c calc'!A1` would execute on open. Prefixing a single quote is the
        // conventional neutraliser: spreadsheets read it as "this is literal" and hide it.
        if (value.Length > 0 && (value[0] == '=' || value[0] == '+' || value[0] == '-' || value[0] == '@'))
        {
            value = "'" + value;
        }

        // Quote when the value could otherwise break the row, and double any embedded quotes.
        return value.IndexOfAny(CsvSpecials) >= 0
            ? "\"" + value.Replace("\"", "\"\"") + "\""
            : value;
    }

    private void OnDelete(object sender, RoutedEventArgs e) => Delete(permanent: false);

    private void OnDeletePermanently(object sender, RoutedEventArgs e) => Delete(permanent: true);

    private void Delete(bool permanent)
    {
        List<FileItem> items = Selected.ToList();
        if (items.Count == 0)
        {
            return;
        }

        // One file is named; several are counted. Naming the one about to be deleted is worth more
        // than any wording of "1 item".
        string what = items.Count == 1
            ? $"\"{items[0].Name}\""
            : Lang.Count("Plural.Items", items.Count);
        string how = Lang.Tr(permanent ? "Dialog.Delete.Permanent" : "Dialog.Delete.Recycle");
        if (!Dialogs.Confirm(this, Lang.Tr("Dialog.Delete.Confirm", how, what),
                System.Windows.MessageBoxImage.Warning))
        {
            return;
        }

        DeleteAsync(items, permanent);
    }

    /// <summary>
    /// Does the deleting off the UI thread.
    ///
    /// Each delete is a synchronous shell call, and a few thousand of them in a loop on the
    /// dispatcher is several seconds of a window Windows paints over with "Not responding" —
    /// during a destructive operation, which is the worst possible moment for the app to look
    /// like it has crashed.
    ///
    /// Paths are resolved on the UI thread first: FullPath walks the live engine index, which is
    /// not safe to walk from a worker. Only the filesystem calls move.
    /// </summary>
    private async void DeleteAsync(List<FileItem> items, bool permanent)
    {
        var targets = new List<(FileItem Item, string Path)>(items.Count);
        foreach (FileItem item in items)
        {
            targets.Add((item, item.FullPath));
        }

        Microsoft.VisualBasic.FileIO.RecycleOption option = permanent
            ? Microsoft.VisualBasic.FileIO.RecycleOption.DeletePermanently
            : Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin;

        Mouse.OverrideCursor = Cursors.Wait;
        StatusText.Text = Lang.Tr("Status.Deleting", Lang.Count("Plural.Items", targets.Count));

        List<FileItem> deleted = new List<FileItem>(targets.Count);
        int failed = 0;

        try
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                foreach ((FileItem item, string path) in targets)
                {
                    if (string.IsNullOrEmpty(path))
                    {
                        failed++;
                        continue;
                    }
                    try
                    {
                        // Microsoft.VisualBasic is the only in-box way to get real Recycle Bin
                        // semantics rather than an unrecoverable delete.
                        //
                        // UIOption.OnlyErrorDialogs, not AllDialogs: the confirmation has already
                        // been asked once for the whole set, and a per-item prompt raised from a
                        // worker thread would be ownerless and could appear behind the window.
                        if (item.IsDirectory)
                        {
                            Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(path,
                                Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, option);
                        }
                        else
                        {
                            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path,
                                Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, option);
                        }
                        deleted.Add(item);
                    }
                    catch
                    {
                        failed++;
                    }
                }
            }).ConfigureAwait(true);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }

        // Marking touches the engine index, so it happens back on the UI thread with everything
        // else that does.
        foreach (FileItem item in deleted)
        {
            // Tell the engine index it is gone, so the row renders struck through. The USN
            // journal would notice eventually; doing it here makes the feedback immediate.
            item.MarkDeleted();
        }

        Log.Write($"delete (permanent={permanent}): {targets.Count} requested, {failed} failed");
        RefreshStatusLine();

        // Mark and redraw rather than re-running the search. Re-running threw away the scroll
        // position and the selection, and made the rows vanish - so there was no way to tell a
        // successful delete from a row you had simply scrolled past.
        _results.Refresh();
        ResyncSelectionTotal();

        if (failed > 0)
        {
            Dialogs.Info(this, Lang.Tr("Dialog.Delete.Failed", Lang.Count("Plural.Items", failed)),
                System.Windows.MessageBoxImage.Warning);
        }
    }

    // ---- context menus ---------------------------------------------------------------------

    /// <summary>
    /// Picks the menu from what is actually under the pointer. Right-clicking the empty space
    /// below the last row used to open the file menu and act on whatever was still selected
    /// somewhere out of sight, which is indistinguishable from acting on the wrong file.
    /// </summary>
    private void OnResultsContextMenuOpening(object sender, System.Windows.Controls.ContextMenuEventArgs e)
    {
        ResultsList.ContextMenu = RowUnderPointer() != null && Selected.Any()
            ? (System.Windows.Controls.ContextMenu)Resources["ResultsMenu"]
            : (System.Windows.Controls.ContextMenu)Resources["BackgroundMenu"];
    }

    /// <summary>
    /// Right-clicking a row that is not selected selects it first — the rule Explorer uses, and
    /// the only way the menu that follows can be about the row that was clicked.
    /// </summary>
    private void OnResultsRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        System.Windows.Controls.ListViewItem row =
            FindAncestor<System.Windows.Controls.ListViewItem>(e.OriginalSource as DependencyObject);
        if (row == null || row.IsSelected)
        {
            return;
        }
        ResultsList.SelectedItems.Clear();
        row.IsSelected = true;
        UpdateSelectionText(null);
    }

    private System.Windows.Controls.ListViewItem RowUnderPointer()
    {
        System.Windows.Point point = Mouse.GetPosition(ResultsList);
        System.Windows.Media.HitTestResult hit =
            System.Windows.Media.VisualTreeHelper.HitTest(ResultsList, point);
        return hit == null
            ? null
            : FindAncestor<System.Windows.Controls.ListViewItem>(hit.VisualHit);
    }

    private void OnSelectAll(object sender, RoutedEventArgs e)
    {
        ResultsList.SelectAll();
        UpdateSelectionText(null);
    }

    private void OnClearSelection(object sender, RoutedEventArgs e)
    {
        ResultsList.SelectedItems.Clear();
        UpdateSelectionText(null);
    }

    private void OnSearchAgain(object sender, RoutedEventArgs e)
    {
        CommitHistory();
        _hasManualSearch = true;
        RunSearch();
    }

    // ---- query field menu ------------------------------------------------------------------

    /// <summary>The field the menu was opened on, or the name box if that cannot be worked out.</summary>
    private Wpf.Ui.Controls.TextBox MenuField(object sender) =>
        (sender as System.Windows.Controls.ContextMenu)?.PlacementTarget as Wpf.Ui.Controls.TextBox
        ?? _menuField
        ?? FilenameBox;

    private Wpf.Ui.Controls.TextBox _menuField;

    /// <summary>
    /// Menus live in the window's resources so three text boxes can share one, and a
    /// ResourceDictionary has no namescope — so <c>x:Name</c> there generates no field and the
    /// items have to be looked up on the instance that was opened.
    /// </summary>
    private static System.Windows.Controls.MenuItem Item(object menu, string name) =>
        (menu as System.Windows.Controls.ContextMenu)?.Items
            .OfType<System.Windows.Controls.MenuItem>()
            .FirstOrDefault(i => i.Name == name);

    private void OnQueryMenuOpened(object sender, RoutedEventArgs e)
    {
        _menuField = (sender as System.Windows.Controls.ContextMenu)?.PlacementTarget
            as Wpf.Ui.Controls.TextBox;

        System.Windows.Controls.MenuItem paste = Item(sender, "PasteAndSearchItem");
        if (paste != null)
        {
            paste.IsEnabled = Clipboard.ContainsText();
        }
    }

    private void OnPasteAndSearch(object sender, RoutedEventArgs e)
    {
        string text;
        try { text = Clipboard.GetText(); }
        catch { return; }        // the clipboard can be locked by another app

        if (string.IsNullOrEmpty(text))
        {
            return;
        }
        // A path pasted from Explorer is almost never wanted verbatim in the name field.
        Wpf.Ui.Controls.TextBox field = _menuField ?? FilenameBox;
        field.Text = text.Trim().Trim('"');
        field.CaretIndex = field.Text.Length;
        field.Focus();

        _typingDebounce.Stop();
        CommitHistory();
        _hasManualSearch = true;
        RunSearch();
    }

    private void OnClearThisField(object sender, RoutedEventArgs e)
    {
        Wpf.Ui.Controls.TextBox field = _menuField ?? FilenameBox;
        field.Clear();
        field.Focus();
    }

    private void OnClearAllFields(object sender, RoutedEventArgs e)
    {
        FilenameBox.Clear();
        DirectoryBox.Clear();
        SizeBox.Clear();
        FilenameBox.Focus();
    }

    // ---- filter menu -----------------------------------------------------------------------

    /// <summary>
    /// Fills the ticks in from the controls rather than tracking them separately, so the menu
    /// cannot drift out of step with the switches it mirrors.
    /// </summary>
    private void OnFilterMenuOpened(object sender, RoutedEventArgs e)
    {
        bool files = FilesToggle.IsChecked == true;
        bool folders = FoldersToggle.IsChecked == true;

        Tick(sender, "MenuBoth", files && folders);
        Tick(sender, "MenuFilesOnly", files && !folders);
        Tick(sender, "MenuFoldersOnly", !files && folders);
    }

    // ---- per-field options -----------------------------------------------------------------

    private bool _nameMatchCase;
    private bool _nameRegex;
    private bool _folderMatchCase;
    private bool _folderRegex;
    private bool _subfolders = true;

    /// <summary>Opens the menu named by the button's Tag, under the button.</summary>
    private void OnFieldOptions(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement button || button.Tag is not string key)
        {
            return;
        }
        System.Windows.Controls.ContextMenu menu =
            (System.Windows.Controls.ContextMenu)Resources[key];
        menu.PlacementTarget = button;
        menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
        menu.IsOpen = true;
    }

    private void OnNameOptionsOpened(object sender, RoutedEventArgs e)
    {
        Tick(sender, "NameMatchCase", _nameMatchCase);
        Tick(sender, "NameRegex", _nameRegex);
    }

    private void OnFolderOptionsOpened(object sender, RoutedEventArgs e)
    {
        Tick(sender, "FolderMatchCase", _folderMatchCase);
        Tick(sender, "FolderRegex", _folderRegex);
        Tick(sender, "FolderSubfolders", _subfolders);
    }

    private void OnNameOptionToggled(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.MenuItem item)
        {
            return;
        }
        if (item.Name == "NameMatchCase") _nameMatchCase = item.IsChecked;
        else if (item.Name == "NameRegex") _nameRegex = item.IsChecked;
        AfterOptionChange();
    }

    private void OnFolderOptionToggled(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.MenuItem item)
        {
            return;
        }
        switch (item.Name)
        {
            case "FolderMatchCase": _folderMatchCase = item.IsChecked; break;
            case "FolderRegex": _folderRegex = item.IsChecked; break;
            case "FolderSubfolders": _subfolders = item.IsChecked; break;
        }
        AfterOptionChange();
    }

    private void AfterOptionChange()
    {
        DescribeActiveOptions();
        OnQueryChanged(this, new RoutedEventArgs());
    }

    /// <summary>
    /// Options moved onto the fields they belong to, which costs the glanceability the old row
    /// of checkboxes had. This line puts back the one thing that row was genuinely good at:
    /// seeing, without opening anything, that a search is not running on default settings.
    /// </summary>
    private void DescribeActiveOptions()
    {
        List<string> parts = new List<string>();
        if (_nameMatchCase) parts.Add(Lang.Tr("Active.Name.Case"));
        if (_nameRegex) parts.Add(Lang.Tr("Active.Name.Regex"));
        if (_folderMatchCase) parts.Add(Lang.Tr("Active.Folder.Case"));
        if (_folderRegex) parts.Add(Lang.Tr("Active.Folder.Regex"));
        if (!_subfolders) parts.Add(Lang.Tr("Active.NoSubfolders"));

        ActiveOptionsText.Text = parts.Count == 0 ? string.Empty : string.Join("  ·  ", parts);
    }

    private void OnSizePreset(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.MenuItem item && item.Tag is string expression)
        {
            SizeBox.Text = expression;
            SizeBox.CaretIndex = SizeBox.Text.Length;
        }
    }

    // ---- the folder field ------------------------------------------------------------------

    /// <summary>
    /// Opens the folder picker at whatever is already typed, and escapes the result when the
    /// field is in regex mode — a Windows path is full of backslashes, and pasting one into a
    /// regular expression unescaped produces a pattern that matches nothing.
    /// </summary>
    private void OnBrowseFolder(object sender, RoutedEventArgs e)
    {
        string current = DirectoryBox.Text?.Trim() ?? string.Empty;
        if (_folderRegex && current.Length > 0)
        {
            try { current = System.Text.RegularExpressions.Regex.Unescape(current); }
            catch { /* not a valid escape sequence; open at it as typed */ }
        }

        Microsoft.Win32.OpenFolderDialog dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = Lang.Tr("Field.Folder.PickerTitle"),
            Multiselect = false,
        };
        if (System.IO.Directory.Exists(current))
        {
            dialog.InitialDirectory = current;
        }
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        string chosen = _folderRegex
            ? System.Text.RegularExpressions.Regex.Escape(dialog.FolderName)
            : dialog.FolderName;
        DirectoryBox.Text = chosen;
        DirectoryBox.CaretIndex = chosen.Length;
    }

    private void OnFolderDragOver(object sender, System.Windows.DragEventArgs e)
    {
        e.Effects = FolderFrom(e) != null
            ? System.Windows.DragDropEffects.Link
            : System.Windows.DragDropEffects.None;
        e.Handled = true;
    }

    /// <summary>
    /// Dropping a folder — or a file, which stands for the folder holding it — fills the field.
    /// It is the shortest path from "this one, here" to a query.
    /// </summary>
    private void OnFolderDrop(object sender, System.Windows.DragEventArgs e)
    {
        string folder = FolderFrom(e);
        if (folder == null)
        {
            return;
        }
        DirectoryBox.Text = _folderRegex
            ? System.Text.RegularExpressions.Regex.Escape(folder)
            : folder;
        DirectoryBox.CaretIndex = DirectoryBox.Text.Length;
        DirectoryBox.Focus();
        e.Handled = true;
    }

    // ---- folder completion -----------------------------------------------------------------

    /// <summary>
    /// Offers the real folders that extend what has been typed.
    ///
    /// Deliberately narrow about when it runs: only for something that already looks like a
    /// path, never in regex mode (where the text is a pattern, not a path), and never for a bare
    /// UNC root — enumerating a server share can block for as long as the network feels like it,
    /// and this runs on the UI thread while someone is typing.
    /// </summary>
    private void UpdateFolderSuggestions()
    {
        string text = DirectoryBox.Text ?? string.Empty;

        if (_folderRegex || text.Length < 2 || !text.Contains('\\') && !text.Contains(':')
            || text.StartsWith(@"\\"))
        {
            FolderPopup.IsOpen = false;
            return;
        }

        string parent;
        string prefix;
        try
        {
            // A trailing separator means "everything inside this one"; otherwise the last
            // segment is what is being completed.
            if (text.EndsWith('\\') || text.EndsWith('/'))
            {
                parent = text;
                prefix = string.Empty;
            }
            else
            {
                parent = System.IO.Path.GetDirectoryName(text);
                prefix = System.IO.Path.GetFileName(text);
            }
        }
        catch
        {
            FolderPopup.IsOpen = false;      // invalid characters for a path
            return;
        }

        if (string.IsNullOrEmpty(parent) || !System.IO.Directory.Exists(parent))
        {
            FolderPopup.IsOpen = false;
            return;
        }

        List<string> matches;
        try
        {
            matches = System.IO.Directory.EnumerateDirectories(parent)
                .Where(d => System.IO.Path.GetFileName(d)
                    .StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Take(50)
                .ToList();
        }
        catch
        {
            FolderPopup.IsOpen = false;      // no permission, or the drive went away
            return;
        }

        // Nothing to add when the only match is what is already typed.
        if (matches.Count == 0
            || (matches.Count == 1 && string.Equals(matches[0], text.TrimEnd('\\', '/'),
                                                    StringComparison.OrdinalIgnoreCase)))
        {
            FolderPopup.IsOpen = false;
            return;
        }

        FolderSuggestions.ItemsSource = matches;
        FolderSuggestions.SelectedIndex = -1;      // typing keeps priority over the list
        FolderPopup.IsOpen = true;
    }

    private void OnFolderSuggestKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            UseFolderSuggestion(FolderSuggestions.SelectedItem as string);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            FolderPopup.IsOpen = false;
            DirectoryBox.Focus();
            e.Handled = true;
        }
    }

    private void OnFolderSuggestClicked(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject source
            && FindAncestor<System.Windows.Controls.ListBoxItem>(source)?.Content is string folder)
        {
            UseFolderSuggestion(folder);
            e.Handled = true;
        }
    }

    private void UseFolderSuggestion(string folder)
    {
        if (string.IsNullOrEmpty(folder))
        {
            return;
        }
        FolderPopup.IsOpen = false;
        _suppressFolderSuggest = true;          // accepting one must not immediately offer more
        DirectoryBox.Text = folder;
        _suppressFolderSuggest = false;
        DirectoryBox.CaretIndex = folder.Length;
        DirectoryBox.Focus();
    }

    private bool _suppressFolderSuggest;

    private static string FolderFrom(System.Windows.DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
        {
            return null;
        }
        if (e.Data.GetData(System.Windows.DataFormats.FileDrop) is not string[] paths
            || paths.Length == 0)
        {
            return null;
        }
        string first = paths[0];
        try
        {
            return System.IO.Directory.Exists(first)
                ? first
                : System.IO.Path.GetDirectoryName(first);
        }
        catch
        {
            return null;
        }
    }

    private static void Tick(object menu, string name, bool on)
    {
        System.Windows.Controls.MenuItem item = Item(menu, name);
        if (item != null)
        {
            item.IsChecked = on;
        }
    }

    private void OnFilterKindChosen(object sender, RoutedEventArgs e)
    {
        // Three mutually exclusive choices, so a click always sets both switches outright
        // instead of toggling the one that was clicked.
        string which = (sender as System.Windows.Controls.MenuItem)?.Name;
        FilesToggle.IsChecked = which != "MenuFoldersOnly";
        FoldersToggle.IsChecked = which != "MenuFilesOnly";
    }

    private void OnResetFilters(object sender, RoutedEventArgs e)
    {
        FilesToggle.IsChecked = true;
        FoldersToggle.IsChecked = true;
        _nameMatchCase = false;
        _nameRegex = false;
        _folderMatchCase = false;
        _folderRegex = false;
        _subfolders = true;
        AfterOptionChange();
    }

    // ---- column header menu ----------------------------------------------------------------

    private string _menuColumn;

    private void OnHeaderMenuOpened(object sender, RoutedEventArgs e)
    {
        System.Windows.Controls.GridViewColumnHeader header =
            (sender as System.Windows.Controls.ContextMenu)?.PlacementTarget
            as System.Windows.Controls.GridViewColumnHeader;
        _menuColumn = KeyOf(header?.Column);

        // The padding header past the last column has no column behind it.
        bool sortable = _menuColumn != null && BuildComparison(_menuColumn) != null;
        foreach (string name in new[] { "HeaderSortAscending", "HeaderSortDescending" })
        {
            System.Windows.Controls.MenuItem item = Item(sender, name);
            if (item == null)
            {
                continue;
            }
            bool descending = name == "HeaderSortDescending";
            item.IsEnabled = sortable;
            // Naming the column is the whole value of this menu over clicking the header, so the
            // name is the translated one — the key would be meaningless on screen.
            item.Header = sortable
                ? Lang.Tr(descending ? "Menu.SortByDescending" : "Menu.SortByAscending",
                          Lang.Tr("Column." + _menuColumn))
                : Lang.Tr(descending ? "Menu.SortDescending" : "Menu.SortAscending");
        }
    }

    private void OnHeaderSort(object sender, RoutedEventArgs e)
    {
        if (_menuColumn == null)
        {
            return;
        }
        _sortColumn = _menuColumn;
        _sortDescending = (sender as System.Windows.Controls.MenuItem)?.Name == "HeaderSortDescending";
        ApplySort();
    }

    private void OnFitColumn(object sender, RoutedEventArgs e)
    {
        System.Windows.Controls.GridViewColumn column = ColumnNamed(_menuColumn);
        if (column == null)
        {
            return;
        }
        // Sizing one column to its contents is a decision about that column, so it takes the
        // columns off auto-fit for the same reason dragging an edge does — otherwise the next
        // resize of the window would silently undo it.
        TakeColumnsOverManually();

        // The documented way to ask a GridViewColumn to measure itself: clear the width, then
        // read back what it settled on.
        column.Width = column.ActualWidth;
        column.Width = double.NaN;
    }

    private void OnResetColumnWidths(object sender, RoutedEventArgs e)
    {
        _settings.AutoFitColumns = true;
        FitColumns(_lastResultsViewportWidth);
    }

    // ---- column widths ---------------------------------------------------------------------

    /// <summary>
    /// How the width above the floors is shared out, in the column order of the XAML: name, path,
    /// size, modified, type, created, accessed. Relative, not absolute — only the ratios matter.
    ///
    /// Weighted hard towards the name and the path, because those are the two columns that get
    /// better with more room. A date column that is wide enough is finished; it does not become
    /// more informative at twice the width, and the space is wanted elsewhere.
    /// </summary>
    private static readonly double[] ColumnShares = { 400, 420, 60, 90, 40, 90, 90 };

    /// <summary>
    /// What each column gets before any of that is shared out.
    ///
    /// These are what the content needs rather than what looks balanced, and the difference is the
    /// point: a name or a path that runs out of room ends in an ellipsis and is still doing its
    /// job, because the eye only needs the start of it. A date that runs out of room reads
    /// "12/17/2024 3:38 A", which is not a shorter date — it is a wrong one. So the three date
    /// columns hold enough for the widest common short format, and the name and path take what is
    /// left over.
    /// </summary>
    private static readonly double[] ColumnFloors = { 130, 130, 80, 155, 50, 155, 155 };

    /// <summary>
    /// What a row spends on itself before any column sees a unit of it: the ListViewItem style's
    /// Margin="2,1" and Padding="6,4" in MainWindow.xaml, added up across both sides. Keep the two
    /// in step — get this wrong and the grid grows a horizontal scrollbar that has nothing to
    /// scroll to.
    /// </summary>
    private const double RowChrome = 2 * 2 + 2 * 6;

    /// <summary>
    /// The last viewport width the results list reported, so Reset can re-fit without waiting for
    /// the next resize.
    /// </summary>
    private double _lastResultsViewportWidth;

    /// <summary>Guards the re-entry: setting widths raises ScrollChanged again.</summary>
    private bool _fittingColumns;

    private void OnResultsScrollChanged(object sender,
                                        System.Windows.Controls.ScrollChangedEventArgs e)
    {
        _lastResultsViewportWidth = e.ViewportWidth;
        if (!_fittingColumns && e.ViewportWidthChange != 0)
        {
            FitColumns(e.ViewportWidth);
        }
    }

    /// <summary>
    /// Shares the width of the results list out between the columns, so that all seven are on
    /// screen whatever the window is doing.
    ///
    /// This exists because of what happens when they do not fit. Columns past the right edge can
    /// only be reached by scrolling sideways, and the scrollbar's range comes from the *rows* —
    /// so with no results there is nothing to scroll and those columns cannot be brought into
    /// view at all. Before any search has run, which is the first thing anyone sees, the grid was
    /// therefore showing a name, a path and a column called "Si".
    ///
    /// Every column keeps its floor first and the remainder is split by share, so widening the
    /// window gives the space to the name and the path rather than padding the date columns.
    /// </summary>
    private void FitColumns(double viewportWidth)
    {
        if (!_settings.AutoFitColumns || viewportWidth <= 0)
        {
            return;
        }

        System.Windows.Controls.GridView view = (System.Windows.Controls.GridView)ResultsList.View;
        int count = Math.Min(view.Columns.Count, ColumnShares.Length);
        if (count == 0)
        {
            return;
        }

        double floors = 0;
        double shares = 0;
        for (int i = 0; i < count; i++)
        {
            floors += ColumnFloors[i];
            shares += ColumnShares[i];
        }

        // The columns do not get the whole viewport. Each row is a Border inside it carrying the
        // ListViewItem style's Margin="2,1" and Padding="6,4" — 16 units across the pair of them —
        // and a row wider than the viewport is exactly what puts a horizontal scrollbar under a
        // grid that looks like it fits. The extra 2 is slack: landing precisely on the viewport
        // leaves a rounding error to decide whether that scrollbar appears, and one that flickers
        // in and out while the window is dragged is worse than one that never shows.
        double available = viewportWidth - RowChrome - 2;

        _fittingColumns = true;
        try
        {
            if (available <= floors || shares <= 0)
            {
                // Narrower than the columns can honestly go. They overflow, and the window's own
                // minimum size is what stops this from being reachable in practice.
                for (int i = 0; i < count; i++)
                {
                    view.Columns[i].Width = ColumnFloors[i];
                }
                return;
            }

            double spare = available - floors;
            for (int i = 0; i < count; i++)
            {
                view.Columns[i].Width = ColumnFloors[i] + spare * ColumnShares[i] / shares;
            }
        }
        finally
        {
            _fittingColumns = false;
        }
    }

    /// <summary>
    /// Hands the columns to the user for good, at the widths they are at right now.
    ///
    /// Called the moment a column edge is dragged. Auto-fit and a dragged width cannot both be
    /// true — the next resize would overwrite the drag — and of the two, the one the user just
    /// did by hand wins.
    /// </summary>
    private void TakeColumnsOverManually()
    {
        if (_fittingColumns)
        {
            return;      // our own width assignment, not a drag
        }
        _settings.AutoFitColumns = false;
    }

    private void OnColumnResized(object sender, RoutedEventArgs e) => TakeColumnsOverManually();

    private System.Windows.Controls.GridViewColumn ColumnNamed(string key)
    {
        if (key == null)
        {
            return null;
        }
        foreach (System.Windows.Controls.GridViewColumn c in
                 ((System.Windows.Controls.GridView)ResultsList.View).Columns)
        {
            if (KeyOf(c) == key)
            {
                return c;
            }
        }
        return null;
    }

    /// <summary>
    /// Shift plus the wheel scrolls sideways, which is what it does in every list with a
    /// horizontal scrollbar. WPF has no built-in binding for it.
    /// </summary>
    private void OnResultsMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.Shift)
        {
            return;
        }
        System.Windows.Controls.ScrollViewer scroller = ResultsScroller();
        if (scroller == null)
        {
            return;
        }
        // Wheel up scrolls left, matching the direction the vertical wheel moves content.
        scroller.ScrollToHorizontalOffset(scroller.HorizontalOffset - e.Delta);
        e.Handled = true;
    }

    private System.Windows.Controls.ScrollViewer _resultsScroller;

    private System.Windows.Controls.ScrollViewer ResultsScroller() =>
        _resultsScroller ??= FindDescendant<System.Windows.Controls.ScrollViewer>(ResultsList);

    private static T FindDescendant<T>(DependencyObject node) where T : DependencyObject
    {
        int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(node);
        for (int i = 0; i < count; i++)
        {
            DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(node, i);
            if (child is T match)
            {
                return match;
            }
            T deeper = FindDescendant<T>(child);
            if (deeper != null)
            {
                return deeper;
            }
        }
        return null;
    }

    private System.Windows.Point _dragOrigin;
    private bool _dragArmed;

    private void OnResultsMouseDown(object sender, MouseButtonEventArgs e)
    {
        _dragOrigin = e.GetPosition(null);

        // Only arm a drag when the press lands on a row that is ALREADY selected — the same rule
        // Explorer uses. Dragging from anywhere else has to stay free for extending a selection,
        // which the previous version made impossible by treating every drag as a file drag.
        _dragArmed = false;
        if (e.OriginalSource is DependencyObject source)
        {
            System.Windows.Controls.ListViewItem row = FindAncestor<System.Windows.Controls.ListViewItem>(source);
            _dragArmed = row is { IsSelected: true };
        }
    }

    private static T FindAncestor<T>(DependencyObject node) where T : DependencyObject
    {
        while (node != null && node is not T)
        {
            node = System.Windows.Media.VisualTreeHelper.GetParent(node);
        }
        return node as T;
    }

    private void OnResultsMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (!_dragArmed || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }
        System.Windows.Vector delta = e.GetPosition(null) - _dragOrigin;
        if (Math.Abs(delta.X) < SystemParameters.MinimumHorizontalDragDistance
            && Math.Abs(delta.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        string[] paths = Selected.Select(i => i.FullPath)
            .Where(p => !string.IsNullOrEmpty(p)).ToArray();
        if (paths.Length == 0)
        {
            return;
        }

        _dragArmed = false;      // one drag per press
        System.Windows.DataObject data = new System.Windows.DataObject();
        data.SetData(System.Windows.DataFormats.FileDrop, paths);
        DragDrop.DoDragDrop(ResultsList, data, System.Windows.DragDropEffects.Copy
                                               | System.Windows.DragDropEffects.Link);
    }

    /// <summary>
    /// Opens something the way Explorer would — and, crucially, NOT as administrator.
    ///
    /// This app runs elevated, because reading the MFT needs a raw volume handle. A child started
    /// with Process.Start inherits that token, so pressing Enter on a result was launching that
    /// document, script or executable with full administrator rights, silently, every time. For a
    /// tool whose entire job is to put every file on the machine one keystroke away, that is the
    /// wrong default by a wide margin: the one thing the user asked for was to *look* at their
    /// files.
    ///
    /// The fix is to have the desktop shell do the launching. Explorer runs at medium integrity,
    /// so anything it starts on our behalf does too. IShellDispatch2.ShellExecute on the shell
    /// window's own dispatch is the documented way to ask it.
    ///
    /// If that route is unavailable — Explorer not running, or the shell refusing the automation
    /// call — fall back to starting it ourselves rather than doing nothing, and say so in the log
    /// so an elevated launch is never a silent surprise.
    /// </summary>
    private static void Launch(string file, string args = null)
    {
        if (string.IsNullOrEmpty(file))
        {
            return;
        }

        if (App.IsElevated() && ShellLaunch.TryOpenAsUser(file, args))
        {
            return;
        }

        try
        {
            if (App.IsElevated())
            {
                Log.Write($"launching elevated (shell hand-off unavailable): {file}");
            }
            Process.Start(new ProcessStartInfo(file)
            {
                Arguments = args ?? string.Empty,
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            // The shell's own message, which is already in the user's Windows language.
            Dialogs.Info(null, ex.Message, System.Windows.MessageBoxImage.Warning);
        }
    }

    // The shell properties dialog has no managed equivalent.
    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO info);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct SHELLEXECUTEINFO
    {
        public int cbSize;
        public uint fMask;
        public IntPtr hwnd;
        [MarshalAs(UnmanagedType.LPWStr)] public string lpVerb;
        [MarshalAs(UnmanagedType.LPWStr)] public string lpFile;
        [MarshalAs(UnmanagedType.LPWStr)] public string lpParameters;
        [MarshalAs(UnmanagedType.LPWStr)] public string lpDirectory;
        public int nShow;
        public IntPtr hInstApp;
        public IntPtr lpIDList;
        [MarshalAs(UnmanagedType.LPWStr)] public string lpClass;
        public IntPtr hkeyClass;
        public uint dwHotKey;
        public IntPtr hIcon;
        public IntPtr hProcess;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr handle);

    private void ShowFileProperties(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        SHELLEXECUTEINFO info = new SHELLEXECUTEINFO
        {
            lpVerb = "properties",
            lpFile = path,
            nShow = 5,
            // Owned by our window, so the sheet is modal to the app and cannot end up behind it.
            hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle,
            fMask = SEE_MASK_INVOKEIDLIST | SEE_MASK_NOCLOSEPROCESS,
        };
        info.cbSize = Marshal.SizeOf<SHELLEXECUTEINFO>();

        if (!ShellExecuteEx(ref info))
        {
            int error = Marshal.GetLastWin32Error();
            Log.Write($"properties failed for \"{path}\": win32 {error}");
            StatusText.Text = Lang.Tr("Status.PropertiesFailed");
            return;
        }

        // NOCLOSEPROCESS means the shell hands back an open process handle and expects us to close
        // it. Not doing so leaked one handle per Alt+Enter for the lifetime of the process.
        if (info.hProcess != IntPtr.Zero)
        {
            CloseHandle(info.hProcess);
        }
    }

    private const uint SEE_MASK_INVOKEIDLIST = 0x0000000C;
    private const uint SEE_MASK_NOCLOSEPROCESS = 0x00000040;

    private static bool IsElevated()
    {
        try
        {
            using WindowsIdentity id = WindowsIdentity.GetCurrent();
            return new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }
}
