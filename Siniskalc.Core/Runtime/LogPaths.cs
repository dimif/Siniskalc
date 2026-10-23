namespace Siniskalc.Core.Runtime
{
    using System;
    using System.IO;

    /// <summary>
    /// Where the log files of the editor are kept. Built from the folders .NET reports for the platform, so it holds on Windows,
    /// Linux and macOS alike.
    /// </summary>
    public static class LogPaths
    {
        private const string LogsFolderName = "logs";

        /// <summary>
        /// Returns the folder logs go to when the user did not choose one for the application called <paramref name="applicationName"/>.
        /// </summary>
        public static string DefaultFolderFor(string applicationName) => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            applicationName,
            LogsFolderName);

        /// <summary>
        /// Returns what to call a savegame in a log: the name of its file, else its <paramref name="name"/>. Never the full path,
        /// which holds the name of the user (see docs/LOGGING.md).
        /// </summary>
        public static string Label(string? file, string? name) =>
            file is not null ? Path.GetFileName(file) : name ?? "(unnamed)";

        /// <summary>
        /// Returns the folder logs go to: the one the user set, or the default for <paramref name="applicationName"/>.
        /// </summary>
        public static string Resolve(UserSettings settings, string applicationName) =>
            string.IsNullOrWhiteSpace(settings.LogFolder) ? DefaultFolderFor(applicationName) : settings.LogFolder;
    }
}
