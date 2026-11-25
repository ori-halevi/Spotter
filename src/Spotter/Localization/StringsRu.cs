using System.Collections.Generic;

namespace Spotter.Localization;

/// <summary>
/// Russian. The only shipped language that needs three plural forms — see
/// <c>Lang.Count</c>. "5 результата" is as obviously wrong to a Russian reader as
/// "5 result" would be to an English one, so the <c>.one</c> / <c>.few</c> / <c>.many</c> keys
/// below are not optional polish.
///
/// Byte units are Cyrillic here, matching Windows in Russian: Б, КБ, МБ, ГБ, ТБ.
/// </summary>
internal static class StringsRu
{
    internal static readonly IReadOnlyDictionary<string, string> Table =
        new Dictionary<string, string>
        {
            // ---- shared ------------------------------------------------------------------------
            ["Common.Save"] = "Сохранить",
            ["Common.Cancel"] = "Отмена",
            ["Common.Remove"] = "Удалить",
            ["Common.OpenFolder"] = "Открыть папку",

            // ---- query fields ------------------------------------------------------------------
            ["Field.Name.Placeholder"] = "Поиск по имени файла…",
            ["Field.Name.Tip.Title"] = "Имя файла",
            ["Field.Name.Tip.Part"] = "Совпадает с любой частью имени: «note» найдёт «notepad.exe».",
            ["Field.Name.Tip.Words"] = "Несколько слов — должны встретиться все, в любом порядке:  log 2024",
            ["Field.Name.Tip.Wildcards"] = "Подстановочные знаки * и ? действуют внутри каждого слова:  *.dll    report?.txt",
            ["Field.Name.Tip.Keys"] = "Стрелка вниз открывает недавние запросы. Ввод ищет сразу, не дожидаясь паузы в наборе.",
            ["Field.Name.OptionsTip"] = "Параметры имени файла: учёт регистра, регулярное выражение, как трактуются разделители.",
            ["Field.Name.RefreshTip"] = "Выполнить поиск заново, то же самое, что F5. Пригодится, когда файл вот-вот будет создан: сам по себе поиск не повторяется.",

            ["Field.Folder.Placeholder"] = "В папке",
            ["Field.Folder.Tip.Title"] = "В папке",
            ["Field.Folder.Tip.Part"] = "Оставляет только результаты, у которых совпадает папка. Достаточно части имени:  windows",
            ["Field.Folder.Tip.Levels"] = "Уровни разделяются знаком \\ или /, и каждая часть проверяется по очереди:  users\\public",
            ["Field.Folder.Tip.Subfolders"] = "Если вложенные папки выключены, совпасть должна папка самого файла, а не какая-либо папка выше.",
            ["Field.Folder.OptionsTip"] = "Параметры папки: учёт регистра, регулярное выражение, вложенные папки.",
            ["Field.Folder.BrowseTip"] = "Выбрать папку. Диалог откроется там, куда указывает набранный путь.",
            ["Field.Folder.PickerTitle"] = "Искать в папке",

            // Abbreviated: this is the narrowest of the three fields, and the unshortened
            // "например" ran past its right edge.
            ["Field.Size.Placeholder"] = "Размер, напр. >100MB",
            ["Field.Size.Tip.Title"] = "Размер",
            ["Field.Size.Tip.Gt"] = "больше чем",
            ["Field.Size.Tip.Le"] = "не больше чем",
            ["Field.Size.Tip.Eq"] = "примерно такого размера",
            ["Field.Size.Tip.About"] = "в пределах 10 %",
            ["Field.Size.Tip.Range"] = "диапазон",
            ["Field.Size.Tip.Units"] = "Единицы B, KB, MB, GB, TB. Размер папки считается по всему её содержимому, поэтому папки тоже могут попасть под фильтр по размеру.",
            ["Field.Size.PresetsTip"] = "Частые фильтры по размеру.",

            // ---- toolbar -----------------------------------------------------------------------
            ["Toolbar.Files"] = "Файлы",
            ["Toolbar.Files.Tip"] = "Включать файлы в результаты. Выключите, чтобы видеть только папки.",
            ["Toolbar.Folders"] = "Папки",
            ["Toolbar.Folders.Tip"] = "Включать папки в результаты. Выключите, чтобы видеть только файлы.",
            ["Toolbar.ActiveOptions.Tip"] = "Параметры, заданные сейчас в полях запроса. Кнопка параметров каждого поля меняет только его собственные.",
            ["Toolbar.Search"] = "Найти",
            ["Toolbar.SearchAgain"] = "Искать снова",
            ["Toolbar.Stop"] = "Остановить",
            ["Toolbar.Search.Tip"] = "Выполнить поиск. Кнопка показана потому, что «Искать по мере ввода» выключено в параметрах; во время поиска она становится кнопкой «Остановить».",
            ["Toolbar.Recent"] = "Недавние",
            ["Toolbar.Recent.Tip"] = "Прежние запросы, самые новые сверху. Стрелка вниз в поле поиска открывает тот же список.",
            ["Toolbar.Options"] = "Параметры",
            ["Toolbar.Options.Tip"] = "Язык, режим поиска, глобальное сочетание клавиш, значок в области уведомлений, тема и размер окна.",

            // ---- active-options summary ---------------------------------------------------------
            ["Active.Name.Case"] = "имя: учёт регистра",
            ["Active.Name.Regex"] = "имя: регулярное выражение",
            ["Active.Name.ExactSeparators"] = "имя: точные разделители",
            ["Active.Folder.Case"] = "папка: учёт регистра",
            ["Active.Folder.Regex"] = "папка: регулярное выражение",
            ["Active.NoSubfolders"] = "без вложенных папок",

            // ---- search history -----------------------------------------------------------------
            ["History.Empty"] = "Недавних запросов пока нет. То, что вы ищете, будет запоминаться здесь.",
            ["History.Hint"] = "Ввод — использовать · Delete — забыть",
            ["History.Clear"] = "Очистить журнал",

            // ---- columns -------------------------------------------------------------------------
            ["Column.Name"] = "Имя",
            ["Column.Path"] = "Путь",
            ["Column.Size"] = "Размер",
            ["Column.Modified"] = "Изменён",
            ["Column.Type"] = "Тип",
            ["Column.Created"] = "Создан",
            ["Column.Accessed"] = "Открыт",
            ["Column.Tip"] = "Щелчок сортирует по этому столбцу, повторный — меняет порядок. Правый щелчок — другие действия.",

            // ---- text-field menu -----------------------------------------------------------------
            ["Menu.Cut"] = "Вырезать",
            ["Menu.Copy"] = "Копировать",
            ["Menu.Paste"] = "Вставить",
            ["Menu.PasteAndSearch"] = "Вставить и найти",
            ["Menu.PasteAndSearch.Tip"] = "Заменяет содержимое поля текстом из буфера обмена и выполняет поиск.",
            ["Menu.RecentSearches"] = "Недавние запросы…",
            ["Menu.ClearField"] = "Очистить это поле",
            ["Menu.ClearAllFields"] = "Очистить все поля поиска",

            // ---- per-field option menus -------------------------------------------------------------
            ["Menu.MatchCase"] = "Учитывать регистр",
            ["Menu.Regex"] = "Регулярное выражение",
            ["Menu.Regex.Name.Tip"] = "Регулярное выражение .NET по имени файла. Разбиение на слова и знаки * и ? при этом не действуют.",
            ["Menu.Regex.Folder.Tip"] = "Регулярное выражение .NET по пути папки. Обратные косые черты нужно экранировать — кнопка «Обзор» делает это за вас.",
            ["Menu.FlexibleSeparators"] = "Точки и дефисы считаются пробелами",
            ["Menu.FlexibleSeparators.Tip"] = "Включено: «Family.Guy» находит и «Family Guy», и «Family_Guy», потому что точка, подчёркивание или дефис между двумя словами считаются разделителем. Выключено: должен стоять в точности тот знак, который вы ввели.",
            ["Menu.Subfolders"] = "Включая вложенные папки",
            ["Menu.Subfolders.Tip"] = "Включено: подходит любая папка выше файла. Выключено: должна совпасть папка самого файла.",

            // ---- size presets --------------------------------------------------------------------
            ["Menu.Size.Empty"] = "Пустые (0 байт)",
            ["Menu.Size.Tiny"] = "Крошечные — меньше 10 KB",
            ["Menu.Size.Under1MB"] = "Меньше 1 MB",
            ["Menu.Size.Over10MB"] = "Больше 10 MB",
            ["Menu.Size.Over100MB"] = "Больше 100 MB",
            ["Menu.Size.Over1GB"] = "Больше 1 GB",
            ["Menu.Size.Over10GB"] = "Больше 10 GB",
            ["Menu.Size.Between"] = "От 100 MB до 1 GB",
            ["Menu.Size.About700MB"] = "Около 700 MB (±10 %)",
            ["Menu.Size.Clear"] = "Убрать фильтр по размеру",

            // ---- kind filter menu ------------------------------------------------------------------
            ["Menu.Filter.Both"] = "Файлы и папки",
            ["Menu.Filter.FilesOnly"] = "Только файлы",
            ["Menu.Filter.FoldersOnly"] = "Только папки",
            ["Menu.Filter.Reset"] = "Сбросить все параметры поиска",
            ["Menu.Filter.Reset.Tip"] = "Возврат к файлам и папкам, без учёта регистра, без регулярных выражений, с вложенными папками.",

            // ---- result menu -------------------------------------------------------------------------
            ["Menu.Open"] = "Открыть",
            ["Menu.OpenFolder"] = "Открыть содержащую папку",
            ["Menu.CopyPath"] = "Копировать полный путь",
            ["Menu.CopyName"] = "Копировать имя файла",
            ["Menu.CopyFolder"] = "Копировать содержащую папку",
            ["Menu.ExportSelected"] = "Экспортировать выбранное…",
            ["Menu.Delete"] = "Удалить",
            ["Menu.DeletePermanently"] = "Удалить безвозвратно",
            ["Menu.Properties"] = "Свойства",

            // ---- background menu ----------------------------------------------------------------------
            ["Menu.SelectAll"] = "Выделить всё",
            ["Menu.ClearSelection"] = "Снять выделение",
            ["Menu.ExportAll"] = "Экспортировать все результаты…",
            ["Menu.SearchAgain"] = "Искать снова",

            // ---- column header menu -------------------------------------------------------------------
            ["Menu.SortAscending"] = "Сортировать по возрастанию",
            ["Menu.SortDescending"] = "Сортировать по убыванию",
            ["Menu.SortByAscending"] = "Сортировать по столбцу «{0}», по возрастанию",
            ["Menu.SortByDescending"] = "Сортировать по столбцу «{0}», по убыванию",
            ["Menu.FitColumn"] = "Подогнать ширину столбца по содержимому",
            ["Menu.ResetColumns"] = "Сбросить ширину всех столбцов",

            // ---- keyboard gestures printed in menus ------------------------------------------------------
            ["Key.Down"] = "Стрелка вниз",
            ["Key.EscEsc"] = "Esc Esc",

            // ---- status bar --------------------------------------------------------------------------------
            ["Status.Starting"] = "Запуск…",
            ["Status.NeedsAdmin"] = "Для чтения таблиц файлов на дисках нужны права администратора.",
            ["Status.Elevate"] = "Перезапустить от имени администратора",
            ["Status.Elevate.Tip"] = "Чтение таблиц файлов требует прав администратора. Эта кнопка перезапускает Spotter с повышенными правами.",
            ["Status.ElevateFailed"] = "Не удалось перезапустить с повышенными правами: {0}",
            ["Status.PropertiesFailed"] = "Не удалось показать свойства этого элемента.",
            ["Status.ReadingDrives"] = "Чтение дисков — результаты будут появляться по мере загрузки…",
            ["Status.Ready"] = "Готово — {0} ({1})",
            ["Status.IndexFailed"] = "Ошибка индексирования: {0}",
            ["Status.HotkeyTaken"] = "Сочетание {0} уже занято другим приложением.",
            ["Status.Demo"] = "ДЕМОРЕЖИМ — {0} (без движка, столбец пути пуст)",
            ["Status.Found"] = "{0} за {1} мс",
            ["Status.Searching.Suffix"] = " …",
            ["Status.Selected"] = "выделено: {0} · {1}",
            ["Status.Selected.Tip"] = "Общий размер выделенных файлов. Папки намеренно не учитываются: размер папки уже включает файлы внутри неё, и учёт обоих посчитал бы их дважды.",
            ["Status.Exporting"] = "Экспорт: {0}…",
            ["Status.Deleting"] = "Удаление: {0}…",

            // ---- indexing progress -----------------------------------------------------------------------------
            ["Index.Reading"] = "чтение дисков…",
            ["Index.ReadingCount"] = "чтение дисков — {0}",
            ["Index.ReadingPercent"] = "чтение дисков — {0}, {1}{2}",
            ["Index.DrivesDone"] = "{0} из {1} дисков",
            ["Index.Rate"] = " · {0}/с",
            ["Index.Remaining"] = " · осталось около {0}",
            ["Index.Volume.Progress"] = "{0}  ({1} из {2})",
            ["Index.Volume.Waiting"] = "ожидание",
            ["Index.Volume.Failed"] = "ошибка",
            ["Index.Volume.NotSupported"] = "не поддерживается",
            ["Index.Volume.SizeUnknown"] = "размер неизвестен",

            // ---- durations and sizes ------------------------------------------------------------------------------
            ["Duration.Seconds"] = "{0} с",
            ["Duration.MinutesSeconds"] = "{0} мин {1} с",
            ["Duration.HoursMinutes"] = "{0} ч {1} мин",
            ["Unit.B"] = "Б",
            ["Unit.KB"] = "КБ",
            ["Unit.MB"] = "МБ",
            ["Unit.GB"] = "ГБ",
            ["Unit.TB"] = "ТБ",
            ["Unit.PB"] = "ПБ",

            // ---- counted things ---------------------------------------------------------------------------------------
            ["Plural.Results.one"] = "{0:N0} результат",
            ["Plural.Results.few"] = "{0:N0} результата",
            ["Plural.Results.many"] = "{0:N0} результатов",
            ["Plural.Files.one"] = "{0:N0} файл",
            ["Plural.Files.few"] = "{0:N0} файла",
            ["Plural.Files.many"] = "{0:N0} файлов",
            ["Plural.Folders.one"] = "{0:N0} папка",
            ["Plural.Folders.few"] = "{0:N0} папки",
            ["Plural.Folders.many"] = "{0:N0} папок",
            ["Plural.Drives.one"] = "{0:N0} диск",
            ["Plural.Drives.few"] = "{0:N0} диска",
            ["Plural.Drives.many"] = "{0:N0} дисков",
            ["Plural.Items.one"] = "{0:N0} элемент",
            ["Plural.Items.few"] = "{0:N0} элемента",
            ["Plural.Items.many"] = "{0:N0} элементов",
            ["Plural.Rows.one"] = "{0:N0} строка",
            ["Plural.Rows.few"] = "{0:N0} строки",
            ["Plural.Rows.many"] = "{0:N0} строк",
            ["Plural.DemoRows.one"] = "{0:N0} синтетическая строка",
            ["Plural.DemoRows.few"] = "{0:N0} синтетические строки",
            ["Plural.DemoRows.many"] = "{0:N0} синтетических строк",
            ["Contents.FilesAndFolders"] = "{0}, {1}",

            // ---- dialogs -----------------------------------------------------------------------------------------------
            ["Dialog.BulkOpen.Verb"] = "открыть",
            ["Dialog.BulkExplorer.Verb"] = "открыть окна проводника для",
            ["Dialog.BulkConfirm"] = "Сейчас будет выполнено: {0} {1}. Продолжить?",
            ["Dialog.NothingToExport"] = "Экспортировать нечего.",
            ["Dialog.AlreadyRunning"] = "Spotter уже запущен, но этой копии не удалось вывести его окно на передний план.\n\nВозможно, он принадлежит другой учётной записи или ещё запускается.",
            ["Dialog.Export.Filter"] = "Файл CSV (*.csv)|*.csv|Текстовый файл (*.txt)|*.txt",
            ["Dialog.Export.FileName"] = "Результаты Spotter.csv",
            ["Dialog.Export.CsvHeader"] = "Имя,Путь,Размер,Изменён",
            ["Dialog.Export.Done"] = "Экспортировано: {0}.\n\nОткрыть файл сейчас?",
            ["Dialog.Export.Failed"] = "Ошибка экспорта: {0}",
            ["Dialog.Delete.Permanent"] = "удалить безвозвратно",
            ["Dialog.Delete.Recycle"] = "переместить в корзину",
            ["Dialog.Delete.Confirm"] = "Действительно {0} {1}?",
            ["Dialog.Delete.Failed"] = "Не удалось удалить: {0}.",

            // ---- notification area ----------------------------------------------------------------------------------------
            ["Tray.Show"] = "Показать Spotter",
            ["Tray.Exit"] = "Выход",
            ["Tray.Hidden.NoHotkey"] = "Программа работает — дважды щёлкните этот значок, чтобы вернуть окно.",
            ["Tray.Hidden.Hotkey"] = "Программа работает — нажмите {0} или дважды щёлкните этот значок.",

            // ---- engine-reported problems -------------------------------------------------------------------------------------
            ["Problem.Name"] = "Имя файла — {0}",
            ["Problem.Folder"] = "В папке — {0}",
            ["Problem.Size"] = "Размер — {0}",
            ["Problem.NoKinds"] = "Включите «Файлы» или «Папки» — когда выключено и то и другое, искать нечего.",
            ["Problem.Unreadable"] = "не удаётся прочитать",

            // ---- options window ---------------------------------------------------------------------------------------------------
            ["Options.Title"] = "Параметры",

            ["Options.Language"] = "Язык",
            ["Options.Language.Hint"] = "Применяется сразу. Форматы чисел и дат по-прежнему следуют региональным параметрам Windows — в Windows это отдельная настройка.",
            ["Options.Language.System"] = "Как в Windows",
            ["Options.Language.SystemDetail"] = "Как в Windows  ({0})",
            ["Options.Language.Unsupported"] = "В Windows выбран язык, которого в Spotter пока нет, поэтому используется английский.",

            ["Options.Searching"] = "Поиск",
            ["Options.SearchAsYouType"] = "Искать по мере ввода",
            ["Options.SearchAsYouType.On"] = "Новый поиск начинается с каждой набранной буквой и при каждом изменении параметра.",
            ["Options.SearchAsYouType.Off"] = "Ничего не ищется, пока вы не нажмёте «Найти» или Ввод. Полезно на загруженной машине или когда запрос совпадает почти со всем диском.",

            ["Options.Hotkey"] = "Глобальное сочетание клавиш",
            ["Options.Hotkey.Hint"] = "Нажмите нужное сочетание, затем «Сохранить». Оно выводит Spotter на передний план откуда угодно; повторное нажатие скрывает окно.",
            ["Options.Hotkey.Placeholder"] = "Щёлкните здесь и нажмите сочетание",
            ["Options.Hotkey.NeedsModifier"] = "Добавьте хотя бы Ctrl, Alt, Shift или Win — одиночная клавиша перехватывалась бы везде, где вы печатаете.",
            ["Options.Hotkey.Disable"] = "Отключить сочетание",

            ["Options.Tray"] = "Область уведомлений",
            ["Options.Tray.Show"] = "Показывать значок в области уведомлений",
            ["Options.Tray.Minimize"] = "Сворачивать в область уведомлений, а не на панель задач",
            ["Options.Tray.Close"] = "При закрытии окна продолжать работу в области уведомлений",

            ["Options.Appearance"] = "Внешний вид",
            ["Options.Theme.Auto"] = "Как в Windows",
            ["Options.Theme.Dark"] = "Тёмная",
            ["Options.Theme.Light"] = "Светлая",

            ["Options.Size"] = "Размер окна",
            ["Options.Size.Hint"] = "Spotter открывается в левой верхней четверти экрана и рисует текст и элементы управления такого размера, какой подходит размеру этого экрана, а не количеству пикселей на нём.",
            ["Options.Size.Auto"] = "Подстроить под этот экран",
            ["Options.Size.Auto.Detail"] = "Для вашего экрана получается {0} %. Снимите флажок, чтобы задать свой размер.",
            ["Options.Size.Percent"] = "{0} %",
            ["Options.Size.Remember"] = "Запоминать, где вы оставили окно",
            ["Options.Size.Remember.Hint"] = "Размер, положение и развёрнутый вид вернутся такими, какими вы их оставили. Не сохраняется лишь одно: окно, оставленное почти полностью за пределами экрана, — вернуть его мышью было бы уже нечем.",
            ["Options.Size.Reset"] = "Сбросить размер и положение",
            ["Options.Size.Reset.Tip"] = "Сразу возвращает окно в левую верхнюю четверть экрана и к размеру по умолчанию.",

            ["Options.Storage"] = "Что программа хранит на вашем компьютере",
            ["Options.Storage.Hint"] = "Больше нигде ничего не записывается. Чтобы удалить всё: удалите файлы Spotter из этих двух папок и уберите задание, указанное ниже.",
            ["Options.Storage.Settings"] = "Настройки",
            ["Options.Storage.AppLog"] = "Журнал приложения",
            ["Options.Storage.EngineLog"] = "Журнал движка",
            ["Options.Storage.EngineLog.Suffix"] = "   (плюс архивы SpotterLog.N.txt.zip)",
            ["Options.Storage.Task"] = "Задание планировщика",
            ["Options.Storage.Open.Tip"] = "Показать этот файл в проводнике.",
            ["Options.Storage.Task.Registered"] = "Планировщик заданий → {0}\nПозволяет этой копии запускаться с повышенными правами без запроса UAC каждый раз.",
            ["Options.Storage.Task.Missing"] = "Не зарегистрировано. Вместо этого Spotter запрашивает права администратора через UAC.",
            ["Options.Storage.Task.Remove.Tip"] = "Удаляет задание. При следующем запуске Spotter снова запросит права администратора через UAC.",
            ["Options.Storage.Task.Remove.Confirm"] = "Удалить задание планировщика «{0}»?\n\nSpotter продолжит работать — он будет каждый раз запрашивать права администратора через UAC и сможет зарегистрировать задание снова, когда вы подтвердите запрос.",
            ["Options.Storage.Task.Remove.Failed"] = "Не удалось удалить задание. Для этого нужны права администратора.",
            ["Options.Storage.ExplorerFailed"] = "Не удалось открыть проводник: {0}",

            ["Options.About"] = "О программе",
            ["Options.About.Version"] = "Spotter {0}",
            ["Options.About.Text"] = "Мгновенный поиск файлов для Windows. Spotter читает главную файловую таблицу NTFS напрямую, а не обходит папки, — поэтому ему нужны права администратора и поэтому результаты появляются со скоростью набора.",
        };
}
