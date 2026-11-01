namespace Siniskalc.Core.Runtime
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Serves the numbered backup service while <see cref="SettingsStore.BackupOnSave"/> is on, and the disabled one while it is off.
    /// The setting is read at each call, so a change takes effect on the next save.
    /// </summary>
    public sealed class FileBackupServiceProvider : IFileBackupServiceProvider
    {
        private static readonly ILogger<FileBackupServiceProvider> Logger = LogFactory.For<FileBackupServiceProvider>();

        private readonly ISettingsStore settings;
        private readonly IFileBackupService enabled;
        private readonly IFileBackupService disabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileBackupServiceProvider"/> class.
        /// </summary>
        /// <param name="settings">The settings that say whether backups are on.</param>
        /// <param name="enabled">The service served while backups are on.</param>
        /// <param name="disabled">The service served while backups are off.</param>
        public FileBackupServiceProvider(ISettingsStore settings, IFileBackupService enabled, IFileBackupService disabled)
        {
            this.settings = settings;
            this.enabled = enabled;
            this.disabled = disabled;
        }

        /// <inheritdoc/>
        public IFileBackupService GetService()
        {
            var backupOnSave = this.settings.BackupOnSave;
            Logger.LogTrace("Backup on save is {BackupOnSave}", backupOnSave);
            return backupOnSave ? this.enabled : this.disabled;
        }
    }
}
