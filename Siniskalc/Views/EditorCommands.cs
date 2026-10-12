namespace Siniskalc.Views
{
    using System.Windows.Input;

    public static class EditorCommands
    {
        public static readonly RoutedUICommand Reload = new RoutedUICommand(
            "Reload",
            nameof(Reload),
            typeof(EditorCommands));

        public static readonly RoutedUICommand DumpToFile = new RoutedUICommand(
            "Export save bytes",
            nameof(DumpToFile),
            typeof(EditorCommands));

        public static readonly RoutedUICommand ViewSaveHex = new RoutedUICommand(
            "View save hex",
            nameof(ViewSaveHex),
            typeof(EditorCommands));

        public static readonly RoutedUICommand OpenIssues = new RoutedUICommand(
            "Report an issue",
            nameof(OpenIssues),
            typeof(EditorCommands));

        public static readonly RoutedUICommand About = new RoutedUICommand(
            "About Siniskalc",
            nameof(About),
            typeof(EditorCommands));

        public static readonly RoutedUICommand LoadFromDump = new RoutedUICommand(
            "Load save from bytes",
            nameof(LoadFromDump),
            typeof(EditorCommands));
    }
}
