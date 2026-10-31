namespace Siniskalc.Wpf.ViewModels
{
    /// <summary>
    /// All the settings of the user in one window: the choices about saving and about the advanced tools (which apply at once when the window
    /// is accepted) and the logging (which applies the next time the editor starts). Nothing changes until <see cref="TrySave"/> keeps them,
    /// so cancelling the window leaves everything as it was.
    /// </summary>
    public sealed class SettingsViewModel : AbstractViewModelBase
    {
        private readonly ShellViewModel shell;
        private bool backupOnSave;
        private bool advancedMode;

        /// <param name="shell">The shell, which holds the choices that apply at once.</param>
        /// <param name="logging">The logging choices, or <c>null</c> when the application keeps no log settings.</param>
        public SettingsViewModel(ShellViewModel shell, LoggingSettingsViewModel? logging)
        {
            this.shell = shell;
            this.Logging = logging;
            this.backupOnSave = shell.BackupOnSave;
            this.advancedMode = shell.IsAdvancedMode;
        }

        /// <summary>
        /// Gets or sets a value indicating whether a copy of a savegame is kept before it is saved over.
        /// </summary>
        public bool BackupOnSave
        {
            get => this.backupOnSave;
            set
            {
                this.backupOnSave = value;
                this.RaisePropertyChanged(nameof(this.BackupOnSave));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the tools for the raw bytes (the Advanced menu and the hex tabs) are shown.
        /// </summary>
        public bool AdvancedMode
        {
            get => this.advancedMode;
            set
            {
                this.advancedMode = value;
                this.RaisePropertyChanged(nameof(this.AdvancedMode));
            }
        }

        /// <summary>
        /// Gets the logging choices, or <c>null</c> when the application keeps none.
        /// </summary>
        public LoggingSettingsViewModel? Logging { get; }

        /// <summary>
        /// Gets a value indicating whether the window has a logging section.
        /// </summary>
        public bool HasLogging => this.Logging != null;

        /// <summary>
        /// Keeps the choices. When the logging choices cannot be kept their error says why, and nothing is changed.
        /// </summary>
        /// <returns>
        /// Whether the choices were kept.
        /// </returns>
        public bool TrySave()
        {
            if (this.Logging != null && !this.Logging.TrySave())
            {
                return false;
            }

            this.shell.BackupOnSave = this.backupOnSave;
            this.shell.IsAdvancedMode = this.advancedMode;
            return true;
        }
    }
}
