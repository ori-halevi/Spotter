# Release notes — Spotter 2.0.1

Paste into the GitHub release description. English first, Hebrew below, as GitHub renders both
in one body.

---

## 🔎 Spotter 2.0.1

A bug-fix release. If your language — or any other setting — kept resetting between launches,
this is the fix.

### Fixed

- **Settings no longer reset between launches.** During startup two Spotter processes briefly
  overlap (the launcher and the elevated copy it hands over to), and they could collide on the
  settings file. A launch that hit that collision came up with defaults — and then saved those
  defaults over your real settings on exit, silently wiping the chosen language, hotkey, window
  layout and search history. Reads and writes now wait out the collision, and a session that
  still could not read the file will never overwrite it.
- **Closing Windows while Spotter sits in the tray no longer loses the session's settings.**
  Shutdown and logoff used to look like an ordinary window close, which close-to-tray
  intercepted — skipping the save. Settings are now saved the moment Windows announces the
  session is ending.

### Download

Two files, same program. The only difference is whether .NET is inside the exe or expected on the
PC already.

| File | Size | Take this one if… |
|---|---|---|
| **`Spotter-2.0.1-win-x64-standalone.exe`** | 66 MB | **You just want it to work.** Nothing to install. |
| `Spotter-2.0.1-win-x64-requires-dotnet8.exe` | 8 MB | You have the [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0), or do not mind installing it. |

Either way it is **one file**. Put it on your desktop, a USB stick, or straight into `C:\` and run
it from there — it writes nothing next to itself. Replacing an existing 2.0.0 exe with this one
keeps all your settings.

### Requirements

Windows 10 (1809) or 11, 64-bit.

---

<div dir="rtl">

## 🔎 ‏Spotter 2.0.1

גרסת תיקוני באגים. אם השפה — או כל הגדרה אחרת — התאפסה לך בין הפעלות, זה התיקון.

### תוקן

- **ההגדרות כבר לא מתאפסות בין הפעלות.** בזמן העלייה חיים לרגע שני תהליכי Spotter במקביל
  (המשגר והעותק המורם שהוא מפעיל), והם עלולים היו להתנגש על קובץ ההגדרות. הפעלה שנתקלה
  בהתנגשות עלתה עם ברירות מחדל — ואז שמרה אותן על הקובץ האמיתי ביציאה, ומחקה בשקט את השפה
  שנבחרה, את מקש הקיצור, את פריסת החלון ואת היסטוריית החיפושים. קריאות וכתיבות ממתינות עכשיו
  עד שההתנגשות חולפת, וסשן שבכל זאת לא הצליח לקרוא את הקובץ לעולם לא ידרוס אותו.
- **כיבוי Windows כש־Spotter יושב במגש כבר לא מאבד את הגדרות הסשן.** כיבוי והתנתקות נראו
  כמו סגירת חלון רגילה, שמצב "סגירה למגש" מיירט — והשמירה דולגה. עכשיו ההגדרות נשמרות ברגע
  ש־Windows מודיע שהסשן מסתיים.

### מה להוריד

שני קבצים, אותה תוכנה. ההבדל היחיד הוא אם מנוע ההרצה של .NET ארוז בפנים או אמור להיות מותקן כבר:

| קובץ | גודל | מתי לבחור בו |
|---|---|---|
| **`Spotter-2.0.1-win-x64-standalone.exe`** | ‎66 MB | **סתם שיעבוד.** אין מה להתקין. |
| `Spotter-2.0.1-win-x64-requires-dotnet8.exe` | ‎8 MB | אם מותקן אצלך [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0), או שלא אכפת לך להתקין |

כך או כך זה **קובץ בודד**. אפשר לשים אותו על שולחן העבודה, על דיסק־און־קי או ישר ב־`C:\` ולהפעיל
משם — הוא לא כותב שום דבר לידו. החלפת exe קיים של 2.0.0 בקובץ הזה שומרת על כל ההגדרות שלך.

### דרישות

‏Windows 10 (1809) או 11, ‏64 סיביות.

</div>
