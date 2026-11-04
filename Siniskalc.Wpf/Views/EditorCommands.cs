namespace Siniskalc.Wpf.Views
{
    using System.Windows.Input;

    /// <summary>
    /// The commands of the menus and the toolbar of the window; the window binds each to what it does.
    /// </summary>
    public static class EditorCommands
    {
        /// <summary>
        /// Reads the open savegame again from its file.
        /// </summary>
        public static readonly RoutedUICommand Reload = new RoutedUICommand(
            "Reload",
            nameof(Reload),
            typeof(EditorCommands));

        /// <summary>
        /// Opens the folder of the open savegame in the file manager.
        /// </summary>
        public static readonly RoutedUICommand GoToSaveFileFolder = new RoutedUICommand(
            "Go to save file folder...",
            nameof(GoToSaveFileFolder),
            typeof(EditorCommands));

        /// <summary>
        /// Writes the bytes of the savefile, uncompressed, to a file.
        /// </summary>
        public static readonly RoutedUICommand DumpToFile = new RoutedUICommand(
            "Export save bytes",
            nameof(DumpToFile),
            typeof(EditorCommands));

        /// <summary>
        /// Shows the bytes of the whole savefile in a hex view.
        /// </summary>
        public static readonly RoutedUICommand ViewSaveHex = new RoutedUICommand(
            "View save hex",
            nameof(ViewSaveHex),
            typeof(EditorCommands));

        /// <summary>
        /// Shows every artifact of the game with its byte code, read only.
        /// </summary>
        public static readonly RoutedUICommand ViewArtifacts = new RoutedUICommand(
            "View Artifacts...",
            nameof(ViewArtifacts),
            typeof(EditorCommands));

        /// <summary>
        /// Shows every creature of the game with its byte code, read only.
        /// </summary>
        public static readonly RoutedUICommand ViewCreatures = new RoutedUICommand(
            "View Creatures...",
            nameof(ViewCreatures),
            typeof(EditorCommands));

        /// <summary>
        /// Opens the page where problems are reported.
        /// </summary>
        public static readonly RoutedUICommand OpenIssues = new RoutedUICommand(
            "Report an issue",
            nameof(OpenIssues),
            typeof(EditorCommands));

        /// <summary>
        /// Opens the folder of the log files.
        /// </summary>
        public static readonly RoutedUICommand ShowLogs = new RoutedUICommand(
            "Show logs",
            nameof(ShowLogs),
            typeof(EditorCommands));

        /// <summary>
        /// Opens the window of the settings.
        /// </summary>
        public static readonly RoutedUICommand EditSettings = new RoutedUICommand(
            "Settings",
            nameof(EditSettings),
            typeof(EditorCommands));

        /// <summary>
        /// Opens the About window.
        /// </summary>
        public static readonly RoutedUICommand About = new RoutedUICommand(
            "About Siniskalc",
            nameof(About),
            typeof(EditorCommands));

        /// <summary>
        /// Opens a savegame from the bytes of a dump.
        /// </summary>
        public static readonly RoutedUICommand LoadFromDump = new RoutedUICommand(
            "Load save from bytes",
            nameof(LoadFromDump),
            typeof(EditorCommands));
    }
}
