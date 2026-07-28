# Release notes — Spotter 2.0.0

Paste into the GitHub release description. English first, Hebrew below, as GitHub renders both
in one body.

---

## 🔎 Spotter 2.0.0

Find any file on your PC before you finish typing its name.

Spotter reads the NTFS master file table directly — the index the filesystem already keeps —
instead of crawling folders in the background. No crawler, no background service, no waiting for
it to catch up.

**8 drives indexed in 54 seconds. 855 results for `notepad` in 228 ms.**

### Download

Two files, same program. The only difference is whether .NET is inside the exe or expected on the
PC already.

| File | Size | Take this one if… |
|---|---|---|
| **`Spotter-2.0.0-win-x64-standalone.exe`** | 66 MB | **You just want it to work.** Nothing to install. |
| `Spotter-2.0.0-win-x64-requires-dotnet8.exe` | 8 MB | You have the [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0), or do not mind installing it. |

Either way it is **one file**. Put it on your desktop, a USB stick, or straight into `C:\` and run
it from there — it writes nothing next to itself.

Reading the file table needs administrator rights. You will get one UAC prompt on first launch;
after that Spotter registers a scheduled task for that location and stops asking.

### What is in it

- **Search as you type**, or on demand
- **Name, folder and size filters**, each with its own case-sensitivity and regex switches
- **A real size grammar** — `> 50 MB`, `~1 GB`, `100 MB To 1 GB`
- **Folder sizes and contents**, counted while indexing runs
- **Six languages** — English, Arabic, German, Hebrew, Russian, Spanish — two of them right to
  left, laid out properly rather than mirrored
- Recent searches, natural sorting (`file2` before `file10`), CSV and text export, delete to
  Recycle Bin, drag results into any app, a global hotkey
- **No tray icon by default.** Closing the window ends the process, so removing a USB drive still
  works
- **Options tells you exactly what Spotter keeps on your PC**, with a button to open each location
  and to remove the scheduled task

### Requirements

Windows 10 (1809) or 11, 64-bit.

---

<div dir="rtl">

## 🔎 ‏Spotter 2.0.0

מוצא כל קובץ במחשב לפני שסיימת להקליד את שמו.

‏Spotter קורא ישירות את טבלת הקבצים של NTFS — האינדקס שמערכת הקבצים כבר מנהלת — במקום לסרוק
תיקיות ברקע. אין סורק, אין שירות שרץ כל הזמן, ואין המתנה.

**8 כוננים נקראים ב־54 שניות. ‏855 תוצאות עבור `notepad` ב־228 מילישניות.**

### מה להוריד

שני קבצים, אותה תוכנה. ההבדל היחיד הוא אם מנוע ההרצה של .NET ארוז בפנים או אמור להיות מותקן כבר:

| קובץ | גודל | מתי לבחור בו |
|---|---|---|
| **`Spotter-2.0.0-win-x64-standalone.exe`** | ‎66 MB | **סתם שיעבוד.** אין מה להתקין. |
| `Spotter-2.0.0-win-x64-requires-dotnet8.exe` | ‎8 MB | אם מותקן אצלך [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0), או שלא אכפת לך להתקין |

כך או כך זה **קובץ בודד**. אפשר לשים אותו על שולחן העבודה, על דיסק־און־קי או ישר ב־`C:\` ולהפעיל
משם — הוא לא כותב שום דבר לידו.

קריאת טבלת הקבצים דורשת הרשאות מנהל. בהפעלה הראשונה תופיע בקשת UAC אחת, ואחריה Spotter רושם משימה
מתוזמנת למיקום הזה ומפסיק לשאול.

### מה יש בפנים

- **חיפוש תוך כדי הקלדה**, או לפי דרישה
- **מסנני שם, תיקייה וגודל**, לכל אחד מתגי רגישות לאותיות וביטוי רגולרי משלו
- **תחביר גודל אמיתי** — `> 50 MB`, `~1 GB`, `100 MB To 1 GB`
- **גודל ותוכן לתיקיות**, נספרים תוך כדי האינדוקס
- **שש שפות** — אנגלית, ערבית, גרמנית, עברית, רוסית וספרדית — שתיים מהן מימין לשמאל, בפריסה נכונה
  ולא בשיקוף של חלון אנגלי
- חיפושים אחרונים, מיון טבעי (`file2` לפני `file10`), ייצוא ל־CSV וטקסט, מחיקה לסל המיחזור, גרירת
  תוצאות לכל תוכנה, ומקש קיצור גלובלי
- **אין אייקון במגש כברירת מחדל.** סגירת החלון מסיימת את התהליך, כך שהוצאה בטוחה של דיסק־און־קי
  ממשיכה לעבוד
- **חלון ההגדרות מראה בדיוק מה Spotter שומר במחשב**, עם כפתור שפותח כל מיקום ומסיר את המשימה
  המתוזמנת

### דרישות

‏Windows 10 (1809) או 11, ‏64 סיביות.

</div>
