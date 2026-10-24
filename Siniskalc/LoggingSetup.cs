namespace Siniskalc
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Events;
    using Serilog.Extensions.Logging;
    using Serilog.Formatting;
    using Serilog.Templates;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// Starts Serilog with what the user set (<see cref="UserSettings.LogFolder"/>, <see cref="UserSettings.LogLevel"/>) and hands it out as
    /// the <see cref="ILoggerFactory"/> the rest of the editor knows. This and <see cref="AppServices"/> are the only code that names Serilog.
    /// The log is a plain text file, one event per line, kept for a week. The file is shared, so a second copy of the editor can log to it too.
    /// </summary>
    internal static class LoggingSetup
    {
        private const string FileName = "siniskalc-.log";
        private const int DaysKept = 7;
        // The subject (a hero or a castle, see LogScopeExtensions) is shown after the category when the line has one.
        private const string Template = "{@t:yyyy-MM-dd HH:mm:ss.fff zzz} [{@l:u3}] {SourceContext}{#if Subject is not null} ({Subject}){#end}: {@m}\n{#if @x is not null}{@x}\n{#end}";

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
                .WriteTo.Debug(Formatter())
                .WriteTo.Console(Formatter());

            var folder = LogPaths.Resolve(settings, applicationName);
            if (LogPaths.TryUse(folder, out var problem))
            {
                configuration.WriteTo.File(
                    Formatter(),
                    Path.Combine(folder, FileName),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: DaysKept,
                    shared: true);
            }
            else if (chosen)
            {
                throw new LoggingException($"The log folder {folder} cannot be used: {problem!.Message}", problem);
            }

            return new SerilogLoggerFactory(configuration.CreateLogger(), dispose: true);
        }

        private static ITextFormatter Formatter() => new ExpressionTemplate(Template);

        private static LogEventLevel LevelOf(LogLevel level) => level switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => Debugger.IsAttached ? LogEventLevel.Debug : LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            _ => LogEventLevel.Fatal,
        };
    }
}
