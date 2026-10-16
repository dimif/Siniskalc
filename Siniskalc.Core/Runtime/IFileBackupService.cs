namespace Siniskalc.Core.Runtime
{
    using System.IO;

    /// <summary>
    /// Creates copies of a file that is about to be replaced.
    /// </summary>
    public interface IFileBackupService
    {
        /// <summary>
        /// Copies <paramref name="filePath"/> aside. Never replaces an earlier copy.
        /// </summary>
        /// <param name="filePath">
        /// The path of the file to back up.
        /// </param>
        /// <returns>
        /// The path of the copy, or <c>null</c> when this service makes none.
        /// </returns>
        /// <exception cref="IOException">
        /// The copy could not be made (including <see cref="FileNotFoundException"/> when the file does not exist).
        /// </exception>
        string? Backup(string filePath);
    }
}
