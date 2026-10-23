namespace Siniskalc
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Events;
    using Serilog.Extensions.Logging;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// Starts Serilog with what the user set (<see cref="UserSettings.LogFolder"/>, <see cref="UserSettings.LogLevel"/>) and hands it out as
    /// the <see cref="ILoggerFactory"/> the rest of the editor knows. This and <see cref="AppServices"/> are the only code that names Serilog.
    /// The log is a plain text file, one event per line, kept for a week.
    /// </summary>
    internal static class LoggingSetup
    {
        private const string FileName = "siniskalc-.log";
        private const int DaysKept = 7;
        private const string Template = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";

        /// <summary>
        /// Starts logging. The folder is the one the user chose, or the one of the platform when they did not.
        /// </summary>
        /// <exception cref="LoggingException">The folder the user chose cannot be used.</exception>
        public static ILoggerFactory Create(UserSettings settings, string applicationName)
        {
            var chosen = !string.IsNullOrWhiteSpace(settings.LogFolder);
            var configuration = new LoggerConfiguration()
                .MinimumLevel.Is(LevelOf(settings.LogLevel))
                .Enrich.FromLogContext()
                .WriteTo.Debug(outputTemplate: Template)
                .WriteTo.Console(outputTemplate: Template);

            var folder = LogPaths.Resolve(settings, applicationName);
            if (TryUse(folder, out var problem))
            {
                configuration.WriteTo.File(
                    Path.Combine(folder, FileName),
                    outputTemplate: Template,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: DaysKept);
            }
            else if (chosen)
            {
                throw new LoggingException($"The log folder {folder} cannot be used: {problem!.Message}", problem);
            }

            return new SerilogLoggerFactory(configuration.CreateLogger(), dispose: true);
        }

        private static LogEventLevel LevelOf(LogLevel level) => level switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => Debugger.IsAttached ? LogEventLevel.Debug : LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            _ => LogEventLevel.Fatal,
        };

        // The file sink opens its file when the first event arrives and keeps failures to itself, so whether the folder can be
        // written is found out here, where it can still be reported.
        private static bool TryUse(string folder, out Exception? problem)
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
    }
}
