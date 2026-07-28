using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace Spotter;

/// <summary>
/// Everything the UI needs from the 2014 engine, behind a small modern surface.
///
/// The important part is how results are moved across threads. The engine calls back
/// fire-and-forget on its own workers and never marshals; dispatching each result individually
/// would drown the UI thread on a query like "*.dll" (309,000 hits in a quarter of a second).
/// So results are pushed onto a concurrent queue and drained on a timer, which is the same
/// shape the WinForms app used and the reason it feels instant.
/// </summary>
public sealed class SearchEngine : IDisposable
{
    private readonly GClass134 _engine = new GClass134();
    private readonly ConcurrentQueue<GClass54> _incoming = new ConcurrentQueue<GClass54>();
    private readonly DispatcherTimer _drain;
    private readonly ResultsView _results;

    /// <summary>
    /// The UI thread, captured at construction. The engine raises its own callbacks on worker
    /// threads, so anything this class re-raises as an event has to be put back on this thread
    /// first — see <see cref="BeginIndexing"/>.
    /// </summary>
    private readonly Dispatcher _ui = Dispatcher.CurrentDispatcher;

    private GClass244 _activeSearch;
    private int _searchGeneration;
    private Stopwatch _searchClock;

    /// <summary>Raised on the UI thread after each batch lands, and once when a search ends.</summary>
    public event Action<SearchStatus> StatusChanged;

    public SearchEngine(ResultsView results)
    {
        _results = results;
        _drain = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(30)
        };
        _drain.Tick += (_, __) => Drain();
        _drain.Start();
    }

    public bool IsIndexed { get; private set; }

    public string[] Volumes { get; private set; } = Array.Empty<string>();

    /// <summary>Raised on the UI thread as each group of volumes finishes caching.</summary>
    public event Action VolumesChanged;

    /// <summary>Raised on the UI thread once every volume is cached.</summary>
    public event Action<TimeSpan> IndexingFinished;

    /// <summary>
    /// Starts reading every volume's file table and returns immediately.
    ///
    /// Crucially it does NOT wait: the engine answers queries against whatever is already
    /// cached, exactly as the 1.5.1 app does. Awaiting full completion — which is what this used
    /// to do — left the user typing into a dead window for the best part of a minute.
    /// </summary>
    public void BeginIndexing()
    {
        Stopwatch clock = Stopwatch.StartNew();

        // Both callbacks arrive on engine worker threads. Raising the events straight from there
        // looked like it worked — the first statement of a handler runs — and then died silently
        // on the handler's first touch of a control, because the engine swallows exceptions its
        // callbacks throw. Symptom: no "Ready — N drives", no indexing line in the log, and no
        // error anywhere. Hence the hop back to the UI thread here rather than in each handler.
        _engine.method_6(() =>
        {
            // A group of drives came online; whatever is on screen is now incomplete.
            Volumes = _engine.method_20()?.ToArray() ?? Array.Empty<string>();
            _ui.BeginInvoke(new Action(() => VolumesChanged?.Invoke()));
        });

        Action onAllCached = null;
        onAllCached = () =>
        {
            _engine.method_5(onAllCached);          // unsubscribe
            IsIndexed = true;
            Volumes = _engine.method_20()?.ToArray() ?? Array.Empty<string>();
            TimeSpan elapsed = clock.Elapsed;
            _ui.BeginInvoke(new Action(() => IndexingFinished?.Invoke(elapsed)));
        };
        _engine.method_4(onAllCached);
        _engine.method_8();
    }

    // ---- indexing progress -----------------------------------------------------------------
    //
    // GClass134.method_22() yields one (bytesRead, total) pair per volume, in the same order as
    // method_20() yields their names. A negative total is a state rather than a size; the values
    // are the ones ReadProgress in the 1.5.1 app switched on.

    private const long StateUnknownSize = -1;
    private const long StateWaiting = -2;
    private const long StateNotSupported = -3;
    private const long StateFinished = -4;
    private const long StateError = -5;

    // A volume that finishes reports -4, which throws away the size it was reporting a moment
    // earlier. Without remembering it, the total shrinks as drives complete and the percentage
    // runs backwards. These keep the last real numbers each volume gave.
    private long[] _knownTotal = Array.Empty<long>();
    private long[] _knownRead = Array.Empty<long>();

    /// <summary>
    /// A snapshot of how far reading the volumes has got. Cheap enough to poll — it walks one
    /// entry per volume and touches no disk.
    /// </summary>
    public IndexProgress GetIndexProgress()
    {
        List<GStruct0<long>> raw;
        try
        {
            IEnumerable<GStruct0<long>> live = _engine.method_22();
            if (live == null)
            {
                return default;
            }
            raw = live.ToList();
        }
        catch
        {
            return default;          // the volume array is rebuilt during startup
        }

        if (_knownTotal.Length < raw.Count)
        {
            Array.Resize(ref _knownTotal, raw.Count);
            Array.Resize(ref _knownRead, raw.Count);
        }

        // Read the names here rather than using the cached Volumes: that field is only refreshed
        // when a group of drives finishes, so early on it is empty and every drive in the
        // breakdown showed as "?". method_20() walks the same array as method_22(), so the two
        // line up index for index whatever state the drives are in.
        string[] names;
        try { names = _engine.method_20()?.ToArray() ?? Array.Empty<string>(); }
        catch { names = Array.Empty<string>(); }

        List<VolumeProgress> volumes = new List<VolumeProgress>(raw.Count);
        long read = 0;
        long total = 0;
        int done = 0;

        for (int i = 0; i < raw.Count; i++)
        {
            long reported = raw[i].gparam_0;
            long size = raw[i].gparam_1;
            string state = null;

            if (size >= 0)
            {
                _knownTotal[i] = size;
                _knownRead[i] = reported;
                if (reported >= size && size > 0)
                {
                    done++;
                }
            }
            else
            {
                switch (size)
                {
                    case StateFinished:
                        // Completed: charge it its full remembered size so the total stays put.
                        _knownRead[i] = _knownTotal[i];
                        done++;
                        break;
                    // Translation keys, not text. This runs below the UI and has no business
                    // knowing what language the window is in; MainWindow.DescribeVolumes resolves
                    // them when it draws the tooltip.
                    case StateError:
                        state = "Index.Volume.Failed";
                        _knownRead[i] = _knownTotal[i] = 0;
                        done++;
                        break;
                    case StateNotSupported:
                        state = "Index.Volume.NotSupported";
                        _knownRead[i] = _knownTotal[i] = 0;
                        done++;
                        break;
                    case StateWaiting:
                        state = "Index.Volume.Waiting";
                        break;
                    case StateUnknownSize:
                        state = "Index.Volume.SizeUnknown";
                        _knownRead[i] = reported;
                        break;
                }
            }

            read += _knownRead[i];
            total += _knownTotal[i];
            volumes.Add(new VolumeProgress(
                i < names.Length ? names[i] : "?",
                _knownRead[i], _knownTotal[i], state));
        }

        return new IndexProgress(read, total, done, raw.Count, volumes);
    }

    /// <summary>
    /// Starts a search, cancelling any previous one. Results arrive asynchronously; watch
    /// <see cref="StatusChanged"/>.
    /// </summary>
    public void Search(SearchQuery query)
    {
        CancelSearch();
        _results.Clear();

        int generation = Interlocked.Increment(ref _searchGeneration);
        Stopwatch clock = _searchClock = Stopwatch.StartNew();
        Volatile.Write(ref _engineElapsedTicks, 0L);
        _batchCount = 0;
        _pendingRows = false;
        _lastPublishUtc = DateTime.MinValue;

        if (query.IsEmpty)
        {
            StatusChanged?.Invoke(new SearchStatus(0, TimeSpan.Zero, true));
            return;
        }

        GClass282 compiled = query.Compile();
        _activeSearch = _engine.method_17(
            compiled,
            false,
            record =>
            {
                if (Volatile.Read(ref _searchGeneration) == generation)
                {
                    _incoming.Enqueue(record);
                }
            },
            () =>
            {
                if (Volatile.Read(ref _searchGeneration) == generation)
                {
                    // Stamp the moment the ENGINE finished, separately from when the UI has
                    // finished drawing it. Without both numbers there is no way to tell whether
                    // a slow search is the engine's fault or ours.
                    //
                    // `clock` is the local captured above rather than the field: by the time this
                    // runs the field may already belong to the next search. The generation guard
                    // makes that harmless, but reading the right stopwatch is simpler than
                    // reasoning about why reading the wrong one does not matter.
                    //
                    // Elapsed is published BEFORE the generation, so a Drain that sees the search
                    // completed is guaranteed to see the timing that goes with it.
                    Volatile.Write(ref _engineElapsedTicks, clock.Elapsed.Ticks);
                    Volatile.Write(ref _completedGeneration, generation);
                }
            });
    }

    // Both are written on an engine worker thread and read on the UI thread, so both go through
    // Volatile: without it the JIT is free to keep the UI thread's view in a register across
    // Drain ticks, and a completed search would never be noticed.
    private int _completedGeneration = -1;
    private long _engineElapsedTicks;
    private int _batchCount;

    /// <summary>How long the engine itself took on the last completed search.</summary>
    public TimeSpan LastEngineElapsed =>
        TimeSpan.FromTicks(Volatile.Read(ref _engineElapsedTicks));

    /// <summary>How many drain batches the last search took to reach the screen.</summary>
    public int LastBatchCount => _batchCount;

    public void CancelSearch()
    {
        Interlocked.Increment(ref _searchGeneration);
        try { _activeSearch?.method_8(); }
        catch { /* already finished */ }
        _activeSearch = null;
        while (_incoming.TryDequeue(out _)) { }
    }

    /// <summary>
    /// Abandons a running search but keeps what it has already found.
    ///
    /// Unlike <see cref="CancelSearch"/>, which exists to clear the way for the next query, this
    /// is a user-visible end to the search: rows that arrived but had not yet been published are
    /// flushed to the screen first, and a completed status is raised so the UI stops looking busy.
    /// A stop that silently discarded the last partial batch would be indistinguishable from a
    /// bug.
    /// </summary>
    public void StopSearch()
    {
        CancelSearch();
        _searchClock?.Stop();

        if (_pendingRows)
        {
            _results.NotifyChanged();
            _pendingRows = false;
        }
        Volatile.Write(ref _completedGeneration, -1);

        StatusChanged?.Invoke(new SearchStatus(
            _results.Count, _searchClock?.Elapsed ?? TimeSpan.Zero, true));
    }

    /// <summary>
    /// How often the grid is allowed to actually redraw while results stream in.
    ///
    /// The drain itself runs every 30 ms to keep the queue short, but each published change is a
    /// Reset, and a Reset makes WPF throw away every realized row container and re-measure. Doing
    /// that 30 times a second was costing more than the search itself. Publishing at ~8 Hz still
    /// reads as live and removes most of the cost.
    /// </summary>
    private static readonly TimeSpan PublishInterval = TimeSpan.FromMilliseconds(125);

    private DateTime _lastPublishUtc = DateTime.MinValue;
    private bool _pendingRows;

    private void Drain()
    {
        List<GClass54> batch = null;
        while (_incoming.TryDequeue(out GClass54 record))
        {
            (batch ??= new List<GClass54>(4096)).Add(record);
            if (batch.Count >= 50000)
            {
                break;   // keep any single tick short so the UI stays responsive
            }
        }

        bool finished = Volatile.Read(ref _completedGeneration) == Volatile.Read(ref _searchGeneration)
                        && _incoming.IsEmpty;

        if (batch != null)
        {
            _results.AddRange(batch, notify: false);
            _pendingRows = true;
            _batchCount++;
        }

        if (!_pendingRows && !finished)
        {
            return;
        }

        DateTime now = DateTime.UtcNow;
        bool due = finished || now - _lastPublishUtc >= PublishInterval;
        if (!due)
        {
            return;
        }

        if (_pendingRows)
        {
            _results.NotifyChanged();
            _pendingRows = false;
        }
        _lastPublishUtc = now;

        if (finished)
        {
            Volatile.Write(ref _completedGeneration, -1);
        }
        StatusChanged?.Invoke(new SearchStatus(
            _results.Count,
            _searchClock?.Elapsed ?? TimeSpan.Zero,
            finished));
    }

    public void Dispose()
    {
        _drain.Stop();
        CancelSearch();
        try { _engine.Dispose(); } catch { }
    }
}

/// <summary>How far one volume has got. <see cref="State"/> is null while it is reading normally.</summary>
public readonly struct VolumeProgress
{
    // State carries a translation key ("Index.Volume.Failed"), not a phrase, so that this type
    // stays below the UI and the tooltip can be drawn in whatever language is current.
    public VolumeProgress(string name, long bytesRead, long bytesTotal, string state)
    {
        Name = name;
        BytesRead = bytesRead;
        BytesTotal = bytesTotal;
        State = state;
    }

    public string Name { get; }
    public long BytesRead { get; }
    public long BytesTotal { get; }
    public string State { get; }

    public double Fraction => BytesTotal > 0
        ? Math.Min(1.0, (double)BytesRead / BytesTotal)
        : 0;
}

/// <summary>Aggregate indexing progress across every volume.</summary>
public readonly struct IndexProgress
{
    public IndexProgress(long bytesRead, long bytesTotal, int volumesDone, int volumesTotal,
                         IReadOnlyList<VolumeProgress> volumes)
    {
        BytesRead = bytesRead;
        BytesTotal = bytesTotal;
        VolumesDone = volumesDone;
        VolumesTotal = volumesTotal;
        Volumes = volumes;
    }

    public long BytesRead { get; }
    public long BytesTotal { get; }
    public int VolumesDone { get; }
    public int VolumesTotal { get; }
    public IReadOnlyList<VolumeProgress> Volumes { get; }

    /// <summary>True once there is enough information to show a real percentage.</summary>
    public bool IsMeasurable => BytesTotal > 0;

    public double Fraction => IsMeasurable
        ? Math.Min(1.0, (double)BytesRead / BytesTotal)
        : 0;
}

public readonly struct SearchStatus
{
    public SearchStatus(int count, TimeSpan elapsed, bool completed)
    {
        Count = count;
        Elapsed = elapsed;
        Completed = completed;
    }

    public int Count { get; }
    public TimeSpan Elapsed { get; }
    public bool Completed { get; }
}

/// <summary>
/// Which field the engine could not make sense of, and why.
///
/// The three field messages come from the engine's own parser, which names the character that
/// failed and is more useful than anything this app could say instead, so they are passed through
/// untranslated. <see cref="Kinds"/> is ours, so it is a translation key.
/// </summary>
public sealed class QueryProblems
{
    public string Filename { get; set; }
    public string Directory { get; set; }
    public string Size { get; set; }

    /// <summary>
    /// Translation key, set when neither files nor folders are included — which can only find
    /// nothing.
    /// </summary>
    public string Kinds { get; set; }

    public bool Any => Filename != null || Directory != null || Size != null || Kinds != null;

    public string First => Filename ?? Directory ?? Size ?? Kinds;
}

/// <summary>What the user typed, before it becomes an engine query object.</summary>
public sealed class SearchQuery
{
    public string Filename { get; set; } = string.Empty;
    public string Directory { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;

    // The engine keeps case and regex separately for the name and the folder, and so does the
    // 1.5.1 UI — each field has its own options button. Tying both to one pair of checkboxes,
    // which is what this used to do, threw that away: you could not search for a case-sensitive
    // name inside a folder matched loosely, or use a regular expression for the folder while
    // typing an ordinary word for the name.
    public bool FilenameIgnoreCase { get; set; } = true;
    public bool FilenameRegex { get; set; }
    public bool DirectoryIgnoreCase { get; set; } = true;
    public bool DirectoryRegex { get; set; }

    public bool IncludeSubdirectories { get; set; } = true;
    public bool IncludeFiles { get; set; } = true;
    public bool IncludeDirectories { get; set; } = true;

    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(Filename)
        && string.IsNullOrWhiteSpace(Directory)
        && string.IsNullOrWhiteSpace(Size);

    public GClass282 Compile()
    {
        // Every string has to be set even when blank: the engine compiles them with
        // string.Split and does not guard against null, and the resulting
        // NullReferenceException is swallowed inside the search worker - it surfaces only as
        // "0 results", which is a genuinely nasty thing to debug.
        GClass282 q = new GClass282();
        q.method_1(Filename ?? string.Empty);
        q.method_3(FilenameIgnoreCase);
        q.method_5(FilenameRegex);
        q.method_7(Directory ?? string.Empty);
        q.method_9(DirectoryIgnoreCase);
        q.method_11(DirectoryRegex);
        q.method_13(IncludeSubdirectories);
        q.method_15(Size ?? string.Empty);
        q.method_23(IncludeFiles);
        q.method_25(IncludeDirectories);
        return q;
    }

    /// <summary>
    /// Builds each matcher up front to find out which field, if any, the engine cannot parse.
    ///
    /// This has to happen here rather than being left to the search: the engine builds its
    /// matchers on a worker thread and swallows what they throw, so a bad regular expression or
    /// an unparseable size reaches the user as "0 results" — the same thing a perfectly good
    /// search that matched nothing reports. The 1.5.1 app checked all three the same way and
    /// coloured the offending box.
    /// </summary>
    public QueryProblems Check()
    {
        GClass282 q = Compile();
        QueryProblems problems = new QueryProblems();

        try { q.method_26(); }
        catch (Exception ex) { problems.Filename = Explain(ex); }

        try { q.method_19(); }
        catch (Exception ex) { problems.Directory = Explain(ex); }

        try { q.method_27(); }
        catch (Exception ex) { problems.Size = Explain(ex); }

        if (!IncludeFiles && !IncludeDirectories)
        {
            // A key, resolved by whoever shows it. See the volume states above.
            problems.Kinds = "Problem.NoKinds";
        }
        return problems;
    }

    /// <summary>
    /// Engine messages are written for a log, not a status bar. The parser's own text is kept
    /// when it says something useful about where the input went wrong, and dropped when it does
    /// not.
    /// </summary>
    private static string Explain(Exception ex)
    {
        // The engine's own parser message when it has one — it names the character that failed, and
        // is worth more than a translated generality. Only the fallback is ours to translate.
        string message = ex.Message ?? Localization.Lang.Tr("Problem.Unreadable");
        int newline = message.IndexOfAny(new[] { '\r', '\n' });
        if (newline > 0)
        {
            message = message.Substring(0, newline);
        }
        return message.Trim();
    }
}
