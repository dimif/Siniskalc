namespace Siniskalc.Core.Runtime
{
    /// <summary>
    /// Opens a web address in the browser of the user, or a folder in the file manager.
    /// </summary>
    public interface IUrlLauncher
    {
        /// <summary>
        /// Safely opens a <paramref name="url"/>.
        /// </summary>
        /// <param name="url">
        /// The URL. Non URLs will be skipped.
        /// </param>
        /// <returns>
        /// Whether the browser was started.
        /// </returns>
        bool TryOpen(string url);

        /// <summary>
        /// Opens an existing <paramref name="folder"/> in the file manager of the user.
        /// </summary>
        /// <param name="folder">
        /// The path of the folder.
        /// </param>
        /// <returns>
        /// Whether the file manager was started; false for a path that is no folder.
        /// </returns>
        bool TryOpenFolder(string folder);
    }
}
