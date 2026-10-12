namespace Siniskalc.Wpf.Views
{
    /// <summary>
    /// Asks the user to pick a file. A test gives the application one that answers without showing anything.
    /// </summary>
    public interface IFileDialogs
    {
        /// <summary>
        /// Asks for an existing file to open. Returns its path, or <c>null</c> when the user cancels.
        /// </summary>
        /// <param name="filter">The file types to offer, as a Windows file dialog filter.</param>
        string? PickFileToOpen(string filter);

        /// <summary>
        /// Asks where to save a file. Returns the path, or <c>null</c> when the user cancels.
        /// </summary>
        /// <param name="filter">The file types to offer, as a Windows file dialog filter.</param>
        /// <param name="fileName">The name to suggest.</param>
        /// <param name="defaultExtension">The extension to add when the user types none, with its dot.</param>
        string? PickFileToSave(string filter, string? fileName = null, string? defaultExtension = null);
    }
}
