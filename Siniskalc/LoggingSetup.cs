namespace Siniskalc
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Debugging;
    using Serilog.Events;
    using Serilog.Extensions.Logging;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// Starts Serilog from the <c>serilog.json</c> of the user (the sinks, the template, the path, the rolling interval, the days kept: the file
    /// the application ships as <c>Siniskalc/serilog.json</c> is copied to the folder of the settings the first time) and hands it out as the
    /// <see cref="ILoggerFactory"/> the rest of the editor knows. The settings of the user replace two things in it: the minimum level
    /// (<see cref="UserSettings.LogLevel"/>) and the log folder (<see cref="UserSettings.LogFolder"/>, which fills <c>{LogFolder}</c> in the path).
    /// This and <see cref="AppCompositionRoot"/> are the only code that names Serilog.
    /// </summary>
    internal static class LoggingSetup
    {
        private const string ResourceName = "Siniskalc.serilog.json";
        private const string FolderToken = "{LogFolder}";
        private const string MinimumLevelKey = "Serilog:MinimumLevel:Default";

        /// <summary>
        /// Gets the text of the Serilog configuration the application ships.
        /// </summary>
        /// <exception cref="InvalidOperationException">The configuration is not part of the application.</exception>
        public static string DefaultConfiguration
        {
            get
            {
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
                    ?? throw new InvalidOperationException($"The Serilog configuration {ResourceName} is not part of the application.");
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Starts logging. The folder is the one the user chose, or the one of the platform when they did not.
        /// </summary>
        /// <param name="settings">What the user set.</param>
        /// <param name="applicationName">The name of the application, for the default folder.</param>
        /// <param name="configPath">Where the Serilog configuration of the user is kept; it is written there when it is missing.</param>
        /// <returns>The factory of the loggers; disposing it flushes the log file.</returns>
        /// <exception cref="LoggingException">The folder the user chose cannot be used, or the Serilog configuration cannot be read or built.</exception>
        public static ILoggerFactory Create(UserSettings settings, string applicationName, string configPath)
        {
            var chosen = !string.IsNullOrWhiteSpace(settings.LogFolder);
            var folder = LogPaths.Resolve(settings, applicationName);
            var usable = LogPaths.TryUse(folder, out var problem);
            if (!usable && chosen)
            {
                throw new LoggingException($"The log folder {folder} cannot be used: {problem!.Message}", problem);
            }

            var text = ReadConfiguration(configPath).Replace(FolderToken, JsonEncodedText.Encode(folder.Replace('\\', '/')).ToString(), StringComparison.Ordinal);
            if (!usable)
            {
                // The default folder is not a mistake of the user: the editor goes on without the file sink.
                text = WithoutFileSink(text, configPath);
            }

            // Serilog does not throw for a sink or an argument it cannot find: it says so to its self log, which is listened to while the logger is built.
            var complaints = new List<string>();
            SelfLog.Enable(complaints.Add);
            try
            {
                var configuration = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(text))).Build();
                var logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .MinimumLevel.Is(MinimumLevelOf(settings, configuration))
                    .CreateLogger();
                if (complaints.Count > 0)
                {
                    logger.Dispose();
                    throw new InvalidOperationException(string.Join(" ", complaints));
                }

                return new SerilogLoggerFactory(logger, dispose: true);
            }
            catch (Exception ex) when (ex is not LoggingException)
            {
                throw new LoggingException($"The Serilog configuration {configPath} cannot be used: {ex.Message}", ex, configPath);
            }
            finally
            {
                SelfLog.Disable();
            }
        }

        // The user's own file when there is one; otherwise the one the application ships, which is also written for the user to edit
        // (when that cannot be done the shipped text is used as it is: the default place is not a mistake of the user).
        private static string ReadConfiguration(string configPath)
        {
            try
            {
                if (File.Exists(configPath))
                {
                    return File.ReadAllText(configPath);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new LoggingException($"The Serilog configuration {configPath} cannot be read: {ex.Message}", ex, configPath);
            }

            var shipped = DefaultConfiguration;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(configPath) ?? string.Empty);
                File.WriteAllText(configPath, shipped);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Not written: the shipped text is used as it is.
            }

            return shipped;
        }

        private static string WithoutFileSink(string text, string configPath)
        {
            try
            {
                var root = JsonNode.Parse(text, documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
                if (root?["Serilog"]?["WriteTo"] is JsonArray sinks)
                {
                    foreach (var sink in sinks.Where(s => s?["Name"]?.GetValue<string>() == "File").ToList())
                    {
                        sinks.Remove(sink);
                    }
                }

                return root?.ToJsonString() ?? text;
            }
            catch (JsonException ex)
            {
                throw new LoggingException($"The Serilog configuration {configPath} cannot be read: {ex.Message}", ex, configPath);
            }
        }

        // The settings of the user win over the file; a debugger attached asks for Debug when the level is the default one.
        private static LogEventLevel MinimumLevelOf(UserSettings settings, IConfiguration configuration)
        {
            var level = settings.LogLevel is { } chosen
                ? LevelOf(chosen)
                : Enum.TryParse<LogEventLevel>(configuration[MinimumLevelKey], ignoreCase: true, out var fromFile) ? fromFile : LogEventLevel.Information;
            return level == LogEventLevel.Information && Debugger.IsAttached ? LogEventLevel.Debug : level;
        }

        private static LogEventLevel LevelOf(LogLevel level) => level switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            _ => LogEventLevel.Fatal,
        };
    }
}
