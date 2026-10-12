namespace Siniskalc.Core.Runtime
{
    /// <summary>
    /// Keeps a copy of a file that is about to be replaced.
    /// </summary>
    public interface ISavefileBackup
    {
        /// <summary>
        /// Copies <paramref name="filePath"/> aside when backups are on and the file exists. Never replaces an earlier copy.
        /// </summary>
        /// <param name="filePath">
        /// The path of the file to back up.
        /// </param>
        /// <returns>
        /// The path of the copy, or <c>null</c> when none was made.
        /// </returns>
        string? Backup(string filePath);
    }
}
