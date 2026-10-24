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
                problem = ex;
                return false;
            }
        }

        /// <summary>
        /// Returns the folder logs go to: the one the user set, or the default for <paramref name="applicationName"/>.
        /// </summary>
        public static string Resolve(UserSettings settings, string applicationName) =>
            string.IsNullOrWhiteSpace(settings.LogFolder) ? DefaultFolderFor(applicationName) : settings.LogFolder;
    }
}
