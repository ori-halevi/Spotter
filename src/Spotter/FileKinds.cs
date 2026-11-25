using System;
using System.Collections.Generic;
using System.Text;

namespace Spotter;

/// <summary>
/// The buckets the file-type filter offers, in the order they are listed in the panel.
///
/// Deliberately coarse. The point of the filter is to answer "show me the videos", not to be a
/// complete taxonomy of file formats — a list long enough to be exhaustive is a list nobody reads,
/// and the custom-extension box below it is the escape hatch for everything a fixed list cannot
/// anticipate.
/// </summary>
public enum FileKind
{
    Image,
    Video,
    Audio,
    Document,
    Archive,
    Code,
    Program,

    /// <summary>
    /// Everything no other bucket claims, including files with no extension at all.
    ///
    /// It exists so that the buckets cover every file rather than a chosen subset: without it,
    /// turning on every category would still quietly drop files whose extension nobody thought of,
    /// and the label would read "all types" while hiding results. With it, "all on" genuinely
    /// means all.
    /// </summary>
    Other,
}

/// <summary>
/// Which extensions belong to which <see cref="FileKind"/>.
///
/// A single dictionary from extension to a <em>bitmask</em> of kinds, for two reasons.
///
/// The lookup runs once per record arriving from the engine — millions of times on a whole-disk
/// search — so it has to be one hash probe and an integer AND, not a walk over eight collections.
///
/// And a mask rather than one kind per extension because an extension can genuinely belong to two
/// worlds. <c>.ts</c> and <c>.mts</c> are the ones that do here: TypeScript sources and MPEG
/// transport streams, and both are listed under video and under code. Forcing such an extension
/// into one bucket means picking which half of the users get a filter that hides exactly what they
/// are looking for, and the only honest answer is that it is in both.
/// </summary>
public static class FileKinds
{
    public static readonly FileKind[] All =
    {
        FileKind.Image, FileKind.Video, FileKind.Audio, FileKind.Document,
        FileKind.Archive, FileKind.Code, FileKind.Program, FileKind.Other,
    };

    /// <summary>The translation key for a kind's label in the panel.</summary>
    public static string LabelKey(FileKind kind) => "Types." + kind;

    public static int Bit(FileKind kind) => 1 << (int)kind;

    /// <summary>Every bucket on, which is what <see cref="FileKindFilter.Everything"/> means.</summary>
    public static readonly int AllMask = (1 << FileKinds.All.Length) - 1;

    private static readonly Dictionary<string, int> Map = Build();

    private static Dictionary<string, int> Build()
    {
        Dictionary<string, int> map = new Dictionary<string, int>(600, StringComparer.OrdinalIgnoreCase);

        Add(map, FileKind.Image,
            "jpg jpeg jpe png gif bmp webp avif jxl tif tiff heic heif ico cur svg svgz "
            + "psd psb xcf ai eps raw cr2 cr3 nef arw dng orf rw2 raf sr2 pef jfif tga dds exr hdr");

        Add(map, FileKind.Video,
            "mp4 m4v mkv avi mov wmv flv f4v webm mpg mpeg mpe m2v ts m2ts mts vob ogv "
            + "3gp 3g2 rm rmvb divx asf mxf braw r3d");

        Add(map, FileKind.Audio,
            "mp3 wav wave flac aac m4a m4b ogg oga opus wma aiff aif aifc alac ape wv "
            + "mid midi mod xm s3m cue dsf dff");

        Add(map, FileKind.Document,
            "pdf doc docx dot dotx rtf odt txt text log md markdown "
            + "xls xlsx xlsm xlt xltx csv tsv ods "
            + "ppt pptx pps ppsx pot potx odp "
            + "epub mobi azw azw3 djvu fb2 chm pages numbers key one tex bib vsd vsdx msg eml");

        Add(map, FileKind.Archive,
            "zip zipx rar r00 7z tar gz tgz bz2 tbz2 xz txz zst lz lz4 lzma arj lzh ace "
            + "cab iso img mdf nrg wim pak");

        Add(map, FileKind.Code,
            "c h cpp cxx cc hpp hxx cs csx vb fs fsx java kt kts scala go rs swift m mm "
            + "py pyw pyx rb pl pm php php3 php4 php5 lua tcl r jl dart groovy "
            + "js mjs cjs jsx ts mts cts tsx vue svelte coffee elm "
            + "html htm xhtml css scss sass less styl "
            + "json json5 jsonc xml xaml yaml yml toml ini cfg conf properties env "
            + "sh bash zsh fish ps1 psm1 psd1 bat cmd awk sed "
            + "sql ddl prisma graphql gql proto thrift "
            + "asm s inc pas pp dpr ada f90 f95 for cob cbl lisp el clj cljs erl ex exs hs "
            + "gradle cmake mk tf tfvars patch diff resx csproj sln vcxproj");

        // Not listed: "makefile" and "dockerfile". They are whole file names rather than
        // extensions, so MaskOf — which reads what follows the last dot — can never match them,
        // and listing them would put two entries in the Code tooltip that the filter does not
        // honour.

        Add(map, FileKind.Program,
            "exe msi msix msixbundle appx appxbundle msp mst com scr cpl ocx "
            + "dll sys drv ax efi ko so dylib a lib pdb "
            + "apk aab ipa deb rpm dmg pkg appimage snap flatpak jar war ear "
            + "vbs wsf hta gadget lnk pif");

        return map;
    }

    private static void Add(Dictionary<string, int> map, FileKind kind, string extensions)
    {
        int bit = Bit(kind);
        foreach (string extension in extensions.Split(' '))
        {
            if (extension.Length > 0)
            {
                map.TryGetValue(extension, out int mask);
                map[extension] = mask | bit;
            }
        }
    }

    /// <summary>
    /// The extensions in each bucket, alphabetically, built once by inverting <see cref="Map"/>.
    ///
    /// Inverted rather than kept as a second list beside the tables above, because the two would
    /// drift: the day someone adds an extension to <see cref="Build"/> and not to the copy is the
    /// day the panel starts describing a filter it does not have.
    ///
    /// An extension in two buckets is listed under both, which is what it means.
    /// </summary>
    private static readonly Dictionary<FileKind, string[]> ByKind = Invert();

    private static Dictionary<FileKind, string[]> Invert()
    {
        Dictionary<FileKind, List<string>> lists = new Dictionary<FileKind, List<string>>();
        foreach (KeyValuePair<string, int> entry in Map)
        {
            foreach (FileKind kind in All)
            {
                if ((entry.Value & Bit(kind)) != 0)
                {
                    if (!lists.TryGetValue(kind, out List<string> list))
                    {
                        lists[kind] = list = new List<string>();
                    }
                    list.Add(entry.Key);
                }
            }
        }

        Dictionary<FileKind, string[]> result = new Dictionary<FileKind, string[]>();
        foreach (KeyValuePair<FileKind, List<string>> entry in lists)
        {
            entry.Value.Sort(StringComparer.OrdinalIgnoreCase);
            result[entry.Key] = entry.Value.ToArray();
        }
        return result;
    }

    /// <summary>
    /// What a bucket holds, as a readable line — what the panel shows on hover. Empty for
    /// <see cref="FileKind.Other"/>, which is defined by what it does not hold and so has a
    /// sentence of its own instead.
    /// </summary>
    public static string ExtensionList(FileKind kind) =>
        ByKind.TryGetValue(kind, out string[] extensions) ? string.Join(", ", extensions) : string.Empty;

    /// <summary>
    /// The kinds a file name could be, as a mask. Zero means no bucket claims it, which the filter
    /// reads as <see cref="FileKind.Other"/>. Never throws.
    /// </summary>
    public static int MaskOf(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return 0;
        }

        int dot = name.LastIndexOf('.');

        // A leading dot is the whole name — ".gitignore" is a dotfile, not an extension — and a
        // trailing dot leaves nothing to look up.
        if (dot <= 0 || dot == name.Length - 1)
        {
            return 0;
        }

        // Nothing real is longer than this, and the guard keeps a pathological name like
        // "backup.2024-01-02-everything-under-the-sun" out of the dictionary probe entirely.
        if (name.Length - dot - 1 > 16)
        {
            return 0;
        }

        return Map.TryGetValue(name.Substring(dot + 1), out int mask) ? mask : 0;
    }

    /// <summary>
    /// Reads the custom-extension box: a free list of extensions, separated by whatever the user
    /// reached for. Leading dots and asterisks are stripped, so "iso, .mkv, *.img" all work —
    /// insisting on one spelling of the same intention would be this box's only hard edge.
    /// </summary>
    public static HashSet<string> ParseExtensions(string text)
    {
        HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(text))
        {
            return set;
        }

        foreach (string piece in text.Split(new[] { ',', ';', ' ', '\t', '|', '/' },
                                            StringSplitOptions.RemoveEmptyEntries))
        {
            string extension = piece.Trim().TrimStart('*', '.');
            if (extension.Length > 0 && extension.Length <= 16)
            {
                set.Add(extension);
            }
        }
        return set;
    }
}

/// <summary>
/// Which kinds of file a search is allowed to return, as a value the UI builds and the engine's
/// intake callback consults.
///
/// Immutable, because it is read on every engine worker thread while the UI thread is free to
/// build the next one: handing the search a snapshot is the whole of the thread safety here.
///
/// The custom extensions are an additional bucket rather than a restriction on the checked ones.
/// The alternative — treating them as "only these" — reads well in isolation and is wrong in
/// combination: with every category still on, typing "mkv" would have to either mean nothing at
/// all or silently throw the categories away, and both are worse than one more bucket.
/// </summary>
public sealed class FileKindFilter
{
    /// <summary>The filter that lets everything through, which is what a fresh launch starts on.</summary>
    public static readonly FileKindFilter Everything =
        new FileKindFilter(FileKinds.AllMask, null);

    private readonly int _mask;
    private readonly HashSet<string> _extensions;    // null = no extra bucket

    /// <summary>
    /// The custom extensions that contain a dot of their own — "tar.gz". They cannot be found by
    /// the single hash probe the rest use, which reads the last segment only and would compare
    /// "tar.gz" against "gz", so they are kept apart and matched by suffix. Almost always empty,
    /// and the common path never looks at them.
    /// </summary>
    private readonly string[] _compound;

    public FileKindFilter(int mask, HashSet<string> extensions)
    {
        _mask = mask & FileKinds.AllMask;
        _extensions = extensions != null && extensions.Count > 0 ? extensions : null;

        if (_extensions != null)
        {
            List<string> compound = null;
            foreach (string extension in _extensions)
            {
                if (extension.IndexOf('.') >= 0)
                {
                    (compound ??= new List<string>()).Add("." + extension);
                }
            }
            _compound = compound?.ToArray();
        }
    }

    public bool Has(FileKind kind) => (_mask & FileKinds.Bit(kind)) != 0;

    /// <summary>
    /// How many distinct kinds of file this filter lets through — the number on the Files label.
    ///
    /// A custom extension that one of the ticked buckets already covers is not counted. Video
    /// plus "mkv" typed in the box is one kind of file, not two, and a label reading "Files (2)"
    /// for it would be describing a filter nobody asked for.
    /// </summary>
    public int LabelCount
    {
        get
        {
            int count = KindCount;
            if (_extensions != null)
            {
                foreach (string extension in _extensions)
                {
                    // Prefixed, because MaskOf reads a file name rather than an extension.
                    if ((FileKinds.MaskOf("x." + extension) & _mask) == 0)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
    }

    /// <summary>How many of the eight buckets are on.</summary>
    public int KindCount
    {
        get
        {
            int count = 0;
            for (int mask = _mask; mask != 0; mask >>= 1)
            {
                count += mask & 1;
            }
            return count;
        }
    }

    /// <summary>
    /// Whether this filter restricts anything at all. The fast path: a search with no restriction
    /// must not pay for a per-record predicate, so the engine checks this once and skips the
    /// filter entirely.
    ///
    /// The custom extensions are not consulted. They can only ever let more files through, so
    /// with every bucket already on they are incapable of restricting anything — and a filter
    /// reported as restrictive because of them would put a count on the Files label that stands
    /// for no files being hidden.
    /// </summary>
    public bool IsEverything => _mask == FileKinds.AllMask;

    /// <summary>
    /// Whether nothing can get through — every bucket off and no custom extension. Surfaced as a
    /// query problem rather than as an empty result, for the same reason turning both Files and
    /// Folders off is: "0 results" is indistinguishable from a search that genuinely found nothing.
    /// </summary>
    public bool MatchesNothing => _mask == 0 && _extensions == null;

    public bool Allows(string name)
    {
        if (_mask == FileKinds.AllMask)
        {
            // Every bucket is on, so the extensions cannot add anything: short-circuit before
            // touching the name at all.
            return true;
        }

        if (_extensions != null && name != null)
        {
            int dot = name.LastIndexOf('.');
            if (dot > 0 && dot < name.Length - 1 && _extensions.Contains(name.Substring(dot + 1)))
            {
                return true;
            }

            if (_compound != null)
            {
                foreach (string suffix in _compound)
                {
                    if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
        }

        int kinds = FileKinds.MaskOf(name);
        return kinds == 0
            ? (_mask & FileKinds.Bit(FileKind.Other)) != 0
            : (_mask & kinds) != 0;
    }

    /// <summary>
    /// A one-line description for the active-options line, e.g. "Video, Audio, iso". Returns null
    /// when there is nothing to say.
    /// </summary>
    public string Describe(Func<string, string> translate)
    {
        if (IsEverything)
        {
            return null;
        }

        StringBuilder text = new StringBuilder();
        foreach (FileKind kind in FileKinds.All)
        {
            if (Has(kind))
            {
                if (text.Length > 0) text.Append(", ");
                text.Append(translate(FileKinds.LabelKey(kind)));
            }
        }
        if (_extensions != null)
        {
            foreach (string extension in _extensions)
            {
                if (text.Length > 0) text.Append(", ");
                text.Append(extension);
            }
        }
        return text.Length > 0 ? text.ToString() : null;
    }
}
