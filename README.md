# 🔎 Spotter

Find any file on your PC before you finish typing its name.

Windows Search crawls your disks in the background, misses things, and still makes you wait.
Spotter reads the NTFS master file table directly — the index the filesystem already keeps — so
there is no crawler, no background service, and no waiting for it to "catch up". Every file on
every drive, one keystroke away.

---

## ⬇️ Download

Every release ships **two files**. They are the same program — the only difference is whether
.NET comes inside the exe or has to already be on the PC.

| File | Size | Take this one if… |
|---|---|---|
| `Spotter-<ver>-win-x64-standalone.exe` | ~66 MB | **You just want it to work.** Nothing to install. |
| `Spotter-<ver>-win-x64-requires-dotnet8.exe` | ~8 MB | You already have the [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0), or do not mind installing it. |

<details>
<summary>What is the actual difference?</summary>

C# code cannot run on Windows by itself — it needs the .NET runtime, a set of libraries that runs
it (WPF, the drawing, all of it). The only question is where that comes from.

**standalone** carries the whole runtime inside the executable. Bigger, but it runs on any
Windows 10/11 x64 with nothing preinstalled. The trade-off is that its copy of .NET is frozen at
build time, so security fixes arrive when a new Spotter release is published rather than through
Windows Update.

**requires-dotnet8** contains only Spotter and expects the runtime to already be installed. If it
is not, Windows offers to download it on first launch. Smaller, and it picks up .NET security
updates on its own.

Like a document that needs Word installed, versus the document and Word shipped together.

</details>

Either way, what you download is **one file**. Put it wherever you like — desktop, a USB stick,
straight into `C:\` — and run it from there. It writes nothing next to itself.

---

## Why Spotter?

- **It is genuinely instant.** 8 drives read in 54 seconds at startup, then 855 results for
  `notepad` in **228 ms**. Not "fast for a search tool" — fast enough that you keep typing and
  the list keeps up.
- **It is one file, and it stays one file.** Drop `Spotter.exe` on your desktop, a USB stick, or
  straight into `C:\` and run it from there for good. It writes nothing beside itself — no logs,
  no config, no folder appearing next to it.
- **It closes when you close it.** No tray icon by default, no process left holding your drives.
  Click X and it is gone, so removing a USB stick still works.
- **It tells you where it keeps things.** Options lists every file and scheduled task it creates
  on your PC, with a button to open each one and to remove them.
- **It speaks your language.** Six of them, including Hebrew and Arabic laid out properly right
  to left — not a mirrored English window.

---

## ✨ Features

🔍 **Search as you type** • 📁 **Filter by name, folder and size** • 🔤 **Per-field case & regex**
• 📏 **Real size grammar** (`> 50 MB`, `~1 GB`, `100 MB To 1 GB`) • 📂 **Folder sizes and contents**
• 🕘 **Recent searches** • ↕️ **Natural sorting** (`file2` before `file10`) • 📤 **CSV & text export**
• 🗑️ **Delete to Recycle Bin** • 🖱️ **Drag results into any app** • ⌨️ **Global hotkey**
• 🌍 **6 languages, 2 right-to-left** • 🌗 **Follows your Windows theme**
• 🖥️ **Sizes itself to your screen**

### A few worth explaining

**Folder sizes.** Folders show what is actually inside them — total bytes, file count, subfolder
count — because the index adds every file to its parents as it reads. A size filter therefore
matches folders too.

**Per-field options.** Case sensitivity and regular expressions belong to the *field*, not the
search. A case-sensitive filename inside a loosely matched folder is a different question from
the reverse, and both are one click away.

**It sizes itself to your screen.** Spotter opens in the top-left quarter of your display, inset
from the corner and from the middle, and draws its text and controls at a size that suits how big
that screen physically is — not how many pixels are on it, so a 4K panel does not get a window
twice the size of the 1080p one beside it. Whatever you then resize, move or maximize it to comes
back next time, and Options can lock the scale anywhere from 50% to 250% instead.

**It tells you when a filter is wrong.** A broken regular expression or an unparseable size turns
that field red and says why, instead of quietly returning zero results that look exactly like
"nothing matched".

---

## 📸 Screenshot

![Spotter](docs/screenshot.png)

---

## 📋 Requirements

**To run**
- Windows 10 (1809) or 11, 64-bit
- [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) — or grab the
  self-contained build, which needs nothing
- Administrator rights: reading the file table needs a raw volume handle. Spotter asks once, then
  registers a scheduled task so later launches from the same folder start without a prompt.

**To build**
- .NET 8 SDK
- A search engine — see below

---

## 🚀 Getting Started

```bash
# Debug build with symbols
build-debug.bat

# Both release files, named and ready to attach to a GitHub release
publish.bat
```

`publish.bat` always builds **both** flavours and leaves exactly two files in `publish\`:

```
Spotter-2.0.0-win-x64-standalone.exe          65.8 MB
Spotter-2.0.0-win-x64-requires-dotnet8.exe     7.7 MB
```

The version in the names is read from the `.csproj`, so it can never drift from what the exe
reports. Each is a single file: the engine, WPF-UI, the runtime configuration, the debug symbols
and WPF's native libraries are all bundled inside.

Then just start typing. Useful switches while developing:

```bash
Spotter.exe --demo 250000   # fake rows: no admin, no engine, for exercising the UI
Spotter.exe --noelevate     # stay an ordinary process, so it can be focused and killed normally
Spotter.exe --lang he       # force a language for one run without changing the saved setting
Spotter.exe --search TERM   # preload the search box
Spotter.exe --bench 3       # re-run the startup query and log how long each pass took
```

---

## ⚠️ The search engine is not in this repository

`src/Spotter` is the application — the window, the search plumbing, the six languages, the
settings. That is what lives here, and it is what the licence covers.

The component that actually reads the file table sits in `engine/`, which is excluded by
`.gitignore`. **A fresh clone will not build until that project exists**: `Spotter.csproj` has a
`ProjectReference` to `..\..\engine\Core\Spotter.Core.csproj`, so the build stops with a
missing-project error rather than anything mysterious.

To build it yourself, supply an engine behind the same small surface — start indexing, report
progress per volume, run a compiled query, stream results back. [`SearchEngine.cs`](src/Spotter/SearchEngine.cs)
is the entire boundary. It is one file, and it is the only place in the app that knows what the
engine looks like.

---

## 🛠️ Tech Stack

- **WPF** on **.NET 8**, with [WPF-UI](https://github.com/lepoco/wpfui) for Fluent styling and a
  Mica backdrop
- **Virtualized results** — rows are built on demand and capped, so a million-row result set
  costs the same as a hundred
- **Staged updates** — the engine answers on its own threads by the hundred thousand; results are
  queued and drained on a timer instead of being marshalled one at a time
- **Compiled localization** — six plain dictionaries in the executable, no satellite assemblies,
  no folders beside the exe

### Where it puts things

| | |
|---|---|
| Settings | `%APPDATA%\Spotter\settings.xml` |
| Log | `%LOCALAPPDATA%\Spotter\Spotter.log` |
| Engine log (off by default) | `%LOCALAPPDATA%\Spotter\Spotter.Engine.log` |
| Elevation task | Task Scheduler, named after the exe's path — moving Spotter deletes the old one |

**Beside the executable: nothing.** Options lists all of the above with a button to open each one,
and to remove the scheduled task.

Design decisions, the measurements behind them, and the mistakes found along the way are written
up in [src/Spotter/README.md](src/Spotter/README.md).

---

## 🗺️ Ideas for later

- 🔤 Search inside file contents
- 🖼️ Preview pane
- 📌 Saved searches
- 🌐 More languages

---

## 📄 License

Licensed under the [Apache License 2.0](LICENSE) — use it, change it, ship it, including
commercially, as long as you keep the notice and state what you changed.

---

## 👤 Author

Made by **[ori-halevi](https://github.com/ori-halevi)**.

Contributions, ideas, and issues are welcome.

---

<div dir="rtl">

## 🇮🇱 בעברית — הדברים החשובים

**Spotter מוצא כל קובץ במחשב לפני שסיימת להקליד את שמו.**

הוא קורא ישירות את טבלת הקבצים של NTFS — האינדקס שמערכת הקבצים כבר מנהלת — במקום לסרוק תיקיות
ברקע. לכן אין סורק, אין שירות שרץ כל הזמן, ואין המתנה. 8 כוננים נקראים ב־54 שניות בהפעלה, ואחר כך
כל חיפוש חוזר תוך רבע שנייה.

### מה להוריד

בכל שחרור יש **שני קבצים**, אותה תוכנה בדיוק. ההבדל היחיד הוא אם מנוע ההרצה של .NET ארוז בפנים או
אמור להיות מותקן כבר במחשב:

| קובץ | גודל | מתי לבחור בו |
|---|---|---|
| `standalone` | ‎66 MB | **סתם שיעבוד.** אין מה להתקין. |
| `requires-dotnet8` | ‎8 MB | אם כבר מותקן אצלך .NET 8 Desktop Runtime, או שלא אכפת לך להתקין |

### קובץ אחד, בכל מקום

מה שמורידים הוא **קובץ בודד**. אפשר לשים אותו על שולחן העבודה, על דיסק־און־קי או ישר ב־`C:\`
ולהפעיל משם דרך קבע. הוא **לא כותב שום דבר לידו** — לא לוגים, לא הגדרות, ולא תיקייה שצצה פתאום.

מה שהוא כן שומר, שמור במקומות המקובלים במחשב, וחלון ההגדרות מציג את כולם עם כפתור שפותח כל אחד:

| | |
|---|---|
| הגדרות | `%APPDATA%\Spotter\settings.xml` |
| לוג | `%LOCALAPPDATA%\Spotter\Spotter.log` |
| משימה מתוזמנת | לפי נתיב הקובץ — העברה של הקובץ מוחקת את הישנה |

### הרשאות מנהל

קריאת טבלת הקבצים דורשת גישה גולמית לכונן, ולכן גם הרשאות מנהל. בהפעלה הראשונה תופיע בקשת UAC
אחת; אחריה נרשמת משימה מתוזמנת עבור המיקום הזה, וכל הפעלה משם כבר לא תשאל.

### עוד דברים ששווה לדעת

- **סגירה היא סגירה.** אין אייקון במגש כברירת מחדל ואין תהליך שנשאר ברקע — מה שחשוב, כי כל עוד
  התוכנה פתוחה היא מחזיקה ידית על כל כונן, וזה חוסם הוצאה בטוחה של דיסק־און־קי.
- **החלון מתאים את עצמו למסך.** הוא נפתח ברבע השמאלי־העליון, במרחק מהפינה ומהאמצע, והטקסט
  והפקדים מצוירים בגודל שמתאים לגודל הפיזי של המסך — לא לכמות הפיקסלים שבו. כל שינוי גודל
  או מיקום נשמר לפעם הבאה, ובאפשרויות אפשר לקבע את הקנה־מידה בעצמך בין 50% ל־250%.
- **שש שפות**, כולל עברית וערבית בפריסה נכונה מימין לשמאל — לא חלון אנגלי הפוך.
- **גם לתיקיות יש גודל** ומספר פריטים, ולכן מסנן גודל תופס גם אותן.
- **קלט שגוי אומר את זה.** ביטוי רגולרי שבור או גודל שלא מתפרש צובעים את השדה באדום ומסבירים,
  במקום להחזיר אפס תוצאות שנראות בדיוק כמו "לא נמצא כלום".

</div>
