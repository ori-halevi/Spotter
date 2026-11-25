using System.Collections.Generic;

namespace Spotter.Localization;

/// <summary>
/// English — the reference table. Every other language is this file with the values replaced, and
/// anything missing from one of them falls back to here, so this is the one that has to be
/// complete.
///
/// Keys read as a path through the UI: where the string appears, then what it is. Grep a key to
/// find its one call site.
/// </summary>
internal static class StringsEn
{
    internal static readonly IReadOnlyDictionary<string, string> Table =
        new Dictionary<string, string>
        {
            // ---- shared ------------------------------------------------------------------------
            ["Common.Save"] = "Save",
            ["Common.Cancel"] = "Cancel",
            ["Common.Remove"] = "Remove",
            ["Common.OpenFolder"] = "Open folder",

            // ---- query fields ------------------------------------------------------------------
            ["Field.Name.Placeholder"] = "Search file name…",
            ["Field.Name.Tip.Title"] = "File name",
            ["Field.Name.Tip.Part"] = "Matches any part of the name, so 'note' finds 'notepad.exe'.",
            ["Field.Name.Tip.Words"] = "Several words must all appear, in any order:  log 2024",
            ["Field.Name.Tip.Wildcards"] = "Wildcards * and ? work per word:  *.dll    report?.txt",
            ["Field.Name.Tip.Keys"] = "Down opens recent searches. Enter searches now instead of waiting for the pause.",
            ["Field.Name.OptionsTip"] = "Options for the file name: match case, regular expression, how separators are treated.",
            ["Field.Name.RefreshTip"] = "Run the search again, the same as F5. Use it when a file is about to be created: nothing re-runs by itself while you wait.",

            ["Field.Folder.Placeholder"] = "In folder",
            ["Field.Folder.Tip.Title"] = "In folder",
            ["Field.Folder.Tip.Part"] = "Keeps only results whose folder matches. Part of a name is enough:  windows",
            ["Field.Folder.Tip.Levels"] = "Separate levels with \\ or / and each part is matched in turn:  users\\public",
            ["Field.Folder.Tip.Subfolders"] = "With Subfolders off, it must match the file's own folder rather than any folder above it.",
            ["Field.Folder.OptionsTip"] = "Options for the folder: match case, regular expression, subfolders.",
            ["Field.Folder.BrowseTip"] = "Pick a folder. Whatever is typed now is where the picker opens.",
            ["Field.Folder.PickerTitle"] = "Search in folder",

            ["Field.Size.Placeholder"] = "Size, e.g. >100MB",
            ["Field.Size.Tip.Title"] = "Size",
            // What each size expression means. The expression itself is the engine's grammar and is
            // the same in every language, so it lives in MainWindow.xaml — where it can also be
            // pinned left-to-right, which a string in this table cannot be.
            ["Field.Size.Tip.Gt"] = "larger than",
            ["Field.Size.Tip.Le"] = "at most",
            ["Field.Size.Tip.Eq"] = "that size, near enough",
            ["Field.Size.Tip.About"] = "within 10% of it",
            ["Field.Size.Tip.Range"] = "a range",
            ["Field.Size.Tip.Units"] = "Units B, KB, MB, GB, TB. A folder is measured by everything inside it, so folders can match a size filter too.",
            ["Field.Size.PresetsTip"] = "Common size filters.",

            // ---- toolbar -----------------------------------------------------------------------
            ["Toolbar.Files"] = "Files",
            ["Toolbar.Files.Tip"] = "Include files in the results. Turn off to list folders only.",
            ["Toolbar.Folders"] = "Folders",
            ["Toolbar.Folders.Tip"] = "Include folders in the results. Turn off to list files only.",

            // The file types panel behind the Files switch.
            ["Types.Title"] = "File types",
            ["Types.All"] = "All types",
            ["Types.Tip"] = "Which kinds of file count. All of them unless you say otherwise.",
            ["Types.Image"] = "Images",
            ["Types.Video"] = "Video",
            ["Types.Audio"] = "Audio",
            ["Types.Document"] = "Documents",
            ["Types.Archive"] = "Archives",
            ["Types.Code"] = "Code and settings",
            ["Types.Program"] = "Programs and installers",
            ["Types.Other"] = "Everything else",
            ["Types.Other.Tip"] = "Every file no other category claims, including files with no extension at all.",
            ["Types.Custom"] = "Other extensions",
            ["Types.Custom.Placeholder"] = "iso, mkv, bak",
            ["Types.Custom.Hint"] = "Let through on top of whatever is ticked above. To find only these, untick the lot.",
            ["Toolbar.ActiveOptions.Tip"] = "Options currently set on the query fields. Each field's options button changes its own.",
            ["Toolbar.Search"] = "Search",
            ["Toolbar.SearchAgain"] = "Search again",
            ["Toolbar.Stop"] = "Stop",
            ["Toolbar.Search.Tip"] = "Run the search. Shown because 'search as you type' is off in Options; while a search is running this becomes Stop.",
            ["Toolbar.Recent"] = "Recent",
            ["Toolbar.Recent.Tip"] = "Searches you have run before, newest first. Down in the search box opens the same list.",
            ["Toolbar.Options"] = "Options",
            ["Toolbar.Options.Tip"] = "Language, search mode, global hotkey, tray icon, theme and window size.",

            // ---- active-options summary ---------------------------------------------------------
            ["Active.Name.Case"] = "name: match case",
            ["Active.Name.Regex"] = "name: regex",
            ["Active.Name.ExactSeparators"] = "name: exact separators",
            ["Active.Folder.Case"] = "folder: match case",
            ["Active.Folder.Regex"] = "folder: regex",
            ["Active.NoSubfolders"] = "no subfolders",

            // ---- search history -----------------------------------------------------------------
            ["History.Empty"] = "No recent searches yet. Terms you search for are remembered here.",
            ["History.Hint"] = "Enter to use · Delete to forget one",
            ["History.Clear"] = "Clear history",

            // ---- columns -------------------------------------------------------------------------
            ["Column.Name"] = "Name",
            ["Column.Path"] = "Path",
            ["Column.Size"] = "Size",
            ["Column.Modified"] = "Modified",
            ["Column.Type"] = "Type",
            ["Column.Created"] = "Created",
            ["Column.Accessed"] = "Accessed",
            ["Column.Tip"] = "Click to sort by this column, click again to reverse it. Right-click for more.",

            // ---- text-field menu -----------------------------------------------------------------
            ["Menu.Cut"] = "Cut",
            ["Menu.Copy"] = "Copy",
            ["Menu.Paste"] = "Paste",
            ["Menu.PasteAndSearch"] = "Paste and search",
            ["Menu.PasteAndSearch.Tip"] = "Replace this field with the clipboard text and search for it.",
            ["Menu.RecentSearches"] = "Recent searches…",
            ["Menu.ClearField"] = "Clear this field",
            ["Menu.ClearAllFields"] = "Clear all search fields",

            // ---- per-field option menus -------------------------------------------------------------
            ["Menu.MatchCase"] = "Match case",
            ["Menu.Regex"] = "Regular expression",
            ["Menu.Regex.Name.Tip"] = "A .NET regular expression over the file name. Words and * ? wildcards stop applying.",
            ["Menu.Regex.Folder.Tip"] = "A .NET regular expression over the folder path. Backslashes need escaping — the Browse button does that for you.",
            ["Menu.FlexibleSeparators"] = "Dots and dashes count as spaces",
            ["Menu.FlexibleSeparators.Tip"] = "On: 'Family.Guy' also finds 'Family Guy' and 'Family_Guy', because a dot, an underscore or a hyphen between two words is treated as a gap. Off: they have to be exactly the character you typed.",
            ["Menu.Subfolders"] = "Include subfolders",
            ["Menu.Subfolders.Tip"] = "On: match any folder above the file. Off: the file's own folder must match.",

            // ---- size presets --------------------------------------------------------------------
            ["Menu.Size.Empty"] = "Empty (0 bytes)",
            ["Menu.Size.Tiny"] = "Tiny — under 10 KB",
            ["Menu.Size.Under1MB"] = "Under 1 MB",
            ["Menu.Size.Over10MB"] = "Over 10 MB",
            ["Menu.Size.Over100MB"] = "Over 100 MB",
            ["Menu.Size.Over1GB"] = "Over 1 GB",
            ["Menu.Size.Over10GB"] = "Over 10 GB",
            ["Menu.Size.Between"] = "Between 100 MB and 1 GB",
            ["Menu.Size.About700MB"] = "About 700 MB (within 10%)",
            ["Menu.Size.Clear"] = "Clear the size filter",

            // ---- kind filter menu ------------------------------------------------------------------
            ["Menu.Filter.Both"] = "Files and folders",
            ["Menu.Filter.FilesOnly"] = "Files only",
            ["Menu.Filter.FoldersOnly"] = "Folders only",
            ["Menu.Filter.Reset"] = "Reset all search options",
            ["Menu.Filter.Reset.Tip"] = "Back to files and folders, no case matching, no regex, subfolders included.",

            // ---- result menu -------------------------------------------------------------------------
            ["Menu.Open"] = "Open",
            ["Menu.OpenFolder"] = "Open containing folder",
            ["Menu.CopyPath"] = "Copy full path",
            ["Menu.CopyName"] = "Copy file name",
            ["Menu.CopyFolder"] = "Copy containing folder",
            ["Menu.ExportSelected"] = "Export selected…",
            ["Menu.Delete"] = "Delete",
            ["Menu.DeletePermanently"] = "Delete permanently",
            ["Menu.Properties"] = "Properties",

            // ---- background menu ----------------------------------------------------------------------
            ["Menu.SelectAll"] = "Select all",
            ["Menu.ClearSelection"] = "Clear selection",
            ["Menu.ExportAll"] = "Export all results…",
            ["Menu.SearchAgain"] = "Search again",

            // ---- column header menu -------------------------------------------------------------------
            ["Menu.SortAscending"] = "Sort ascending",
            ["Menu.SortDescending"] = "Sort descending",
            ["Menu.SortByAscending"] = "Sort by {0}, ascending",
            ["Menu.SortByDescending"] = "Sort by {0}, descending",
            ["Menu.FitColumn"] = "Fit this column to its contents",
            ["Menu.ResetColumns"] = "Reset all column widths",

            // ---- keyboard gestures printed in menus ------------------------------------------------------
            // Modifier combinations (Ctrl+C, Alt+Enter) are what is printed on the keyboard, so they
            // stay as they are in every language. Only the ones that are words get translated.
            ["Key.Down"] = "Down",
            ["Key.EscEsc"] = "Esc Esc",

            // ---- status bar --------------------------------------------------------------------------------
            ["Status.Starting"] = "Starting…",
            ["Status.NeedsAdmin"] = "Administrator rights are needed to read the drive file tables.",
            ["Status.Elevate"] = "Restart as administrator",
            ["Status.Elevate.Tip"] = "Reading drive file tables needs administrator rights. This restarts Spotter elevated.",
            ["Status.ElevateFailed"] = "Could not restart elevated: {0}",
            ["Status.PropertiesFailed"] = "Could not show properties for that item.",
            ["Status.ReadingDrives"] = "Reading drives — results will fill in as they load…",
            ["Status.Ready"] = "Ready — {0} ({1})",
            ["Status.IndexFailed"] = "Indexing failed: {0}",
            ["Status.HotkeyTaken"] = "Hotkey {0} is already in use by another app.",
            ["Status.Demo"] = "DEMO MODE — {0} (no engine, Path is blank)",
            ["Status.Found"] = "{0} in {1} ms",
            ["Status.Searching.Suffix"] = " …",
            ["Status.Selected"] = "{0} selected · {1}",
            ["Status.Selected.Tip"] = "Total size of the selected files. Folders are left out on purpose — a folder's size already includes the files inside it, so counting both would count them twice.",
            ["Status.Exporting"] = "Exporting {0}…",
            ["Status.Deleting"] = "Deleting {0}…",

            // ---- indexing progress -----------------------------------------------------------------------------
            ["Index.Reading"] = "reading drives…",
            ["Index.ReadingCount"] = "reading drives — {0}",
            ["Index.ReadingPercent"] = "reading drives — {0}, {1}{2}",
            ["Index.DrivesDone"] = "{0} of {1} drives",
            ["Index.Rate"] = " · {0}/s",
            ["Index.Remaining"] = " · about {0} left",
            ["Index.Volume.Progress"] = "{0}  ({1} of {2})",
            ["Index.Volume.Waiting"] = "waiting",
            ["Index.Volume.Failed"] = "failed",
            ["Index.Volume.NotSupported"] = "not supported",
            ["Index.Volume.SizeUnknown"] = "size unknown",

            // ---- durations and sizes ------------------------------------------------------------------------------
            ["Duration.Seconds"] = "{0} sec",
            ["Duration.MinutesSeconds"] = "{0} min {1} sec",
            ["Duration.HoursMinutes"] = "{0} h {1} min",
            // Byte units. Windows leaves these in Latin script in Hebrew, Spanish and German, and
            // uses Cyrillic in Russian; each table follows its own platform convention.
            ["Unit.B"] = "B",
            ["Unit.KB"] = "KB",
            ["Unit.MB"] = "MB",
            ["Unit.GB"] = "GB",
            ["Unit.TB"] = "TB",
            ["Unit.PB"] = "PB",

            // ---- counted things ---------------------------------------------------------------------------------------
            ["Plural.Results.one"] = "{0:N0} result",
            ["Plural.Results.other"] = "{0:N0} results",
            ["Plural.Files.one"] = "{0:N0} file",
            ["Plural.Files.other"] = "{0:N0} files",
            ["Plural.Folders.one"] = "{0:N0} folder",
            ["Plural.Folders.other"] = "{0:N0} folders",
            ["Plural.Drives.one"] = "{0:N0} drive",
            ["Plural.Drives.other"] = "{0:N0} drives",
            ["Plural.Items.one"] = "{0:N0} item",
            ["Plural.Items.other"] = "{0:N0} items",
            ["Plural.Rows.one"] = "{0:N0} row",
            ["Plural.Rows.other"] = "{0:N0} rows",
            ["Plural.DemoRows.one"] = "{0:N0} synthetic row",
            ["Plural.DemoRows.other"] = "{0:N0} synthetic rows",
            ["Contents.FilesAndFolders"] = "{0}, {1}",

            // ---- dialogs -----------------------------------------------------------------------------------------------
            ["Dialog.BulkOpen.Verb"] = "open",
            ["Dialog.BulkExplorer.Verb"] = "open Explorer windows for",
            ["Dialog.BulkConfirm"] = "This will {0} {1}. Continue?",
            ["Dialog.NothingToExport"] = "There is nothing to export.",
            ["Dialog.AlreadyRunning"] = "Spotter is already running, but this copy could not bring its window to the front.\n\nIt may belong to another user account, or be starting up.",
            ["Menu.NewInstance"] = "New instance",
            ["Tray.NewInstance"] = "New instance",
            ["NewInstance.Tip"] = "Open a second Spotter window with a search of its own.",
            ["Window.ExtraInstance"] = "Spotter — extra instance",
            ["Dialog.NewInstanceFailed"] = "Another instance could not be started. The log file has the reason.",
            ["Dialog.Export.Filter"] = "CSV file (*.csv)|*.csv|Text file (*.txt)|*.txt",
            ["Dialog.Export.FileName"] = "Spotter results.csv",
            ["Dialog.Export.CsvHeader"] = "Name,Path,Size,Modified",
            ["Dialog.Export.Done"] = "Exported {0}.\n\nOpen it now?",
            ["Dialog.Export.Failed"] = "Export failed: {0}",
            ["Dialog.Delete.Permanent"] = "permanently delete",
            ["Dialog.Delete.Recycle"] = "move to the Recycle Bin",
            ["Dialog.Delete.Confirm"] = "Are you sure you want to {0} {1}?",
            ["Dialog.Delete.Failed"] = "{0} could not be deleted.",

            // ---- notification area ----------------------------------------------------------------------------------------
            ["Tray.Show"] = "Show Spotter",
            ["Tray.Exit"] = "Exit",
            ["Tray.Hidden.NoHotkey"] = "Still running — double-click this icon to bring it back.",
            ["Tray.Hidden.Hotkey"] = "Still running — press {0} or double-click this icon.",

            // ---- engine-reported problems -------------------------------------------------------------------------------------
            ["Problem.Name"] = "File name — {0}",
            ["Problem.Folder"] = "In folder — {0}",
            ["Problem.Size"] = "Size — {0}",
            ["Problem.NoKinds"] = "Turn on Files or Folders — with both off there is nothing to find.",
            ["Problem.NoTypes"] = "No file type is ticked — open the list beside Files and tick one, or turn Folders on.",
            ["Problem.Unreadable"] = "cannot be read",

            // ---- options window ---------------------------------------------------------------------------------------------------
            ["Options.Title"] = "Options",

            ["Options.Language"] = "Language",
            ["Options.Language.Hint"] = "Takes effect straight away. Number and date formats keep following your Windows regional settings, which is a separate choice in Windows.",
            ["Options.Language.System"] = "Same as Windows",
            ["Options.Language.SystemDetail"] = "Same as Windows  ({0})",
            ["Options.Language.Unsupported"] = "Windows is set to a language Spotter does not have yet, so it uses English.",

            ["Options.Searching"] = "Searching",
            ["Options.SearchAsYouType"] = "Search as you type",
            ["Options.RememberFilters"] = "Remember the Files and Folders switches",
            ["Options.RememberFilters.Hint"] = "Off: both start every run switched on. A window that opens with one of them off finds nothing of that kind and looks as though the file is simply not there. The file type list follows the same setting.",
            ["Options.SearchAsYouType.On"] = "A new search starts on each letter typed or setting changed.",
            ["Options.SearchAsYouType.Off"] = "Nothing is searched until you press Search or Enter. Useful on a busy machine, or when a query matches most of the disk.",

            ["Options.Hotkey"] = "Global hotkey",
            ["Options.Hotkey.Hint"] = "Press the combination you want, then Save. It brings Spotter to the front from anywhere; pressing it again hides it.",
            ["Options.Hotkey.Placeholder"] = "Click here and press a combination",
            ["Options.Hotkey.NeedsModifier"] = "Add at least one of Ctrl, Alt, Shift or Win — a key on its own would be captured everywhere you type.",
            ["Options.Hotkey.Disable"] = "Disable hotkey",

            ["Options.Tray"] = "Notification area",
            ["Options.Tray.Show"] = "Show a tray icon",
            ["Options.Tray.Minimize"] = "Minimize to the tray instead of the taskbar",
            ["Options.Tray.Close"] = "Closing the window keeps it running in the tray",

            ["Options.Appearance"] = "Appearance",
            ["Options.Theme.Auto"] = "Follow Windows",
            ["Options.Theme.Dark"] = "Dark",
            ["Options.Theme.Light"] = "Light",

            ["Options.Size"] = "Window size",
            ["Options.Size.Hint"] = "Spotter opens in the top-left quarter of your screen, and draws its text and controls at a size that suits how big that screen is — not how many pixels it has.",
            ["Options.Size.Auto"] = "Match this screen",
            ["Options.Size.Auto.Detail"] = "Your screen works out at {0}%. Untick to lock a size of your own.",
            ["Options.Size.Percent"] = "{0}%",
            ["Options.Size.Remember"] = "Remember where you leave the window",
            ["Options.Size.Remember.Hint"] = "The size, the position and whether it was maximized all come back next time. A window left almost entirely off the screen is the one arrangement never kept, because there would be no way to drag it back.",
            ["Options.Size.Reset"] = "Reset size and position",
            ["Options.Size.Reset.Tip"] = "Puts the window back in the top-left quarter of the screen at its default size, straight away.",

            ["Options.Storage"] = "What this app stores on your PC",
            ["Options.Storage.Hint"] = "Nothing else is written anywhere. Removing all of it is: delete these two folders' Spotter files and remove the task below.",
            ["Options.Storage.Settings"] = "Settings",
            ["Options.Storage.AppLog"] = "App log",
            ["Options.Storage.EngineLog"] = "Engine log",
            ["Options.Storage.EngineLog.Suffix"] = "   (plus rotated SpotterLog.N.txt.zip)",
            ["Options.Storage.Task"] = "Scheduled task",
            ["Options.Storage.Open.Tip"] = "Show this file in Explorer.",
            ["Options.Storage.Task.Registered"] = "Task Scheduler → {0}\nLets this copy start elevated without a UAC prompt each time.",
            ["Options.Storage.Task.Missing"] = "Not registered. Spotter asks for administrator with a UAC prompt instead.",
            ["Options.Storage.Task.Remove.Tip"] = "Deletes the task. Spotter will ask for administrator with a UAC prompt again the next time it starts.",
            ["Options.Storage.Task.Remove.Confirm"] = "Remove the scheduled task \"{0}\"?\n\nSpotter will still run — it will ask for administrator with a UAC prompt each time, and can register the task again after you approve one.",
            ["Options.Storage.Task.Remove.Failed"] = "The task could not be removed. Removing it needs administrator rights.",
            ["Options.Storage.ExplorerFailed"] = "Could not open Explorer: {0}",

            ["Options.About"] = "About",
            ["Options.About.Version"] = "Spotter {0}",
            ["Options.About.Text"] = "Instant file search for Windows. Spotter reads the NTFS master file table directly instead of walking folders, which is why it needs administrator rights and why results appear as fast as you can type.",
        };
}
