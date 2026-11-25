# Release notes — Spotter 2.2.0

Paste into the GitHub release description. English first, Hebrew below, as GitHub renders both
in one body.

---

## 🔎 Spotter 2.2.0

Four small things, all of them about waiting less.

### New

- **A refresh button beside the search field.** An index knows every file on the machine — but it
  cannot know about a file that has not been written yet, so waiting for one means running the
  same search again and again. F5 always did that; now so does one button at the end of the name
  field, which keeps the caret where it was so you can carry on typing. It greys out when there is
  nothing to search for.
- **More than one window at once.** `Ctrl+N`, **New instance** in the results background menu, or
  the same item in the tray menu, opens a second Spotter with its own search — for comparing two
  sets of results side by side, or keeping one search while you run another. Launching the
  shortcut again still brings back the window you already have, which is what it is for; only
  these three ask for a new one. Each extra window reads the drives again, which is why it is
  asked for rather than assumed, and it leaves the first window in charge of the restore hotkey
  and of your saved settings.
- **Files and Folders start every run switched on.** Narrowing a search to one kind is nearly
  always about that search. A window that opens with Folders still off finds no folder whatever
  you type and says nothing about it — which looks exactly like the thing not being there, the one
  question you opened Spotter to answer. **Options → Searching → Remember the Files and Folders
  switches** brings back the old behaviour, carrying on from whatever was last on screen.
- **The three query fields are shorter.** Six pixels of padding nobody asked for, above the
  results, returned to the results.

### Upgrading

Settings, hotkey, language and search history all carry over. The Files and Folders switches are
the one thing that will look different: both come up on, every run, until you tick the new option
in Options.

### Download

Two files, same program. The only difference is whether .NET is inside the exe or expected on the
PC already.

| File | Size | Take this one if… |
|---|---|---|
| **`Spotter-2.2.0-win-x64-standalone.exe`** | 66 MB | **You just want it to work.** Nothing to install. |
| `Spotter-2.2.0-win-x64-requires-dotnet8.exe` | 8 MB | You have the [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0), or do not mind installing it. |

Either way it is **one file**. Put it on your desktop, a USB stick, or straight into `C:\` and run
it from there — it writes nothing next to itself. Replacing an existing 2.x exe with this one keeps
all your settings.

### Requirements

Windows 10 (1809) or 11, 64-bit.

---

<div dir="rtl">

## 🔎 ‏Spotter 2.2.0

ארבעה דברים קטנים, וכולם על פחות המתנה.

### חדש

- **כפתור רענון לצד שדה החיפוש.** האינדקס מכיר כל קובץ במחשב — אבל אין שום דרך שיכיר קובץ שעוד
  לא נכתב, ולכן המתנה לקובץ כזה היא הרצה של אותו חיפוש שוב ושוב. ‏F5 תמיד עשה את זה; עכשיו גם
  כפתור אחד בקצה שדה השם, שמשאיר את הסמן במקומו כדי שאפשר יהיה להמשיך להקליד. הוא מושתק כשאין
  מה לחפש.
- **יותר מחלון אחד בו־זמנית.** ‏`Ctrl+N`, **מופע חדש** בתפריט ההקשר של רקע התוצאות, או אותו פריט
  בתפריט המגש — כל אחד מהם פותח Spotter נוסף עם חיפוש משלו, להשוואה בין שתי רשימות תוצאות או
  לשמירת חיפוש אחד בזמן שמריצים אחר. הרצה חוזרת של קיצור הדרך עדיין מקפיצה לקדמה את החלון הקיים,
  שזו בדיוק מטרתה; רק שלוש הדרכים האלה מבקשות חלון חדש. כל חלון נוסף קורא את הכוננים מחדש, ולכן
  מבקשים אותו ולא מקבלים אותו מאליו, והוא משאיר לחלון הראשון את מקש הקיצור ואת ההגדרות השמורות.
- **"קבצים" ו"תיקיות" נדלקים בכל הפעלה.** צמצום חיפוש לסוג אחד הוא כמעט תמיד עניין של אותו חיפוש.
  חלון שנפתח כש"תיקיות" כבוי לא מוצא שום תיקייה, מה שלא יוקלד, ולא אומר על כך מילה — וזה נראה
  בדיוק כמו "הדבר לא קיים", שזו השאלה היחידה שבגללה פתחת את Spotter. **אפשרויות ← חיפוש ← לזכור
  את המתגים קבצים ותיקיות** מחזיר את ההתנהגות הקודמת, וממשיך ממה שהיה אחרון על המסך.
- **שלושת שדות החיפוש נמוכים יותר.** שישה פיקסלים של ריפוד שאיש לא ביקש, מעל התוצאות, חזרו
  לתוצאות.

### שדרוג

ההגדרות, מקש הקיצור, השפה והיסטוריית החיפושים עוברים כרגיל. הדבר היחיד שייראה אחרת הוא המתגים
"קבצים" ו"תיקיות": שניהם יעלו דולקים בכל הפעלה, עד שתסמן את האפשרות החדשה באפשרויות.

### מה להוריד

שני קבצים, אותה תוכנה. ההבדל היחיד הוא אם מנוע ההרצה של .NET ארוז בפנים או אמור להיות מותקן כבר:

| קובץ | גודל | מתי לבחור בו |
|---|---|---|
| **`Spotter-2.2.0-win-x64-standalone.exe`** | ‎66 MB | **סתם שיעבוד.** אין מה להתקין. |
| `Spotter-2.2.0-win-x64-requires-dotnet8.exe` | ‎8 MB | אם מותקן אצלך [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0), או שלא אכפת לך להתקין |

כך או כך זה **קובץ בודד**. אפשר לשים אותו על שולחן העבודה, על דיסק־און־קי או ישר ב־`C:\` ולהפעיל
משם — הוא לא כותב שום דבר לידו. החלפת exe קיים של 2.x בקובץ הזה שומרת על כל ההגדרות שלך.

### דרישות

‏Windows 10 (1809) או 11, ‏64 סיביות.

</div>
