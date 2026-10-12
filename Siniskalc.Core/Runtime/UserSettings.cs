namespace Siniskalc.Core.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;

    /// <summary>
    /// User preferences persisted as JSON under the user's AppData folder.
    /// </summary>
    public sealed class UserSettings
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        /// <summary>
        /// Gets or sets a list of recently loaded files.
        /// </summary>
        public List<string> RecentFiles { get; set; } = [];

        /// <summary>
        /// Gets or sets a value indicating whether a copy of a savefile is kept before it is replaced. On by default.
        /// </summary>
        public bool BackupOnSave { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the tools for working with the raw bytes (the Advanced menu, the hex tabs) are shown. Off by default.
        /// </summary>
        public bool AdvancedMode { get; set; }

        /// <summary>
        /// Returns where the settings of the application called <paramref name="applicationName"/> are kept for the current user.
        /// </summary>
        /// <returns>
        /// The path.
        /// </returns>
        public static string PathFor(string applicationName) => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            applicationName,
            "settings.json");

        public static UserSettings Load(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    return JsonSerializer.Deserialize<UserSettings>(File.ReadAllText(path)) ?? new UserSettings();
                }
            }
            catch (Exception)
            {
                // TODO:LOG warning: the settings file {path} could not be read ({exception type and message}); defaults are used and the file is replaced by the next save. Consider keeping a copy of the unreadable file.
                // A missing or corrupt settings file falls back to defaults.
            }

            return new UserSettings();
        }

        public void Save(string path)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
                File.WriteAllText(path, JsonSerializer.Serialize(this, JsonOptions));
            }
            catch (Exception)
            {
                // TODO:LOG warning: the settings could not be written to {path} ({exception type and message}); they are kept for this session only.
                // Failing to persist settings should never interrupt editing.
            }
        }
    }
}
