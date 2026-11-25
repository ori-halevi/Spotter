using System.Collections.Generic;

namespace Spotter.Localization;

/// <summary>
/// Hebrew. Right to left — see <see cref="Lang.FlowDirection"/> for what that changes.
///
/// Two things here are about the bidirectional algorithm rather than about translation.
///
/// <see cref="L"/> is a LEFT-TO-RIGHT MARK, used where an embedded technical token starts with
/// punctuation: <c>*.dll</c> and <c>'note'</c> would otherwise have that punctuation resolved to
/// the paragraph direction and reordered to the far side of the word, so <c>*.dll</c> reaches a
/// Hebrew reader looking like <c>dll.*</c>.
///
/// The size operators need more than a mark. A <c>&gt;</c> between Hebrew and a number has
/// something that counts as right-to-left on both sides — a European number does, when the
/// algorithm resolves the punctuation beside it — so in a right-to-left paragraph it takes the
/// paragraph's direction and is *mirrored*: the help for "&gt; 50 MB" reaches a Hebrew user as
/// "&lt; 50 MB", the opposite filter. So the elements that show those strings are given an explicit
/// left-to-right direction in MainWindow.xaml, and where the string still opens with Hebrew — the
/// size placeholder — it also carries <see cref="L"/> before the operator. Both are needed; each
/// alone leaves the operator mirrored.
/// </summary>
internal static class StringsHe
{
    /// <summary>
    /// LEFT-TO-RIGHT MARK (U+200E). Written as an escape rather than pasted in, because a mark that
    /// matters and cannot be seen is a mark the next person deletes by accident.
    /// </summary>
    private const string L = "\u200e";

    internal static readonly IReadOnlyDictionary<string, string> Table =
        new Dictionary<string, string>
        {
            // ---- shared ------------------------------------------------------------------------
            ["Common.Save"] = "שמירה",
            ["Common.Cancel"] = "ביטול",
            ["Common.Remove"] = "הסרה",
            ["Common.OpenFolder"] = "פתיחת התיקייה",

            // ---- query fields ------------------------------------------------------------------
            ["Field.Name.Placeholder"] = "חיפוש שם קובץ…",
            ["Field.Name.Tip.Title"] = "שם הקובץ",
            // The marks are before the opening quote and the asterisk: without them the bidi
            // algorithm hands that punctuation to the paragraph and moves it to the other end of
            // the word, so 'note' arrives as note' ' and *.dll as dll.*
            ["Field.Name.Tip.Part"] = "מתאים לכל חלק מהשם, כך ש־" + L + "'note'"
                                      + " מוצא את " + L + "'notepad.exe'.",
            ["Field.Name.Tip.Words"] = "כמה מילים — כולן חייבות להופיע, בכל סדר:  log 2024",
            ["Field.Name.Tip.Wildcards"] = "התווים הכלליים * ו־? פועלים לכל מילה בנפרד:  "
                                           + L + "*.dll    report?.txt",
            ["Field.Name.Tip.Keys"] = "חץ למטה פותח את החיפושים האחרונים. Enter מחפש מיד, בלי להמתין להפסקת ההקלדה.",
            ["Field.Name.OptionsTip"] = "אפשרויות לשם הקובץ: התאמת רישיות, ביטוי רגולרי, איך מתייחסים למפרידים.",
            ["Field.Name.RefreshTip"] = "הרצה מחדש של החיפוש, בדיוק כמו F5. שימושי כשקובץ עומד להיווצר: שום דבר לא מתרענן מעצמו בזמן ההמתנה.",

            ["Field.Folder.Placeholder"] = "בתיקייה",
            ["Field.Folder.Tip.Title"] = "בתיקייה",
            ["Field.Folder.Tip.Part"] = "משאיר רק תוצאות שהתיקייה שלהן מתאימה. די בחלק מהשם:  windows",
            ["Field.Folder.Tip.Levels"] = "הפרדה בין רמות באמצעות \\ או /, וכל חלק מותאם בתורו:  users\\public",
            ["Field.Folder.Tip.Subfolders"] = "כשתיקיות משנה כבויות, ההתאמה חייבת להיות לתיקייה שהקובץ יושב בה ולא לתיקייה כלשהי מעליה.",
            ["Field.Folder.OptionsTip"] = "אפשרויות לתיקייה: התאמת רישיות, ביטוי רגולרי, תיקיות משנה.",
            ["Field.Folder.BrowseTip"] = "בחירת תיקייה. מה שמוקלד כרגע הוא המקום שבו ייפתח החלון.",
            ["Field.Folder.PickerTitle"] = "חיפוש בתיקייה",

            // Deliberately the reverse of the other languages' word order, because this field is laid
            // out left-to-right (see MainWindow.xaml) and what matters is the order it is *read* in.
            // Expression first in the string puts it on the left of the line, which is where a
            // Hebrew reader arrives last — so the eye still meets "גודל, לדוגמה" and then the example.
            // The mark is load-bearing too: without an L to its left, the ">" has something counting
            // as right-to-left on both sides and is mirrored into "<", the opposite filter.
            ["Field.Size.Placeholder"] = L + ">100MB " + "גודל, לדוגמה",
            ["Field.Size.Tip.Title"] = "גודל",
            ["Field.Size.Tip.Gt"] = "גדול מ־",
            ["Field.Size.Tip.Le"] = "לכל היותר",
            ["Field.Size.Tip.Eq"] = "בערך הגודל הזה",
            ["Field.Size.Tip.About"] = "בטווח של 10% ממנו",
            ["Field.Size.Tip.Range"] = "טווח",
            ["Field.Size.Tip.Units"] = "היחידות הן B, KB, MB, GB, TB. גודלה של תיקייה נמדד לפי כל מה שבתוכה, ולכן גם תיקיות יכולות להתאים לסינון לפי גודל.",
            ["Field.Size.PresetsTip"] = "סינוני גודל נפוצים.",

            // ---- toolbar -----------------------------------------------------------------------
            ["Toolbar.Files"] = "קבצים",
            ["Toolbar.Files.Tip"] = "לכלול קבצים בתוצאות. כיבוי יציג תיקיות בלבד.",
            ["Toolbar.Folders"] = "תיקיות",
            ["Toolbar.Folders.Tip"] = "לכלול תיקיות בתוצאות. כיבוי יציג קבצים בלבד.",
            ["Toolbar.ActiveOptions.Tip"] = "האפשרויות שמוגדרות כעת בשדות החיפוש. לכל שדה יש כפתור אפשרויות משלו.",
            ["Toolbar.Search"] = "חיפוש",
            ["Toolbar.SearchAgain"] = "חיפוש מחדש",
            ["Toolbar.Stop"] = "עצירה",
            ["Toolbar.Search.Tip"] = "הרצת החיפוש. מוצג משום ש״חיפוש תוך כדי הקלדה״ כבוי באפשרויות; בזמן שהחיפוש רץ הכפתור הופך לעצירה.",
            ["Toolbar.Recent"] = "אחרונים",
            ["Toolbar.Recent.Tip"] = "חיפושים שכבר ביצעת, החדשים ראשונים. חץ למטה בתיבת החיפוש פותח את אותה רשימה.",
            ["Toolbar.Options"] = "אפשרויות",
            ["Toolbar.Options.Tip"] = "שפה, אופן החיפוש, מקש קיצור גלובלי, סמל באזור ההודעות, ערכת נושא וגודל החלון.",

            // ---- active-options summary ---------------------------------------------------------
            ["Active.Name.Case"] = "שם: התאמת רישיות",
            ["Active.Name.Regex"] = "שם: ביטוי רגולרי",
            ["Active.Name.ExactSeparators"] = "שם: מפרידים מדויקים",
            ["Active.Folder.Case"] = "תיקייה: התאמת רישיות",
            ["Active.Folder.Regex"] = "תיקייה: ביטוי רגולרי",
            ["Active.NoSubfolders"] = "בלי תיקיות משנה",

            // ---- search history -----------------------------------------------------------------
            ["History.Empty"] = "אין עדיין חיפושים אחרונים. מונחים שתחפש ייזכרו כאן.",
            ["History.Hint"] = "Enter לשימוש · Delete למחיקת פריט",
            ["History.Clear"] = "ניקוי ההיסטוריה",

            // ---- columns -------------------------------------------------------------------------
            ["Column.Name"] = "שם",
            ["Column.Path"] = "נתיב",
            ["Column.Size"] = "גודל",
            ["Column.Modified"] = "שונה",
            ["Column.Type"] = "סוג",
            ["Column.Created"] = "נוצר",
            ["Column.Accessed"] = "גישה אחרונה",
            ["Column.Tip"] = "לחיצה ממיינת לפי העמודה, לחיצה נוספת הופכת את הסדר. לחיצה ימנית לאפשרויות נוספות.",

            // ---- text-field menu -----------------------------------------------------------------
            ["Menu.Cut"] = "גזירה",
            ["Menu.Copy"] = "העתקה",
            ["Menu.Paste"] = "הדבקה",
            ["Menu.PasteAndSearch"] = "הדבקה וחיפוש",
            ["Menu.PasteAndSearch.Tip"] = "החלפת תוכן השדה בטקסט שבלוח וחיפוש שלו.",
            ["Menu.RecentSearches"] = "חיפושים אחרונים…",
            ["Menu.ClearField"] = "ניקוי השדה הזה",
            ["Menu.ClearAllFields"] = "ניקוי כל שדות החיפוש",

            // ---- per-field option menus -------------------------------------------------------------
            ["Menu.MatchCase"] = "התאמת רישיות",
            ["Menu.Regex"] = "ביטוי רגולרי",
            ["Menu.Regex.Name.Tip"] = "ביטוי רגולרי של " + L + ".NET על שם הקובץ. חלוקה למילים והתווים הכלליים * ו־? מפסיקים לפעול.",
            ["Menu.Regex.Folder.Tip"] = "ביטוי רגולרי של " + L + ".NET על נתיב התיקייה. יש להשתמש בתו בריחה לפני קווים נטויים הפוכים — כפתור העיון עושה זאת בשבילך.",
            ["Menu.FlexibleSeparators"] = "נקודות ומקפים נחשבים לרווחים",
            ["Menu.FlexibleSeparators.Tip"] = "דולק: החיפוש 'Family.Guy' מוצא גם 'Family Guy' וגם 'Family_Guy', כי נקודה, קו תחתון או מקף בין שתי מילים נחשבים להפרדה. כבוי: הם חייבים להיות בדיוק התו שהקלדת.",
            ["Menu.Subfolders"] = "לכלול תיקיות משנה",
            ["Menu.Subfolders.Tip"] = "פעיל: התאמה לכל תיקייה מעל הקובץ. כבוי: התיקייה של הקובץ עצמו חייבת להתאים.",

            // ---- size presets --------------------------------------------------------------------
            ["Menu.Size.Empty"] = "ריק (0 בתים)",
            ["Menu.Size.Tiny"] = "זעיר — פחות מ־10 KB",
            ["Menu.Size.Under1MB"] = "פחות מ־1 MB",
            ["Menu.Size.Over10MB"] = "יותר מ־10 MB",
            ["Menu.Size.Over100MB"] = "יותר מ־100 MB",
            ["Menu.Size.Over1GB"] = "יותר מ־1 GB",
            ["Menu.Size.Over10GB"] = "יותר מ־10 GB",
            ["Menu.Size.Between"] = "בין 100 MB ל־1 GB",
            ["Menu.Size.About700MB"] = "בערך 700 MB (בטווח של 10%)",
            ["Menu.Size.Clear"] = "ניקוי סינון הגודל",

            // ---- kind filter menu ------------------------------------------------------------------
            ["Menu.Filter.Both"] = "קבצים ותיקיות",
            ["Menu.Filter.FilesOnly"] = "קבצים בלבד",
            ["Menu.Filter.FoldersOnly"] = "תיקיות בלבד",
            ["Menu.Filter.Reset"] = "איפוס כל אפשרויות החיפוש",
            ["Menu.Filter.Reset.Tip"] = "חזרה לקבצים ותיקיות, בלי התאמת רישיות, בלי ביטוי רגולרי, כולל תיקיות משנה.",

            // ---- result menu -------------------------------------------------------------------------
            ["Menu.Open"] = "פתיחה",
            ["Menu.OpenFolder"] = "פתיחת התיקייה המכילה",
            ["Menu.CopyPath"] = "העתקת הנתיב המלא",
            ["Menu.CopyName"] = "העתקת שם הקובץ",
            ["Menu.CopyFolder"] = "העתקת התיקייה המכילה",
            ["Menu.ExportSelected"] = "ייצוא הנבחרים…",
            ["Menu.Delete"] = "מחיקה",
            ["Menu.DeletePermanently"] = "מחיקה לצמיתות",
            ["Menu.Properties"] = "מאפיינים",

            // ---- background menu ----------------------------------------------------------------------
            ["Menu.SelectAll"] = "בחירת הכול",
            ["Menu.ClearSelection"] = "ביטול הבחירה",
            ["Menu.ExportAll"] = "ייצוא כל התוצאות…",
            ["Menu.SearchAgain"] = "חיפוש מחדש",

            // ---- column header menu -------------------------------------------------------------------
            ["Menu.SortAscending"] = "מיון בסדר עולה",
            ["Menu.SortDescending"] = "מיון בסדר יורד",
            ["Menu.SortByAscending"] = "מיון לפי {0}, בסדר עולה",
            ["Menu.SortByDescending"] = "מיון לפי {0}, בסדר יורד",
            ["Menu.FitColumn"] = "התאמת רוחב העמודה לתוכן",
            ["Menu.ResetColumns"] = "איפוס רוחב כל העמודות",

            // ---- keyboard gestures printed in menus ------------------------------------------------------
            ["Key.Down"] = "חץ למטה",
            ["Key.EscEsc"] = "Esc Esc",

            // ---- status bar --------------------------------------------------------------------------------
            ["Status.Starting"] = "מתחיל…",
            ["Status.NeedsAdmin"] = "כדי לקרוא את טבלאות הקבצים של הכוננים דרושות הרשאות מנהל מערכת.",
            ["Status.Elevate"] = "הפעלה מחדש כמנהל מערכת",
            ["Status.Elevate.Tip"] = "קריאת טבלאות הקבצים של הכוננים דורשת הרשאות מנהל מערכת. פעולה זו מפעילה מחדש את Spotter עם הרשאות מוגברות.",
            ["Status.ElevateFailed"] = "ההפעלה מחדש עם הרשאות מוגברות נכשלה: {0}",
            ["Status.PropertiesFailed"] = "לא ניתן היה להציג את המאפיינים של הפריט הזה.",
            ["Status.ReadingDrives"] = "קורא את הכוננים — התוצאות ייכנסו ככל שהם נטענים…",
            ["Status.Ready"] = "מוכן — {0} ({1})",
            ["Status.IndexFailed"] = "האינדוקס נכשל: {0}",
            ["Status.HotkeyTaken"] = "מקש הקיצור {0} כבר תפוס על ידי יישום אחר.",
            ["Status.Demo"] = "מצב הדגמה — {0} (בלי מנוע, עמודת הנתיב ריקה)",
            ["Status.Found"] = "{0} תוך {1} מ״ש",
            ["Status.Searching.Suffix"] = " …",
            ["Status.Selected"] = "{0} נבחרו · {1}",
            ["Status.Selected.Tip"] = "הגודל הכולל של הקבצים שנבחרו. תיקיות מושמטות בכוונה — גודל של תיקייה כבר כולל את הקבצים שבתוכה, וספירה של שניהם הייתה סופרת אותם פעמיים.",
            ["Status.Exporting"] = "מייצא {0}…",
            ["Status.Deleting"] = "מוחק {0}…",

            // ---- indexing progress -----------------------------------------------------------------------------
            ["Index.Reading"] = "קורא את הכוננים…",
            ["Index.ReadingCount"] = "קורא את הכוננים — {0}",
            ["Index.ReadingPercent"] = "קורא את הכוננים — {0}, {1}{2}",
            ["Index.DrivesDone"] = "{0} מתוך {1} כוננים",
            ["Index.Rate"] = " · {0}/ש׳",
            ["Index.Remaining"] = " · נותרו בערך {0}",
            ["Index.Volume.Progress"] = "{0}  ({1} מתוך {2})",
            ["Index.Volume.Waiting"] = "ממתין",
            ["Index.Volume.Failed"] = "נכשל",
            ["Index.Volume.NotSupported"] = "לא נתמך",
            ["Index.Volume.SizeUnknown"] = "גודל לא ידוע",

            // ---- durations and sizes ------------------------------------------------------------------------------
            ["Duration.Seconds"] = "{0} שנ׳",
            ["Duration.MinutesSeconds"] = "{0} דק׳ {1} שנ׳",
            ["Duration.HoursMinutes"] = "{0} שע׳ {1} דק׳",
            // Windows in Hebrew leaves the byte units in Latin script, so these stay as they are.
            ["Unit.B"] = "B",
            ["Unit.KB"] = "KB",
            ["Unit.MB"] = "MB",
            ["Unit.GB"] = "GB",
            ["Unit.TB"] = "TB",
            ["Unit.PB"] = "PB",

            // ---- counted things ---------------------------------------------------------------------------------------
            ["Plural.Results.one"] = "תוצאה אחת",
            ["Plural.Results.other"] = "{0:N0} תוצאות",
            ["Plural.Files.one"] = "קובץ אחד",
            ["Plural.Files.other"] = "{0:N0} קבצים",
            ["Plural.Folders.one"] = "תיקייה אחת",
            ["Plural.Folders.other"] = "{0:N0} תיקיות",
            ["Plural.Drives.one"] = "כונן אחד",
            ["Plural.Drives.other"] = "{0:N0} כוננים",
            ["Plural.Items.one"] = "פריט אחד",
            ["Plural.Items.other"] = "{0:N0} פריטים",
            ["Plural.Rows.one"] = "שורה אחת",
            ["Plural.Rows.other"] = "{0:N0} שורות",
            ["Plural.DemoRows.one"] = "שורה סינתטית אחת",
            ["Plural.DemoRows.other"] = "{0:N0} שורות סינתטיות",
            ["Contents.FilesAndFolders"] = "{0}, {1}",

            // ---- dialogs -----------------------------------------------------------------------------------------------
            ["Dialog.BulkOpen.Verb"] = "לפתוח",
            ["Dialog.BulkExplorer.Verb"] = "לפתוח חלונות סייר עבור",
            ["Dialog.BulkConfirm"] = "פעולה זו עומדת {0} {1}. להמשיך?",
            ["Dialog.NothingToExport"] = "אין מה לייצא.",
            ["Dialog.AlreadyRunning"] = "Spotter כבר פועל, אבל העותק הזה לא הצליח להביא את החלון שלו לחזית.\n\nייתכן שהוא שייך לחשבון משתמש אחר, או שהוא בעיצומה של עלייה.",
            ["Menu.NewInstance"] = "מופע חדש",
            ["Tray.NewInstance"] = "מופע חדש",
            ["NewInstance.Tip"] = "פתיחת חלון Spotter שני, עם חיפוש משלו.",
            ["Window.ExtraInstance"] = "Spotter — מופע נוסף",
            ["Dialog.NewInstanceFailed"] = "לא היה אפשר להפעיל מופע נוסף. הסיבה רשומה בקובץ היומן.",
            ["Dialog.Export.Filter"] = "קובץ CSV (*.csv)|*.csv|קובץ טקסט (*.txt)|*.txt",
            ["Dialog.Export.FileName"] = "תוצאות Spotter.csv",
            ["Dialog.Export.CsvHeader"] = "שם,נתיב,גודל,שונה",
            ["Dialog.Export.Done"] = "יוצאו {0}.\n\nלפתוח את הקובץ עכשיו?",
            ["Dialog.Export.Failed"] = "הייצוא נכשל: {0}",
            ["Dialog.Delete.Permanent"] = "למחוק לצמיתות",
            ["Dialog.Delete.Recycle"] = "להעביר לסל המיחזור",
            ["Dialog.Delete.Confirm"] = "בטוח שברצונך {0} {1}?",
            ["Dialog.Delete.Failed"] = "לא ניתן היה למחוק {0}.",

            // ---- notification area ----------------------------------------------------------------------------------------
            ["Tray.Show"] = "הצגת Spotter",
            ["Tray.Exit"] = "יציאה",
            ["Tray.Hidden.NoHotkey"] = "עדיין פועל — לחיצה כפולה על הסמל תחזיר את החלון.",
            ["Tray.Hidden.Hotkey"] = "עדיין פועל — הקש {0} או לחץ לחיצה כפולה על הסמל.",

            // ---- engine-reported problems -------------------------------------------------------------------------------------
            ["Problem.Name"] = "שם הקובץ — {0}",
            ["Problem.Folder"] = "בתיקייה — {0}",
            ["Problem.Size"] = "גודל — {0}",
            ["Problem.NoKinds"] = "יש להפעיל קבצים או תיקיות — כששניהם כבויים אין מה למצוא.",
            ["Problem.Unreadable"] = "לא ניתן לקריאה",

            // ---- options window ---------------------------------------------------------------------------------------------------
            ["Options.Title"] = "אפשרויות",

            ["Options.Language"] = "שפה",
            ["Options.Language.Hint"] = "נכנס לתוקף מיד. תבניות המספרים והתאריכים ממשיכות לעקוב אחרי הגדרות האזור של Windows, שהן הגדרה נפרדת במערכת.",
            ["Options.Language.System"] = "כמו Windows",
            ["Options.Language.SystemDetail"] = "כמו Windows  ({0})",
            ["Options.Language.Unsupported"] = "Windows מוגדר לשפה ש־Spotter עדיין לא תומך בה, ולכן נעשה שימוש באנגלית.",

            ["Options.Searching"] = "חיפוש",
            ["Options.SearchAsYouType"] = "חיפוש תוך כדי הקלדה",
            ["Options.RememberFilters"] = "לזכור את המתגים \"קבצים\" ו\"תיקיות\"",
            ["Options.RememberFilters.Hint"] = "כשכבוי: שניהם נדלקים מחדש בכל הפעלה. חלון שנפתח כשאחד מהם כבוי לא מוצא דבר מאותו סוג, ונראה כאילו הקובץ פשוט לא קיים.",
            ["Options.SearchAsYouType.On"] = "חיפוש חדש מתחיל בכל אות שמוקלדת או בכל שינוי הגדרה.",
            ["Options.SearchAsYouType.Off"] = "שום דבר לא נחפש עד ללחיצה על חיפוש או על Enter. שימושי במחשב עמוס, או כששאילתה מתאימה לרוב הדיסק.",

            ["Options.Hotkey"] = "מקש קיצור גלובלי",
            ["Options.Hotkey.Hint"] = "הקש את הצירוף הרצוי ואז שמור. הוא מביא את Spotter לחזית מכל מקום; הקשה נוספת מסתירה אותו.",
            ["Options.Hotkey.Placeholder"] = "לחץ כאן והקש צירוף",
            ["Options.Hotkey.NeedsModifier"] = "יש להוסיף לפחות אחד מ־Ctrl, Alt, Shift או Win — מקש בודד היה נתפס בכל מקום שבו מקלידים.",
            ["Options.Hotkey.Disable"] = "ביטול מקש הקיצור",

            ["Options.Tray"] = "אזור ההודעות",
            ["Options.Tray.Show"] = "הצגת סמל באזור ההודעות",
            ["Options.Tray.Minimize"] = "מזעור לאזור ההודעות במקום לשורת המשימות",
            ["Options.Tray.Close"] = "סגירת החלון משאירה את התוכנה פועלת באזור ההודעות",

            ["Options.Appearance"] = "מראה",
            ["Options.Theme.Auto"] = "לפי Windows",
            ["Options.Theme.Dark"] = "כהה",
            ["Options.Theme.Light"] = "בהיר",

            ["Options.Size"] = "גודל החלון",
            ["Options.Size.Hint"] = "Spotter נפתח ברבע השמאלי-העליון של המסך, ומצייר את הטקסט והפקדים בגודל שמתאים לגודל המסך — לא לכמות הפיקסלים שבו.",
            ["Options.Size.Auto"] = "התאמה למסך הזה",
            ["Options.Size.Auto.Detail"] = "למסך שלך יוצא {0}%. הסר את הסימון כדי לקבע גודל משלך.",
            ["Options.Size.Percent"] = "{0}%",
            ["Options.Size.Remember"] = "לזכור איפה השארת את החלון",
            ["Options.Size.Remember.Hint"] = "כל שינוי גודל, הזזה או הגדלה למסך מלא יחזור בפעם הבאה. המצב היחיד שלעולם לא נשמר הוא חלון שנשאר כמעט לגמרי מחוץ למסך, כי אז לא הייתה שום דרך לגרור אותו בחזרה.",
            ["Options.Size.Reset"] = "איפוס גודל ומיקום",
            ["Options.Size.Reset.Tip"] = "מחזיר את החלון מיד לרבע השמאלי-העליון של המסך, בגודל ברירת המחדל.",

            ["Options.Storage"] = "מה התוכנה שומרת במחשב שלך",
            ["Options.Storage.Hint"] = "שום דבר נוסף לא נכתב בשום מקום. כדי להסיר את הכול: מחק את קובצי Spotter משתי התיקיות האלה והסר את המשימה המתוזמנת שלמטה.",
            ["Options.Storage.Settings"] = "הגדרות",
            ["Options.Storage.AppLog"] = "יומן היישום",
            ["Options.Storage.EngineLog"] = "יומן המנוע",
            ["Options.Storage.EngineLog.Suffix"] = "   (בתוספת קובצי SpotterLog.N.txt.zip מסובבים)",
            ["Options.Storage.Task"] = "משימה מתוזמנת",
            ["Options.Storage.Open.Tip"] = "הצגת הקובץ בסייר הקבצים.",
            ["Options.Storage.Task.Registered"] = "מתזמן המשימות ← {0}\nמאפשר לעותק הזה לעלות עם הרשאות מוגברות בלי בקשת UAC בכל הפעלה.",
            ["Options.Storage.Task.Missing"] = "לא רשומה. במקום זאת Spotter מבקש הרשאות מנהל מערכת בבקשת UAC.",
            ["Options.Storage.Task.Remove.Tip"] = "מוחק את המשימה. בהפעלה הבאה Spotter יבקש שוב הרשאות מנהל מערכת בבקשת UAC.",
            ["Options.Storage.Task.Remove.Confirm"] = "להסיר את המשימה המתוזמנת \"{0}\"?\n\nSpotter ימשיך לפעול — הוא יבקש הרשאות מנהל מערכת בבקשת UAC בכל פעם, ויוכל לרשום את המשימה מחדש אחרי שתאשר אחת.",
            ["Options.Storage.Task.Remove.Failed"] = "לא ניתן היה להסיר את המשימה. ההסרה דורשת הרשאות מנהל מערכת.",
            ["Options.Storage.ExplorerFailed"] = "לא ניתן היה לפתוח את סייר הקבצים: {0}",

            ["Options.About"] = "אודות",
            ["Options.About.Version"] = "Spotter {0}",
            ["Options.About.Text"] = "חיפוש קבצים מיידי ל‏-Windows. ‏Spotter קורא ישירות את טבלת הקבצים הראשית של NTFS במקום לסרוק תיקיות — זו הסיבה שהוא דורש הרשאות מנהל מערכת, וזו הסיבה שהתוצאות מופיעות במהירות ההקלדה שלך.",
        };
}
