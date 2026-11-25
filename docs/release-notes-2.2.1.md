# Release notes — Spotter 2.2.1

Paste into the GitHub release description. English first, Hebrew below, as GitHub renders both
in one body.

---

## 🔎 Spotter 2.2.1

Coming from 2.1.0, this is two releases at once: **2.2.0 was never published here**, so
everything in it arrives with this one.

### New in 2.2.1


- **Filter the results by what kind of file they are.** **Files** is now a switch with a panel
  behind it: the switch decides whether files appear at all, and the chevron beside it opens
  eight buckets — images, video, audio, documents, archives, code and settings, programs and
  installers, everything else — plus a box for any extension the buckets do not cover. Hovering a
  bucket lists what is in it. The label carries the count while the filter is narrowed, so the
  row reads **Files (2)** rather than looking untouched while half the answers are missing.

  The custom extensions are an extra bucket rather than a restriction on the ticked ones: to find
  only `.iso`, untick the categories. The whole thing resets on every launch together with Files
  and Folders, under the same option in **Options → Searching**.

### New in 2.2.0, published here for the first time

- **A refresh button beside the search field.** An index knows every file on the machine — but it
  cannot know about a file that has not been written yet, so waiting for one means running the
  same search again and again. F5 always did that; now so does one button at the end of the name
  field, which keeps the caret where it was. It greys out when there is nothing to search for.
- **More than one window at once.** `Ctrl+N`, the **+** beside minimize and close, **New
  instance** in the results background menu, or the same item in the tray menu, opens a second
  Spotter with its own search. Launching the shortcut again still brings back the window you
  already have, which is what it is for; only those four ask for a new one.
- **Files and Folders start every run switched on.** Narrowing a search to one kind is nearly
  always about that search, and a window that opens with Folders still off finds no folder
  whatever you type and says nothing about it — which looks exactly like the thing not being
  there, the one question you opened Spotter to answer. **Options → Searching** brings back the
  old behaviour.
- **One order for the results, not one per run.** A re-run used to come back sorted differently.
- **The three query fields are shorter**, and so is the title bar: padding nobody asked for,
  returned to the results.

### Fixed

- **A second window no longer opens exactly on top of the first.** It steps clear by one title
  bar, keeps itself inside the work area, and never starts maximized.
- **A second window takes over when the first one closes.** Until now it stayed a second-class
  window for good: no restore hotkey, deaf to a re-launch, and silently writing no settings —
  while still holding the slot that stopped a fresh launch from working. It now picks the role up
  within a few seconds, keeping whatever the window that closed had written.
- **Saving Options or clearing the history in a second window says so** when the settings belong
  to the other window, instead of closing the dialog as though it had worked.
- **The size field's placeholder is readable again** — the name field gives it a quarter of its
  width.
- **Pasting a list of extensions works**, including one copied line-per-line, and anything that
  could never match a file name is dropped rather than counted as an active filter.
- **Dotfiles are found by what they are**: `.env`, `.env.local`, `.gitignore` and the rest sit
  under code and settings.
- **A control character pasted into a search no longer loses your settings.** One could stop
  every save for the rest of the session — window position, column widths, language, history —
  without a word.

### Download

Two files, same program. The only difference is whether .NET is inside the exe or expected on the
PC already.

| File | Size | Take this one if… |
|---|---|---|
| **`Spotter-2.2.1-win-x64-standalone.exe`** | 69 MB | **You just want it to work.** Nothing to install. |
| `Spotter-2.2.1-win-x64-requires-dotnet8.exe` | 8 MB | You have the [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0), or do not mind installing it. |

Either way it is **one file**. Put it on your desktop, a USB stick, or straight into `C:\` and run
it from there — it writes nothing next to itself. Replacing an existing 2.x exe with this one keeps
all your settings.

### Requirements

Windows 10 (1809) or 11, 64-bit.

### Also

Subtitles, macro-enabled Office documents, Matroska audio, internet shortcuts, split RAR volumes
and about forty other extensions the buckets were missing. Keyboard access to the file types
panel. Six languages throughout, as always.

---

<div dir="rtl">

## 🔎 ספוטר 2.2.1

למי שמגיע מ‑2.1.0 אלה שני שחרורים באחד: **2.2.0 מעולם לא פורסם כאן**, אז כל מה שהיה בו מגיע עם זה.

### חדש ב‑2.2.1


- **סינון התוצאות לפי סוג הקובץ.** המתג **קבצים** הוא מעכשיו מתג עם חלונית מאחוריו: המתג קובע אם
  קבצים מופיעים בכלל, והחץ שלידו פותח שמונה סוגים — תמונות, וידאו, אודיו, מסמכים, ארכיונים, קוד
  והגדרות, תוכנות והתקנות, כל השאר — ובנוסף שדה לכל סיומת שהרשימה לא מכסה. ריחוף על סוג מציג מה יש
  בו. כשהסינון מצומצם התווית נושאת את המספר, כך שהשורה אומרת **קבצים (2)** במקום להיראות כאילו לא
  נגעו בה בזמן שחצי מהתשובות חסרות.

  הסיומות בשדה הן תוספת ולא הגבלה על המסומנים: כדי למצוא רק `.iso` יש לבטל את סימון הסוגים. הכול
  מתאפס בכל הפעלה יחד עם קבצים ותיקיות, תחת אותה הגדרה ב**אפשרויות ← חיפוש**.

### חדש ב‑2.2.0, שמתפרסם כאן לראשונה

- **כפתור רענון לייד שדה החיפוש.** אינדקס יודע על כל קובץ במחשב — אבל לא יכול לדעת על קובץ שעוד
  לא נכתב, אז המתנה לכזה פירושה להריץ שוב ושוב את אותו חיפוש. F5 תמיד עשה את זה; עכשיו גם כפתור
  אחד בקצה שדה השם, ששומר את הסמן במקומו. הוא מאפיר כשאין מה לחפש.
- **יותר מחלון אחד.** `Ctrl+N`, ה‑**+** לייד מזעור וסגירה, **מופע חדש** בתפריט הרקע של התוצאות,
  או אותו פריט בתפריט המגש — כל אחד מהם פותח ספוטר נוסף עם חיפוש משלו. הפעלה חוזרת של הקיצור עדיין
  מחזירה את החלון שכבר קיים, וזה בדיוק מה שהיא נועדה לעשות.
- **קבצים ותיקיות נדלקים בכל הפעלה.** צמצום לסוג אחד הוא כמעט תמיד עניין של אותו חיפוש, וחלון
  שנפתח כשתיקיות כבוי לא מוצא שום תיקייה ולא אומר על כך דבר — מה שנראה בדיוק כמו הקובץ שלא קיים,
  השאלה היחידה שבגללה פתחת את ספוטר. **אפשרויות ← חיפוש** מחזיר את ההתנהגות הישנה.
- **סדר אחד לתוצאות, לא סדר לכל הרצה.** הרצה חוזרת נהגה לחזור ממוינת אחרת.
- **שלושת שדות החיפוש נמוכים יותר**, וגם שורת הכותרת: רווח שאף אחד לא ביקש, שחוזר לתוצאות.

### תוקן

- **חלון שני כבר לא נפתח בדיוק מעל הראשון.** הוא מתרחק בשורת כותרת אחת, נשאר בתוך אזור העבודה,
  ולא נפתח ממוקסם.
- **חלון שני מקבל את התפקיד כשהראשון נסגר.** עד עכשיו הוא נשאר חלון סוג ב׳ לתמיד: בלי קיצור
  שחזור, חירש להפעלה חוזרת, ובלי לכתוב הגדרות — בזמן שהוא עדיין מחזיק את הסלוט שמנע מהפעלה חדשה
  לעבוד. עכשיו הוא לוקח את התפקיד תוך כמה שניות, ושומר את מה שהחלון שנסגר הספיק לכתוב.
- **שמירה באפשרויות או ניקוי היסטוריה בחלון שני אומרים את זה** כשההגדרות שייכות לחלון האחר, במקום
  לסגור את הדיאלוג כאילו זה עבד.
- **ה‑placeholder של שדה הגודל קריא שוב** — שדה השם נותן לו רבע מהרוחב שלו.
- **הדבקת רשימת סיומות עובדת**, כולל כזו שהועתקה שורה‑בשורה, וכל מה שלא יכול להתאים לשם קובץ נזרק
  במקום להיספר כסינון פעיל.
- **קבצי נקודה נמצאים לפי מה שהם**: `.env`, `.env.local`, `.gitignore` וחבריהם יושבים תחת קוד
  והגדרות.
- **תו בקרה שהודבק לחיפוש כבר לא מאבד את ההגדרות.** אחד כזה יכול היה לעצור כל שמירה עד סוף הריצה
  — מיקום החלון, רוחבי העמודות, השפה, ההיסטוריה — בלי מילה.

### הורדה

שני קבצים, אותה תוכנה. ההבדל היחיד הוא אם ‎.NET נמצא בתוך ה‑exe או מצופה להיות מותקן כבר.

| קובץ | גודל | קח את זה אם… |
|---|---|---|
| **`Spotter-2.2.1-win-x64-standalone.exe`** | 69MB | **אתה פשוט רוצה שזה יעבוד.** אין מה להתקין. |
| `Spotter-2.2.1-win-x64-requires-dotnet8.exe` | 8MB | יש לך [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0), או שלא אכפת לך להתקין. |

כך או כך זה **קובץ אחד**. שים אותו על שולחן העבודה, על דיסק און קי או ישר ב‑`C:\` — הוא לא כותב
כלום לידו. החלפת exe קיים מגרסה 2.x בזה שומרת את כל ההגדרות.

### דרישות

Windows 10 (1809) או 11, 64 ביט.

### ועוד

כתוביות, מסמכי אופיס עם מאקרו, אודיו Matroska, קיצורי אינטרנט, כרכי RAR מפוצלים ועוד כארבעים
סיומות שחסרו ברשימות. גישה מהמקלדת לחלונית סוגי הקבצים. שש שפות לכל אורך הדרך, כרגיל.

</div>
