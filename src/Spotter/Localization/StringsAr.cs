using System.Collections.Generic;

namespace Spotter.Localization;

/// <summary>
/// Arabic. Right to left, and the second such language here — which is the point: the direction
/// work in MainWindow.xaml and PopupChrome is driven by <see cref="LanguageInfo.IsRightToLeft"/>
/// rather than by anything Hebrew-specific, so this table needed no new plumbing.
///
/// Terminology follows Windows in Arabic: سلة المحذوفات, مستكشف الملفات, منطقة الإعلامات,
/// برنامج جدولة المهام.
///
/// Two things beyond translation:
///
/// <see cref="L"/> is a LEFT-TO-RIGHT MARK, for the same reason as in the Hebrew table: an embedded
/// token that begins with punctuation — <c>*.dll</c>, <c>'note'</c>, <c>.NET</c> — otherwise has that
/// punctuation handed to the paragraph and moved to the far side of the word.
///
/// Plurals use all six CLDR categories. Arabic is the language where getting this wrong is most
/// visible, because the rule inverts halfway: 3–10 takes the plural (<c>5 نتائج</c>) while 11–99
/// takes the singular (<c>25 نتيجة</c>). See <c>Lang.FormFor</c>.
/// </summary>
internal static class StringsAr
{
    /// <summary>LEFT-TO-RIGHT MARK (U+200E), as an escape so it cannot be deleted by accident.</summary>
    private const string L = "\u200e";

    internal static readonly IReadOnlyDictionary<string, string> Table =
        new Dictionary<string, string>
        {
            // ---- shared ------------------------------------------------------------------------
            ["Common.Save"] = "حفظ",
            ["Common.Cancel"] = "إلغاء",
            ["Common.Remove"] = "إزالة",
            ["Common.OpenFolder"] = "فتح المجلد",

            // ---- query fields ------------------------------------------------------------------
            ["Field.Name.Placeholder"] = "البحث عن اسم ملف…",
            ["Field.Name.Tip.Title"] = "اسم الملف",
            ["Field.Name.Tip.Part"] = "يطابق أي جزء من الاسم، فـ" + L + "'note'" + " يجد " + L + "'notepad.exe'.",
            ["Field.Name.Tip.Words"] = "عند كتابة عدة كلمات يجب أن تظهر كلها، بأي ترتيب:  log 2024",
            ["Field.Name.Tip.Wildcards"] = "حرفا البدل * و؟ يعملان داخل كل كلمة على حدة:  "
                                           + L + "*.dll    report?.txt",
            ["Field.Name.Tip.Keys"] = "السهم لأسفل يفتح عمليات البحث الأخيرة. Enter يبحث فورًا دون انتظار توقف الكتابة.",
            ["Field.Name.OptionsTip"] = "خيارات اسم الملف: مطابقة حالة الأحرف، تعبير عادي، طريقة معاملة الفواصل.",
            ["Field.Name.RefreshTip"] = "تنفيذ البحث من جديد، تمامًا مثل F5. مفيد عندما يكون الملف على وشك الإنشاء: لا شيء يتكرر من تلقاء نفسه أثناء الانتظار.",

            ["Field.Folder.Placeholder"] = "في المجلد",
            ["Field.Folder.Tip.Title"] = "في المجلد",
            ["Field.Folder.Tip.Part"] = "يُبقي النتائج التي يطابق مجلدها فقط. يكفي جزء من الاسم:  windows",
            ["Field.Folder.Tip.Levels"] = "افصل المستويات بـ \\ أو /، ويُطابَق كل جزء بدوره:  users\\public",
            ["Field.Folder.Tip.Subfolders"] = "عند إيقاف المجلدات الفرعية يجب أن يطابق مجلد الملف نفسه، لا أي مجلد أعلى منه.",
            ["Field.Folder.OptionsTip"] = "خيارات المجلد: مطابقة حالة الأحرف، تعبير عادي، المجلدات الفرعية.",
            ["Field.Folder.BrowseTip"] = "اختيار مجلد. يُفتح مربع الاختيار عند المسار المكتوب حاليًا.",
            ["Field.Folder.PickerTitle"] = "البحث في المجلد",

            // Reversed relative to the other languages on purpose: this field is laid out
            // left-to-right (see MainWindow.xaml), so putting the expression first in the string
            // places it on the left of the line — which is where an Arabic reader arrives last.
            // The mark is required as well, or the ">" is mirrored into "<", the opposite filter.
            ["Field.Size.Placeholder"] = L + ">100MB " + "الحجم، مثلاً",
            ["Field.Size.Tip.Title"] = "الحجم",
            ["Field.Size.Tip.Gt"] = "أكبر من",
            ["Field.Size.Tip.Le"] = "على الأكثر",
            ["Field.Size.Tip.Eq"] = "بهذا الحجم تقريبًا",
            // Western digits, not Arabic-Indic: every other number on screen is formatted from
            // CurrentCulture, which follows the user's Windows regional settings rather than the UI
            // language, so hardcoding ١٠ here would be the one number that disagreed with the rest.
            ["Field.Size.Tip.About"] = "في حدود 10% منه",
            ["Field.Size.Tip.Range"] = "نطاق",
            ["Field.Size.Tip.Units"] = "الوحدات هي B, KB, MB, GB, TB. ويُقاس حجم المجلد بكل ما يحتويه، لذلك يمكن أن تطابق المجلدات مرشِّح الحجم أيضًا.",
            ["Field.Size.PresetsTip"] = "مرشِّحات الحجم الشائعة.",

            // ---- toolbar -----------------------------------------------------------------------
            ["Toolbar.Files"] = "الملفات",
            ["Toolbar.Files.Tip"] = "تضمين الملفات في النتائج. أوقفه لعرض المجلدات فقط.",
            ["Toolbar.Folders"] = "المجلدات",
            ["Toolbar.Folders.Tip"] = "تضمين المجلدات في النتائج. أوقفه لعرض الملفات فقط.",

            // The file types panel behind the Files switch.
            ["Types.Title"] = "أنواع الملفات",
            ["Types.All"] = "كل الأنواع",
            ["Types.Tip"] = "أنواع الملفات التي تُحتسب. جميعها إلا إذا حُدِّد غير ذلك.",
            ["Types.Image"] = "صور",
            ["Types.Video"] = "فيديو",
            ["Types.Audio"] = "صوت",
            ["Types.Document"] = "مستندات",
            ["Types.Archive"] = "أرشيفات",
            ["Types.Code"] = "برمجة وإعدادات",
            ["Types.Program"] = "برامج ومثبِّتات",
            ["Types.Other"] = "كل ما عدا ذلك",
            ["Types.Other.Tip"] = "كل ملف لا تطالب به أي فئة أخرى، بما في ذلك الملفات بلا امتداد أصلًا.",
            ["Types.Custom"] = "امتدادات أخرى",
            ["Types.Custom.Placeholder"] = "bak, tmp, sqlite",
            ["Types.Custom.Hint"] = "تُضاف إلى ما هو محدَّد أعلاه. للبحث عنها وحدها، أزل التحديد عن الأنواع.",
            ["Toolbar.ActiveOptions.Tip"] = "الخيارات المحددة حاليًا في حقول البحث. زر الخيارات في كل حقل يغيّر خياراته وحده.",
            ["Toolbar.Search"] = "بحث",
            ["Toolbar.SearchAgain"] = "بحث من جديد",
            ["Toolbar.Stop"] = "إيقاف",
            ["Toolbar.Search.Tip"] = "تنفيذ البحث. يظهر هذا الزر لأن «البحث أثناء الكتابة» متوقف في الخيارات؛ وأثناء البحث يتحول إلى «إيقاف».",
            ["Toolbar.Recent"] = "الأخيرة",
            ["Toolbar.Recent.Tip"] = "عمليات البحث السابقة، الأحدث أولاً. والسهم لأسفل في مربع البحث يفتح القائمة نفسها.",
            ["Toolbar.Options"] = "خيارات",
            ["Toolbar.Options.Tip"] = "اللغة وطريقة البحث ومفتاح الاختصار العام ورمز منطقة الإعلامات والمظهر وحجم النافذة.",

            // ---- active-options summary ---------------------------------------------------------
            ["Active.Name.Case"] = "الاسم: مطابقة حالة الأحرف",
            ["Active.Name.Regex"] = "الاسم: تعبير عادي",
            ["Active.Name.ExactSeparators"] = "الاسم: فواصل مطابقة",
            ["Active.Folder.Case"] = "المجلد: مطابقة حالة الأحرف",
            ["Active.Folder.Regex"] = "المجلد: تعبير عادي",
            ["Active.NoSubfolders"] = "بدون مجلدات فرعية",

            // ---- search history -----------------------------------------------------------------
            ["History.Empty"] = "لا توجد عمليات بحث أخيرة بعد. ما تبحث عنه سيُحفظ هنا.",
            ["History.Hint"] = "Enter للاستخدام · Delete للحذف",
            ["History.Clear"] = "محو السجل",

            // ---- columns -------------------------------------------------------------------------
            ["Column.Name"] = "الاسم",
            ["Column.Path"] = "المسار",
            ["Column.Size"] = "الحجم",
            ["Column.Modified"] = "تاريخ التعديل",
            ["Column.Type"] = "النوع",
            ["Column.Created"] = "تاريخ الإنشاء",
            ["Column.Accessed"] = "تاريخ الوصول",
            ["Column.Tip"] = "انقر للفرز حسب هذا العمود، وانقر مرة أخرى لعكس الترتيب. انقر بالزر الأيمن للمزيد.",

            // ---- text-field menu -----------------------------------------------------------------
            ["Menu.Cut"] = "قص",
            ["Menu.Copy"] = "نسخ",
            ["Menu.Paste"] = "لصق",
            ["Menu.PasteAndSearch"] = "لصق وبحث",
            ["Menu.PasteAndSearch.Tip"] = "يستبدل محتوى هذا الحقل بنص الحافظة ويبحث عنه.",
            ["Menu.RecentSearches"] = "عمليات البحث الأخيرة…",
            ["Menu.ClearField"] = "محو هذا الحقل",
            ["Menu.ClearAllFields"] = "محو جميع حقول البحث",

            // ---- per-field option menus -------------------------------------------------------------
            ["Menu.MatchCase"] = "مطابقة حالة الأحرف",
            ["Menu.Regex"] = "تعبير عادي",
            ["Menu.Regex.Name.Tip"] = "تعبير عادي في " + L + ".NET على اسم الملف. ويتوقف عندها تقسيم الكلمات وحرفا البدل * و؟.",
            ["Menu.Regex.Folder.Tip"] = "تعبير عادي في " + L + ".NET على مسار المجلد. ويجب استخدام حرف تخطٍّ قبل الشرطات المائلة العكسية — وزر الاستعراض يفعل ذلك عنك.",
            ["Menu.FlexibleSeparators"] = "النقاط والشرطات تُعامَل كمسافات",
            ["Menu.FlexibleSeparators.Tip"] = "مفعّل: البحث عن 'Family.Guy' يجد أيضًا 'Family Guy' و'Family_Guy'، لأن النقطة أو الشرطة السفلية أو الشرطة بين كلمتين تُعدّ فاصلًا. معطّل: يجب أن يكون التو نفسه الذي كتبته تمامًا.",
            ["Menu.Subfolders"] = "تضمين المجلدات الفرعية",
            ["Menu.Subfolders.Tip"] = "مفعّل: يطابق أي مجلد أعلى الملف. متوقف: يجب أن يطابق مجلد الملف نفسه.",

            // ---- size presets --------------------------------------------------------------------
            ["Menu.Size.Empty"] = "فارغة (0 بايت)",
            ["Menu.Size.Tiny"] = "صغيرة جدًا — أقل من 10 KB",
            ["Menu.Size.Under1MB"] = "أقل من 1 MB",
            ["Menu.Size.Over10MB"] = "أكبر من 10 MB",
            ["Menu.Size.Over100MB"] = "أكبر من 100 MB",
            ["Menu.Size.Over1GB"] = "أكبر من 1 GB",
            ["Menu.Size.Over10GB"] = "أكبر من 10 GB",
            ["Menu.Size.Between"] = "بين 100 MB و1 GB",
            ["Menu.Size.About700MB"] = "نحو 700 MB (بفارق 10%)",
            ["Menu.Size.Clear"] = "إزالة مرشِّح الحجم",

            // ---- kind filter menu ------------------------------------------------------------------
            ["Menu.Filter.Both"] = "الملفات والمجلدات",
            ["Menu.Filter.FilesOnly"] = "الملفات فقط",
            ["Menu.Filter.FoldersOnly"] = "المجلدات فقط",
            ["Menu.Filter.Reset"] = "إعادة تعيين جميع خيارات البحث",
            ["Menu.Filter.Reset.Tip"] = "العودة إلى الملفات والمجلدات، دون مطابقة حالة الأحرف، ودون تعبير عادي، مع تضمين المجلدات الفرعية. وأنواع الملفات تعود إلى الكل.",

            // ---- result menu -------------------------------------------------------------------------
            ["Menu.Open"] = "فتح",
            ["Menu.OpenFolder"] = "فتح المجلد الحاوي",
            ["Menu.CopyPath"] = "نسخ المسار الكامل",
            ["Menu.CopyName"] = "نسخ اسم الملف",
            ["Menu.CopyFolder"] = "نسخ المجلد الحاوي",
            ["Menu.ExportSelected"] = "تصدير المحدد…",
            ["Menu.Delete"] = "حذف",
            ["Menu.DeletePermanently"] = "حذف نهائي",
            ["Menu.Properties"] = "خصائص",

            // ---- background menu ----------------------------------------------------------------------
            ["Menu.SelectAll"] = "تحديد الكل",
            ["Menu.ClearSelection"] = "إلغاء التحديد",
            ["Menu.ExportAll"] = "تصدير كل النتائج…",
            ["Menu.SearchAgain"] = "بحث من جديد",

            // ---- column header menu -------------------------------------------------------------------
            ["Menu.SortAscending"] = "فرز تصاعدي",
            ["Menu.SortDescending"] = "فرز تنازلي",
            ["Menu.SortByAscending"] = "فرز حسب {0}، تصاعديًا",
            ["Menu.SortByDescending"] = "فرز حسب {0}، تنازليًا",
            ["Menu.FitColumn"] = "ملاءمة عرض العمود للمحتوى",
            ["Menu.ResetColumns"] = "إعادة تعيين عرض كل الأعمدة",

            // ---- keyboard gestures printed in menus ------------------------------------------------------
            ["Key.Down"] = "السهم لأسفل",
            ["Key.EscEsc"] = "Esc Esc",

            // ---- status bar --------------------------------------------------------------------------------
            ["Status.Starting"] = "جارٍ البدء…",
            ["Status.NeedsAdmin"] = "تلزم صلاحيات المسؤول لقراءة جداول ملفات الأقراص.",
            ["Status.Elevate"] = "إعادة التشغيل كمسؤول",
            ["Status.Elevate.Tip"] = "قراءة جداول ملفات الأقراص تتطلب صلاحيات المسؤول. هذا يعيد تشغيل Spotter بصلاحيات مرتفعة.",
            ["Status.ElevateFailed"] = "تعذّرت إعادة التشغيل بصلاحيات مرتفعة: {0}",
            ["Status.PropertiesFailed"] = "تعذّر عرض خصائص هذا العنصر.",
            ["Status.ReadingDrives"] = "جارٍ قراءة الأقراص — وستظهر النتائج تدريجيًا مع تحميلها…",
            ["Status.Ready"] = "جاهز — {0} ({1})",
            ["Status.IndexFailed"] = "فشلت الفهرسة: {0}",
            ["Status.HotkeyTaken"] = "مفتاح الاختصار {0} مستخدَم بالفعل من تطبيق آخر.",
            ["Status.Demo"] = "وضع العرض — {0} (بدون محرك، وعمود المسار فارغ)",
            ["Status.Found"] = "{0} في {1} مللي ثانية",
            ["Status.Searching.Suffix"] = " …",
            ["Status.Selected"] = "المحدد: {0} · {1}",
            ["Status.Selected.Tip"] = "الحجم الإجمالي للملفات المحددة. وتُستثنى المجلدات عن قصد — فحجم المجلد يشمل أصلاً الملفات التي داخله، وعدّ الاثنين يعدّها مرتين.",
            ["Status.Exporting"] = "جارٍ تصدير {0}…",
            ["Status.Deleting"] = "جارٍ حذف {0}…",

            // ---- indexing progress -----------------------------------------------------------------------------
            ["Index.Reading"] = "جارٍ قراءة الأقراص…",
            ["Index.ReadingCount"] = "جارٍ قراءة الأقراص — {0}",
            ["Index.ReadingPercent"] = "جارٍ قراءة الأقراص — {0}، {1}{2}",
            ["Index.DrivesDone"] = "{0} من {1} محرك أقراص",
            ["Index.Rate"] = " · {0}/ث",
            ["Index.Remaining"] = " · يتبقى نحو {0}",
            ["Index.Volume.Progress"] = "{0}  ({1} من {2})",
            ["Index.Volume.Waiting"] = "في الانتظار",
            ["Index.Volume.Failed"] = "فشل",
            ["Index.Volume.NotSupported"] = "غير مدعوم",
            ["Index.Volume.SizeUnknown"] = "الحجم غير معروف",

            // ---- durations and sizes ------------------------------------------------------------------------------
            ["Duration.Seconds"] = "{0} ث",
            ["Duration.MinutesSeconds"] = "{0} د {1} ث",
            ["Duration.HoursMinutes"] = "{0} س {1} د",
            // Left in Latin script, as Windows and most Arabic software show byte units, and because
            // an Arabic abbreviation next to Western digits reorders badly.
            ["Unit.B"] = "B",
            ["Unit.KB"] = "KB",
            ["Unit.MB"] = "MB",
            ["Unit.GB"] = "GB",
            ["Unit.TB"] = "TB",
            ["Unit.PB"] = "PB",

            // ---- counted things ---------------------------------------------------------------------------------------
            // Six forms each. The pair worth checking is few/many: 3–10 plural, 11–99 singular.
            ["Plural.Results.zero"] = "لا نتائج",
            ["Plural.Results.one"] = "نتيجة واحدة",
            ["Plural.Results.two"] = "نتيجتان",
            ["Plural.Results.few"] = "{0:N0} نتائج",
            ["Plural.Results.many"] = "{0:N0} نتيجة",
            ["Plural.Results.other"] = "{0:N0} نتيجة",

            ["Plural.Files.zero"] = "لا ملفات",
            ["Plural.Files.one"] = "ملف واحد",
            ["Plural.Files.two"] = "ملفان",
            ["Plural.Files.few"] = "{0:N0} ملفات",
            ["Plural.Files.many"] = "{0:N0} ملفًا",
            ["Plural.Files.other"] = "{0:N0} ملف",

            ["Plural.Folders.zero"] = "لا مجلدات",
            ["Plural.Folders.one"] = "مجلد واحد",
            ["Plural.Folders.two"] = "مجلدان",
            ["Plural.Folders.few"] = "{0:N0} مجلدات",
            ["Plural.Folders.many"] = "{0:N0} مجلدًا",
            ["Plural.Folders.other"] = "{0:N0} مجلد",

            ["Plural.Drives.zero"] = "لا محركات أقراص",
            ["Plural.Drives.one"] = "محرك أقراص واحد",
            ["Plural.Drives.two"] = "محركا أقراص",
            ["Plural.Drives.few"] = "{0:N0} محركات أقراص",
            ["Plural.Drives.many"] = "{0:N0} محرك أقراص",
            ["Plural.Drives.other"] = "{0:N0} محرك أقراص",

            ["Plural.Items.zero"] = "لا عناصر",
            ["Plural.Items.one"] = "عنصر واحد",
            ["Plural.Items.two"] = "عنصران",
            ["Plural.Items.few"] = "{0:N0} عناصر",
            ["Plural.Items.many"] = "{0:N0} عنصرًا",
            ["Plural.Items.other"] = "{0:N0} عنصر",

            ["Plural.Rows.zero"] = "لا صفوف",
            ["Plural.Rows.one"] = "صف واحد",
            ["Plural.Rows.two"] = "صفان",
            ["Plural.Rows.few"] = "{0:N0} صفوف",
            ["Plural.Rows.many"] = "{0:N0} صفًا",
            ["Plural.Rows.other"] = "{0:N0} صف",

            ["Plural.DemoRows.zero"] = "لا صفوف اصطناعية",
            ["Plural.DemoRows.one"] = "صف اصطناعي واحد",
            ["Plural.DemoRows.two"] = "صفان اصطناعيان",
            ["Plural.DemoRows.few"] = "{0:N0} صفوف اصطناعية",
            ["Plural.DemoRows.many"] = "{0:N0} صفًا اصطناعيًا",
            ["Plural.DemoRows.other"] = "{0:N0} صف اصطناعي",

            ["Contents.FilesAndFolders"] = "{0}، {1}",

            // ---- dialogs -----------------------------------------------------------------------------------------------
            ["Dialog.BulkOpen.Verb"] = "فتح",
            ["Dialog.BulkExplorer.Verb"] = "فتح نوافذ مستكشف لـ",
            ["Dialog.BulkConfirm"] = "سيؤدي هذا إلى {0} {1}. هل تريد المتابعة؟",
            ["Dialog.NothingToExport"] = "لا يوجد ما يمكن تصديره.",
            ["Dialog.AlreadyRunning"] = "‏Spotter قيد التشغيل بالفعل، لكن هذه النسخة لم تتمكن من إظهار نافذته في المقدمة.\n\nقد يكون تابعًا لحساب مستخدم آخر، أو لا يزال في طور البدء.",
            ["Menu.NewInstance"] = "نسخة جديدة",
            ["Tray.NewInstance"] = "نسخة جديدة",
            ["NewInstance.Tip"] = "فتح نافذة ثانية من Spotter ببحث خاص بها.",
            ["Window.ExtraInstance"] = "‏Spotter — نسخة إضافية",
            ["Dialog.NewInstanceFailed"] = "لم يتمكّن البرنامج من تشغيل نسخة أخرى. السبب مُدوَّن في ملف السجل.",
            ["Dialog.Export.Filter"] = "ملف CSV (*.csv)|*.csv|ملف نصي (*.txt)|*.txt",
            ["Dialog.Export.FileName"] = "نتائج Spotter.csv",
            ["Dialog.Export.CsvHeader"] = "الاسم,المسار,الحجم,تاريخ التعديل",
            ["Dialog.Export.Done"] = "تم تصدير {0}.\n\nهل تريد فتحه الآن؟",
            ["Dialog.Export.Failed"] = "فشل التصدير: {0}",
            ["Dialog.Delete.Permanent"] = "الحذف النهائي",
            ["Dialog.Delete.Recycle"] = "النقل إلى سلة المحذوفات",
            // "لـ" is attached to {1} with no space, which is how the preposition is written in
            // Arabic — "لـ3 عناصر", not "لـ 3 عناصر". The English template puts nothing there.
            ["Dialog.Delete.Confirm"] = "هل تريد بالتأكيد {0} لـ{1}؟",
            ["Dialog.Delete.Failed"] = "تعذّر حذف {0}.",

            // ---- notification area ----------------------------------------------------------------------------------------
            ["Tray.Show"] = "إظهار Spotter",
            ["Tray.Exit"] = "خروج",
            ["Tray.Hidden.NoHotkey"] = "لا يزال قيد التشغيل — انقر نقرًا مزدوجًا على هذا الرمز لإعادته.",
            ["Tray.Hidden.Hotkey"] = "لا يزال قيد التشغيل — اضغط {0} أو انقر نقرًا مزدوجًا على هذا الرمز.",

            // ---- engine-reported problems -------------------------------------------------------------------------------------
            ["Problem.Name"] = "اسم الملف — {0}",
            ["Problem.Folder"] = "في المجلد — {0}",
            ["Problem.Size"] = "الحجم — {0}",
            ["Problem.NoKinds"] = "فعّل «الملفات» أو «المجلدات» — فمع إيقاف الاثنين لا يوجد ما يمكن إيجاده.",
            ["Problem.NoTypes"] = "لم يُحدَّد أي نوع ملف — افتح القائمة المجاورة لـ«الملفات» وحدِّد نوعًا، أو فعّل «المجلدات».",
            ["Problem.Unreadable"] = "غير قابل للقراءة",

            // ---- options window ---------------------------------------------------------------------------------------------------
            ["Options.Title"] = "خيارات",

            ["Options.Language"] = "اللغة",
            ["Options.Language.Hint"] = "يسري فورًا. وتبقى تنسيقات الأرقام والتواريخ تابعة لإعدادات المنطقة في Windows، وهي إعداد منفصل هناك.",
            ["Options.Language.System"] = "مثل Windows",
            ["Options.Language.SystemDetail"] = "مثل Windows  ({0})",
            ["Options.Language.Unsupported"] = "‏Windows مضبوط على لغة لا يوفرها Spotter بعد، لذلك يستخدم الإنجليزية.",

            ["Options.Searching"] = "البحث",
            ["Options.SearchAsYouType"] = "البحث أثناء الكتابة",
            ["Options.RememberFilters"] = "تذكّر مفتاحَي «الملفات» و«المجلدات» وأنواع الملفات",
            ["Options.RememberFilters.Hint"] = "عند الإيقاف: يبدأ المفتاحان مُشغَّلين في كل تشغيل. النافذة التي تُفتح وأحدهما مُطفأ لا تجد شيئًا من ذلك النوع، فتبدو كأن الملف غير موجود أصلًا. وقائمة أنواع الملفات تتبع الإعداد نفسه.",
            ["Options.SearchAsYouType.On"] = "يبدأ بحث جديد مع كل حرف تكتبه ومع كل تغيير في الإعدادات.",
            ["Options.SearchAsYouType.Off"] = "لا يُجرى أي بحث حتى تضغط «بحث» أو Enter. مفيد على جهاز مشغول، أو عندما يطابق الاستعلام معظم القرص.",

            ["Options.Hotkey"] = "مفتاح اختصار عام",
            ["Options.Hotkey.Hint"] = "اضغط التركيبة التي تريدها ثم «حفظ». تُظهر Spotter في المقدمة من أي مكان، وضغطها مرة أخرى يخفيه.",
            ["Options.Hotkey.Placeholder"] = "انقر هنا واضغط تركيبة",
            ["Options.Hotkey.NeedsModifier"] = "أضف واحدًا على الأقل من Ctrl أو Alt أو Shift أو Win — فالمفتاح المنفرد سيُعترَض في كل مكان تكتب فيه.",
            ["Options.Hotkey.Disable"] = "تعطيل مفتاح الاختصار",

            ["Options.Tray"] = "منطقة الإعلامات",
            ["Options.Tray.Show"] = "إظهار رمز في منطقة الإعلامات",
            ["Options.Tray.Minimize"] = "التصغير إلى منطقة الإعلامات بدلاً من شريط المهام",
            ["Options.Tray.Close"] = "إغلاق النافذة يُبقي البرنامج يعمل في منطقة الإعلامات",

            ["Options.Appearance"] = "المظهر",
            ["Options.Theme.Auto"] = "مثل Windows",
            ["Options.Theme.Dark"] = "داكن",
            ["Options.Theme.Light"] = "فاتح",

            ["Options.Size"] = "حجم النافذة",
            ["Options.Size.Hint"] = "يفتح Spotter في الربع العلوي الأيسر من الشاشة، ويرسم النصوص وعناصر التحكم بحجم يناسب حجم تلك الشاشة، لا عدد وحدات البكسل فيها.",
            ["Options.Size.Auto"] = "الملاءمة لهذه الشاشة",
            ["Options.Size.Auto.Detail"] = "شاشتك تعطي {0}%. أزل التحديد لتثبيت حجم من اختيارك.",
            ["Options.Size.Percent"] = "{0}%",
            ["Options.Size.Remember"] = "تذكّر المكان الذي تترك فيه النافذة",
            ["Options.Size.Remember.Hint"] = "يعود الحجم والموضع والتكبير كما تركتها. الحالة الوحيدة التي لا تُحفظ أبدًا هي نافذة تُركت خارج الشاشة كلها تقريبًا، إذ لن يبقى سبيل لسحبها إلى الداخل مرة أخرى.",
            ["Options.Size.Reset"] = "إعادة تعيين الحجم والموضع",
            ["Options.Size.Reset.Tip"] = "يعيد النافذة فورًا إلى الربع العلوي الأيسر من الشاشة بحجمها الافتراضي.",

            ["Options.Storage"] = "ما يحفظه هذا التطبيق على جهازك",
            ["Options.Storage.Hint"] = "لا يُكتب أي شيء آخر في أي مكان. ولإزالة كل شيء: احذف ملفات Spotter من هذين المجلدين وأزل المهمة المذكورة أدناه.",
            ["Options.Storage.Settings"] = "الإعدادات",
            ["Options.Storage.AppLog"] = "سجل التطبيق",
            ["Options.Storage.EngineLog"] = "سجل المحرك",
            ["Options.Storage.EngineLog.Suffix"] = "   (بالإضافة إلى ملفات SpotterLog.N.txt.zip المؤرشفة)",
            ["Options.Storage.Task"] = "مهمة مجدولة",
            ["Options.Storage.Open.Tip"] = "إظهار هذا الملف في مستكشف الملفات.",
            ["Options.Storage.Task.Registered"] = "برنامج جدولة المهام ← {0}\nيتيح لهذه النسخة البدء بصلاحيات مرتفعة دون مطالبة UAC كل مرة.",
            ["Options.Storage.Task.Missing"] = "غير مسجَّلة. وبدلاً من ذلك يطلب Spotter صلاحيات المسؤول عبر مطالبة UAC.",
            ["Options.Storage.Task.Remove.Tip"] = "يحذف المهمة. وسيطلب Spotter صلاحيات المسؤول عبر مطالبة UAC من جديد في المرة القادمة.",
            ["Options.Storage.Task.Remove.Confirm"] = "هل تريد إزالة المهمة المجدولة ‏«{0}»؟\n\n‏سيتابع Spotter عمله — لكنه سيطلب صلاحيات المسؤول عبر مطالبة UAC كل مرة، ويمكنه تسجيل المهمة مرة أخرى بعد موافقتك على إحداها.",
            ["Options.Storage.Task.Remove.Failed"] = "تعذّرت إزالة المهمة. فإزالتها تتطلب صلاحيات المسؤول.",
            ["Options.Storage.ExplorerFailed"] = "تعذّر فتح مستكشف الملفات: {0}",

            ["Options.About"] = "حول",
            ["Options.About.Version"] = "Spotter {0}",
            ["Options.About.Text"] = "بحث فوري عن الملفات في Windows‏. يقرأ Spotter جدول الملفات الرئيسي في NTFS مباشرةً بدلاً من المرور على المجلدات، ولهذا يحتاج إلى صلاحيات المسؤول، ولهذا تظهر النتائج بسرعة كتابتك.",
        };
}
