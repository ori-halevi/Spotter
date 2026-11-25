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
/// It is not allocation-free: the probe needs the extension as a string, and net8.0 has no
/// span-keyed dictionary lookup (that arrives with .NET 9). The substring is short-lived and
/// never escapes, so the cost is gen0 churn rather than anything that accumulates.
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

    /// <summary>
    /// Keys produced by a generated run rather than typed out. They match like any other, but
    /// they are left out of the hover lists: a hundred split-RAR volume numbers is ninety-nine
    /// more than anyone needs to see, and the Archive tooltip exists to answer "is the extension
    /// I want in here?" — a question a wall of r00, r01, r02 actively prevents.
    /// </summary>
    private static readonly HashSet<string> Generated =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>What to show in place of a generated run, per bucket.</summary>
    private static readonly Dictionary<FileKind, string> GeneratedLabel =
        new Dictionary<FileKind, string> { [FileKind.Archive] = "r00-r99" };

    private static readonly Dictionary<string, int> Map = Build();

    private static Dictionary<string, int> Build()
    {
        Dictionary<string, int> map = new Dictionary<string, int>(600, StringComparer.OrdinalIgnoreCase);

        Add(map, FileKind.Image,
            "jpg jpeg jpe png gif bmp webp avif jxl tif tiff heic heif ico cur svg svgz "
            + "psd psb xcf ai eps raw cr2 cr3 nef arw dng orf rw2 raf sr2 pef jfif tga dds exr hdr");

        Add(map, FileKind.Video,
            "mp4 m4v mkv avi mov wmv flv f4v webm mpg mpeg mpe m2v ts m2ts mts vob ogv "
            + "3gp 3g2 rm rmvb divx asf mxf braw r3d "
            // Subtitles. Not video files, but "show me what is beside my videos" is the
            // search people actually run, and a subtitle in "everything else" answers it
            // with silence.
            + "srt ass ssa vtt sub idx sbv smi");

        Add(map, FileKind.Audio,
            "mp3 wav wave flac aac m4a m4b ogg oga opus wma aiff aif aifc alac ape wv "
            + "mid midi mod xm s3m cue dsf dff mka ac3 dts eac3 amr au ra caf aa aax");

        Add(map, FileKind.Document,
            "pdf doc docx dot dotx rtf odt txt text log md markdown "
            + "xls xlsx xlsm xlt xltx csv tsv ods "
            + "ppt pptx pps ppsx pot potx odp "
            + "epub mobi azw azw3 djvu fb2 chm pages numbers key one tex bib vsd vsdx msg eml "
            // The macro-enabled trio. xlsm was here and its two siblings were not, so a
            // Report.docm beside a Report.xlsx was the only one of the pair in Documents.
            + "docm dotm pptm potm xlsb xps oxps pub vcf ics wpd");

        Add(map, FileKind.Archive,
            "zip zipx rar r00 7z tar gz tgz bz2 tbz2 xz txz zst lz lz4 lzma arj lzh ace "
            + "cab iso img mdf nrg wim pak cbz cbr cb7 sit sitx z lzo par2");

        Add(map, FileKind.Code,
            "c h cpp cxx cc hpp hxx cs csx vb fs fsx java kt kts scala go rs swift m mm "
            + "py pyw pyx rb pl pm php php3 php4 php5 lua tcl r jl dart groovy "
            + "js mjs cjs jsx ts mts cts tsx vue svelte coffee elm "
            + "html htm xhtml css scss sass less styl "
            + "json json5 jsonc xml xaml yaml yml toml ini cfg conf properties env "
            + "sh bash zsh fish ps1 psm1 psd1 bat cmd awk sed "
            + "sql ddl prisma graphql gql proto thrift "
            + "asm s inc pas pp dpr ada f90 f95 for cob cbl lisp el clj cljs erl ex exs hs "
            + "gradle cmake mk tf tfvars patch diff resx csproj sln vcxproj "
            // vb and fs are listed as sources above and csproj as a project file, so leaving
            // these two out put VB and F# projects in "everything else" alone.
            + "vbproj fsproj props targets nuspec ipynb reg "
            // Reachable only because MaskOf now reads a leading dot as introducing the
            // extension — see the note there.
            + "editorconfig gitignore gitattributes npmrc babelrc prettierrc eslintrc");

        // Not listed: "makefile" and "dockerfile". They are whole file names rather than
        // extensions, so MaskOf — which reads what follows the last dot — can never match them,
        // and listing them would put two entries in the Code tooltip that the filter does not
        // honour.

        Add(map, FileKind.Program,
            "exe msi msix msixbundle appx appxbundle msp mst com scr cpl ocx "
            + "dll sys drv ax efi ko so dylib a lib pdb "
            + "apk aab ipa deb rpm dmg pkg appimage snap flatpak jar war ear "
            + "vbs wsf hta gadget lnk pif url msu appinstaller vbe jse");

        // A split RAR set is named .r00 through .r99 — a hundred extensions that are one thing,
        // and the kind of list nobody maintains by hand. Only .r00 was here, which is the one
        // part of a split archive a user is least likely to go looking for on its own.
        for (int part = 0; part < 100; part++)
        {
            string key = "r" + part.ToString("D2", System.Globalization.CultureInfo.InvariantCulture);
            map.TryGetValue(key, out int existing);
            map[key] = existing | Bit(FileKind.Archive);
            Generated.Add(key);
        }

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
                    if (!Generated.Contains(entry.Key))
                    {
                        list.Add(entry.Key);
                    }
                }
            }
        }

        Dictionary<FileKind, string[]> result = new Dictionary<FileKind, string[]>();
        foreach (KeyValuePair<FileKind, List<string>> entry in lists)
        {
            if (GeneratedLabel.TryGetValue(entry.Key, out string label))
            {
                entry.Value.Add(label);
            }
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

        // A trailing dot leaves nothing to look up, and a name with no dot at all has no
        // extension to read.
        //
        // A *leading* dot does count, which this used to refuse. ".env" is a settings file and
        // "env" is in the code bucket, so reading a dotfile as having no extension dropped it
        // into "everything else" while the panel listed "env" under code — the filter
        // contradicting its own tooltip. The common dotfiles are listed under code because of
        // this, so ".gitignore" and ".editorconfig" are now findable by what they are.
        if (dot < 0 || dot == name.Length - 1)
        {
            return 0;
        }

        // Nothing real is longer than this, and the guard keeps a pathological name like
        // "backup.2024-01-02-everything-under-the-sun" out of the dictionary probe entirely.
        if (name.Length - dot - 1 > 16)
        {
            return 0;
        }

        if (Map.TryGetValue(name.Substring(dot + 1), out int mask))
        {
            return mask;
        }

        // A dotfile with something appended — ".env.local", ".env.production", ".babelrc.js".
        // The last segment names nothing, but the first one does, and it is the first one that
        // says what the file is. Without this, the leading-dot fix above reached ".env" and left
        // behind the spelling people actually hunt for: ".env.local" is the copy that is
        // gitignored, and therefore the one that is never where they expect it.
        if (name[0] == '.')
        {
            int second = name.IndexOf('.', 1);
            if (second > 1 && second - 1 <= 16
                && Map.TryGetValue(name.Substring(1, second - 1), out int leading))
            {
                return leading;
            }
        }
        return 0;
    }

    /// <summary>
    /// Every character this box treats as "next extension".
    ///
    /// The line breaks are the ones that matter. A list of extensions is something people paste
    /// — out of a wiki, a chat message, a spreadsheet column — and a single-line WPF text box
    /// keeps the newlines in its Text. Without them here, a pasted "mkv⏎iso⏎avi" became one
    /// token that no file could ever match, while the label counted it as an active type: a
    /// silent failure that also lied about itself.
    ///
    /// The no-break space comes from web pages, and the Arabic comma from an Arabic keyboard —
    /// this app ships in Arabic, and there "iso، mkv" is the natural way to type this.
    /// </summary>
    private static readonly char[] ExtensionSeparators =
    {
        ',', ';', ' ', '\t', '\r', '\n', '|', '/',
        ' ',   // no-break space
        '،',   // Arabic comma
        '、',   // ideographic comma
    };

    /// <summary>
    /// Reads the custom-extension box: a free list of extensions, separated by whatever the user
    /// reached for. Dots and asterisks are stripped from both ends, so "iso", ".mkv", "*.img" and
    /// a half-typed "mkv." all mean the same thing — insisting on one spelling of the same
    /// intention would be this box's only hard edge.
    ///
    /// Anything still carrying whitespace, a control character or a path character is dropped
    /// rather than kept. It cannot match a file name, and an entry that cannot match is worse
    /// than no entry at all, because the label counts it as a type that is letting files through.
    /// </summary>
    public static HashSet<string> ParseExtensions(string text)
    {
        HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(text))
        {
            return set;
        }

        foreach (string piece in text.Split(ExtensionSeparators, StringSplitOptions.RemoveEmptyEntries))
        {
            string extension = piece.Trim().Trim('*', '.');
            if (extension.Length > 0 && extension.Length <= 16 && IsUsableExtension(extension))
            {
                set.Add(extension);
            }
        }
        return set;
    }

    /// <summary>
    /// Characters that cannot appear in a Windows file name, so an entry holding one can never
    /// match anything. Taken from the framework rather than listed by hand: the hand-written
    /// list missed the quote, which is how an extension is written nearly everywhere it is
    /// documented — paste "*.mkv" with its quotes and the entry was counted and dead.
    /// </summary>
    private static readonly char[] Illegal = System.IO.Path.GetInvalidFileNameChars();

    private static bool IsUsableExtension(string extension)
    {
        foreach (char c in extension)
        {
            // Format characters — the bidi marks and zero-width joiners that come along with a
            // copy out of a right-to-left document or a web page — are neither whitespace nor
            // control, so they survive every other test and leave an entry that looks exactly
            // like a working one.
            if (char.IsWhiteSpace(c) || char.IsControl(c)
                || char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.Format
                || c == '*' || c == '?' || Array.IndexOf(Illegal, c) >= 0)
            {
                return false;
            }
        }
        return true;
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
                    if (!CoveredByKinds(extension))
                    {
                        count++;
                    }
                }
            }
            return count;
        }
    }

    /// <summary>
    /// Whether the ticked buckets already let this extension through, so naming it in the box
    /// adds nothing.
    ///
    /// Prefixed with a stem because <see cref="FileKinds.MaskOf"/> reads a file name rather than
    /// an extension, and routed through the same unclaimed-means-Other rule as
    /// <see cref="Allows"/>: an extension in no bucket is covered exactly when "everything else"
    /// is ticked, which is the case that made "Everything else" plus "bak" count as two.
    /// </summary>
    private bool CoveredByKinds(string extension)
    {
        int kinds = FileKinds.MaskOf("x." + extension);
        return kinds == 0
            ? (_mask & FileKinds.Bit(FileKind.Other)) != 0
            : (_mask & kinds) != 0;
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
            // The same rule MaskOf uses, including the leading dot and its 16-character guard:
            // typing "env" in the box has to reach ".env" for the same reason ticking "code and
            // settings" does, and a pathological name should not allocate a long string for a
            // lookup that ParseExtensions has already made impossible.
            int dot = name.LastIndexOf('.');
            if (dot >= 0 && dot < name.Length - 1 && name.Length - dot - 1 <= 16
                && _extensions.Contains(name.Substring(dot + 1)))
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
    /// A one-line description of what gets through, e.g. "Video, Audio, iso", for the chevron's
    /// tooltip. Returns null when there is nothing to say.
    ///
    /// Deliberately not on the active-options line beside it, which exists to show that a search
    /// is not running on default settings: the count on the Files label is already that signal
    /// for this filter, and spelling the same thing twice in one row is how a row stops being
    /// read at all.
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
                // Left out when a ticked bucket already covers it, so the tooltip and the count
                // beside it are describing the same filter.
                if (CoveredByKinds(extension))
                {
                    continue;
                }
                if (text.Length > 0) text.Append(", ");
                text.Append(extension);
            }
        }
        return text.Length > 0 ? text.ToString() : null;
    }
}
