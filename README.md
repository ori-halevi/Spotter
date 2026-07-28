# Spotter

Instant file search for Windows.

Spotter reads the NTFS master file table directly instead of walking folders, so it does not
crawl, does not maintain a background service, and does not need a warm-up before it is useful.
Type, and results are there.

    8 drives indexed in 54 seconds
    855 results for "notepad" in 228 ms

## What it does

- **Search as you type**, or on demand — one setting, and the button appears when you turn the
  live search off.
- **Name, folder and size filters**, each with its own case and regular-expression switches,
  because the combinations matter: a case-sensitive name inside a loosely matched folder is a
  different search from the reverse.
- **Folder sizes and contents**, counted while indexing runs, so a folder row shows what is
  actually inside it.
- **Six languages** — English, Arabic, German, Hebrew, Russian, Spanish — two of them right to
  left, laid out properly rather than mirrored badly.
- Recent searches, column sorting in natural order (`file2` before `file10`), CSV and text
  export, delete to Recycle Bin, drag-and-drop out to Explorer, a global hotkey, an optional
  tray icon that is **off** by default.

Closing the window closes the program. It holds a raw handle on every volume while it runs, so
leaving it alive in the background would block things like safely removing a USB drive.

## Requirements

Windows 10 1809 or later, x64, and the **.NET 8 Desktop Runtime** for the default build.

Reading the file table needs administrator rights. Spotter asks once, then registers a scheduled
task for that executable's path so later launches from the same place start without a prompt.
**Options → What this app stores on your PC** lists every file and task it creates, with a button
to open each one and to remove the task.

## Building

    build-debug.bat        debug build, symbols, no optimisation
    publish.bat            release build - needs the .NET 8 Desktop Runtime on the target PC
    publish.bat full       one self-contained exe, runs on any Windows x64

Or directly:

    dotnet build src\Spotter\Spotter.csproj -c Debug

### The search engine is not in this repository

`src\Spotter` is the application: the window, the search plumbing, the localisation, the
settings. That is what is published here.

The file-table reader it links against lives in `engine\`, which is excluded by `.gitignore`.
**A fresh clone will not build until that project exists** — `Spotter.csproj` has a
`ProjectReference` to `..\..\engine\Core\Spotter.Core.csproj`, and without it the build stops
with a missing-project error rather than anything mysterious.

If you are reading this on GitHub and want a working build, you will need to supply an engine of
your own behind the same small surface: a type that starts indexing, reports progress per volume,
runs a compiled query, and streams results back. `SearchEngine.cs` is the entire boundary — it is
one file, and it is the only place in the app that knows what the engine looks like.

## Running it

    Spotter.exe

    Spotter.exe --demo 250000   synthetic rows; no admin, no engine, for exercising the UI
    Spotter.exe --noelevate     stay an ordinary process, so it can be focused and killed normally
    Spotter.exe --lang he       force a language for one run without changing the saved setting
    Spotter.exe --search TERM   preload the search box at startup
    Spotter.exe --bench N       re-run the startup query N more times and log each

Developer notes, design decisions and the measurements behind them are in
[src/Spotter/README.md](src/Spotter/README.md).

## Licence

Copyright (C) 2026. All rights reserved unless a licence file says otherwise.

If you want people to be able to use, copy or contribute to this, add a licence — without one,
the default is that nobody may do any of those things. `MIT` is the usual choice for something
like this; drop the text in a `LICENSE` file at the root and GitHub will pick it up.
