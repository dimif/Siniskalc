namespace Siniskalc.Core.Runtime
{
    /// <summary>
    /// Opens a web address in the browser of the user.
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
    }
}
