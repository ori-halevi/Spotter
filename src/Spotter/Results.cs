using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace Spotter;

/// <summary>
/// One row. Wraps an engine record and formats it lazily — a whole-disk search can return
/// hundreds of thousands of rows and only the visible handful ever need their strings built.
///
/// Note the engine record is a live handle into the volume index, not a detached copy: the full
/// path is resolved by walking the parent chain, so the engine must stay alive while rows are
/// on screen.
/// </summary>
public sealed class FileItem
{
    private readonly GClass54 _record;
    private string _fullPath;
    private string _directory;
    private string _sizeText;

    public FileItem(GClass54 record)
    {
        _record = record;
    }

    public GClass54 Record => _record;

    public bool IsDirectory => _record.method_0();

    public string Name => _record.string_0;

    public string Directory => _directory ??= SafeCall(() => _record.method_8());

    public string FullPath => _fullPath ??= SafeCall(() => _record.method_6());

    // The three date fields are not in the order their names suggest. Verified against the
    // original FileView column getters (method_72/73/74): Modify is dateTime_1, Creation is
    // dateTime_0, Access is dateTime_2. Getting this wrong silently shows creation time under a
    // "Modified" header, which is exactly what happened here before it was checked.
    public DateTime Modified => _record.dateTime_1;

    public DateTime Created => _record.dateTime_0;

    public DateTime Accessed => _record.dateTime_2;

    public string Extension
    {
        get
        {
            string name = Name;
            if (IsDirectory || string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }
            int dot = name.LastIndexOf('.');
            return dot > 0 && dot < name.Length - 1 ? name.Substring(dot + 1) : string.Empty;
        }
    }

    /// <summary>
    /// True once the record is gone from the volume index — either because it was deleted from
    /// here, or because something else removed it after the search found it.
    ///
    /// Rows are struck through rather than removed. It looks dated, but it answers a question
    /// vanishing cannot: whether the file you were looking at is gone, or whether you simply
    /// lost it in the list.
    /// </summary>
    public bool IsDeleted
    {
        get
        {
            try { return _record.method_11(); }
            catch { return false; }
        }
    }

    /// <summary>Marks the record deleted in the engine's index.</summary>
    public void MarkDeleted()
    {
        try { _record.ginterface3_0?.imethod_13(_record); }
        catch { /* the volume may already have noticed via the USN journal */ }
    }

    /// <summary>
    /// Formatted size, for folders as well as files.
    ///
    /// The engine totals a folder's contents while it indexes: every file it reads adds its
    /// length to <c>long_0</c> on each of its ancestors (see <c>GClass153.method_6</c>). So a
    /// folder's size is already there for the asking — this used to render blank for folders and
    /// throw the whole thing away.
    ///
    /// Cached even for folders, whose total keeps growing until the last drive has been read:
    /// the growing figure still reaches the screen, because the only thing that makes WPF re-read
    /// this binding is a Reset — and a Reset goes through <see cref="ResultsView.Refresh"/>, which
    /// throws every wrapper away and builds a new one. Re-formatting on each render instead cost
    /// an allocation per visible folder row per redraw and changed nothing.
    /// </summary>
    public string SizeText => _sizeText ??= FormatSize();

    public long SizeBytes => _record.long_0;

    /// <summary>
    /// For a folder: how much is inside it, counted the same way the size is — everything below
    /// it, not just its immediate children. Null for files, which have no such thing.
    /// </summary>
    public string Contents
    {
        get
        {
            if (!IsDirectory)
            {
                return null;
            }
            int files = SafeCount(() => _record.method_1());
            int folders = SafeCount(() => _record.method_3());
            if (files < 0 && folders < 0)
            {
                return null;
            }
            return Localization.Lang.Tr("Contents.FilesAndFolders",
                Localization.Lang.Count("Plural.Files", Math.Max(0, files)),
                Localization.Lang.Count("Plural.Folders", Math.Max(0, folders)));
        }
    }

    private static int SafeCount(Func<int> f)
    {
        try { return f(); }
        catch { return -1; }
    }

    /// <summary>Shell icon for the file type. Resolved on demand and shared per extension.</summary>
    public System.Windows.Media.ImageSource Icon => ShellIcons.Get(Name, IsDirectory);

    /// <summary>
    /// Which way this row's name should be laid out, from the name's own first strong character —
    /// the rule HTML calls <c>dir="auto"</c>.
    ///
    /// The Name column is the only one whose content is really language rather than machine data,
    /// so it is the only one that cannot simply be pinned left-to-right. Under a right-to-left
    /// paragraph direction the bidi algorithm mirrors brackets and parentheses, so a Hebrew user
    /// would see "[2024] report.txt" as "]2024[ report.txt". Deciding per row means an English
    /// name and a Hebrew name both read correctly in the same list.
    /// </summary>
    public System.Windows.FlowDirection NameDirection => DirectionOf(Name);

    internal static System.Windows.FlowDirection DirectionOf(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            foreach (char c in text)
            {
                // Hebrew, Arabic and the presentation forms, i.e. everything that makes a name
                // right-to-left. Anything else strong enough to decide is Latin-like.
                if (c >= 0x0590 && c <= 0x08FF || c >= 0xFB1D && c <= 0xFEFC)
                {
                    return System.Windows.FlowDirection.RightToLeft;
                }
                if (char.IsLetter(c))
                {
                    return System.Windows.FlowDirection.LeftToRight;
                }
            }
        }
        // Digits, punctuation or nothing at all: no strong character to follow, and a path-like
        // name is far more likely than a right-to-left one.
        return System.Windows.FlowDirection.LeftToRight;
    }

    private string FormatSize()
    {
        long bytes = _record.long_0;
        if (bytes < 0)
        {
            return string.Empty;      // not counted yet, or not applicable
        }
        return Localization.Lang.FormatBytes(bytes);
    }

    private static string SafeCall(Func<string> f)
    {
        // Path resolution walks the live index; a record can go stale mid-search.
        try { return f() ?? string.Empty; }
        catch { return string.Empty; }
    }

    // Identity follows the underlying record, so a row keeps its selection even if the display
    // cache evicts and rebuilds the wrapper.
    public override bool Equals(object obj) => obj is FileItem other && ReferenceEquals(other._record, _record);

    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(_record);
}

/// <summary>
/// The stable identity of a result column: <c>local:ColumnKey.Value="Size"</c> in the GridView.
///
/// It exists because the column's <c>Header</c> cannot be an identifier once it is translated. The
/// header carries the display name *and* the sort arrow — "Size ▼", "גודל ▲" — so the code that
/// used to read it back to decide how to sort would, in any language but English, look up a
/// comparison that does not exist and quietly stop sorting.
///
/// An attached property rather than the column's position, because <c>GridView</c> lets the user
/// drag columns into a different order.
/// </summary>
public static class ColumnKey
{
    public static readonly System.Windows.DependencyProperty ValueProperty =
        System.Windows.DependencyProperty.RegisterAttached(
            "Value", typeof(string), typeof(ColumnKey),
            new System.Windows.PropertyMetadata(null));

    public static void SetValue(System.Windows.DependencyObject element, string value) =>
        element.SetValue(ValueProperty, value);

    public static string GetValue(System.Windows.DependencyObject element) =>
        (string)element.GetValue(ValueProperty);
}

/// <summary>
/// Flips the sign of a double. Used to slide the column headers by the scroll offset: the
/// headers sit outside the scrolling area now, so they have to be moved rather than scrolled.
/// </summary>
public sealed class NegateConverter : System.Windows.Data.IValueConverter
{
    public object Convert(object value, Type targetType, object parameter,
                          System.Globalization.CultureInfo culture) =>
        value is double d ? -d : 0d;

    public object ConvertBack(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// The results list WPF binds to.
///
/// Two things it deliberately does NOT do:
///
/// 1. It is not an ObservableCollection. Results stream in from engine worker threads by the
///    hundred thousand; one change notification per row would bury the dispatcher. Rows are
///    appended in batches and a single Reset is raised per batch.
/// 2. It does not materialise a wrapper per row. Wrappers are built on demand by the indexer —
///    which is all WPF's UI virtualization ever asks for — and held in a bounded cache. The
///    1.5.1 app cached one ListViewItem per row forever, so scrolling a million-row result set
///    held a million of them; this keeps a fixed ceiling.
/// </summary>
public sealed class ResultsView : IList, INotifyCollectionChanged
{
    private const int CacheLimit = 4096;

    private readonly List<GClass54> _rows = new List<GClass54>();
    private readonly Dictionary<int, FileItem> _cache = new Dictionary<int, FileItem>();
    private readonly Queue<int> _cacheOrder = new Queue<int>();

    /// <summary>
    /// Record → row index, built on first use and dropped whenever the order changes.
    ///
    /// WPF asks an ItemsSource for <see cref="IndexOf"/> constantly once a selection exists — and
    /// a List.IndexOf over a few hundred thousand rows is a linear scan each time, which turns
    /// ordinary selection work quadratic. Built lazily because a streaming search has no
    /// selection yet, so the map is never paid for until someone actually clicks something.
    /// </summary>
    private Dictionary<GClass54, int> _positions;

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    /// <summary>
    /// How many rows the UI has been told about. Rows appended between publishes are held back
    /// from <see cref="Count"/> and the indexer.
    ///
    /// WPF's ItemContainerGenerator tracks (count at last Reset + adds - removes) and throws
    /// "An ItemsControl is inconsistent with its items source" the moment Count disagrees. So
    /// batching notifications is only legal if the collection also *appears* not to have grown
    /// yet — reporting the true row count while sitting on unannounced rows is what breaks it.
    /// </summary>
    private int _publishedCount;

    public int Count => _publishedCount;

    public bool IsFixedSize => false;
    public bool IsReadOnly => true;
    public bool IsSynchronized => false;
    public object SyncRoot => this;

    public object this[int index]
    {
        get
        {
            if (index < 0 || index >= _publishedCount)
            {
                return null;
            }
            if (_cache.TryGetValue(index, out FileItem cached))
            {
                return cached;
            }
            FileItem item = new FileItem(_rows[index]);
            _cache[index] = item;
            _cacheOrder.Enqueue(index);
            if (_cacheOrder.Count > CacheLimit)
            {
                _cache.Remove(_cacheOrder.Dequeue());
            }
            return item;
        }
        set => throw new NotSupportedException();
    }

    /// <summary>
    /// Appends a batch. UI thread only.
    ///
    /// <paramref name="notify"/> exists because a Reset notification is expensive — WPF drops
    /// every realized container and re-measures — while appending to the backing list is nearly
    /// free. During a streaming search the rows go in silently and the caller notifies on its
    /// own, slower cadence.
    /// </summary>
    public void AddRange(List<GClass54> batch, bool notify = true)
    {
        if (batch == null || batch.Count == 0)
        {
            return;
        }
        _rows.AddRange(batch);
        _positions = null;
        if (notify)
        {
            NotifyChanged();
        }
    }

    /// <summary>
    /// Makes every row appended since the last publish visible to the UI, in one notification.
    /// </summary>
    public void NotifyChanged()
    {
        _publishedCount = _rows.Count;
        _positions = null;      // the map is keyed to a published count that just changed
        Raise();
    }

    private sealed class RecordIdentity : IEqualityComparer<GClass54>
    {
        public static readonly RecordIdentity Instance = new RecordIdentity();

        public bool Equals(GClass54 a, GClass54 b) => ReferenceEquals(a, b);

        public int GetHashCode(GClass54 record) =>
            System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(record);
    }

    public void Clear()
    {
        _rows.Clear();
        _publishedCount = 0;
        _cache.Clear();
        _cacheOrder.Clear();
        _positions = null;
        Raise();
    }

    /// <summary>
    /// Sorts in place using an engine-level comparison, then invalidates the display cache.
    /// </summary>
    public void Sort(Comparison<GClass54> comparison)
    {
        _rows.Sort(comparison);
        _publishedCount = _rows.Count;
        _cache.Clear();
        _cacheOrder.Clear();
        _positions = null;
        Raise();
    }

    /// <summary>
    /// Redraws the visible rows without changing what is in the list — used after a delete, so
    /// the struck-through state appears while scroll position and selection stay put.
    /// </summary>
    public void Refresh()
    {
        _cache.Clear();
        _cacheOrder.Clear();
        Raise();
    }

    public GClass54 RecordAt(int index) =>
        index >= 0 && index < _publishedCount ? _rows[index] : null;

    /// <summary>
    /// Every published record, copied out without going through the indexer.
    ///
    /// This exists for export. Walking the indexer instead pulls all several hundred thousand
    /// rows through a 4,096-entry cache, evicting the rows actually on screen — so the grid had
    /// to rebuild every visible wrapper the moment the export finished.
    /// </summary>
    public List<GClass54> SnapshotRecords() => _rows.GetRange(0, _publishedCount);

    private void Raise() =>
        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

    public IEnumerator GetEnumerator()
    {
        for (int i = 0; i < _publishedCount; i++)
        {
            yield return this[i];
        }
    }

    /// <summary>
    /// Position of a row, or -1.
    ///
    /// Only published rows count. Reporting an index for a row appended but not yet announced
    /// would hand WPF a position past <see cref="Count"/>, which is exactly the inconsistency
    /// <see cref="_publishedCount"/> exists to prevent.
    /// </summary>
    public int IndexOf(object value)
    {
        if (value is not FileItem item)
        {
            return -1;
        }

        if (_positions == null)
        {
            // Identity, matching FileItem.Equals — two distinct records are never "the same row"
            // even if the engine ever gave them value equality.
            _positions = new Dictionary<GClass54, int>(_publishedCount, RecordIdentity.Instance);
            for (int i = 0; i < _publishedCount; i++)
            {
                _positions[_rows[i]] = i;
            }
        }

        return _positions.TryGetValue(item.Record, out int index) && index < _publishedCount
            ? index
            : -1;
    }

    public bool Contains(object value) => IndexOf(value) >= 0;

    public void CopyTo(Array array, int index)
    {
        for (int i = 0; i < _publishedCount; i++)
        {
            array.SetValue(this[i], index + i);
        }
    }

    public int Add(object value) => throw new NotSupportedException();
    public void Insert(int index, object value) => throw new NotSupportedException();
    public void Remove(object value) => throw new NotSupportedException();
    public void RemoveAt(int index) => throw new NotSupportedException();
}
