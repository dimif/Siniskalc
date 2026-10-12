namespace Siniskalc.Tests
{
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// File dialogs that show nothing: the test says which file the user would pick (null is a cancelled dialog) and what it was asked.
    /// </summary>
    internal sealed class ScriptedFileDialogs : IFileDialogs
    {
        public string? PathToOpen { get; set; }

        public string? PathToSave { get; set; }

        public string? SuggestedName { get; private set; }

        public int Asked { get; private set; }

        public string? PickFileToOpen(string filter)
        {
            this.Asked++;
            return this.PathToOpen;
        }

        public string? PickFileToSave(string filter, string? fileName = null, string? defaultExtension = null)
        {
            this.Asked++;
            this.SuggestedName = fileName;
            return this.PathToSave;
        }
    }
}
