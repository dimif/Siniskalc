namespace Siniskalc.Core.Runtime
{
    /// <summary>The one place that holds the <see cref="UserSettings"/> of the user and writes them out.</summary>
    public interface ISettingsStore
    {
        /// <summary>Gets the settings. Call <see cref="Save"/> after changing them.</summary>
        UserSettings Settings { get; }

        /// <summary>Gets or sets whether a copy of a savefile is kept before it is replaced. Setting it saves.</summary>
        bool BackupOnSave { get; set; }

        /// <summary>Gets or sets whether the tools for the raw bytes are shown. Setting it saves.</summary>
        bool AdvancedMode { get; set; }

        /// <summary>Writes the settings to their file.</summary>
        /// <exception cref="SettingsException">The file cannot be written.</exception>
        void Save();
    }
}
