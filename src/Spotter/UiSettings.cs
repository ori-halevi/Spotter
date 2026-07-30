using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Spotter;

/// <summary>
/// Window geometry, column layout, sort and search options, persisted between runs.
///
/// Stored beside the 1.5.1 app's own options file so everything Spotter keeps lives in one
/// place, but in a separate file — the old XML has a different shape and is still read by the
/// WinForms build.
///
/// Every load is best-effort: a corrupt or partial file falls back to defaults rather than
/// stopping the app from starting.
/// </summary>
public sealed class UiSettings
{
    public double WindowWidth { get; set; } = 1200;
    public double WindowHeight { get; set; } = 720;
    public double WindowLeft { get; set; } = double.NaN;
    public double WindowTop { get; set; } = double.NaN;
    public bool Maximized { get; set; }

    /// <summary>Column widths in display order. Empty means "use the XAML defaults".</summary>
    public List<double> ColumnWidths { get; set; } = new List<double>();

    public string SortColumn { get; set; }
    public bool SortDescending { get; set; }

    /// <summary>Global restore hotkey, e.g. "Ctrl+Alt+F". Blank disables it.</summary>
    public string RestoreHotkey { get; set; } = "Ctrl+Alt+F";

    /// <summary>Auto (follow Windows), Dark or Light.</summary>
    public string Theme { get; set; } = "Auto";

    /// <summary>
    /// Two-letter code of the UI language, or blank for "follow the Windows display language".
    ///
    /// Blank is the default and stays blank until someone picks a language explicitly, so that
    /// changing the Windows display language still moves the app with it. Writing the detected
    /// code in here at first run would silently pin the app to whatever Windows happened to be
    /// set to that day.
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Off, which is what 1.5.1 defaults to — see <c>GClass283.method_6</c>, the original's own
    /// defaults initializer: tray icon false, minimize-to-tray true, close-to-tray false.
    ///
    /// It matters more than a default usually does. With a tray icon the window can leave the
    /// screen while the process keeps running, and this process holds a raw handle on every
    /// volume, which is enough to block safely removing a drive. Closing should mean closed
    /// unless someone has asked for otherwise.
    /// </summary>
    public bool ShowTrayIcon { get; set; }

    /// <summary>Only has any effect when <see cref="ShowTrayIcon"/> is on.</summary>
    public bool MinimizeToTray { get; set; } = true;

    public bool CloseToTray { get; set; }

    // Case and regex are per field, as they are in the engine. MatchCase/Regex keep their old
    // names and now mean the file-name field, so a settings file written by an earlier build
    // still loads and still means what it looks like it means.
    public bool MatchCase { get; set; }
    public bool Regex { get; set; }
    public bool FolderMatchCase { get; set; }
    public bool FolderRegex { get; set; }

    public bool Subfolders { get; set; } = true;
    public bool IncludeFiles { get; set; } = true;
    public bool IncludeFolders { get; set; } = true;

    /// <summary>
    /// The 1.5.1 "Search On-Demand" option, under the name its tooltip actually described:
    /// "auto-start new search on each letter typed". Off means nothing runs until Search is
    /// pressed.
    /// </summary>
    public bool SearchAsYouType { get; set; } = true;

    /// <summary>
    /// Recent file-name queries, most recent first. Capped at <see cref="HistoryLimit"/> — this
    /// is a convenience, not an archive, and an unbounded list would grow forever in a file that
    /// is read synchronously at startup.
    /// </summary>
    public List<string> RecentSearches { get; set; } = new List<string>();

    public const int HistoryLimit = 25;

    /// <summary>
    /// Records a committed query at the head of the history.
    ///
    /// Only committed terms get here — see <c>MainWindow.CommitHistory</c> — but a term can still
    /// be committed while it is being refined (moving focus, then coming back to type more). So
    /// an entry the new term merely extends is dropped: searching for "notepad" leaves "notepad",
    /// not "note", "notep", "notepa", "notepad".
    /// </summary>
    public void RememberSearch(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return;
        }
        term = term.Trim();

        RecentSearches.RemoveAll(t => string.Equals(t, term, StringComparison.OrdinalIgnoreCase));
        if (RecentSearches.Count > 0
            && term.StartsWith(RecentSearches[0], StringComparison.OrdinalIgnoreCase))
        {
            RecentSearches.RemoveAt(0);
        }

        RecentSearches.Insert(0, term);
        if (RecentSearches.Count > HistoryLimit)
        {
            RecentSearches.RemoveRange(HistoryLimit, RecentSearches.Count - HistoryLimit);
        }
    }

    /// <summary>
    /// Where the settings live. Public because the app should be able to tell the user where its
    /// files are rather than leaving them to find out.
    /// </summary>
    public static string FilePath => System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Spotter", "settings.xml");

    /// <summary>
    /// The elevation handover means two Spotter processes briefly overlap — the launcher and the
    /// elevated copy it starts, plus sometimes an old instance still writing its settings on the
    /// way out. Their reads and writes of this one file collide for a few milliseconds, which is
    /// long enough to matter and short enough that waiting it out always works.
    /// </summary>
    private const int IoAttempts = 10;
    private const int IoRetryDelayMs = 100;

    /// <summary>
    /// Set when the file existed but stayed unreadable through every retry. Serialized never —
    /// XmlSerializer only looks at public members — and checked by <see cref="Save"/>: a settings
    /// object that could not read the file must not overwrite it.
    /// </summary>
    private bool _unreadable;

    public static UiSettings Load()
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    // A missing file is a first run — unless another instance is mid-save, which
                    // leaves its .tmp visible while File.Replace has the real file in flux. That
                    // instant used to be read as "no settings", and the defaults it produced were
                    // later saved over the user's real file: the language, and everything else,
                    // reset on the next launch with nothing in the log.
                    if (!File.Exists(FilePath + ".tmp"))
                    {
                        return new UiSettings();
                    }
                    throw new IOException("another instance is replacing the settings file");
                }
                using FileStream stream = File.OpenRead(FilePath);
                if (new XmlSerializer(typeof(UiSettings)).Deserialize(stream) is UiSettings loaded)
                {
                    return loaded;
                }
                return new UiSettings();
            }
            catch (IOException) when (attempt < IoAttempts)
            {
                System.Threading.Thread.Sleep(IoRetryDelayMs);
            }
            catch (IOException ex)
            {
                if (!File.Exists(FilePath))
                {
                    // The .tmp that kept us retrying is a leftover from a crashed save, not a
                    // save in progress — there is no real file here to protect, so these are
                    // ordinary first-run defaults and saving over the leftover is fine.
                    return new UiSettings();
                }

                // Still locked after every retry. Run on defaults, but remember that a real file
                // is sitting there unread — Save must not replace it with these defaults.
                Log.Write("could not read settings, running on defaults for this session: "
                          + ex.Message);
                return new UiSettings { _unreadable = true };
            }
            catch (Exception ex)
            {
                // Corrupt or unparseable. Defaults are the only option, and overwriting a file
                // that cannot be parsed loses nothing.
                Log.Write("could not load settings, using defaults: " + ex.Message);
                return new UiSettings();
            }
        }
    }

    /// <summary>
    /// Writes the settings out, replacing the previous file only once the new one is complete.
    ///
    /// Writing in place — File.Create truncates, then serializes — meant a crash or a shutdown
    /// mid-write left a half-written file. The next Load could not parse it, fell back to
    /// defaults, and every setting plus the whole search history was gone with nothing said. The
    /// window is small but the loss is total and silent, which is the worst combination.
    /// </summary>
    public void Save()
    {
        if (_unreadable)
        {
            // These settings are defaults standing in for a file that could not be read. Writing
            // them out is exactly the bug this guards against: every stored setting replaced by
            // its default because of one transient lock at startup.
            Log.Write("not saving settings: the existing file was unreadable at startup and "
                      + "would be overwritten with defaults");
            return;
        }

        for (int attempt = 1; ; attempt++)
        {
            try
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FilePath));

                string temporary = FilePath + ".tmp";
                using (FileStream stream = File.Create(temporary))
                {
                    new XmlSerializer(typeof(UiSettings)).Serialize(stream, this);
                    stream.Flush(flushToDisk: true);
                }

                if (File.Exists(FilePath))
                {
                    // Replace is atomic where the filesystem can manage it, and keeps the old file
                    // until the new one is in place.
                    File.Replace(temporary, FilePath, destinationBackupFileName: null);
                }
                else
                {
                    File.Move(temporary, FilePath);
                }
                return;
            }
            catch (IOException) when (attempt < IoAttempts)
            {
                // Another instance is reading or replacing the file right now — the same
                // milliseconds-long overlap Load waits out.
                System.Threading.Thread.Sleep(IoRetryDelayMs);
            }
            catch (Exception ex)
            {
                Log.Write("could not save settings: " + ex.Message);
                return;
            }
        }
    }
}
