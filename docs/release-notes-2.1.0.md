# Release notes — Spotter 2.1.0

Paste into the GitHub release description. English first, Hebrew below, as GitHub renders both
in one body.

---

## 🔎 Spotter 2.1.0

Spotter now fits your screen instead of filling it.

### New

- **It opens in the top-left quarter of your screen**, inset from the corner and from the middle,
  so you can see the desktop around it and the other three quarters stay clear for whatever you
  are searching *for*. It never opens covering the whole display.
- **Text, controls and icons are drawn at a size that suits how big your screen physically is** —
  not how many pixels are on it. A 27-inch 4K panel that Windows drives at 150% gets the same
  size as the 27-inch 1080p panel next to it, because it is the same size. One measurement moves
  everything together: text, padding, icons, row heights, the title bar, tooltips and menus.
- **Options → Window size** gives you the whole thing. Leave it matching your screen, or untick
  and lock any scale from 50% to 250%. The slider previews live on the window behind it, and
  Cancel puts it back.
- **Whatever you resize, move or maximize it to comes back next time**, and can be turned off or
  reset from the same place. The one arrangement never kept is a window left almost entirely off
  the screen — there would be no way to drag it back.
- **All seven columns now share out the width of the window**, so every one of them is on screen
  with its name whatever size the window is. Dragging any column edge hands the widths back to
  you for good — and they are remembered; **Reset all column widths** returns them to sharing.
- **A dot, an underscore or a hyphen in the file name counts as a gap between words.** Searching
  `Family.Guy` now also finds `Family Guy` and `Family_Guy`, because which one a file used is an
  accident of whoever named it. `*.dll`, `.dll` and `v1.5` are left exactly as typed, and the whole
  thing can be switched off in the file name's own options menu.

### Fixed

- **Columns past the right edge could not be reached at all when there were no results.** The
  horizontal scrollbar's range comes from the rows, so with an empty list there was nothing to
  scroll — and the columns that did not fit simply could not be brought into view. That was the
  state of the grid before any search had run, which is the first thing anyone sees.
- **A long file name was cut off mid-letter** instead of ending in an ellipsis.
- **Right-clicking a result needed two clicks** — the first only selected the row.
- **Double-clicking the icon at the end of the title bar now closes the window**, as it does in
  every other Windows program. It was maximizing instead.
- **File icons are sharper** at scales above 100%, which is where the shell's 16-pixel icon runs
  out of detail.

### Upgrading

Settings, hotkey, language and search history all carry over. The one thing that does not is the
remembered window position from 2.0.x — it means something different now, so it is dropped once and
the window opens where 2.1.0 puts it. Move it wherever you like and it will stay there.

### Download

Two files, same program. The only difference is whether .NET is inside the exe or expected on the
PC already.

| File | Size | Take this one if… |
|---|---|---|
| **`Spotter-2.1.0-win-x64-standalone.exe`** | 66 MB | **You just want it to work.** Nothing to install. |
| `Spotter-2.1.0-win-x64-requires-dotnet8.exe` | 8 MB | You have the [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0), or do not mind installing it. |

Either way it is **one file**. Put it on your desktop, a USB stick, or straight into `C:\` and run
it from there — it writes nothing next to itself. Replacing an existing 2.0.x exe with this one
keeps all your settings.

### Requirements

Windows 10 (1809) or 11, 64-bit.

---

<div dir="rtl">

## 🔎 ‏Spotter 2.1.0

‏Spotter מתאים את עצמו למסך שלך במקום למלא אותו.

### חדש

- **הוא נפתח ברבע השמאלי־העליון של המסך**, במרחק מהפינה ומהאמצע, כך שרואים את שולחן העבודה
  סביבו ושלושת הרבעים האחרים נשארים פנויים למה שחיפשת. הוא כבר לא נפתח על כל המסך.
- **הטקסט, הפקדים והאייקונים מצוירים בגודל שמתאים לגודל הפיזי של המסך** — לא לכמות הפיקסלים
  שבו. מסך 4K של 27 אינץ' ש־Windows מריץ ב־150% מקבל בדיוק את אותו גודל כמו מסך 1080p של
  27 אינץ' לידו, כי זה אותו גודל. מדידה אחת מזיזה את הכול יחד: טקסט, ריפוד, אייקונים, גובה
  שורות, פס הכותרת, החלוניות והתפריטים.
- **אפשרויות ← גודל החלון** נותן לך שליטה מלאה. אפשר להשאיר התאמה למסך, או להסיר את הסימון
  ולנעול כל קנה־מידה בין 50% ל־250%. הסליידר מציג תצוגה מקדימה חיה על החלון שמאחוריו, וביטול
  מחזיר הכול.
- **כל שינוי גודל, הזזה או הגדלה למסך מלא חוזר בפעם הבאה**, ואפשר לכבות או לאפס את זה מאותו
  מקום. המצב היחיד שלעולם לא נשמר הוא חלון שנשאר כמעט לגמרי מחוץ למסך — לא הייתה שום דרך
  לגרור אותו בחזרה.
- **שבע העמודות מתחלקות ברוחב החלון**, כך שכולן על המסך עם השם שלהן בכל גודל חלון. גרירה של
  גבול עמודה מוסרת לך את השליטה לתמיד — והרוחב נשמר; **איפוס רוחב כל העמודות** מחזיר את החלוקה.
- **נקודה, קו תחתון או מקף בשם הקובץ נחשבים להפרדה בין מילים.** חיפוש `Family.Guy` מוצא עכשיו
  גם `Family Guy` וגם `Family_Guy`, כי איזה מהם קובץ משתמש בו הוא מקרי לגמרי. `*.dll`, `.dll`
  ו־`v1.5` נשארים בדיוק כפי שהוקלדו, ואפשר לכבות את כל העניין בתפריט האפשרויות של שדה השם.

### תוקן

- **עמודות שמעבר לקצה לא היו ניתנות להגעה בכלל כשלא היו תוצאות.** הטווח של פס הגלילה האופקי
  מגיע מהשורות, ולכן ברשימה ריקה לא היה מה לגלול — והעמודות שלא נכנסו פשוט לא ניתנות היו
  להבאה לתצוגה. זה היה מצב הרשת לפני שרץ חיפוש כלשהו, כלומר הדבר הראשון שכל אחד רואה.
- **שם קובץ ארוך נחתך באמצע אות** במקום להסתיים בשלוש נקודות.
- **לחיצה ימנית על תוצאה דרשה שתי לחיצות** — הראשונה רק בחרה את השורה.
- **לחיצה כפולה על האייקון שבקצה שורת הכותרת סוגרת עכשיו את החלון**, כמו בכל תוכנת Windows
  אחרת. קודם היא הגדילה אותו למסך מלא.
- **אייקוני הקבצים חדים יותר** בקנה־מידה מעל 100%, שם נגמר הפירוט לאייקון של 16 פיקסלים
  שמערכת ההפעלה מספקת.

### שדרוג

ההגדרות, מקש הקיצור, השפה והיסטוריית החיפושים עוברים כרגיל. הדבר היחיד שלא עובר הוא מיקום
החלון שנשמר ב־2.0.x — המשמעות שלו שונה עכשיו, ולכן הוא נזרק פעם אחת והחלון נפתח במקום ש־2.1.0
בוחר. תזיז אותו לאן שנוח לך והוא יישאר שם.

### מה להוריד

שני קבצים, אותה תוכנה. ההבדל היחיד הוא אם מנוע ההרצה של .NET ארוז בפנים או אמור להיות מותקן כבר:

| קובץ | גודל | מתי לבחור בו |
|---|---|---|
| **`Spotter-2.1.0-win-x64-standalone.exe`** | ‎66 MB | **סתם שיעבוד.** אין מה להתקין. |
| `Spotter-2.1.0-win-x64-requires-dotnet8.exe` | ‎8 MB | אם מותקן אצלך [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0), או שלא אכפת לך להתקין |

כך או כך זה **קובץ בודד**. אפשר לשים אותו על שולחן העבודה, על דיסק־און־קי או ישר ב־`C:\` ולהפעיל
משם — הוא לא כותב שום דבר לידו. החלפת exe קיים של 2.0.x בקובץ הזה שומרת על כל ההגדרות שלך.

### דרישות

‏Windows 10 (1809) או 11, ‏64 סיביות.

</div>
