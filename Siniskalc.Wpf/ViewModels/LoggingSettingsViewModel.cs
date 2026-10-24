namespace Siniskalc.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Input;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// The page of the settings where the user chooses how much is logged and where the log files go. The choice is kept in the
    /// settings of the user and read when the editor starts, so it applies the next time.
    /// </summary>
    public sealed class LoggingSettingsViewModel : AbstractViewModelBase
    {
        private readonly SettingsStore store;
        private readonly IFileDialogs dialogs;
        private LogLevel level;
        private string folder;
        private string error = string.Empty;

        /// <param name="store">Where the preferences of the user are kept.</param>
        /// <param name="defaultFolder">The folder the logs go to when the user chooses none.</param>
        /// <param name="dialogs">Asks the user for a folder.</param>
        public LoggingSettingsViewModel(SettingsStore store, string defaultFolder, IFileDialogs dialogs)
        {
            this.store = store;
            this.DefaultFolder = defaultFolder;
            this.dialogs = dialogs;
            this.level = store.Settings.LogLevel;
            this.folder = store.Settings.LogFolder ?? string.Empty;
            this.BrowseCommand = new DelegateCommand(_ => this.Browse());
            this.UseDefaultCommand = new DelegateCommand(_ => this.Folder = string.Empty);
        }

        /// <summary>
        /// Gets the levels the user can choose, from the one that writes the most to the one that writes the least.
        /// </summary>
        public IReadOnlyList<LogLevel> Levels { get; } =
        [
            LogLevel.Trace,
            LogLevel.Debug,
            LogLevel.Information,
            LogLevel.Warning,
            LogLevel.Error,
            LogLevel.Critical,
        ];

        public LogLevel Level
        {
            get => this.level;
            set
            {
                this.level = value;
                this.RaisePropertyChanged(nameof(this.Level));
            }
        }

        /// <summary>
        /// Gets or sets the folder the user chose; empty for the default one.
        /// </summary>
        public string Folder
        {
            get => this.folder;
            set
            {
                this.folder = value;
                this.RaisePropertyChanged(nameof(this.Folder));
            }
        }

        /// <summary>
        /// Gets the folder the logs go to when none is chosen.
        /// </summary>
        public string DefaultFolder { get; }

        /// <summary>
        /// Gets why the choice was not kept, or an empty text.
        /// </summary>
        public string Error
        {
            get => this.error;
            private set
            {
                this.error = value;
                this.RaisePropertyChanged(nameof(this.Error));
                this.RaisePropertyChanged(nameof(this.HasError));
            }
        }

        public bool HasError => this.error.Length > 0;

        public ICommand BrowseCommand { get; }

        public ICommand UseDefaultCommand { get; }

        /// <summary>
        /// Keeps the choice in the settings, unless the folder cannot be used or the settings cannot be written; <see cref="Error"/> says why.
        /// </summary>
        /// <returns>
        /// Whether the choice was kept.
        /// </returns>
        public bool TrySave()
        {
            var chosen = this.folder.Trim();
            if (chosen.Length > 0)
            {
                if (!Path.IsPathFullyQualified(chosen))
                {
                    this.Error = $"The folder {chosen} must be a full path, such as {this.DefaultFolder}.";
                    return false;
                }

                if (!LogPaths.TryUse(chosen, out var problem))
                {
                    this.Error = $"The folder {chosen} cannot be used: {problem!.Message}";
                    return false;
                }
            }

            this.store.Settings.LogLevel = this.level;
            this.store.Settings.LogFolder = chosen.Length == 0 ? null : chosen;
            try
            {
                this.store.Save();
            }
            catch (SettingsException ex)
            {
                this.Error = ex.Message;
                return false;
            }

            this.Error = string.Empty;
            return true;
        }

        private void Browse()
        {
            var picked = this.dialogs.PickFolder(this.folder.Length > 0 ? this.folder : this.DefaultFolder, "Choose the folder for the log files");
            if (picked != null)
            {
                this.Folder = picked;
            }
        }
    }
}
