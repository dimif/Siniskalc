namespace Siniskalc.Core.Runtime
{
    using System;
    using System.IO;

    /// <summary>
    /// The one place that holds the <see cref="UserSettings"/> of the user and writes them to their file. Everything that keeps a
    /// preference shares one store, so that saving one preference never overwrites another.
    /// </summary>
    public sealed class SettingsStore : ISettingsStore
    {
        private readonly string path;
        private readonly UserSettings settings;
        private readonly bool fromLegacy;

        /// <param name="path">Where the settings are kept.</param>
        /// <param name="legacyPath">
        /// Where an earlier version kept them. Read once when there is no file at <paramref name="path"/> yet, so a rename of
        /// the editor does not lose the settings.
        /// </param>
        public SettingsStore(string path, string? legacyPath = null)
        {
            this.path = path;
            this.fromLegacy = !File.Exists(path) && legacyPath is not null && File.Exists(legacyPath);
            this.settings = UserSettings.Load(this.fromLegacy ? legacyPath! : path);
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

        /// <summary>
        /// Makes sure the settings file is there: when the user has none yet, writes the default settings file the application ships (or the
        /// settings an earlier version kept, when they were read from there), so the user can see and edit it.
        /// </summary>
        /// <exception cref="SettingsException">The file cannot be written.</exception>
        public void EnsureCreated()
        {
            if (File.Exists(this.path))
            {
                return;
            }

            if (this.fromLegacy)
            {
                this.Save();
                return;
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(this.path) ?? string.Empty);
                File.WriteAllText(this.path, DefaultSettings.Json);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new SettingsException(this.path, $"Failed to save the settings to {this.path}: {ex.Message}", ex);
            }
        }
    }
}
