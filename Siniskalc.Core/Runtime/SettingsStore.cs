namespace Siniskalc.Core.Runtime
{
    using System.IO;

    /// <summary>
    /// The one place that holds the <see cref="UserSettings"/> of the user and writes them to their file. Everything that keeps a
    /// preference shares one store, so that saving one preference never overwrites another.
    /// </summary>
    public sealed class SettingsStore
    {
        private readonly string path;
        private readonly UserSettings settings;

        /// <param name="path">Where the settings are kept.</param>
        /// <param name="legacyPath">
        /// Where an earlier version kept them. Read once when there is no file at <paramref name="path"/> yet, so a rename of
        /// the editor does not lose the settings.
        /// </param>
        public SettingsStore(string path, string? legacyPath = null)
        {
            this.path = path;
            var loadFrom = !File.Exists(path) && legacyPath is not null && File.Exists(legacyPath) ? legacyPath : path;
            this.settings = UserSettings.Load(loadFrom);
        }

        /// <summary>
        /// Gets the settings. Call <see cref="Save"/> after changing them.
        /// </summary>
        public UserSettings Settings => this.settings;

        /// <summary>
        /// Gets or sets whether a copy of a savefile is kept before it is replaced. On unless the user turned it off.
        /// </summary>
        public bool BackupOnSave
        {
            get => this.settings.BackupOnSave;
            set
            {
                this.settings.BackupOnSave = value;
                this.Save();
            }
        }

        /// <summary>
        /// Gets or sets whether the tools for working with the raw bytes (the Advanced menu, the hex tabs) are shown.
        /// </summary>
        public bool AdvancedMode
        {
            get => this.settings.AdvancedMode;
            set
            {
                this.settings.AdvancedMode = value;
                this.Save();
            }
        }

        public void Save() => this.settings.Save(this.path);
    }
}
