namespace Siniskalc.Core.Runtime
{
    /// <summary>
    /// Makes no backup: used while the user has backups turned off.
    /// </summary>
    public sealed class DisabledFileBackupService : IFileBackupService
    {
        /// <inheritdoc/>
        public string? Backup(string filePath) => null;
    }
}
