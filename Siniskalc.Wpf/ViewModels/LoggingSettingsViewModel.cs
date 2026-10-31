namespace Siniskalc.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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
        private readonly ISettingsStore store;
        private readonly IFileDialogs dialogs;
        private LogLevelChoice selectedLevel;
        private string folder;
        private string error = string.Empty;

        /// <param name="store">Where the preferences of the user are kept.</param>
        /// <param name="defaultFolder">The folder the logs go to when the user chooses none.</param>
        /// <param name="dialogs">Asks the user for a folder.</param>
        public LoggingSettingsViewModel(ISettingsStore store, string defaultFolder, IFileDialogs dialogs)
        {
            this.store = store;
            this.DefaultFolder = defaultFolder;
            this.dialogs = dialogs;
            this.selectedLevel = this.Levels.Single(choice => choice.Level == store.Settings.LogLevel);
            this.folder = store.Settings.LogFolder ?? string.Empty;
            this.BrowseCommand = new DelegateCommand(_ => this.Browse());
            this.UseDefaultCommand = new DelegateCommand(_ => this.Folder = string.Empty);
        }

        /// <summary>
        /// Gets the levels the user can choose: first "the level of serilog.json", then from the one that writes the most to the one that writes the least.
        /// </summary>
        public IReadOnlyList<LogLevelChoice> Levels { get; } =
        [
            new(null, "Default (serilog.json)"),
            new(LogLevel.Trace, "Trace"),
            new(LogLevel.Debug, "Debug"),
            new(LogLevel.Information, "Information"),
            new(LogLevel.Warning, "Warning"),
            new(LogLevel.Error, "Error"),
            new(LogLevel.Critical, "Critical"),
        ];

        /// <summary>
        /// Gets or sets the entry of the list that is chosen.
        /// </summary>
        public LogLevelChoice SelectedLevel
        {
            get => this.selectedLevel;
            set
            {
                this.selectedLevel = value;
                this.RaisePropertyChanged(nameof(this.SelectedLevel));
                this.RaisePropertyChanged(nameof(this.Level));
            }
        }

        /// <summary>
        /// Gets or sets the level that is chosen, or <c>null</c> for the minimum level of serilog.json.
        /// </summary>
        public LogLevel? Level
        {
            get => this.selectedLevel.Level;
            set => this.SelectedLevel = this.Levels.Single(choice => choice.Level == value);
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

            this.store.Settings.LogLevel = this.selectedLevel.Level;
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
