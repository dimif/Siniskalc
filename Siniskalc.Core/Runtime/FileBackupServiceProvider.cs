namespace Siniskalc.Core.Runtime
{
    /// <summary>
    /// Serves the numbered backup service while <see cref="SettingsStore.BackupOnSave"/> is on, and the disabled one while it is off.
    /// The setting is read at each call, so a change takes effect on the next save.
    /// </summary>
    public sealed class FileBackupServiceProvider : IFileBackupServiceProvider
    {
        private readonly SettingsStore settings;
        private readonly IFileBackupService enabled;
        private readonly IFileBackupService disabled;

        public FileBackupServiceProvider(SettingsStore settings, IFileBackupService enabled, IFileBackupService disabled)
        {
            this.settings = settings;
            this.enabled = enabled;
            this.disabled = disabled;
        }

        /// <inheritdoc/>
        public IFileBackupService GetService() => this.settings.BackupOnSave ? this.enabled : this.disabled;
    }
}
