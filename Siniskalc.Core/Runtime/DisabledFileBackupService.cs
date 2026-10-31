namespace Siniskalc.Core.Runtime
{
    using System.IO;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Makes no backup: used while the user has backups turned off.
    /// </summary>
    public sealed class DisabledFileBackupService : IFileBackupService
    {
        private static readonly ILogger<DisabledFileBackupService> Logger = LogFactory.For<DisabledFileBackupService>();

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="filePath">The path of the file that is about to be replaced; not looked at.</param>
        /// <returns>Always <see langword="null"/>, as no copy is made.</returns>
        public string? Backup(string filePath)
        {
            Logger.LogTrace("Backups are off, {File} is not copied", Path.GetFileName(filePath));
            return null;
        }
    }
}
