using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Spotter.Localization;

/// <summary>One shipped language.</summary>
public sealed class LanguageInfo
{
    public LanguageInfo(string code, string nativeName, string englishName,
                        bool rightToLeft, IReadOnlyDictionary<string, string> strings)
    {
        Code = code;
        NativeName = nativeName;
        EnglishName = englishName;
        IsRightToLeft = rightToLeft;
        Strings = strings;
    }

    /// <summary>Two-letter ISO code, which is also what the settings file stores.</summary>
    public string Code { get; }

    /// <summary>What the language calls itself — the only name worth showing in a language picker.</summary>
    public string NativeName { get; }

    public string EnglishName { get; }

    public bool IsRightToLeft { get; }

    public IReadOnlyDictionary<string, string> Strings { get; }

    /// <summary>
    /// Shown in the picker as "עברית (Hebrew)": the native name is the one someone looking for
    /// their own language scans for, the English one is what makes the list navigable to everyone
    /// else — including whoever picked the wrong entry and now cannot read the picker.
    /// </summary>
    public string DisplayName =>
        NativeName == EnglishName ? NativeName : $"{NativeName}  ({EnglishName})";
}

/// <summary>
/// The whole translation layer.
///
/// Every language is compiled into the executable as a plain dictionary — no .resx, no satellite
/// assemblies, no files beside the exe. Spotter is a single portable executable and adding a
/// per-language DLL folder next to it would undo that.
///
/// <see cref="Instance"/> exposes an indexer so XAML can bind through it; raising
/// <c>PropertyChanged("Item[]")</c> re-evaluates every one of those bindings at once, which is
/// what makes switching language take effect without a restart.
/// </summary>
public sealed class Lang : INotifyPropertyChanged
{
    /// <summary>The binding source for <c>{loc:Loc Key}</c>. One instance, for the app's life.</summary>
    public static Lang Instance { get; } = new Lang();

    private Lang()
    {
    }

    // ---- the catalogue ----------------------------------------------------------------------

    /// <summary>
    /// Shipped languages, in the order the picker shows them: English first as the fallback, then
    /// the rest alphabetically by English name.
    /// </summary>
    public static IReadOnlyList<LanguageInfo> Available { get; } = new[]
    {
        new LanguageInfo("en", "English", "English", false, StringsEn.Table),
        new LanguageInfo("ar", "العربية", "Arabic", true, StringsAr.Table),
        new LanguageInfo("de", "Deutsch", "German", false, StringsDe.Table),
        new LanguageInfo("he", "עברית", "Hebrew", true, StringsHe.Table),
        new LanguageInfo("ru", "Русский", "Russian", false, StringsRu.Table),
        new LanguageInfo("es", "Español", "Spanish", false, StringsEs.Table),
    };

    private static readonly LanguageInfo English = Available[0];

    /// <summary>
    /// The Windows display language as it was when the process started.
    ///
    /// Captured once, and deliberately not read live: <see cref="Use"/> overwrites
    /// <c>CurrentUICulture</c>, so asking that later answers "what are we showing?" rather than
    /// "what is Windows set to?". Reading it live made "Same as Windows" quietly mean "same as
    /// now" — it labelled itself with whatever had just been picked, and selecting it did nothing.
    /// </summary>
    private static readonly CultureInfo SystemUiCulture = CultureInfo.CurrentUICulture;

    public static LanguageInfo Current { get; private set; } = English;

    /// <summary>Raised after <see cref="Use"/> changes the language, for text set from code.</summary>
    public static event Action Changed;

    // ---- choosing a language ------------------------------------------------------------------

    /// <summary>
    /// Applies a stored preference: a language code, or blank/unknown for "follow Windows".
    /// Anything Windows is set to that is not shipped here lands on English.
    /// </summary>
    public static void Initialize(string preferredCode)
    {
        Use(string.IsNullOrWhiteSpace(preferredCode) ? SystemCode() : preferredCode);
    }

    /// <summary>
    /// The shipped language matching the Windows display language, or English.
    ///
    /// Matched on the two-letter code rather than the full culture name, so de-AT gets German and
    /// es-MX gets Spanish instead of silently falling through to English.
    /// </summary>
    public static string SystemCode()
    {
        try
        {
            CultureInfo culture = SystemUiCulture;
            while (culture != null && !string.IsNullOrEmpty(culture.Name))
            {
                LanguageInfo hit = Find(culture.TwoLetterISOLanguageName);
                if (hit != null)
                {
                    return hit.Code;
                }
                CultureInfo parent = culture.Parent;
                if (ReferenceEquals(parent, culture))
                {
                    break;
                }
                culture = parent;
            }
        }
        catch
        {
            // A broken or exotic culture is not a reason to fail to start.
        }
        return English.Code;
    }

    /// <summary>
    /// False when Windows is set to a language this app does not ship, which is the only case where
    /// "Same as Windows" silently means English and therefore the only case worth saying so.
    /// </summary>
    public static bool SystemLanguageIsAvailable =>
        Find(SystemUiCulture.TwoLetterISOLanguageName) != null;

    /// <summary>The Windows display language's culture name, for the log.</summary>
    public static string SystemCultureName => SystemUiCulture.Name;

    public static LanguageInfo Find(string code) =>
        string.IsNullOrWhiteSpace(code)
            ? null
            : Available.FirstOrDefault(
                l => string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Switches language. Safe to call with a code that is not shipped — that lands on English
    /// rather than leaving the app half-translated.
    /// </summary>
    public static void Use(string code)
    {
        LanguageInfo chosen = Find(code) ?? English;
        if (ReferenceEquals(chosen, Current))
        {
            return;
        }
        Current = chosen;

        // The UI language is what these strings are; the number and date formats stay on the
        // user's Windows regional settings, which is a separate choice in Windows and not ours to
        // override. It is also the safe one: the search engine parses "1.5 GB" with the current
        // culture, and forcing, say, German here would change what a decimal point means to it.
        try
        {
            CultureInfo ui = CultureInfo.GetCultureInfo(chosen.Code);
            System.Threading.Thread.CurrentThread.CurrentUICulture = ui;
            CultureInfo.DefaultThreadCurrentUICulture = ui;
        }
        catch
        {
        }

        Instance.RaiseAll();
        Changed?.Invoke();
    }

    // ---- lookup --------------------------------------------------------------------------------

    /// <summary>
    /// The translated string, falling back to English and then to the key itself. A key that
    /// shows up on screen verbatim is a missing translation announcing itself, which is a great
    /// deal easier to spot than a blank label.
    /// </summary>
    public static string Get(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return string.Empty;
        }
        if (Current.Strings.TryGetValue(key, out string text))
        {
            return text;
        }
        return English.Strings.TryGetValue(key, out string fallback) ? fallback : key;
    }

    /// <summary>Shorthand for <see cref="Get"/>.</summary>
    public static string Tr(string key) => Get(key);

    /// <summary>
    /// Translated and formatted. Formatting uses the *current culture*, not the UI language, so
    /// the numbers inside a sentence are grouped the way the rest of Windows groups them.
    /// </summary>
    public static string Tr(string key, params object[] args)
    {
        string format = Get(key);
        try
        {
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }
        catch (FormatException)
        {
            // A translation with a stray brace should show as itself, not take the window down.
            return format;
        }
    }

    // ---- plurals --------------------------------------------------------------------------------

    /// <summary>
    /// The CLDR plural categories, as far as the shipped languages need them. English, German and
    /// Spanish use two; Hebrew is treated as two; Russian needs three; Arabic uses all six.
    /// </summary>
    private enum PluralForm
    {
        Zero,
        One,
        Two,
        Few,
        Many,
        Other,
    }

    /// <summary>
    /// "3 results" in the shape the current language actually uses.
    ///
    /// English, German and Spanish need two forms; Hebrew is treated as two as well, which is how
    /// modern Hebrew counts things outside a few fixed dual nouns. Russian needs three, and
    /// getting that wrong is not a subtlety — "5 результата" reads as broken as "5 results" would
    /// read as "5 result".
    ///
    /// <paramref name="key"/> is a prefix: the lookup is <c>key.one</c> / <c>key.few</c> /
    /// <c>key.many</c> / <c>key.other</c>, each of which takes the count as {0}.
    /// </summary>
    public static string Count(string key, long n)
    {
        PluralForm form = FormFor(n);
        string suffix = form switch
        {
            PluralForm.Zero => ".zero",
            PluralForm.One => ".one",
            PluralForm.Two => ".two",
            PluralForm.Few => ".few",
            PluralForm.Many => ".many",
            _ => ".other",
        };

        string format = Current.Strings.TryGetValue(key + suffix, out string exact)
            ? exact
            : Current.Strings.TryGetValue(key + ".other", out string other)
                ? other
                : Get(key + suffix);

        try
        {
            return string.Format(CultureInfo.CurrentCulture, format, n);
        }
        catch (FormatException)
        {
            return format;
        }
    }

    private static PluralForm FormFor(long n)
    {
        long abs = Math.Abs(n);

        if (Current.Code == "ar")
        {
            // Arabic counts in six categories, and the two counter-intuitive ones matter most: a
            // number ending 11–99 takes the *singular* ("25 نتيجة"), while 3–10 takes the plural
            // ("5 نتائج"). Collapsing them the way English does is wrong in both directions.
            if (abs == 0) return PluralForm.Zero;
            if (abs == 1) return PluralForm.One;
            if (abs == 2) return PluralForm.Two;

            long hundred = abs % 100;
            if (hundred >= 3 && hundred <= 10) return PluralForm.Few;
            return hundred >= 11 && hundred <= 99 ? PluralForm.Many : PluralForm.Other;
        }

        if (Current.Code == "ru")
        {
            long mod10 = abs % 10;
            long mod100 = abs % 100;
            if (mod10 == 1 && mod100 != 11)
            {
                return PluralForm.One;
            }
            if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14))
            {
                return PluralForm.Few;
            }
            return PluralForm.Many;
        }
        return abs == 1 ? PluralForm.One : PluralForm.Other;
    }

    // ---- byte sizes ------------------------------------------------------------------------------

    private static readonly string[] UnitKeys =
    {
        "Unit.B", "Unit.KB", "Unit.MB", "Unit.GB", "Unit.TB", "Unit.PB",
    };

    /// <summary>
    /// "1.4 MB", with the unit in the current language — Russian wants МБ, not MB.
    ///
    /// One implementation for the whole app. The size column, the selection total and the indexing
    /// rate each had their own copy of this loop, with the units spelled out inline, so localizing
    /// them separately would have been three chances to disagree about what a kilobyte is called.
    /// </summary>
    public static string FormatBytes(long bytes)
    {
        double value = bytes;
        int unit = 0;
        while (value >= 1024 && unit < UnitKeys.Length - 1)
        {
            value /= 1024;
            unit++;
        }
        // Whole bytes are never shown with a fraction: "512 B", not "512.0 B".
        return unit == 0
            ? string.Format(CultureInfo.CurrentCulture, "{0:N0} {1}", bytes, Get(UnitKeys[0]))
            : string.Format(CultureInfo.CurrentCulture, "{0:0.#} {1}", value, Get(UnitKeys[unit]));
    }

    // ---- direction ------------------------------------------------------------------------------

    /// <summary>
    /// Which way the layout runs. Bound by both windows, so switching to Hebrew mirrors them
    /// without a restart.
    ///
    /// WPF mirrors the whole coordinate system for RightToLeft, so margins, alignments and docking
    /// all follow on their own. Two things it also mirrors that it should not are handled at the
    /// point of use: <see cref="System.Windows.Controls.Image"/> content, which would flip every
    /// file icon, and text that is really machine data — a path, a size, a date — which reads
    /// wrong under a right-to-left paragraph direction.
    /// </summary>
    public FlowDirection FlowDirection =>
        Current.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

    public bool IsRightToLeft => Current.IsRightToLeft;

    /// <summary>
    /// Where a line of text starts. Used on the result cells, which stay left-to-right internally
    /// so paths and sizes read correctly, but still need to sit against the leading edge of a
    /// mirrored column rather than drifting to the far side of it.
    /// </summary>
    public TextAlignment StartAlignment =>
        Current.IsRightToLeft ? TextAlignment.Right : TextAlignment.Left;

    // ---- binding plumbing -------------------------------------------------------------------------

    /// <summary>Indexer so XAML can bind a key: <c>{loc:Loc Menu.Open}</c> compiles to this.</summary>
    public string this[string key] => Get(key);

    public event PropertyChangedEventHandler PropertyChanged;

    private void RaiseAll()
    {
        // "Item[]" is the WPF convention for "every indexer binding is stale".
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        foreach (string name in new[]
                 {
                     nameof(FlowDirection), nameof(IsRightToLeft), nameof(StartAlignment),
                 })
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

/// <summary>
/// <c>{loc:Loc Some.Key}</c> in XAML.
///
/// It produces a binding rather than a string so that the text updates when the language changes.
/// A plain string would be correct exactly once, at load, and every window already open would keep
/// the old language until it was closed and reopened.
/// </summary>
[MarkupExtensionReturnType(typeof(string))]
public sealed class LocExtension : MarkupExtension
{
    public LocExtension()
    {
    }

    public LocExtension(string key)
    {
        Key = key;
    }

    [ConstructorArgument("key")]
    public string Key { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        Binding binding = new Binding($"[{Key}]")
        {
            Source = Lang.Instance,
            Mode = BindingMode.OneWay,
        };
        return binding.ProvideValue(serviceProvider);
    }
}

/// <summary>
/// <c>{loc:LangProp FlowDirection}</c> — binds one of <see cref="Lang"/>'s own properties, for
/// direction and alignment. Saves repeating the source and mode at a dozen call sites.
/// </summary>
public sealed class LangPropExtension : MarkupExtension
{
    public LangPropExtension()
    {
    }

    public LangPropExtension(string path)
    {
        Path = path;
    }

    [ConstructorArgument("path")]
    public string Path { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        Binding binding = new Binding(Path)
        {
            Source = Lang.Instance,
            Mode = BindingMode.OneWay,
        };
        return binding.ProvideValue(serviceProvider);
    }
}
