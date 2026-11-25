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
    // ---- window arrangement ---------------------------------------------------------------------
    //
    // All four are NaN until a run has something worth remembering. There is deliberately no
    // hard-coded default size here any more: the size the window should open at depends on the
    // screen it is opening on, so it is worked out at startup by WindowLayout.Default rather than
    // frozen into a number that was right for one monitor.

    public double WindowWidth { get; set; } = double.NaN;
    public double WindowHeight { get; set; } = double.NaN;
    public double WindowLeft { get; set; } = double.NaN;
    public double WindowTop { get; set; } = double.NaN;
    public bool Maximized { get; set; }

    /// <summary>
    /// What the four values above mean. Zero — which is what an older settings file deserializes
    /// to, and what <see cref="WindowLayout.Forget"/> writes — means "nothing worth restoring", so
    /// the window opens where <see cref="WindowLayout.Default"/> puts it.
    /// </summary>
    public int PlacementVersion { get; set; }

    /// <summary>
    /// Whether the size and position the user leaves the window at come back next time. On by
    /// default: moving a window is a decision, and having it moved back is the app overruling it.
    ///
    /// Turning it off is for the case where that decision is not one: a machine whose monitors come
    /// and go, where every launch should just start from the default.
    /// </summary>
    public bool RememberPlacement { get; set; } = true;

    // ---- interface size --------------------------------------------------------------------------

    /// <summary>
    /// Whether the size of the text and controls is worked out from the screen — see
    /// <see cref="UiScale"/> — or locked to <see cref="Scale"/>.
    /// </summary>
    public bool ScaleFollowsScreen { get; set; } = true;

    /// <summary>
    /// The locked scale, 1.0 being the size the layout was drawn at. Only consulted when
    /// <see cref="ScaleFollowsScreen"/> is off, and clamped on the way in — a settings file saying
    /// 40 must not produce a window with one visible button.
    /// </summary>
    public double Scale { get; set; } = 1.0;

    /// <summary>
    /// Column widths in display order. Only consulted when <see cref="AutoFitColumns"/> is off —
    /// they are still written either way, so turning auto-fit off by dragging an edge starts from
    /// what was on screen rather than from a stale layout.
    /// </summary>
    public List<double> ColumnWidths { get; set; } = new List<double>();

    /// <summary>
    /// Whether the seven columns share out the width of the results list between them instead of
    /// keeping fixed widths.
    ///
    /// On by default, and it has to be: fixed widths that add up to more than the window leave the
    /// last columns off the right edge, where — with no rows to scroll — there is nothing to scroll
    /// them into view with, so those columns simply cannot be seen. Dragging any column edge is
    /// taken as "I will do this myself" and turns it off; Reset all column widths turns it back on.
    /// </summary>
    public bool AutoFitColumns { get; set; } = true;

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

    /// <summary>
    /// Whether a dot, underscore or hyphen typed in the file name counts as a gap between words.
    /// On by default — see <see cref="SearchQuery.EffectiveFilename"/> for why that is the useful
    /// way round.
    /// </summary>
    public bool FlexibleSeparators { get; set; } = true;

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
    private static bool _loggedSecondarySkip;

    public void Save()
    {
        // An extra instance (--new-instance) reads this file and never writes it. Save replaces
        // the whole document, so the second window to close would otherwise roll back everything
        // the first one changed — its window geometry, its column widths, and any option the user
        // had just set in Options. One owner of the file, chosen the same way the show signal and
        // the hotkey are: whichever instance claimed the single-instance slot.
        if (App.SecondaryInstance)
        {
            if (!_loggedSecondarySkip)
            {
                _loggedSecondarySkip = true;
                Log.Write("not saving settings: this is an extra instance");
            }
            return;
        }

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
