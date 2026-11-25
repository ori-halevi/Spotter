using System.Collections.Generic;

namespace Spotter.Localization;

/// <summary>
/// German. Terminology follows Windows itself — Papierkorb, Explorer, Infobereich,
/// Aufgabenplanung — rather than a literal rendering of the English, so that a word means here
/// what it means everywhere else on the machine.
/// </summary>
internal static class StringsDe
{
    internal static readonly IReadOnlyDictionary<string, string> Table =
        new Dictionary<string, string>
        {
            // ---- shared ------------------------------------------------------------------------
            ["Common.Save"] = "Speichern",
            ["Common.Cancel"] = "Abbrechen",
            ["Common.Remove"] = "Entfernen",
            ["Common.OpenFolder"] = "Ordner öffnen",

            // ---- query fields ------------------------------------------------------------------
            ["Field.Name.Placeholder"] = "Dateiname suchen…",
            ["Field.Name.Tip.Title"] = "Dateiname",
            ["Field.Name.Tip.Part"] = "Findet jeden Teil des Namens: 'note' findet 'notepad.exe'.",
            ["Field.Name.Tip.Words"] = "Mehrere Wörter müssen alle vorkommen, in beliebiger Reihenfolge:  log 2024",
            ["Field.Name.Tip.Wildcards"] = "Die Platzhalter * und ? gelten je Wort:  *.dll    report?.txt",
            ["Field.Name.Tip.Keys"] = "Pfeil nach unten öffnet die letzten Suchen. Enter sucht sofort, ohne die Tipppause abzuwarten.",
            ["Field.Name.OptionsTip"] = "Optionen für den Dateinamen: Groß-/Kleinschreibung, regulärer Ausdruck, Umgang mit Trennzeichen.",
            ["Field.Name.RefreshTip"] = "Die Suche erneut ausführen, genau wie F5. Nützlich, wenn eine Datei gleich angelegt wird: von selbst läuft die Suche nicht erneut.",

            ["Field.Folder.Placeholder"] = "In Ordner",
            ["Field.Folder.Tip.Title"] = "In Ordner",
            ["Field.Folder.Tip.Part"] = "Behält nur Treffer, deren Ordner passt. Ein Teil des Namens genügt:  windows",
            ["Field.Folder.Tip.Levels"] = "Ebenen mit \\ oder / trennen; jeder Teil wird der Reihe nach geprüft:  users\\public",
            ["Field.Folder.Tip.Subfolders"] = "Ohne Unterordner muss der Ordner der Datei selbst passen, nicht irgendein Ordner darüber.",
            ["Field.Folder.OptionsTip"] = "Optionen für den Ordner: Groß-/Kleinschreibung, regulärer Ausdruck, Unterordner.",
            ["Field.Folder.BrowseTip"] = "Ordner auswählen. Der Dialog öffnet sich dort, wo der eingetippte Pfad hinzeigt.",
            ["Field.Folder.PickerTitle"] = "In Ordner suchen",

            ["Field.Size.Placeholder"] = "Größe, z. B. >100MB",
            ["Field.Size.Tip.Title"] = "Größe",
            ["Field.Size.Tip.Gt"] = "größer als",
            ["Field.Size.Tip.Le"] = "höchstens",
            ["Field.Size.Tip.Eq"] = "ungefähr diese Größe",
            ["Field.Size.Tip.About"] = "innerhalb von 10 %",
            ["Field.Size.Tip.Range"] = "ein Bereich",
            ["Field.Size.Tip.Units"] = "Einheiten B, KB, MB, GB, TB. Ein Ordner wird nach seinem gesamten Inhalt gemessen, deshalb können auch Ordner auf einen Größenfilter passen.",
            ["Field.Size.PresetsTip"] = "Häufige Größenfilter.",

            // ---- toolbar -----------------------------------------------------------------------
            ["Toolbar.Files"] = "Dateien",
            ["Toolbar.Files.Tip"] = "Dateien in die Ergebnisse aufnehmen. Aus: nur Ordner anzeigen.",
            ["Toolbar.Folders"] = "Ordner",
            ["Toolbar.Folders.Tip"] = "Ordner in die Ergebnisse aufnehmen. Aus: nur Dateien anzeigen.",

            // The file types panel behind the Files switch.
            ["Types.Title"] = "Dateitypen",
            ["Types.All"] = "Alle Typen",
            ["Types.Tip"] = "Welche Dateitypen zählen. Ohne Angabe alle.",
            ["Types.Image"] = "Bilder",
            ["Types.Video"] = "Video",
            ["Types.Audio"] = "Audio",
            ["Types.Document"] = "Dokumente",
            ["Types.Archive"] = "Archive",
            ["Types.Code"] = "Code und Einstellungen",
            ["Types.Program"] = "Programme und Installationsdateien",
            ["Types.Other"] = "Alles andere",
            ["Types.Other.Tip"] = "Jede Datei, die keine andere Kategorie beansprucht, einschließlich Dateien ganz ohne Endung.",
            ["Types.Custom"] = "Weitere Endungen",
            ["Types.Custom.Placeholder"] = "bak, tmp, sqlite",
            ["Types.Custom.Hint"] = "Kommen zu dem hinzu, was oben angekreuzt ist. Nur diese finden: alle Haken entfernen.",
            ["Toolbar.ActiveOptions.Tip"] = "Die derzeit gesetzten Optionen der Suchfelder. Jedes Feld ändert über seine eigene Schaltfläche nur die eigenen.",
            ["Toolbar.Search"] = "Suchen",
            ["Toolbar.SearchAgain"] = "Erneut suchen",
            ["Toolbar.Stop"] = "Anhalten",
            ["Toolbar.Search.Tip"] = "Die Suche ausführen. Sichtbar, weil „Suchen während der Eingabe“ in den Optionen aus ist; während einer laufenden Suche wird daraus Anhalten.",
            ["Toolbar.Recent"] = "Zuletzt",
            ["Toolbar.Recent.Tip"] = "Frühere Suchen, die neuesten zuerst. Pfeil nach unten im Suchfeld öffnet dieselbe Liste.",
            ["Toolbar.Options"] = "Optionen",
            ["Toolbar.Options.Tip"] = "Sprache, Suchmodus, globale Tastenkombination, Infobereich-Symbol, Design und Fenstergröße.",

            // ---- active-options summary ---------------------------------------------------------
            ["Active.Name.Case"] = "Name: Groß-/Kleinschreibung",
            ["Active.Name.Regex"] = "Name: Regex",
            ["Active.Name.ExactSeparators"] = "Name: exakte Trennzeichen",
            ["Active.Folder.Case"] = "Ordner: Groß-/Kleinschreibung",
            ["Active.Folder.Regex"] = "Ordner: Regex",
            ["Active.NoSubfolders"] = "ohne Unterordner",

            // ---- search history -----------------------------------------------------------------
            ["History.Empty"] = "Noch keine letzten Suchen. Wonach Sie suchen, wird hier gemerkt.",
            ["History.Hint"] = "Enter zum Übernehmen · Entf zum Vergessen",
            ["History.Clear"] = "Verlauf löschen",

            // ---- columns -------------------------------------------------------------------------
            ["Column.Name"] = "Name",
            ["Column.Path"] = "Pfad",
            ["Column.Size"] = "Größe",
            ["Column.Modified"] = "Geändert am",
            ["Column.Type"] = "Typ",
            ["Column.Created"] = "Erstellt am",
            ["Column.Accessed"] = "Letzter Zugriff",
            ["Column.Tip"] = "Klicken sortiert nach dieser Spalte, nochmal klicken kehrt die Reihenfolge um. Rechtsklick für mehr.",

            // ---- text-field menu -----------------------------------------------------------------
            ["Menu.Cut"] = "Ausschneiden",
            ["Menu.Copy"] = "Kopieren",
            ["Menu.Paste"] = "Einfügen",
            ["Menu.PasteAndSearch"] = "Einfügen und suchen",
            ["Menu.PasteAndSearch.Tip"] = "Ersetzt dieses Feld durch den Text aus der Zwischenablage und sucht danach.",
            ["Menu.RecentSearches"] = "Letzte Suchen…",
            ["Menu.ClearField"] = "Dieses Feld leeren",
            ["Menu.ClearAllFields"] = "Alle Suchfelder leeren",

            // ---- per-field option menus -------------------------------------------------------------
            ["Menu.MatchCase"] = "Groß-/Kleinschreibung beachten",
            ["Menu.Regex"] = "Regulärer Ausdruck",
            ["Menu.Regex.Name.Tip"] = "Ein .NET-regulärer Ausdruck auf den Dateinamen. Wortweise Suche und die Platzhalter * und ? gelten dann nicht mehr.",
            ["Menu.Regex.Folder.Tip"] = "Ein .NET-regulärer Ausdruck auf den Ordnerpfad. Backslashes müssen maskiert werden — die Schaltfläche Durchsuchen erledigt das für Sie.",
            ["Menu.FlexibleSeparators"] = "Punkte und Bindestriche gelten als Leerzeichen",
            ["Menu.FlexibleSeparators.Tip"] = "Ein: 'Family.Guy' findet auch 'Family Guy' und 'Family_Guy', denn ein Punkt, ein Unterstrich oder ein Bindestrich zwischen zwei Wörtern gilt als Trennung. Aus: Es muss genau das eingegebene Zeichen sein.",
            ["Menu.Subfolders"] = "Unterordner einbeziehen",
            ["Menu.Subfolders.Tip"] = "Ein: jeder Ordner oberhalb der Datei darf passen. Aus: der Ordner der Datei selbst muss passen.",

            // ---- size presets --------------------------------------------------------------------
            ["Menu.Size.Empty"] = "Leer (0 Bytes)",
            ["Menu.Size.Tiny"] = "Winzig — unter 10 KB",
            ["Menu.Size.Under1MB"] = "Unter 1 MB",
            ["Menu.Size.Over10MB"] = "Über 10 MB",
            ["Menu.Size.Over100MB"] = "Über 100 MB",
            ["Menu.Size.Over1GB"] = "Über 1 GB",
            ["Menu.Size.Over10GB"] = "Über 10 GB",
            ["Menu.Size.Between"] = "Zwischen 100 MB und 1 GB",
            ["Menu.Size.About700MB"] = "Etwa 700 MB (±10 %)",
            ["Menu.Size.Clear"] = "Größenfilter aufheben",

            // ---- kind filter menu ------------------------------------------------------------------
            ["Menu.Filter.Both"] = "Dateien und Ordner",
            ["Menu.Filter.FilesOnly"] = "Nur Dateien",
            ["Menu.Filter.FoldersOnly"] = "Nur Ordner",
            ["Menu.Filter.Reset"] = "Alle Suchoptionen zurücksetzen",
            ["Menu.Filter.Reset.Tip"] = "Zurück zu Dateien und Ordnern, ohne Groß-/Kleinschreibung, ohne Regex, mit Unterordnern. Dateitypen wieder alle.",

            // ---- result menu -------------------------------------------------------------------------
            ["Menu.Open"] = "Öffnen",
            ["Menu.OpenFolder"] = "Übergeordneten Ordner öffnen",
            ["Menu.CopyPath"] = "Vollständigen Pfad kopieren",
            ["Menu.CopyName"] = "Dateinamen kopieren",
            ["Menu.CopyFolder"] = "Übergeordneten Ordner kopieren",
            ["Menu.ExportSelected"] = "Auswahl exportieren…",
            ["Menu.Delete"] = "Löschen",
            ["Menu.DeletePermanently"] = "Endgültig löschen",
            ["Menu.Properties"] = "Eigenschaften",

            // ---- background menu ----------------------------------------------------------------------
            ["Menu.SelectAll"] = "Alles auswählen",
            ["Menu.ClearSelection"] = "Auswahl aufheben",
            ["Menu.ExportAll"] = "Alle Ergebnisse exportieren…",
            ["Menu.SearchAgain"] = "Erneut suchen",

            // ---- column header menu -------------------------------------------------------------------
            ["Menu.SortAscending"] = "Aufsteigend sortieren",
            ["Menu.SortDescending"] = "Absteigend sortieren",
            ["Menu.SortByAscending"] = "Nach {0} aufsteigend sortieren",
            ["Menu.SortByDescending"] = "Nach {0} absteigend sortieren",
            ["Menu.FitColumn"] = "Spaltenbreite an Inhalt anpassen",
            ["Menu.ResetColumns"] = "Alle Spaltenbreiten zurücksetzen",

            // ---- keyboard gestures printed in menus ------------------------------------------------------
            ["Key.Down"] = "Pfeil ab",
            ["Key.EscEsc"] = "Esc Esc",

            // ---- status bar --------------------------------------------------------------------------------
            ["Status.Starting"] = "Wird gestartet…",
            ["Status.NeedsAdmin"] = "Zum Lesen der Dateitabellen der Laufwerke sind Administratorrechte erforderlich.",
            ["Status.Elevate"] = "Als Administrator neu starten",
            ["Status.Elevate.Tip"] = "Das Lesen der Dateitabellen erfordert Administratorrechte. Startet Spotter mit erhöhten Rechten neu.",
            ["Status.ElevateFailed"] = "Neustart mit erhöhten Rechten nicht möglich: {0}",
            ["Status.PropertiesFailed"] = "Die Eigenschaften dieses Elements konnten nicht angezeigt werden.",
            ["Status.ReadingDrives"] = "Laufwerke werden gelesen — Ergebnisse erscheinen nach und nach…",
            ["Status.Ready"] = "Bereit — {0} ({1})",
            ["Status.IndexFailed"] = "Indizierung fehlgeschlagen: {0}",
            ["Status.HotkeyTaken"] = "Die Tastenkombination {0} wird bereits von einer anderen App verwendet.",
            ["Status.Demo"] = "DEMOMODUS — {0} (keine Engine, Pfad bleibt leer)",
            ["Status.Found"] = "{0} in {1} ms",
            ["Status.Searching.Suffix"] = " …",
            ["Status.Selected"] = "{0} ausgewählt · {1}",
            ["Status.Selected.Tip"] = "Gesamtgröße der ausgewählten Dateien. Ordner bleiben bewusst außen vor — die Größe eines Ordners enthält seine Dateien bereits, beides zu zählen würde sie doppelt zählen.",
            ["Status.Exporting"] = "{0} werden exportiert…",
            ["Status.Deleting"] = "{0} werden gelöscht…",

            // ---- indexing progress -----------------------------------------------------------------------------
            ["Index.Reading"] = "Laufwerke werden gelesen…",
            ["Index.ReadingCount"] = "Laufwerke werden gelesen — {0}",
            ["Index.ReadingPercent"] = "Laufwerke werden gelesen — {0}, {1}{2}",
            ["Index.DrivesDone"] = "{0} von {1} Laufwerken",
            ["Index.Rate"] = " · {0}/s",
            ["Index.Remaining"] = " · noch etwa {0}",
            ["Index.Volume.Progress"] = "{0}  ({1} von {2})",
            ["Index.Volume.Waiting"] = "wartet",
            ["Index.Volume.Failed"] = "fehlgeschlagen",
            ["Index.Volume.NotSupported"] = "nicht unterstützt",
            ["Index.Volume.SizeUnknown"] = "Größe unbekannt",

            // ---- durations and sizes ------------------------------------------------------------------------------
            ["Duration.Seconds"] = "{0} Sek.",
            ["Duration.MinutesSeconds"] = "{0} Min. {1} Sek.",
            ["Duration.HoursMinutes"] = "{0} Std. {1} Min.",
            ["Unit.B"] = "B",
            ["Unit.KB"] = "KB",
            ["Unit.MB"] = "MB",
            ["Unit.GB"] = "GB",
            ["Unit.TB"] = "TB",
            ["Unit.PB"] = "PB",

            // ---- counted things ---------------------------------------------------------------------------------------
            ["Plural.Results.one"] = "{0:N0} Treffer",
            ["Plural.Results.other"] = "{0:N0} Treffer",
            ["Plural.Files.one"] = "{0:N0} Datei",
            ["Plural.Files.other"] = "{0:N0} Dateien",
            ["Plural.Folders.one"] = "{0:N0} Ordner",
            ["Plural.Folders.other"] = "{0:N0} Ordner",
            ["Plural.Drives.one"] = "{0:N0} Laufwerk",
            ["Plural.Drives.other"] = "{0:N0} Laufwerke",
            ["Plural.Items.one"] = "{0:N0} Element",
            ["Plural.Items.other"] = "{0:N0} Elemente",
            ["Plural.Rows.one"] = "{0:N0} Zeile",
            ["Plural.Rows.other"] = "{0:N0} Zeilen",
            ["Plural.DemoRows.one"] = "{0:N0} synthetische Zeile",
            ["Plural.DemoRows.other"] = "{0:N0} synthetische Zeilen",
            ["Contents.FilesAndFolders"] = "{0}, {1}",

            // ---- dialogs -----------------------------------------------------------------------------------------------
            ["Dialog.BulkOpen.Verb"] = "öffnet",
            ["Dialog.BulkExplorer.Verb"] = "öffnet Explorer-Fenster für",
            ["Dialog.BulkConfirm"] = "Dies {0} {1}. Fortfahren?",
            ["Dialog.NothingToExport"] = "Es gibt nichts zu exportieren.",
            ["Dialog.AlreadyRunning"] = "Spotter läuft bereits, aber diese Kopie konnte dessen Fenster nicht in den Vordergrund holen.\n\nEs kann zu einem anderen Benutzerkonto gehören oder gerade starten.",
            ["Menu.NewInstance"] = "Neue Instanz",
            ["Tray.NewInstance"] = "Neue Instanz",
            ["NewInstance.Tip"] = "Ein zweites Spotter-Fenster mit eigener Suche öffnen.",
            ["Window.ExtraInstance"] = "Spotter — zusätzliche Instanz",
            ["Dialog.NewInstanceFailed"] = "Eine weitere Instanz konnte nicht gestartet werden. Der Grund steht in der Protokolldatei.",
            ["Dialog.Export.Filter"] = "CSV-Datei (*.csv)|*.csv|Textdatei (*.txt)|*.txt",
            ["Dialog.Export.FileName"] = "Spotter-Ergebnisse.csv",
            ["Dialog.Export.CsvHeader"] = "Name,Pfad,Größe,Geändert am",
            ["Dialog.Export.Done"] = "{0} exportiert.\n\nJetzt öffnen?",
            ["Dialog.Export.Failed"] = "Export fehlgeschlagen: {0}",
            ["Dialog.Delete.Permanent"] = "endgültig löschen",
            ["Dialog.Delete.Recycle"] = "in den Papierkorb verschieben",
            ["Dialog.Delete.Confirm"] = "Möchten Sie {1} wirklich {0}?",
            ["Dialog.Delete.Failed"] = "{0} konnten nicht gelöscht werden.",

            // ---- notification area ----------------------------------------------------------------------------------------
            ["Tray.Show"] = "Spotter anzeigen",
            ["Tray.Exit"] = "Beenden",
            ["Tray.Hidden.NoHotkey"] = "Läuft weiter — Doppelklick auf dieses Symbol holt das Fenster zurück.",
            ["Tray.Hidden.Hotkey"] = "Läuft weiter — {0} drücken oder auf dieses Symbol doppelklicken.",

            // ---- engine-reported problems -------------------------------------------------------------------------------------
            ["Problem.Name"] = "Dateiname — {0}",
            ["Problem.Folder"] = "In Ordner — {0}",
            ["Problem.Size"] = "Größe — {0}",
            ["Problem.NoKinds"] = "Dateien oder Ordner einschalten — mit beidem aus gibt es nichts zu finden.",
            ["Problem.NoTypes"] = "Kein Dateityp angekreuzt — die Liste neben „Dateien“ öffnen und einen ankreuzen, oder Ordner einschalten.",
            ["Problem.Unreadable"] = "kann nicht gelesen werden",

            // ---- options window ---------------------------------------------------------------------------------------------------
            ["Options.Title"] = "Optionen",

            ["Options.Language"] = "Sprache",
            ["Options.Language.Hint"] = "Wirkt sofort. Zahlen- und Datumsformate richten sich weiterhin nach Ihren Windows-Regionseinstellungen — das ist in Windows eine eigene Einstellung.",
            ["Options.Language.System"] = "Wie Windows",
            ["Options.Language.SystemDetail"] = "Wie Windows  ({0})",
            ["Options.Language.Unsupported"] = "Windows ist auf eine Sprache eingestellt, die Spotter noch nicht hat; deshalb wird Englisch verwendet.",

            ["Options.Searching"] = "Suche",
            ["Options.SearchAsYouType"] = "Suchen während der Eingabe",
            ["Options.RememberFilters"] = "Die Schalter „Dateien“ und „Ordner“ und die Dateitypen merken",
            ["Options.RememberFilters.Hint"] = "Aus: beide sind bei jedem Start eingeschaltet. Ein Fenster, das mit einem ausgeschalteten Schalter öffnet, findet davon nichts — und sieht aus, als wäre die Datei einfach nicht da. Die Liste der Dateitypen folgt derselben Einstellung.",
            ["Options.SearchAsYouType.On"] = "Jeder getippte Buchstabe und jede geänderte Einstellung startet eine neue Suche.",
            ["Options.SearchAsYouType.Off"] = "Es wird nichts gesucht, bis Sie Suchen oder Enter drücken. Nützlich auf einem ausgelasteten Rechner oder wenn eine Suche fast die ganze Platte trifft.",

            ["Options.Hotkey"] = "Globale Tastenkombination",
            ["Options.Hotkey.Hint"] = "Drücken Sie die gewünschte Kombination und dann Speichern. Sie holt Spotter von überall nach vorn; nochmal gedrückt blendet sie es wieder aus.",
            ["Options.Hotkey.Placeholder"] = "Hier klicken und Kombination drücken",
            ["Options.Hotkey.NeedsModifier"] = "Mindestens eines von Strg, Alt, Umschalt oder Windows hinzufügen — eine einzelne Taste würde überall abgefangen, wo Sie tippen.",
            ["Options.Hotkey.Disable"] = "Tastenkombination deaktivieren",

            ["Options.Tray"] = "Infobereich",
            ["Options.Tray.Show"] = "Symbol im Infobereich anzeigen",
            ["Options.Tray.Minimize"] = "In den Infobereich statt in die Taskleiste minimieren",
            ["Options.Tray.Close"] = "Beim Schließen im Infobereich weiterlaufen",

            ["Options.Appearance"] = "Darstellung",
            ["Options.Theme.Auto"] = "Wie Windows",
            ["Options.Theme.Dark"] = "Dunkel",
            ["Options.Theme.Light"] = "Hell",

            ["Options.Size"] = "Fenstergröße",
            ["Options.Size.Hint"] = "Spotter öffnet sich im linken oberen Viertel Ihres Bildschirms und stellt Text und Bedienelemente in einer Größe dar, die zur Größe dieses Bildschirms passt — nicht zu seiner Pixelzahl.",
            ["Options.Size.Auto"] = "An diesen Bildschirm anpassen",
            ["Options.Size.Auto.Detail"] = "Für Ihren Bildschirm ergeben sich {0} %. Zum Festlegen einer eigenen Größe abwählen.",
            ["Options.Size.Percent"] = "{0} %",
            ["Options.Size.Remember"] = "Merken, wo Sie das Fenster lassen",
            ["Options.Size.Remember.Hint"] = "Größe, Position und Vollbild kommen beim nächsten Start so zurück, wie Sie sie hinterlassen haben. Nur eines wird nie übernommen: ein Fenster, das fast vollständig außerhalb des Bildschirms liegt — es ließe sich nicht mehr zurückziehen.",
            ["Options.Size.Reset"] = "Größe und Position zurücksetzen",
            ["Options.Size.Reset.Tip"] = "Setzt das Fenster sofort auf seine Standardgröße im linken oberen Viertel des Bildschirms zurück.",

            ["Options.Storage"] = "Was diese App auf Ihrem PC speichert",
            ["Options.Storage.Hint"] = "Sonst wird nirgendwo etwas geschrieben. Alles entfernen heißt: die Spotter-Dateien in diesen beiden Ordnern löschen und die unten genannte Aufgabe entfernen.",
            ["Options.Storage.Settings"] = "Einstellungen",
            ["Options.Storage.AppLog"] = "App-Protokoll",
            ["Options.Storage.EngineLog"] = "Engine-Protokoll",
            ["Options.Storage.EngineLog.Suffix"] = "   (dazu die rotierten SpotterLog.N.txt.zip)",
            ["Options.Storage.Task"] = "Geplante Aufgabe",
            ["Options.Storage.Open.Tip"] = "Diese Datei im Explorer anzeigen.",
            ["Options.Storage.Task.Registered"] = "Aufgabenplanung → {0}\nLässt diese Kopie mit erhöhten Rechten starten, ohne jedes Mal eine UAC-Abfrage.",
            ["Options.Storage.Task.Missing"] = "Nicht registriert. Spotter fragt stattdessen per UAC-Abfrage nach Administratorrechten.",
            ["Options.Storage.Task.Remove.Tip"] = "Löscht die Aufgabe. Spotter fragt beim nächsten Start wieder per UAC-Abfrage nach Administratorrechten.",
            ["Options.Storage.Task.Remove.Confirm"] = "Die geplante Aufgabe „{0}“ entfernen?\n\nSpotter läuft weiter — es fragt dann jedes Mal per UAC-Abfrage nach Administratorrechten und kann die Aufgabe danach wieder registrieren.",
            ["Options.Storage.Task.Remove.Failed"] = "Die Aufgabe konnte nicht entfernt werden. Dafür sind Administratorrechte nötig.",
            ["Options.Storage.ExplorerFailed"] = "Explorer konnte nicht geöffnet werden: {0}",

            ["Options.About"] = "Info",
            ["Options.About.Version"] = "Spotter {0}",
            ["Options.About.Text"] = "Sofortige Dateisuche für Windows. Spotter liest die NTFS-Master File Table direkt, statt Ordner zu durchlaufen — deshalb braucht es Administratorrechte, und deshalb erscheinen Ergebnisse so schnell, wie Sie tippen können.",
        };
}
