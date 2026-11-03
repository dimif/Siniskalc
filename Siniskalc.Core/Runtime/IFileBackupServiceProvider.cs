namespace Siniskalc.Core.Runtime
{
    /// <summary>
    /// Serves the backup service that the current settings ask for.
    /// </summary>
    public interface IFileBackupServiceProvider
    {
        /// <summary>
        /// Gets the backup service for the settings as they are now.
        /// </summary>
        IFileBackupService GetService();
    }
}
