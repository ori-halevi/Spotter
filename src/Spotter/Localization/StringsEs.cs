using System.Collections.Generic;

namespace Spotter.Localization;

/// <summary>
/// Spanish, neutral rather than regional, and formal ("usted") wherever the app addresses the
/// user directly — which is what Windows itself does in Spanish. Terminology follows the system:
/// Papelera de reciclaje, Explorador de archivos, área de notificación, Programador de tareas.
/// </summary>
internal static class StringsEs
{
    internal static readonly IReadOnlyDictionary<string, string> Table =
        new Dictionary<string, string>
        {
            // ---- shared ------------------------------------------------------------------------
            ["Common.Save"] = "Guardar",
            ["Common.Cancel"] = "Cancelar",
            ["Common.Remove"] = "Quitar",
            ["Common.OpenFolder"] = "Abrir carpeta",

            // ---- query fields ------------------------------------------------------------------
            ["Field.Name.Placeholder"] = "Buscar nombre de archivo…",
            ["Field.Name.Tip.Title"] = "Nombre de archivo",
            ["Field.Name.Tip.Part"] = "Coincide con cualquier parte del nombre: 'note' encuentra 'notepad.exe'.",
            ["Field.Name.Tip.Words"] = "Si escribe varias palabras, deben aparecer todas, en cualquier orden:  log 2024",
            ["Field.Name.Tip.Wildcards"] = "Los comodines * y ? se aplican a cada palabra:  *.dll    report?.txt",
            ["Field.Name.Tip.Keys"] = "La flecha abajo abre las búsquedas recientes. Entrar busca ya, sin esperar la pausa de escritura.",
            ["Field.Name.OptionsTip"] = "Opciones del nombre de archivo: mayúsculas y minúsculas, expresión regular, cómo se tratan los separadores.",
            ["Field.Name.RefreshTip"] = "Ejecutar la búsqueda de nuevo, igual que F5. Útil cuando un archivo está a punto de crearse: nada se repite por sí solo mientras esperas.",

            ["Field.Folder.Placeholder"] = "En la carpeta",
            ["Field.Folder.Tip.Title"] = "En la carpeta",
            ["Field.Folder.Tip.Part"] = "Solo conserva los resultados cuya carpeta coincida. Basta con parte del nombre:  windows",
            ["Field.Folder.Tip.Levels"] = "Separe los niveles con \\ o / y cada parte se comprueba por turno:  users\\public",
            ["Field.Folder.Tip.Subfolders"] = "Con las subcarpetas desactivadas debe coincidir la carpeta del propio archivo, no cualquier carpeta superior.",
            ["Field.Folder.OptionsTip"] = "Opciones de la carpeta: mayúsculas y minúsculas, expresión regular, subcarpetas.",
            ["Field.Folder.BrowseTip"] = "Elegir una carpeta. El selector se abre donde apunte lo que haya escrito.",
            ["Field.Folder.PickerTitle"] = "Buscar en la carpeta",

            ["Field.Size.Placeholder"] = "Tamaño, p. ej. >100MB",
            ["Field.Size.Tip.Title"] = "Tamaño",
            ["Field.Size.Tip.Gt"] = "mayor que",
            ["Field.Size.Tip.Le"] = "como máximo",
            ["Field.Size.Tip.Eq"] = "aproximadamente ese tamaño",
            ["Field.Size.Tip.About"] = "dentro de un 10 %",
            ["Field.Size.Tip.Range"] = "un intervalo",
            ["Field.Size.Tip.Units"] = "Unidades B, KB, MB, GB, TB. Una carpeta se mide por todo lo que contiene, así que las carpetas también pueden coincidir con un filtro de tamaño.",
            ["Field.Size.PresetsTip"] = "Filtros de tamaño habituales.",

            // ---- toolbar -----------------------------------------------------------------------
            ["Toolbar.Files"] = "Archivos",
            ["Toolbar.Files.Tip"] = "Incluir archivos en los resultados. Desactívelo para ver solo carpetas.",
            ["Toolbar.Folders"] = "Carpetas",
            ["Toolbar.Folders.Tip"] = "Incluir carpetas en los resultados. Desactívelo para ver solo archivos.",

            // The file types panel behind the Files switch.
            ["Types.Title"] = "Tipos de archivo",
            ["Types.All"] = "Todos los tipos",
            ["Types.Tip"] = "Qué tipos de archivo cuentan. Todos, salvo que se indique otra cosa.",
            ["Types.Image"] = "Imágenes",
            ["Types.Video"] = "Vídeo",
            ["Types.Audio"] = "Audio",
            ["Types.Document"] = "Documentos",
            ["Types.Archive"] = "Archivos comprimidos",
            ["Types.Code"] = "Código y configuración",
            ["Types.Program"] = "Programas e instaladores",
            ["Types.Other"] = "Todo lo demás",
            ["Types.Other.Tip"] = "Todos los archivos que ninguna otra categoría reclama, incluidos los archivos sin extensión alguna.",
            ["Types.Custom"] = "Otras extensiones",
            ["Types.Custom.Placeholder"] = "iso, mkv, bak",
            ["Types.Custom.Hint"] = "Se añaden a lo marcado arriba. Para buscar solo estas, desmarque los tipos.",
            ["Toolbar.ActiveOptions.Tip"] = "Opciones activas en los campos de búsqueda. El botón de opciones de cada campo cambia solo las suyas.",
            ["Toolbar.Search"] = "Buscar",
            ["Toolbar.SearchAgain"] = "Buscar de nuevo",
            ["Toolbar.Stop"] = "Detener",
            ["Toolbar.Search.Tip"] = "Ejecutar la búsqueda. Aparece porque «Buscar mientras escribe» está desactivado en Opciones; mientras una búsqueda está en curso este botón pasa a Detener.",
            ["Toolbar.Recent"] = "Recientes",
            ["Toolbar.Recent.Tip"] = "Búsquedas anteriores, la más reciente primero. La flecha abajo en el cuadro de búsqueda abre la misma lista.",
            ["Toolbar.Options"] = "Opciones",
            ["Toolbar.Options.Tip"] = "Idioma, modo de búsqueda, tecla rápida global, icono del área de notificación, tema y tamaño de la ventana.",

            // ---- active-options summary ---------------------------------------------------------
            ["Active.Name.Case"] = "nombre: mayúsculas y minúsculas",
            ["Active.Name.Regex"] = "nombre: expresión regular",
            ["Active.Name.ExactSeparators"] = "nombre: separadores exactos",
            ["Active.Folder.Case"] = "carpeta: mayúsculas y minúsculas",
            ["Active.Folder.Regex"] = "carpeta: expresión regular",
            ["Active.NoSubfolders"] = "sin subcarpetas",

            // ---- search history -----------------------------------------------------------------
            ["History.Empty"] = "Todavía no hay búsquedas recientes. Lo que busque se recordará aquí.",
            ["History.Hint"] = "Entrar para usar · Supr para olvidar una",
            ["History.Clear"] = "Borrar el historial",

            // ---- columns -------------------------------------------------------------------------
            ["Column.Name"] = "Nombre",
            ["Column.Path"] = "Ruta de acceso",
            ["Column.Size"] = "Tamaño",
            ["Column.Modified"] = "Modificado",
            ["Column.Type"] = "Tipo",
            ["Column.Created"] = "Creado",
            ["Column.Accessed"] = "Último acceso",
            ["Column.Tip"] = "Haga clic para ordenar por esta columna; otro clic invierte el orden. Clic derecho para más opciones.",

            // ---- text-field menu -----------------------------------------------------------------
            ["Menu.Cut"] = "Cortar",
            ["Menu.Copy"] = "Copiar",
            ["Menu.Paste"] = "Pegar",
            ["Menu.PasteAndSearch"] = "Pegar y buscar",
            ["Menu.PasteAndSearch.Tip"] = "Sustituye este campo por el texto del portapapeles y lo busca.",
            ["Menu.RecentSearches"] = "Búsquedas recientes…",
            ["Menu.ClearField"] = "Vaciar este campo",
            ["Menu.ClearAllFields"] = "Vaciar todos los campos de búsqueda",

            // ---- per-field option menus -------------------------------------------------------------
            ["Menu.MatchCase"] = "Distinguir mayúsculas y minúsculas",
            ["Menu.Regex"] = "Expresión regular",
            ["Menu.Regex.Name.Tip"] = "Una expresión regular de .NET sobre el nombre de archivo. La búsqueda por palabras y los comodines * y ? dejan de aplicarse.",
            ["Menu.Regex.Folder.Tip"] = "Una expresión regular de .NET sobre la ruta de la carpeta. Las barras invertidas hay que escaparlas — el botón Examinar lo hace por usted.",
            ["Menu.FlexibleSeparators"] = "Los puntos y guiones cuentan como espacios",
            ["Menu.FlexibleSeparators.Tip"] = "Activado: 'Family.Guy' encuentra también 'Family Guy' y 'Family_Guy', porque un punto, un guion bajo o un guion entre dos palabras se toma como separación. Desactivado: tiene que ser exactamente el carácter que escribió.",
            ["Menu.Subfolders"] = "Incluir subcarpetas",
            ["Menu.Subfolders.Tip"] = "Activado: vale cualquier carpeta por encima del archivo. Desactivado: debe coincidir la carpeta del propio archivo.",

            // ---- size presets --------------------------------------------------------------------
            ["Menu.Size.Empty"] = "Vacío (0 bytes)",
            ["Menu.Size.Tiny"] = "Diminuto — menos de 10 KB",
            ["Menu.Size.Under1MB"] = "Menos de 1 MB",
            ["Menu.Size.Over10MB"] = "Más de 10 MB",
            ["Menu.Size.Over100MB"] = "Más de 100 MB",
            ["Menu.Size.Over1GB"] = "Más de 1 GB",
            ["Menu.Size.Over10GB"] = "Más de 10 GB",
            ["Menu.Size.Between"] = "Entre 100 MB y 1 GB",
            ["Menu.Size.About700MB"] = "Unos 700 MB (±10 %)",
            ["Menu.Size.Clear"] = "Quitar el filtro de tamaño",

            // ---- kind filter menu ------------------------------------------------------------------
            ["Menu.Filter.Both"] = "Archivos y carpetas",
            ["Menu.Filter.FilesOnly"] = "Solo archivos",
            ["Menu.Filter.FoldersOnly"] = "Solo carpetas",
            ["Menu.Filter.Reset"] = "Restablecer todas las opciones de búsqueda",
            ["Menu.Filter.Reset.Tip"] = "Vuelve a archivos y carpetas, sin distinguir mayúsculas, sin expresión regular y con subcarpetas.",

            // ---- result menu -------------------------------------------------------------------------
            ["Menu.Open"] = "Abrir",
            ["Menu.OpenFolder"] = "Abrir la carpeta contenedora",
            ["Menu.CopyPath"] = "Copiar la ruta completa",
            ["Menu.CopyName"] = "Copiar el nombre de archivo",
            ["Menu.CopyFolder"] = "Copiar la carpeta contenedora",
            ["Menu.ExportSelected"] = "Exportar la selección…",
            ["Menu.Delete"] = "Eliminar",
            ["Menu.DeletePermanently"] = "Eliminar permanentemente",
            ["Menu.Properties"] = "Propiedades",

            // ---- background menu ----------------------------------------------------------------------
            ["Menu.SelectAll"] = "Seleccionar todo",
            ["Menu.ClearSelection"] = "Anular la selección",
            ["Menu.ExportAll"] = "Exportar todos los resultados…",
            ["Menu.SearchAgain"] = "Buscar de nuevo",

            // ---- column header menu -------------------------------------------------------------------
            ["Menu.SortAscending"] = "Ordenar ascendente",
            ["Menu.SortDescending"] = "Ordenar descendente",
            ["Menu.SortByAscending"] = "Ordenar por {0}, ascendente",
            ["Menu.SortByDescending"] = "Ordenar por {0}, descendente",
            ["Menu.FitColumn"] = "Ajustar esta columna al contenido",
            ["Menu.ResetColumns"] = "Restablecer el ancho de todas las columnas",

            // ---- keyboard gestures printed in menus ------------------------------------------------------
            ["Key.Down"] = "Flecha abajo",
            ["Key.EscEsc"] = "Esc Esc",

            // ---- status bar --------------------------------------------------------------------------------
            ["Status.Starting"] = "Iniciando…",
            ["Status.NeedsAdmin"] = "Se necesitan permisos de administrador para leer las tablas de archivos de las unidades.",
            ["Status.Elevate"] = "Reiniciar como administrador",
            ["Status.Elevate.Tip"] = "Leer las tablas de archivos de las unidades requiere permisos de administrador. Esto reinicia Spotter con permisos elevados.",
            ["Status.ElevateFailed"] = "No se pudo reiniciar con permisos elevados: {0}",
            ["Status.PropertiesFailed"] = "No se pudieron mostrar las propiedades de ese elemento.",
            ["Status.ReadingDrives"] = "Leyendo las unidades: los resultados irán apareciendo a medida que se carguen…",
            ["Status.Ready"] = "Listo — {0} ({1})",
            ["Status.IndexFailed"] = "Error al indizar: {0}",
            ["Status.HotkeyTaken"] = "La combinación {0} ya la está usando otra aplicación.",
            ["Status.Demo"] = "MODO DEMO — {0} (sin motor, la ruta queda vacía)",
            ["Status.Found"] = "{0} en {1} ms",
            ["Status.Searching.Suffix"] = " …",
            ["Status.Selected"] = "{0} seleccionados · {1}",
            ["Status.Selected.Tip"] = "Tamaño total de los archivos seleccionados. Las carpetas quedan fuera a propósito: el tamaño de una carpeta ya incluye los archivos que contiene, así que contar ambos los contaría dos veces.",
            ["Status.Exporting"] = "Exportando {0}…",
            ["Status.Deleting"] = "Eliminando {0}…",

            // ---- indexing progress -----------------------------------------------------------------------------
            ["Index.Reading"] = "leyendo las unidades…",
            ["Index.ReadingCount"] = "leyendo las unidades — {0}",
            ["Index.ReadingPercent"] = "leyendo las unidades — {0}, {1}{2}",
            ["Index.DrivesDone"] = "{0} de {1} unidades",
            ["Index.Rate"] = " · {0}/s",
            ["Index.Remaining"] = " · quedan unos {0}",
            ["Index.Volume.Progress"] = "{0}  ({1} de {2})",
            ["Index.Volume.Waiting"] = "esperando",
            ["Index.Volume.Failed"] = "error",
            ["Index.Volume.NotSupported"] = "no compatible",
            ["Index.Volume.SizeUnknown"] = "tamaño desconocido",

            // ---- durations and sizes ------------------------------------------------------------------------------
            ["Duration.Seconds"] = "{0} s",
            ["Duration.MinutesSeconds"] = "{0} min {1} s",
            ["Duration.HoursMinutes"] = "{0} h {1} min",
            ["Unit.B"] = "B",
            ["Unit.KB"] = "KB",
            ["Unit.MB"] = "MB",
            ["Unit.GB"] = "GB",
            ["Unit.TB"] = "TB",
            ["Unit.PB"] = "PB",

            // ---- counted things ---------------------------------------------------------------------------------------
            ["Plural.Results.one"] = "{0:N0} resultado",
            ["Plural.Results.other"] = "{0:N0} resultados",
            ["Plural.Files.one"] = "{0:N0} archivo",
            ["Plural.Files.other"] = "{0:N0} archivos",
            ["Plural.Folders.one"] = "{0:N0} carpeta",
            ["Plural.Folders.other"] = "{0:N0} carpetas",
            ["Plural.Drives.one"] = "{0:N0} unidad",
            ["Plural.Drives.other"] = "{0:N0} unidades",
            ["Plural.Items.one"] = "{0:N0} elemento",
            ["Plural.Items.other"] = "{0:N0} elementos",
            ["Plural.Rows.one"] = "{0:N0} fila",
            ["Plural.Rows.other"] = "{0:N0} filas",
            ["Plural.DemoRows.one"] = "{0:N0} fila sintética",
            ["Plural.DemoRows.other"] = "{0:N0} filas sintéticas",
            ["Contents.FilesAndFolders"] = "{0}, {1}",

            // ---- dialogs -----------------------------------------------------------------------------------------------
            ["Dialog.BulkOpen.Verb"] = "abrir",
            ["Dialog.BulkExplorer.Verb"] = "abrir ventanas del Explorador para",
            ["Dialog.BulkConfirm"] = "Esto va a {0} {1}. ¿Continuar?",
            ["Dialog.NothingToExport"] = "No hay nada que exportar.",
            ["Dialog.AlreadyRunning"] = "Spotter ya se está ejecutando, pero esta copia no ha podido traer su ventana al frente.\n\nPuede pertenecer a otra cuenta de usuario, o estar iniciándose.",
            ["Menu.NewInstance"] = "Nueva instancia",
            ["Tray.NewInstance"] = "Nueva instancia",
            ["NewInstance.Tip"] = "Abrir una segunda ventana de Spotter con su propia búsqueda.",
            ["Window.ExtraInstance"] = "Spotter — instancia adicional",
            ["Dialog.NewInstanceFailed"] = "No se ha podido iniciar otra instancia. El motivo está en el archivo de registro.",
            ["Dialog.Export.Filter"] = "Archivo CSV (*.csv)|*.csv|Archivo de texto (*.txt)|*.txt",
            ["Dialog.Export.FileName"] = "Resultados de Spotter.csv",
            ["Dialog.Export.CsvHeader"] = "Nombre,Ruta de acceso,Tamaño,Modificado",
            ["Dialog.Export.Done"] = "Se exportaron {0}.\n\n¿Abrirlo ahora?",
            ["Dialog.Export.Failed"] = "Error al exportar: {0}",
            ["Dialog.Delete.Permanent"] = "eliminar permanentemente",
            ["Dialog.Delete.Recycle"] = "enviar a la Papelera de reciclaje",
            ["Dialog.Delete.Confirm"] = "¿Seguro que quiere {0} {1}?",
            ["Dialog.Delete.Failed"] = "No se pudieron eliminar {0}.",

            // ---- notification area ----------------------------------------------------------------------------------------
            ["Tray.Show"] = "Mostrar Spotter",
            ["Tray.Exit"] = "Salir",
            ["Tray.Hidden.NoHotkey"] = "Sigue en ejecución: haga doble clic en este icono para recuperarlo.",
            ["Tray.Hidden.Hotkey"] = "Sigue en ejecución: pulse {0} o haga doble clic en este icono.",

            // ---- engine-reported problems -------------------------------------------------------------------------------------
            ["Problem.Name"] = "Nombre de archivo — {0}",
            ["Problem.Folder"] = "En la carpeta — {0}",
            ["Problem.Size"] = "Tamaño — {0}",
            ["Problem.NoKinds"] = "Active Archivos o Carpetas: con los dos desactivados no hay nada que encontrar.",
            ["Problem.NoTypes"] = "No hay ningún tipo de archivo marcado: abra la lista junto a Archivos y marque uno, o active Carpetas.",
            ["Problem.Unreadable"] = "no se puede leer",

            // ---- options window ---------------------------------------------------------------------------------------------------
            ["Options.Title"] = "Opciones",

            ["Options.Language"] = "Idioma",
            ["Options.Language.Hint"] = "Se aplica al instante. Los formatos de número y fecha siguen usando la configuración regional de Windows, que allí es una opción aparte.",
            ["Options.Language.System"] = "El de Windows",
            ["Options.Language.SystemDetail"] = "El de Windows  ({0})",
            ["Options.Language.Unsupported"] = "Windows está configurado en un idioma que Spotter todavía no tiene, así que usa inglés.",

            ["Options.Searching"] = "Búsqueda",
            ["Options.SearchAsYouType"] = "Buscar mientras escribe",
            ["Options.RememberFilters"] = "Recordar los interruptores Archivos y Carpetas",
            ["Options.RememberFilters.Hint"] = "Desactivado: los dos empiezan encendidos en cada ejecución. Una ventana que se abre con uno apagado no encuentra nada de ese tipo, y parece que el archivo simplemente no está. La lista de tipos de archivo sigue el mismo ajuste.",
            ["Options.SearchAsYouType.On"] = "Cada letra escrita o ajuste cambiado inicia una búsqueda nueva.",
            ["Options.SearchAsYouType.Off"] = "No se busca nada hasta que pulse Buscar o Entrar. Útil en un equipo cargado, o cuando una consulta coincide con casi todo el disco.",

            ["Options.Hotkey"] = "Tecla rápida global",
            ["Options.Hotkey.Hint"] = "Pulse la combinación que quiera y luego Guardar. Trae Spotter al frente desde cualquier sitio; pulsarla otra vez lo oculta.",
            ["Options.Hotkey.Placeholder"] = "Haga clic aquí y pulse una combinación",
            ["Options.Hotkey.NeedsModifier"] = "Añada al menos Ctrl, Alt, Mayús o Windows: una tecla sola se capturaría en todas partes donde escriba.",
            ["Options.Hotkey.Disable"] = "Desactivar la tecla rápida",

            ["Options.Tray"] = "Área de notificación",
            ["Options.Tray.Show"] = "Mostrar un icono en el área de notificación",
            ["Options.Tray.Minimize"] = "Minimizar al área de notificación en lugar de a la barra de tareas",
            ["Options.Tray.Close"] = "Al cerrar la ventana, seguir en ejecución en el área de notificación",

            ["Options.Appearance"] = "Apariencia",
            ["Options.Theme.Auto"] = "El de Windows",
            ["Options.Theme.Dark"] = "Oscuro",
            ["Options.Theme.Light"] = "Claro",

            ["Options.Size"] = "Tamaño de la ventana",
            ["Options.Size.Hint"] = "Spotter se abre en el cuarto superior izquierdo de la pantalla y dibuja su texto y sus controles a un tamaño acorde con lo grande que es esa pantalla, no con cuántos píxeles tiene.",
            ["Options.Size.Auto"] = "Ajustar a esta pantalla",
            ["Options.Size.Auto.Detail"] = "Para su pantalla sale {0} %. Desmárquelo para fijar un tamaño propio.",
            ["Options.Size.Percent"] = "{0} %",
            ["Options.Size.Remember"] = "Recordar dónde deja la ventana",
            ["Options.Size.Remember.Hint"] = "El tamaño, la posición y la maximización vuelven tal como los dejó. Lo único que nunca se guarda es una ventana que quede casi por completo fuera de la pantalla, porque ya no habría forma de arrastrarla de vuelta.",
            ["Options.Size.Reset"] = "Restablecer tamaño y posición",
            ["Options.Size.Reset.Tip"] = "Devuelve la ventana al cuarto superior izquierdo de la pantalla con su tamaño predeterminado, al instante.",

            ["Options.Storage"] = "Lo que esta aplicación guarda en su PC",
            ["Options.Storage.Hint"] = "No se escribe nada más en ningún sitio. Para quitarlo todo: borre los archivos de Spotter de estas dos carpetas y elimine la tarea de abajo.",
            ["Options.Storage.Settings"] = "Configuración",
            ["Options.Storage.AppLog"] = "Registro de la aplicación",
            ["Options.Storage.EngineLog"] = "Registro del motor",
            ["Options.Storage.EngineLog.Suffix"] = "   (más los SpotterLog.N.txt.zip rotados)",
            ["Options.Storage.Task"] = "Tarea programada",
            ["Options.Storage.Open.Tip"] = "Mostrar este archivo en el Explorador.",
            ["Options.Storage.Task.Registered"] = "Programador de tareas → {0}\nPermite que esta copia se inicie con permisos elevados sin un aviso de UAC cada vez.",
            ["Options.Storage.Task.Missing"] = "No registrada. En su lugar, Spotter pide permisos de administrador con un aviso de UAC.",
            ["Options.Storage.Task.Remove.Tip"] = "Elimina la tarea. La próxima vez que se inicie, Spotter volverá a pedir permisos de administrador con un aviso de UAC.",
            ["Options.Storage.Task.Remove.Confirm"] = "¿Quitar la tarea programada «{0}»?\n\nSpotter seguirá funcionando: pedirá permisos de administrador con un aviso de UAC cada vez, y podrá volver a registrar la tarea cuando apruebe uno.",
            ["Options.Storage.Task.Remove.Failed"] = "No se pudo quitar la tarea. Para hacerlo se necesitan permisos de administrador.",
            ["Options.Storage.ExplorerFailed"] = "No se pudo abrir el Explorador: {0}",

            ["Options.About"] = "Acerca de",
            ["Options.About.Version"] = "Spotter {0}",
            ["Options.About.Text"] = "Búsqueda instantánea de archivos para Windows. Spotter lee directamente la tabla maestra de archivos de NTFS en lugar de recorrer carpetas: por eso necesita permisos de administrador y por eso los resultados aparecen tan rápido como escribes.",
        };
}
