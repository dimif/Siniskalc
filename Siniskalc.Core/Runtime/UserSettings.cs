namespace Siniskalc.Core.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// User preferences persisted as JSON under the user's AppData folder.
    /// </summary>
    public sealed class UserSettings
    {
        private static readonly ILogger<UserSettings> Logger = LogFactory.For<UserSettings>();

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
        /// Gets or sets the folder log files are written to. <see langword="null"/> (the default) means the folder of the platform (<see cref="LogPaths"/>).
        /// Read at start-up, so a change applies the next time the editor starts.
        /// </summary>
        public string? LogFolder { get; set; }

        /// <summary>
        /// Gets or sets the least important level that is written to the log, or <see langword="null"/> (the default) to leave it to the
        /// minimum level of <c>serilog.json</c>. Read at start-up.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LogLevel? LogLevel { get; set; }

        /// <summary>
        /// Returns where the settings of the application called <paramref name="applicationName"/> are kept for the current user.
        /// </summary>
        /// <param name="applicationName">The name of the application, used as the folder under the roaming application data folder.</param>
        /// <returns>
        /// The path.
        /// </returns>
        public static string PathFor(string applicationName) => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            applicationName,
            "settings.json");

        /// <summary>
        /// Reads the settings from <paramref name="path"/>. A missing file gives the defaults.
        /// </summary>
        /// <param name="path">The path of the settings file.</param>
        /// <returns>The settings in the file, or the defaults when there is no file or it holds nothing.</returns>
        /// <exception cref="SettingsException">The file exists but cannot be read or understood.</exception>
        public static UserSettings Load(string path)
        {
            if (!File.Exists(path))
            {
                Logger.LogDebug("No settings file at {Path}, using the defaults", path);
                return new UserSettings();
            }

            try
            {
                var loaded = JsonSerializer.Deserialize<UserSettings>(File.ReadAllText(path));
                Logger.LogDebug("Read the settings from {Path}", path);
                return loaded ?? new UserSettings();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                throw new SettingsException(path, $"The settings file {path} cannot be read: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Writes the settings to <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path of the settings file.</param>
        /// <exception cref="SettingsException">The file cannot be written.</exception>
        public void Save(string path)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
                File.WriteAllText(path, JsonSerializer.Serialize(this, JsonOptions));
                Logger.LogDebug("Wrote the settings to {Path}", path);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new SettingsException(path, $"Failed to save the settings to {path}: {ex.Message}", ex);
            }
        }
    }
}
