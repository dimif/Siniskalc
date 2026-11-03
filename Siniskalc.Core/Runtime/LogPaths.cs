namespace Siniskalc.Core.Runtime
{
    using System;
    using System.IO;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Where the log files of the editor are kept. Built from the folders .NET reports for the platform, so it holds on Windows,
    /// Linux and macOS alike.
    /// </summary>
    public static class LogPaths
    {
        private const string LogsFolderName = "logs";
        private const string SerilogConfigFileName = "serilog.json";

        private static readonly ILogger Logger = LogFactory.For(typeof(LogPaths));

        /// <summary>
        /// Returns the folder logs go to when the user did not choose one for the application called <paramref name="applicationName"/>.
        /// </summary>
        /// <param name="applicationName">The name of the application, used as the folder under the local application data folder.</param>
        /// <returns>The path of the folder.</returns>
        public static string DefaultFolderFor(string applicationName) => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            applicationName,
            LogsFolderName);

        /// <summary>
        /// Returns what to call a savegame in a log: the name of its file, else its <paramref name="name"/>. Never the full path,
        /// which holds the name of the user (see docs/LOGGING.md).
        /// </summary>
        /// <param name="file">The path of the savegame, or <see langword="null"/> when it has none.</param>
        /// <param name="name">What the savegame is called when it has no file.</param>
        /// <returns>The file name of <paramref name="file"/>; else <paramref name="name"/>; else "(unnamed)".</returns>
        public static string Label(string? file, string? name) =>
            file is not null ? Path.GetFileName(file) : name ?? "(unnamed)";

        /// <summary>
        /// Returns where the Serilog configuration of the user is kept: <c>serilog.json</c> in the folder of the settings file.
        /// </summary>
        /// <param name="settingsPath">The path of the settings file.</param>
        /// <returns>The path of <c>serilog.json</c> next to the settings file.</returns>
        public static string SerilogConfigPathFor(string settingsPath) => Path.Combine(
            Path.GetDirectoryName(settingsPath) ?? string.Empty,
            SerilogConfigFileName);

        /// <summary>
        /// Tells whether log files can be written to <paramref name="folder"/>, creating it when it is not there. The file sink opens its
        /// file when the first event arrives and keeps failures to itself, so this is how a folder is found out to be unusable while it
        /// can still be reported.
        /// </summary>
        /// <param name="folder">The folder to try.</param>
        /// <param name="problem">Why the folder cannot be used, or <c>null</c>.</param>
        /// <returns>
        /// Whether a file can be written to the folder.
        /// </returns>
        public static bool TryUse(string folder, out Exception? problem)
        {
            try
            {
                Directory.CreateDirectory(folder);
                var probe = Path.Combine(folder, Path.GetRandomFileName());
                File.WriteAllBytes(probe, []);
                File.Delete(probe);
                problem = null;
                return true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException)
            {
                Logger.LogDebug(ex, "The log folder {Folder} cannot be used", folder);
                problem = ex;
                return false;
            }
        }

        /// <summary>
        /// Returns the folder logs go to: the one the user set, or the default for <paramref name="applicationName"/>.
        /// </summary>
        /// <param name="settings">The settings of the user, which may name a log folder.</param>
        /// <param name="applicationName">The name of the application, for the default folder.</param>
        /// <returns>The path of the folder.</returns>
        public static string Resolve(UserSettings settings, string applicationName) =>
            string.IsNullOrWhiteSpace(settings.LogFolder) ? DefaultFolderFor(applicationName) : settings.LogFolder;
    }
}
