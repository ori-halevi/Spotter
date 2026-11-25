# Spotter — developer notes

**Version 2.0.0.**

Design decisions, the measurements behind them, and the mistakes found along the way. The
user-facing introduction is in the [repository README](../../README.md).

Spotter is a .NET 8 WPF application using [WPF-UI](https://github.com/lepoco/wpfui) for Fluent
styling and a Mica backdrop. It talks to the file-table engine through one file,
[`SearchEngine.cs`](SearchEngine.cs), and to nothing else in `Spotter.Core` — that boundary is
deliberate and worth keeping.

## Requirements

Windows 10 1809 or later, x64.

Both publish modes produce **one `Spotter.exe` and nothing else** — see `publish.bat`. The default
is framework-dependent (7 MB) and needs the **.NET 8 Desktop Runtime**; without it, launching
produces a Windows dialog offering to download it. `publish.bat full` is self-contained (66 MB)
and needs nothing preinstalled.

Getting to a genuinely single file took four settings, and each of them defaults the other way:
`PublishSingleFile` to bundle the managed assemblies, `DebugType=embedded` in *both* projects so
no `.pdb` survives alongside, `IncludeNativeLibrariesForSelfExtract` for WPF's native DLLs in the
self-contained build, and no `App.config` at all — that one ships as `Spotter.dll.config`.

Long paths are declared in the manifest, so results whose full path is over 260 characters can be
opened, deleted and inspected rather than merely listed.

## Running it

    bin/Release/net8.0-windows/Spotter.exe

Reading the MFT needs a raw volume handle, which needs administrator, so the app relaunches
itself elevated. The manifest stays `asInvoker` rather than `requireAdministrator` — see
`ElevationTask.cs`: the first elevated run registers a scheduled task for *this executable's
path*, and every later launch from that path starts elevated with no prompt at all. Copy the exe
elsewhere and that copy prompts once and registers its own, so the app stays portable.

If elevation is declined the window still opens, says so in the status bar, and offers **Restart
as administrator**.

    Spotter.exe --noelevate

Runs as an ordinary process. Drive reads then fail, but the window can be focused, driven with
synthetic input and killed from a normal developer shell, none of which an elevated one allows.

    Spotter.exe --demo 250000

Development scaffolding. Fills the grid with synthetic rows so the list, icons, sorting and
selection can be exercised with no admin rights and no engine. The `Path` column stays blank in
this mode because path resolution walks the live index and there isn't one.

    Spotter.exe --lang he

Forces a language for one run without touching the saved setting, which is what makes a
right-to-left layout inspectable — `--demo --lang he` gives an unelevated, screenshottable Hebrew
window. Codes are `en`, `ar`, `de`, `es`, `he`, `ru`; anything else falls back to English.

### Opening a result does not run it as administrator

The app runs elevated, so anything it starts with `Process.Start` inherits that token. Pressing
Enter on a result was therefore launching that document, script or executable with full
administrator rights, silently, every time — in a tool whose whole purpose is to put every file on
the machine one keystroke away.

`ShellLaunch.TryOpenAsUser` hands the launch to Explorer instead, through the desktop window's
`IShellDispatch2`. Explorer runs at medium integrity as the logged-on user, so what it starts does
too. If that route is unavailable — Explorer not running, the shell refusing the automation call —
it falls back to starting the process directly and writes a line to the log, so an elevated launch
is never a silent surprise.

Only the launch moves. Reading the MFT still needs the raw volume handle, and that still needs
administrator.

## What is here

| File | Role |
|---|---|
| `MainWindow.xaml(.cs)` | The window: query fields, options, results grid, commands, sorting, history |
| `SearchEngine.cs` | Wraps `GClass134`; owns the staging queue, the drain timer and the thread boundary |
| `Results.cs` | `FileItem` (one row) and `ResultsView` (the virtualizing collection) |
| `ShellIcons.cs` | `SHGetFileInfo` icons, cached per extension, sized for the interface scale |
| `OptionsWindow.xaml(.cs)` | Language, search mode, hotkey picker, tray, theme and window size |
| `UiSettings.cs` | Everything persisted between runs, including the search history |
| `TrayIcon.cs`, `Hotkey.cs` | Notification area, and the global restore hotkey |
| `ElevationTask.cs` | "Ask for administrator once per location, then never again" |
| `Dialogs.cs` | The app's message boxes, in one place so they are right-to-left aware |
| `Log.cs` | Append-only diagnostics under `%LOCALAPPDATA%\Spotter` |
| `Localization/Lang.cs` | Language selection, lookup, plurals, byte units, flow direction |
| `PopupChrome.cs` | Gives tooltips and context menus the reading direction and the scale |
| `Displays.cs` | The attached monitors: usable area and per-monitor DPI |
| `UiScale.cs` | One shared `ScaleTransform` — how large the whole interface is drawn |
| `WindowLayout.cs` | Where a window opens, how small it may get, what is worth remembering |
| `Localization/Strings*.cs` | One table per language: `En`, `Ar`, `De`, `Es`, `He`, `Ru` |

## Three design points worth knowing

**Results are staged, not dispatched.** The engine calls back fire-and-forget on its own worker
threads and never marshals. A query like `.dll` returns ~309,000 hits in about a quarter of a
second; marshalling each one to the UI thread would bury the dispatcher. Results go onto a
`ConcurrentQueue` and a 30 ms `DispatcherTimer` drains them in batches, raising exactly one
collection notification per batch. This is the same shape the WinForms app used, and it is why
both feel instant.

**Rows are built on demand, with a ceiling.** `ResultsView` implements `IList` and creates a
`FileItem` only when WPF's virtualization asks for an index, keeping at most 4,096 of them. The
1.5.1 app cached one `ListViewItem` per row forever, so scrolling a million-row result set held a
million of them. `FileItem` equality follows the underlying engine record so a row keeps its
selection even when the cache evicts and rebuilds the wrapper.

**Indexing reports a real percentage, because the engine knows one.** `GClass134.method_22()`
yields a `(bytesRead, total)` pair per volume, aligned with the names from `method_20()`, and a
negative total is a state rather than a size — `-4` finished, `-5` failed, `-3` not supported,
`-2` waiting, `-1` size unknown. `SearchEngine.GetIndexProgress` aggregates that into a
percentage, a rate and an estimate, which is the difference between "still working" and "about 12
seconds left" over a minute-long wait.

One wrinkle worth keeping: a volume that finishes reports `-4` and stops reporting the size it
gave a moment ago. Without remembering the last real figure per volume, the denominator shrinks
as drives complete and the percentage walks backwards.

**Nothing the engine calls back on is the UI thread, and it swallows what those callbacks throw.**
`SearchEngine` is the boundary: every event it raises is put back on the dispatcher before the
window sees it. Getting this wrong does not crash and does not log — the handler runs until its
first touch of a control, throws there, and the exception disappears into the engine. It cost a
"Ready — N drives" status line and the indexing entry in the log, both of which simply stopped
appearing with nothing to say why.

## Six languages, and what right-to-left actually costs

English, Arabic, German, Hebrew, Russian and Spanish — two of them right to left. The default is the
Windows display language, falling back to English when Windows is set to something not shipped here
— and **Options → Language** says so out loud in that case, because silently showing English looks
like the setting was ignored.

Every language is a plain `Dictionary<string, string>` compiled into the executable. No `.resx`, no
satellite assemblies, no per-language folder beside the exe: this is a single portable executable and
a `de\`, `es\`, `he\`, `ru\` litter would undo that. `StringsEn.cs` is the reference table and the
fallback for anything missing, so a key that is absent everywhere shows up on screen *as the key* —
easier to spot than a blank label.

`{loc:Loc Some.Key}` in XAML resolves to a **binding**, not a string, so changing language updates
open windows in place. Text set from code cannot do that, so `MainWindow.Relocalize` and
`OptionsWindow.Relocalize` re-render it on `Lang.Changed`. The picker applies immediately rather than
on Save — someone who has landed in a language they cannot read needs to see the change to know they
have found the right entry — and Cancel, or the title-bar close, puts it back.

**The UI language and the number/date format are separate settings**, as they are in Windows. Only
`CurrentUICulture` moves; `CurrentCulture` keeps following the regional settings. That is also the
safe choice: the 2014 engine parses `1.5 GB` with the current culture, and forcing German would
change what a decimal point means to it.

Three things that are easy to get wrong and were:

- **Plurals.** Two forms is an English assumption. Russian needs three, and Arabic uses all six CLDR
  categories — with the rule inverting halfway, so 3–10 takes the plural (`5 نتائج`) while 11–99
  takes the *singular* (`25 نتيجة`). `Lang.Count` picks `.zero` / `.one` / `.two` / `.few` / `.many`
  / `.other`, and a language that does not define a form falls through to `.other`.
- **Counted phrases must carry their own adjective.** `Status.Demo` used to wrap a counted phrase in
  a sentence that repeated the noun, giving `DEMO MODE — 400 rows synthetic rows`. The adjective now
  lives inside the plural forms, where it can also agree in gender and number.
- **Byte units are translated too** — Russian Windows says `МБ`, not `MB`. One formatter,
  `Lang.FormatBytes`, so the size column, the selection total and the indexing rate cannot disagree.

### Right to left

`FlowDirection` on the window mirrors it, and WPF mirrors the whole layout coordinate system, so
margins, alignments and docking follow with no help. What WPF also mirrors, and must not, is the
interesting part.

All of it is driven by `LanguageInfo.IsRightToLeft` rather than by anything Hebrew-specific, which is
why adding Arabic as the second right-to-left language needed a string table and one flag — no new
plumbing, and nothing in the XAML to revisit.

**Icons are never mirrored.** WPF flips `Image` content under `RightToLeft`, and a mirrored
file-type icon is just a broken icon, so the result cells pin theirs `LeftToRight`. Nothing else here
is a directional glyph — the toolbar's magnifier, gear, folder and clock read the same either way, so
mirroring them would only damage them. The sort arrows are vertical. What *does* mirror is layout:
the icon sits after the label in Hebrew, the caption buttons move to the top left, the scrollbar to
the left, the Name column to the right — the same as Explorer in Hebrew.

**Machine data is not language.** A path, a size, a date and an extension are pinned `LeftToRight`
and aligned to the column's leading edge, because a right-to-left paragraph direction reorders them:
a trailing separator jumps to the far end. The exception is the file name, the one column whose
content really is language — `FileItem.NameDirection` picks a direction from the name's own first
strong character, the rule HTML calls `dir="auto"`, so `notepad.exe` and a Hebrew name both read
correctly in the same list.

**The size field is left-to-right in every language.** What goes in it is an expression in the
engine's grammar, and in a right-to-left paragraph the bidi algorithm gives `>` the paragraph's
direction and then *mirrors* it — so a Hebrew user typing "greater than" would watch it appear as
"less than", the opposite filter. No arrangement of marks fixes that, because a European number
counts as right-to-left when the punctuation beside it is resolved. Only the base direction does. The
visible cost is that this one field keeps its icon on the left while its neighbours move theirs to
the right; a wrong operator is worse.

In the size tooltip both halves of each line need different directions: the line follows the UI, so
the expression is read first, while the expression itself is a `Run` pinned `LeftToRight` so its
operator survives. Laying the whole line out left-to-right also renders every character correctly —
and puts the meaning before the expression for a Hebrew reader, i.e. backwards. That one is worth
knowing about because it looks fine until someone reads it.

`StringsHe.cs` and `StringsAr.cs` carry a few `‎` LEFT-TO-RIGHT MARKs where a token starts with
punctuation:
without one, `*.dll` reaches a Hebrew reader looking like `dll.*`. They are written as escapes, not
pasted in — a mark that matters and cannot be seen is a mark the next person deletes by accident.
WPF honours `LRM`/`RLM` and **ignores** the explicit embedding controls `LRE`/`PDF`, which is worth
knowing before spending an afternoon on marks that do nothing.

### Popups inherit nothing, and fixing that has a trap

A tooltip, a context menu and a message box are each their own top-level window rather than part of
the window's visual tree, so none of them inherits `FlowDirection`. In Hebrew that showed as a
tooltip whose body was right-aligned sitting under a heading still flush left.

Two fixes that look right and are not, both found by screenshot:

1. **An implicit `Style` with a FlowDirection setter.** Written with
   `BasedOn="{StaticResource {x:Type ToolTip}}"` it appears to extend the theme, but WPF-UI registers
   no implicit style under that key — so the lookup falls through to the *system* theme and every
   tooltip silently reverts to stock light-grey WPF chrome inside a dark Fluent window.
2. **A class handler on `Loaded`.** It never runs. WPF raises `Loaded` through its own broadcast
   helper rather than the routed-event path, so `EventManager` class handlers are not invoked — and
   it fails without a word.

`PopupChrome` uses the *opening* events instead, which are ordinary routed events raised on the
owner before the popup is shown, so placement is computed from the direction rather than corrected
after it. It sets a local value and leaves Style and Template alone. A popup can opt out with
`PopupChrome.SetFollow(x, false)` — the indexing tooltip does, being a table of volume names and
byte counts.

The same boundary is why `PopupChrome` also hands each popup the interface scale: a
`LayoutTransform` on a window's content no more reaches a context menu than a `FlowDirection` does.

`MessageBox` is a Win32 dialog and understands none of this, but it does understand
`MessageBoxOptions.RtlReading`, which is why every message box in the app goes through `Dialogs`.

Keyboard gestures printed in menus (`Ctrl+C`, `Del`, `Alt+Enter`) stay in Latin in every language,
because that is what is printed on the key. Only the ones that are words are translated.

### Verified by looking at it

English, Hebrew, Russian and Arabic were each run under `--demo --lang <code>` and screenshotted: the
grid, the toolbar, the status line, the size tooltip, a result context menu and the Options window.
That is how the duplicated noun, the mirrored `>`, the reversed tooltip line order and the light-grey
tooltip were all found — none of them fails a build, and none is visible in the source.

One trap when capturing an RTL window: WPF sets `WS_EX_LAYOUTRTL` on the HWND, and GDI mirrors
`PrintWindow` output through it, so the capture comes back with every glyph reversed. Screen capture
does not do this. If a screenshot of a Hebrew window looks like it was held up to a mirror, it is the
capture, not the app.

## Diagnostics

`%LOCALAPPDATA%\Spotter\Spotter.log`. Records indexing time, the volumes found
and every completed search with its result count and duration. It is how an elevated run gets
observed, because an ordinary process cannot drive one: UIPI blocks input injection into a
higher-integrity window, and `SetForegroundWindow` is refused. (Screen *capture* does work — the
composited desktop is readable regardless of integrity level. It is input, not pixels, that is
blocked.)

Not next to the exe, which is where it used to go. Under Program Files an ordinary user cannot
write there at all, and when the app *is* elevated it creates a file the user then cannot delete.

### The engine's own log is off

Separately from the above, the 2014 engine has its own logger, and it defaults to on. It records
**every USN journal event on every volume** — the name of every file created, deleted or renamed
anywhere on the machine — into the install directory.

Its rotation is the part that has to be seen to be believed. Turning it back on for one 100-second
verification run produced a 6.5 MB log **plus 118 numbered zip archives**, all beside the exe.

It is off, and off in the *code* — `Spotter.Utils.Settings` declares `LogEnabled` as `False` via
`DefaultSettingValue`. It used to be on by default and switched off by an `App.config` shipped
beside the exe, which stopped being an option the moment Spotter became a single file: delete the
sidecar and the dangerous default comes back. A safe default has to be the built-in one.

Set it to `True` there when you need to see what the engine is doing. It writes to
`%LOCALAPPDATA%\Spotter` — `GClass95` resolves a bare file name under per-user data rather than
`AppContext.BaseDirectory`, so even switched on it cannot litter the folder Spotter was dropped
into.

`--search <term>` preloads the search box at startup, which is what makes an automated elevated
check possible at all.

Writes are best-effort: `Log.Write` swallows its own failures so diagnostics can never take the
app down. Worth knowing when a line you expected is not there — absence is not evidence that the
code did not run.

## Performance, measured

`--bench N` re-runs the startup query N more times and logs each. That is how the numbers below
were obtained, and it is worth re-running after any change to the search path.

When the app still waited for indexing to finish before accepting a query, five identical searches
run straight afterwards cost:

    1115 ms → 1085 → 1039 → 702 → 225 ms

The first was roughly 5x the steady state — page faults into the freshly built index and JIT of
the matching paths. That is what `SearchEngine.WarmUpAsync` existed to absorb.

It has been removed, because searching now starts *with* indexing rather than after it: the query
typed first runs for the whole of the index, streaming results in as drives come online, and pays
the cold start there — where the user is already waiting — instead of in the first search they
care about the answer to. `--search notepad --bench 3` now logs:

    indexed 8 volume(s) in 54.2s: C:,D:,E:,F:,G:,H:,J:,K:
    855 results — engine 186 ms, ui +64 ms
    855 results — engine 195 ms, ui +30 ms
    855 results — engine 190 ms, ui +30 ms
    855 results — engine 198 ms, ui +16 ms

The measurement that settled how drive arrivals are handled came from an earlier build that logged
the streaming search separately: issued half a second into a 54 s index, it delivered its results
in 125 batches along the way and finished with **855 — the same count as every search run after
indexing completed**. An in-flight search therefore keeps picking up drives as they come online,
which is why only one re-run is worth doing, at the end. Re-running per drive-group, which the
code used to intend, would have cleared the grid eight times and traded a steady stream of results
for a series of resets.

**UI overhead is 6–76 ms**, across both `notepad` (855 results) and `Program` (1,594, many of them
folders). An earlier guess that the staging queue and rendering were responsible for a slow search
was wrong; the cost was entirely the engine's cold start.

One measuring trap, since `--bench` exists to be trusted: a run that reported `ui +4789 ms` turned
out to be the machine taking two full-screen 2560×1440 screenshots during that exact window. The
same query re-measured at 30, 76, 6 and 16 ms. Take the numbers when nothing else is running.

Three things that also mattered:

- The grid publishes changes at ~8 Hz while draining at 30 Hz. Every publish is a Reset, and a
  Reset makes WPF discard and re-measure every realized row.
- Anything else on that ~8 Hz path has to be free when nothing changed. Re-assigning the Search
  button's content and a fresh `SymbolIcon` on each status update — a few lines that looked like
  nothing — took UI overhead from 12–34 ms to 78–92 ms per search. Re-templating one button, at
  eight times a second, is the whole of that.
- `App.xaml.cs` holds a single-instance mutex. Without it, each launch started another full index
  of every volume; three concurrent instances turned a 55 s index into minutes of disk contention.
  That is why an ordinary second launch hands over to the running window rather than opening its
  own — the behaviour the tray icon and the restore hotkey both depend on.
- More than one instance is available on request: `--new-instance`, `Ctrl+N`, "New instance" in the
  results background menu or the tray menu, a middle-click on the tray icon, shift held at launch,
  or "New instance" on the taskbar button's jump list. Middle-clicking the taskbar button — the
  gesture most people arrive with — cannot be answered: the shell starts the executable with no
  arguments and the button is released long before there is a process, so it is the same launch as
  an ordinary click. Shift is read before `TryElevate`, so the flag reaches the elevated copy. Each one is a real second process
  with its own engine and its own full index pass, which is the cost of the feature and the reason
  it is not the default. An extra instance is deliberately not the owning one: it registers no
  restore hotkey (`RegisterHotKey` gives a combination to one window only), does not listen for the
  show signal, says so in its title bar, and never writes the settings file — `Save` replaces the
  whole document, so the second window to close would otherwise roll back everything the first had
  changed, including options just set in Options.

## Development elevation

`grant-dev-elevation.ps1` (run once, elevated) registers an on-demand scheduled task that launches
**this exe only** — a fixed path, not an arbitrary command — at `RunLevel=Highest`. After that,
elevated runs need no UAC click:

    $s = New-Object -ComObject Schedule.Service; $s.Connect()
    $s.GetFolder('\').GetTask('Spotter.DevElevation').Run('--search notepad --bench 3')
    Stop-ScheduledTask -TaskName 'Spotter.DevElevation'   # also kills the elevated instance

It is a standing elevated entry point for an executable under the user profile, so anything
running as that user could replace the file. Remove with
`Unregister-ScheduledTask -TaskName 'Spotter.DevElevation' -Confirm:$false`.

`ElevationTask.cs` since does the same thing for ordinary use, under a name derived from the
executable's path (`Spotter.<md5-of-path>`). The script is still the way to start an
elevated run *with arguments* from a script, which is what `--bench` measuring needs; the app's
own task always launches with none. Both carry the same trade-off — remove either the same way.

## Status

**Verified end to end.** An elevated run indexed 8 volumes (`C: D: E: F: G: H: J: K:`) in 54.2 s
and returned 855 results for `notepad`, repeatably — the same count the headless harness gets from
the engine directly, so the UI is not losing or inventing rows.

Working: six languages, two of them right to left (see above), Fluent dark/light following the
system, Mica, live search with a 180 ms debounce,
per-field case/regex options, folder browsing, completion and drag-and-drop, size presets,
per-field validation, a determinate indexing progress bar with per-drive detail, rate and time
remaining, virtualized results (checked against 250,000 rows), per-extension shell icons, sorting
using the engine's own natural-order comparers (`file2` still sorts before `file10`, and every
ordering is made total by the full path, so the same search twice is the same list twice), open, open
containing folder, copy name/path/folder, CSV and plain-text export, delete to Recycle Bin and
permanent delete with confirmation, drag-and-drop out to Explorer, properties, multi-select with
a running size total.

Keyboard: in the results, Enter opens, Alt+Enter properties, Ctrl+A selects all, Ctrl+C copies
paths, Ctrl+S exports, Del / Shift+Del delete, Esc returns to the search box. In a query field,
Enter searches immediately instead of waiting out the debounce, and Esc clears that field —
pressing it again, now that the field is empty, resets all three. Down opens the history in the
name field and the folder completions in the folder field. Ctrl+N opens a second instance. F5
re-runs the search from anywhere in the window, and the refresh button at the start of the name
field does the same with the mouse — it is there because the one thing a live index cannot do is
tell you about a file that has not been written yet, so waiting for one means re-running the same
query repeatedly.

Every gesture printed in a menu is a real binding. Two of them were not when the menus were first
written, which is worth stating because a menu is where users go to *learn* the shortcuts: an
`InputGestureText` that nothing implements teaches a key that does nothing.

Also working: tray icon with minimize-to-tray and optional close-to-tray, a global restore hotkey
(`Ctrl+Alt+F` by default — press it again to dismiss), all seven columns, and persisted window
geometry, column widths, sort and search options.

The Files and Folders switches are the one exception to that last point: both start every run
switched on, because of how they fail. A window that opens with Folders still off finds no folder
whatever is typed, and nothing says so — it looks exactly like the thing not being there, which is
the question the search was asked to answer. Narrowing to one kind is nearly always about the
search in hand. "Remember the Files and Folders switches" in Options restores the old behaviour,
and the two values are written on exit either way, so turning it on carries on from what was last
on screen.

### File types

**Files** is a switch with a panel behind it: the switch decides whether files appear at all, the
chevron beside it opens eight buckets — images, video, audio, documents, archives, code and
settings, programs and installers, everything else — plus a free-text box for extensions no bucket
covers. The label carries the count while the filter is narrowed, so the row reads "Files (2)"
rather than looking untouched while half the answers are missing.

Hovering a category lists the extensions it holds, built by inverting the extension table rather
than written out a second time — a row named "Documents" is a promise about a list the user cannot
see, and the only way to know whether the extension they want is in it is to be told. "Everything
else" has no list to show and gets a sentence instead.

Three decisions in that are not obvious.

**The engine cannot do this.** Its name query is an AND of substrings, so "mp4 or mkv or avi" is
not expressible in it at all. The filter is applied in `SearchEngine.Search`, in the intake
callback, before the record reaches the queue — not over the finished list. The status count, the
sort, the export and the folder totals all read the published rows, so a row filtered out later
would have been counted, sorted and exported first.

**An extension can be in two buckets**, so `FileKinds` maps each extension to a *mask* rather than
to one kind. `.ts` and `.mts` are the pair that need it here — TypeScript sources and MPEG
transport streams — and both are listed under video and under code. Forcing such an extension into
one bucket means choosing which half of the users get a filter that hides what they are looking
for.

**"Everything else" is a bucket, not a leftover.** Without it, ticking all seven real categories
would still drop files whose extension nobody listed, and the label would say nothing was filtered
while results went missing.

A leading dot introduces an extension rather than cancelling it: `.env` is a settings file and
`env` is in the code bucket, so reading a dotfile as extensionless put it in "everything else"
while the panel listed `env` under code — the filter contradicting its own tooltip. The common
dotfiles are listed, so `.gitignore` and `.editorconfig` are found by what they are.

The custom extensions are an additional bucket rather than a restriction on the ticked ones. The
other reading — "only these" — is fine in isolation and wrong in combination: with every category
still on, typing `mkv` would have to either mean nothing or silently throw the categories away. To
find only `.iso`, untick the categories.

Every bucket off with an empty extension box excludes files exactly as thoroughly as the Files
switch does, so it is reported as a query problem rather than as an empty result — see below.

The extension box is something people **paste** into — out of a wiki, a chat message, a
spreadsheet column — and a single-line WPF text box keeps the newlines. So the separators include CR and LF,
the no-break space that comes with a web page, and the Arabic comma, which is how
this gets typed on an Arabic keyboard. Without them a pasted list collapsed into one token that
no file could match, while the label counted it as an active type: silent, and lying about itself.
Anything still carrying whitespace or a control character after parsing is dropped rather than
kept, because an entry that cannot match is worse than no entry — the label counts it.

The box is also stripped of characters XML cannot carry before it reaches the settings file. One
of them — U+000B, which a paste out of Word can supply — makes `XmlSerializer` throw, and
`UiSettings.Save` catches and logs, so a single such character would silently abandon every save
for the rest of the session: window geometry, column widths, language, history, all of it.

The panel is a `Popup`, not a `ContextMenu` like the per-field options buttons, because it ends in
a text box: a menu swallows the space bar and closes on any letter it takes for an access key. Its
rows are built in code from `FileKinds.All`, so a new kind means a line in the enum and a label in
the six tables, not a line of XAML.

### Being the extra window is a role, not a property of the process

Ownership of the machine-wide pieces — the single-instance mutex, the show signal, the restore
hotkey and the settings file — used to be decided once at startup and never looked at again. That
was survivable while a second instance needed a command-line flag. The "+" button made it one
click, and then closing the first window left the second one crippled for the rest of its life:
no hotkey, deaf to a re-launch, and silently writing nothing.

Worse, the extra instance held a handle to the mutex it could not serve. A named mutex lives as
long as any handle to it is open, so after the owner exited the *name* was still taken while the
show signal was gone — a later launch found "already running", had nothing to signal, and gave
up. Verified rather than assumed: with a second handle open, `TryOpenExisting` still succeeds
after the first is disposed, and fails once both are.

So an extra instance now lets go of the mutex immediately, and polls every four seconds to see
whether the name has come free. When it has, it takes the mutex, registers the hotkey, starts
listening for the show signal, drops "extra instance" from its title and starts writing settings.
Polled because there is nothing to wait on: a mutex gives no "released" notification to anyone
not already queued on it.

Until that happens, the two actions that used to claim success and revert — Save in Options,
which applies theme, language and scale live, and Clear history, which empties the list on screen
— say in the status bar that the file belongs to the other window.

The hotkey uses `RegisterHotKey`, not the machine-wide low-level keyboard hook the 1.5.1 app
installed. The hook sees every keystroke on the system and filters them itself; this asks Windows
for one combination and nothing else. If the combination is already taken the status bar says so
rather than leaving a key that silently does nothing.

### Search history

Recent file-name queries, most recent first, capped at 25 and persisted. **Recent** opens the
list, so does Down in the search box; Enter uses one, Delete forgets one.

The interesting part is what gets recorded. Recording every keystroke would fill the list with the
prefixes of one word — `n`, `no`, `not`, `note`, `notep` — which is the failure that makes a
history worse than none. So only deliberate signals commit a term: Enter, leaving the box, opening
a result, pressing Search, closing the window. As a backstop, a term that merely extends the
newest entry replaces it, so refining `note` into `notepad` across two commits still leaves one
entry.

It is a `Popup`, not an `AutoSuggestBox`. Suggestions that filter as you type would be competing
with the search box itself, where every keystroke is already a query against the whole disk.

### Options tells you where everything is

An app should not make you go and find out what it left on your machine. **Options → What this
app stores on your PC** lists every location with an **Open folder** button that shows the file
selected in Explorer:

| | |
|---|---|
| Settings | `%APPDATA%\Spotter\settings.xml` |
| App log | `%LOCALAPPDATA%\Spotter\Spotter.log` |
| Engine log (off) | `%LOCALAPPDATA%\Spotter\Spotter.Engine.log` + rotated `.log.zip` |
| Scheduled task | `Spotter.<md5-of-exe-path>`, with a **Remove** button |

That is the complete list — three files and one scheduled task, and **none of them beside the
executable**. Spotter ships as a single file meant to be dropped anywhere and run from there, so
anything it wrote next to itself would litter a desktop or a USB stick root; under Program Files
it would fail outright for an ordinary user, and an elevated run would leave a file the user
afterwards cannot delete.

Three separate places had to be fixed for that to be true, and each was a default someone had to
notice: `Log.cs` fell back to `AppContext.BaseDirectory` when per-user data was unavailable, the
engine's `GClass95` resolved any relative log name against `AppContext.BaseDirectory`, and
`App.config` — the file that switched the engine logger off — was itself a sidecar. Removing it
meant moving its settings into `DefaultSettingValue` attributes, or deleting it would have
switched the noisiest logger in the program back on.

The scheduled task is the only entry that changes something outside those folders, which is why
it gets a Remove button rather than just a path. Removing it costs nothing but a UAC prompt on
the next launch — the app re-registers after you approve one.

It also cleans up after itself. The task name is derived from the executable's path, so moving a
single portable file — the normal thing to do with one — would otherwise leave a task behind
pointing at nothing, and a habit of moving it would fill Task Scheduler with dead entries nobody
would think to clear. `RemoveTasksForOtherLocations` deletes every `Spotter.*` task that is not
this location's, on each elevated start.

It deletes by *name*, not by testing whether the target still exists, and that is deliberate: a
copy on a USB stick has a perfectly valid target that simply is not plugged in, and an existence
test would delete its task every time you ran the copy on `C:`. The cost of the simpler rule is
one UAC prompt the next time that other copy starts.

**About** sits below it: version, what the app actually is (a .NET 8 WPF app over the
engine, reading the MFT directly), and the path it is running from. Every path is a read-only
text box rather than a label, so it can be selected and copied.

### Closing the window ends the process

It has to, and it did not reliably.

Two things in the engine work against it. It starts threads without `IsBackground` (`GClass102`,
`GClass270`), and one foreground thread keeps the CLR alive long after `Main` returns. And its
volume readers never close the handles they opened — `GClass156.Dispose` only clears a "keep
reading" flag, leaving the `SafeFileHandle` to a finalizer that may never run.

Put together, that is a process nobody can see, still holding `\\.\C:` and every other volume
open — enough for Windows to refuse to safely remove a USB drive. A file search tool has no
business doing that to someone.

So `App.OnExit` ends the process deliberately, after settings are saved, the tray icon and hotkey
are released and the mutex is dropped. Ending it hands every remaining handle straight back to
Windows.

**There is no tray icon by default**, which is 1.5.1's default too — `GClass283.method_6`, the
original's own defaults initializer, sets tray icon false, minimize-to-tray true, close-to-tray
false. This port had it on, which was simply wrong. It matters more than a default usually does:
a tray icon lets the window leave the screen while the process keeps running, and this process
holds a raw handle on every volume. Closing should mean closed unless someone asked otherwise.

Worth being straight about the part this does *not* fix: while Spotter is open it holds a
raw handle on every volume, because that is how it reads the MFT and follows the USN journal.
Freeing a drive means closing the app — which now actually closes it. Turn the tray icon on in
**Options** if you want the old behaviour back, and know that a window tucked into the tray is
still a running process holding those handles.

### Folders have sizes, because the engine already knows them

`GClass153.method_6` runs as each entry is read: for every file it walks up the parent chain and
adds the file's length to `long_0` on each ancestor, and bumps two counters — `method_1()` files
below it, `method_3()` folders below it. So a folder's size and contents are recursive totals the
index has been accumulating all along.

The port was rendering folders with a blank Size column and throwing all of it away. Now the Size
column fills in for folders too, and hovering a folder's name shows what is inside it.

Two consequences worth knowing:

- The totals grow while indexing runs, so the grid redraws every ~2 s until the last drive is in,
  then once more. A redraw is a Reset and a Reset rebuilds every realized row, which is why it is
  every two seconds and not every poll.
- **A size filter matches folders too** — that is why `>100MB` returns folders. The docs here
  previously claimed the opposite.

The running total for a multi-selection still counts files only. A folder's size already includes
the files inside it, so adding both would count them twice.

### The window is sized against the screen, not the resolution

A search window is something you glance at, not something you live in, so it never opens filling
the display. On a first run it opens in the **top-left quarter** of the work area, inset on all
four sides so it touches neither the screen's own corner nor the middle of the imaginary cross that
quarters it. `WindowLayout.Default` works that rectangle out; there is no hard-coded default size
in `UiSettings` any more, because the right size depends on the screen and a number cannot.

The interface is drawn at a scale worked out the same way. `UiScale` owns one `ScaleTransform` that
every window hangs its content off as a `LayoutTransform`, so one number moves text, padding,
icons, row heights and the title bar together — a font size would have moved the words and left
everything else where it was.

"Screen size, not resolution" is the whole point, and it is why `Displays` divides each monitor's
work area by that monitor's own DPI before anything looks at it. A device-independent unit is 1/96
inch, so a 27-inch 4K panel that Windows drives at 150% measures the same as the 27-inch 1080p
panel beside it, and gets the same answer. A pixel count would have made the 4K panel twice the
screen and doubled everything on it.

The scale follows the diagonal of that area, heavily damped and clamped to 0.75–1.5: a screen twice
the size wants slightly bigger type and a great deal more rows, not twice the type. The reference
sits between the two shapes of ordinary desktop monitor rather than on top of either — 1920x1080 is
2184 units across the diagonal, 2560x1440 at 125% is 2327, and both are a normal monitor at a normal
viewing distance — so the whole of that band rounds to 100% instead of drifting across it. Options
can lock it anywhere from 50% to 250% instead, previewing live and reverting on Cancel, exactly as
the language picker does.

Three things a `LayoutTransform` cannot carry, each handled where it breaks:

- **Tooltips and context menus** render in their own top-level windows. `PopupChrome` hands them
  the transform as they open, alongside the reading direction — see the popup section above.
- **The history and folder-suggestion popups** are ours, so their content carries the transform
  directly, and the width binding moved from the `Popup` to the scaled `Border` inside it. Left on
  the Popup it would have been the box's unscaled width, which stops matching what is on screen the
  moment the scale is not 1.
- **A combo box's drop-down** belongs to the theme's template, so `PopupChrome.ScaleDropDown`
  reaches it through `PART_Popup` when it opens.

The window's `MinWidth`/`MinHeight` are maintained from code rather than XAML for the same reason:
they belong to the frame, which sits outside the transform, so at 140% the same content genuinely
needs 40% more window. They are capped at the work area, because a minimum larger than the monitor
is a window that cannot be moved or closed.

### Everything you do to the window is kept, with two exceptions

Resizing, moving and maximizing all come back next time. `WindowLayout.IsUsable` rejects exactly
two arrangements, and only because both are ones the user cannot undo:

- **No title bar left to grab.** Fewer than 180 units of it on any one monitor — per monitor, not
  summed, because half a title bar on each of two screens leaves nothing clickable in the gap.
- **Almost nothing on screen.** Less than 30% of the window inside a work area. A window
  deliberately pushed half off the edge is a real thing people do and is kept.

The same check runs on the way out and on the way back in, so an arrangement is not stored if it
would strand the window, and a stored one is dropped if the monitor it lived on has since been
unplugged. A stored size larger than the monitor it lands on is trimmed rather than discarded —
that is a screen that shrank, not a user doing something odd.

`PlacementVersion` in the settings file says what the four geometry numbers mean. An older file
deserializes it as 0, which is read as "nothing worth restoring", so the 2.0 default of a 1200x720
window centred on the screen is dropped once and replaced by the quarter above.

### Scrolling and columns

Three things a list is expected to do that this one did not:

- **Shift + wheel scrolls sideways.** WPF has no built-in binding for it.
- **The vertical scrollbar starts below the column headers.** The stock GridView scroll template
  puts the headers inside the scrolling area, so the scrollbar ran the full height of the list
  with its track up level with the header row. The headers now get their own row above it and are
  slid sideways by the horizontal offset so they still track the columns.
- **Column edges can be dragged.** Retemplating `GridViewColumnHeader` to theme it had dropped
  `PART_HeaderGripper` — the Thumb that `GridViewColumnHeader` looks up by name to implement
  resizing. Restoring it brings back dragging and double-click-to-fit.

### Separators in the file name are a gap, not a character

The engine splits the name query on spaces and tabs and requires every word as a substring —
`GClass168.smethod_1`, which is the 1.5.1 app's own splitter, unchanged. That already makes
`family guy` find `Family.Guy.S01E01.mkv`: both words are in it. What it did not do is the reverse.
Type the name the way the file actually spells it, `Family.Guy`, and it became one literal substring
that the spaced-out copy of the same show does not contain.

Which is backwards. Whether a release used dots, underscores or spaces is an accident of whoever
packed it, and nobody searching for it is trying to express a preference about it. So
`SearchQuery.EffectiveFilename` rewrites those separators into the spaces the engine already
understands, before it ever sees the query. No engine code changes and the fast substring matchers
are kept — a regular expression per file over a 309,000-hit result set is not the same thing at all.

It rewrites one whitespace-delimited word at a time and leaves a word alone unless splitting it is
unambiguously what was meant. Three guards, each earning its place:

| Input | Becomes | Why |
|---|---|---|
| `Family.Guy` | `Family Guy` | the case this exists for |
| `Wi-Fi` | `Wi Fi` | finds `WiFi`, `Wi-Fi`, `Wi Fi` |
| `*.dll` | unchanged | a wildcard pattern; the engine has its own meaning for it |
| `.dll` | unchanged | leading separator is a character, not a gap — splitting would widen it to every name containing "dll" |
| `v1.5` | unchanged | would break into a one-character piece, and a bare `5` matches most of a disk |

Off by a tick in the name field's options menu, which is where the per-field behaviour lives.

### The columns share out the window

All seven columns divide the width of the results list between them, and they do it again every
time that width changes. This is not tidiness. Columns past the right edge can only be reached by
scrolling sideways, and the horizontal scrollbar's range comes from the **rows** — so with no
results there is nothing to scroll, and those columns cannot be brought into view at all. Before
any search has run, which is the first thing anyone ever sees, the grid was showing a name, a path,
and a column called "Si".

Each column takes a floor first and the remainder is split by share. The floors are what the
content needs, not what looks balanced, and the asymmetry is deliberate: a name or a path that runs
out of room ends in an ellipsis and is still doing its job, because the eye only needs the start of
it. A date that runs out of room reads `12/17/2024 3:38 A`, which is not a shorter date — it is a
wrong one. So the three date columns hold enough for the widest common short format and the name
and path take what is left, and the shares then send almost all the growth from a wider window back
to those two.

The date floor is **measured**, not written down. `12/30/2026 10:58 PM` and `30.12.2026 22:58`
differ by about a third, and a figure picked to be safe for the widest of them would take that third
out of the name column in every locale that does not need it. So it is measured with `FormattedText`
in the grid's own font and the user's own regional format — a separate Windows setting from the
display language, which `Lang.Use` deliberately leaves alone, so it never changes while the app runs.

`RowChrome` comes off the top: each row is a `Border` carrying the `ListViewItem` style's
`Margin="2,1"` and `Padding="6,4"`, sixteen units the columns never see. Forgetting it puts a
horizontal scrollbar under a grid that looks like it fits. And when even the floors do not fit — a
wide date format at the default window size — they are all squeezed proportionally rather than
allowed to overflow, because a slightly tight date is worth more than a column pushed off an edge
there may be nothing to scroll it back from.

The handover to the user watches `GridViewColumn.Width` itself, through a
`DependencyPropertyDescriptor`, rather than the gesture. The obvious route — the gripper's
`DragCompleted`, which bubbles to the header — misses two ways: `GridViewColumnHeader` marks that
event handled on its way past, so a style `EventSetter` never sees it, and double-clicking the
gripper to size a column to its contents is not a drag at all. Every one of them ends up writing
`Width`. **Reset all column widths** hands it back. `Fit this column to its contents` also turns it off, for the same reason: it is a decision about
one column, and the next window resize would otherwise undo it silently.

The name cell is a `DockPanel` rather than a `StackPanel`, which matters only now that the column
can be narrow: a horizontal StackPanel measures its children with unlimited width, so the
`TextTrimming="CharacterEllipsis"` on the name never fired and a long name was cut off mid-letter by
the cell's clip instead.

### Right-click menus

Four of them, chosen by what is under the pointer: a query field, the filter row, a result row,
and the empty space below the results.

The last two were one menu, which is the bug that motivated splitting them. Right-clicking the
background opened the file menu, and its Delete and Properties acted on whatever was still
selected somewhere out of view — indistinguishable from acting on the wrong file. The background
now offers only what applies to the list as a whole. Right-clicking a row that is not selected
selects it first, the rule Explorer uses, so the menu that follows is about the row that was
actually clicked.

The query fields used to fall back to the stock Cut/Copy/Paste, which is a text-editing menu in a
search box — the editing commands are still there, joined by the ones that are about searching.
The filter row and the column headers had no menu at all; they now offer what you would want from
them, including the column-width commands that a header drag cannot express.

### A right-click has to open a menu on the first click

WPF works out whether there is a menu to open *before* it raises `ContextMenuOpening`. A
`ContextMenu` assigned only inside that handler is therefore assigned one click too late: the first
right-click found null and opened nothing, and the second found what the first had left behind — so
the gesture read as "click once to select, again for the menu".

The menu is now chosen on the button going down, in `OnResultsRightButtonDown`, alongside the
selection it already had to make there. `ContextMenuOpening` still chooses too, for the keyboard
route in (Shift+F10 and the menu key, where there is no button press), and the list is given a menu
in the constructor so the property is never null to begin with.

### The title bar icon is the system-menu button

Double-clicking it closes the window, which Windows has attached to a window's system-menu icon
since version 3. WPF-UI's title bar does not implement it and let the double-click through as one on
the bar, so it maximized instead.

A WPF mouse handler cannot do this, and finding that out is the whole lesson. WPF-UI draws its title
bar through `System.Windows.Shell.WindowChrome`, and WindowChrome answers `WM_NCHITTEST` with
`HTCAPTION` across that strip — so as far as Windows is concerned the caption is *non-client*,
Windows handles the click itself, and no mouse event is ever raised for WPF to see. Marking the icon
`WindowChrome.IsHitTestVisibleInChrome` would bring the events back, but only over the icon's opaque
pixels: an `.ico` has transparent corners, and a click on one falls through to the caption again.

So it is answered where the question is actually asked. A hook on the window returns `HTSYSMENU` for
the icon's rectangle, and Windows then does the rest properly and for free: one click opens the
system menu, two close the window. `WM_NCLBUTTONDBLCLK` is handled as well, for the case where
another hook claimed the hit test first.

The rectangle comes from `PointToScreen` on the icon, so it follows the interface scale without
being told, and it is built from two opposite corners rather than a corner and a size — under
`RightToLeft` an element's own origin is its right-hand edge, and `Rect` normalises that either way.
The lParam halves are read as **signed**: a window on a monitor to the left of the primary one has
negative screen coordinates, and reading them unsigned puts the pointer 65,000 pixels away.

### The query fields

Each field carries its own options, because the engine stores them that way and the 1.5.1 UI
presented them that way — a small options button per field, exactly where the original put one.
The port had collapsed case and regex onto one shared pair of checkboxes, which quietly removed
combinations the engine supports: a case-sensitive name inside a loosely matched folder, or a
regular expression for the folder while the name stays an ordinary word.

Since the options are no longer a visible row, the row shows a short summary of any that are not
at their defaults — the one thing a row of checkboxes was genuinely good at.

**In folder** has what it needs to be typed into: a **Browse** button, completion from the real
filesystem as you type (Down steps into the list), and drag-and-drop — drop a folder to search
it, or a file to search the folder holding it. Browse and drop both escape the path when the
field is in regex mode, since a Windows path is mostly backslashes and pasting one raw produces a
pattern that matches nothing. Completion deliberately does not run in regex mode, for a bare
`\\server` root, or for anything that is not already shaped like a path.

**Size** has a menu of the common filters, each written in the grammar the engine parses, so the
menu doubles as a set of worked examples of what can be typed by hand.

### Invalid input says so

Every field is checked before the search runs. The offending one gets a red border and the status
bar says what is wrong with it — "Size — Invalid expression on char 0."

This matters more than it sounds. The engine builds its matchers on a worker thread and swallows
what they throw, so an unparseable size or a broken regular expression arrived as **0 results** —
byte for byte the same answer as a perfectly good search that matched nothing. The 1.5.1 app
checked all three fields up front and coloured the box that failed; this does the same, in the
status bar rather than a modal dialog, because it runs while you type. Turning both Files and
Folders off is caught the same way, since it can only ever find nothing — as is a file type panel
with every bucket unticked, which is the same state reached from the other direction and gets its
own message, because what the user has to change to fix it is different.

### Tooltips

Every control has one, and the three query fields carry their real syntax, taken from the engine
rather than guessed:

- **File name** — matches any part of the name; several words must all appear; `*` and `?` per word
- **In folder** — part of a path; `\` or `/` separates levels
- **Size** — `> 50 MB`, `<= 100 KB`, `= 1 MB`, `~1 GB` (within 10%), `200 MB To 300 MB`

The size grammar is `GClass119`'s regex, and the examples are the ones the 1.5.1 app showed from
its own help icon. `~` really is ±10% and `=` really is a small tolerance rather than an exact
match, which is worth knowing before writing a tooltip that says otherwise.

Those size expressions are the engine's own grammar and are the same in every language, so they are
literals in the XAML rather than translated strings — only their meanings come from the tables. That
is also what lets each one be pinned left-to-right; see the right-to-left section above.

### Search on demand

The 1.5.1 option of that name, under the name its own tooltip used: **Search as you type**, in
Options, on by default. Turning it off stops anything running until **Search** is pressed — the
button the old app showed in exactly this case, and which doubles as **Stop** while a search runs.

Stopping keeps what has already been found, including the last partial batch, rather than clearing
the grid: a stop that discarded rows would be indistinguishable from a bug.

The mode also gates the one automatic re-search there is, the final pass once every drive is
indexed. Re-running whatever happened to be typed at that moment would quietly defeat the whole
point of the mode, so it only re-runs a query the user actually asked for — one run through the
Search button, Enter, the history, or `--search`.

Settings live in `%APPDATA%\Spotter\settings.xml`. **Options** covers the language,
the search mode, the hotkey, the tray, the theme and the window size; the rest of the file — window
geometry, column widths, sort, the search history — is written by the app and can be edited by hand. `Language` is
blank for "follow Windows" and stays blank until someone picks one explicitly, so changing the
Windows display language still moves the app with it.

Deleting marks the row struck through and dimmed rather than re-running the search. Re-running
threw away the scroll position and the selection and made rows vanish, so there was no way to tell
a successful delete from a row you had merely scrolled past.

Not yet ported from 1.5.1: "start with Windows". The update checker is deliberately not ported —
both of its servers have returned 404 since long before this rewrite.

Exporting and deleting both run off the UI thread. Each formats or resolves its strings on the
dispatcher first — `FullPath` walks the live engine index, which is not safe to walk from a worker
— and only the file I/O moves. Deleting a few thousand files used to hold the dispatcher for
several seconds, so Windows painted "Not responding" over the window in the middle of a
destructive operation, which is the worst possible moment to look like a crash.

Exported CSV neutralises a leading `=`, `+`, `-` or `@` in any field. File names are not our text
— they are whatever happens to be on the disk — and a file called `=cmd|'/c calc'!A1` would
otherwise execute when the export is opened in Excel.
