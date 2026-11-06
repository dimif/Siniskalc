namespace Siniskalc
{
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Runtime;
    using Siniskalc.Wpf.ViewModels;
    using Siniskalc.Wpf.Views;
    using H2Runtime = Siniskalc.Homm2.Runtime;

    /// <summary>
    /// The composition root: builds each component once, from what it is given, and hands them to <see cref="AppServices"/>. What the games
    /// share is built here; what only a game uses is built by the factory of that game. Nothing else creates a component that another class
    /// depends on, and <see cref="AppServices"/> creates none.
    /// </summary>
    public static class AppCompositionRoot
    {
        private static readonly ILogger Logger = LogFactory.For(typeof(AppCompositionRoot));

        /// <summary>
        /// Starts the application as the user has it: reads the settings, starts logging with the Serilog configuration beside them, and
        /// composes every component.
        /// </summary>
        /// <param name="settingsPath">Where the settings of the user are kept.</param>
        /// <param name="heroNameEncoding">How hero names are written in the savefile.</param>
        /// <param name="definitions">Reads the shipped game definitions.</param>
        /// <returns>The components.</returns>
        /// <exception cref="SettingsException">The settings file exists but cannot be read.</exception>
        /// <exception cref="LoggingException">The log folder the user chose cannot be used, or the Serilog configuration cannot be read or built.</exception>
        /// <exception cref="GameDefinitionException">A standard game definition cannot be read.</exception>
        public static AppServices Start(string settingsPath, Encoding heroNameEncoding, IGameDefinitionReader definitions)
        {
            var settings = new SettingsStore(settingsPath);
            settings.EnsureCreated();
            var loggerFactory = LoggingSetup.Create(settings.Settings, AppInfo.Name, LogPaths.SerilogConfigPathFor(settingsPath));
            return Compose(settings, loggerFactory, heroNameEncoding, definitions);
        }

        /// <summary>
        /// Composes every component from the settings and the logging it is given: installs the logging, builds what the games share and
        /// the module of each game.
        /// </summary>
        /// <param name="settings">The store of the settings of the user, already created.</param>
        /// <param name="loggerFactory">Makes the loggers; the application disposes it when it exits.</param>
        /// <param name="heroNameEncoding">How hero names are written in the savefile.</param>
        /// <param name="definitions">Reads the game definitions.</param>
        /// <returns>The components.</returns>
        /// <exception cref="GameDefinitionException">A standard game definition cannot be read.</exception>
        public static AppServices Compose(SettingsStore settings, ILoggerFactory loggerFactory, Encoding heroNameEncoding, IGameDefinitionReader definitions)
        {
            LogFactory.Install(loggerFactory);
            var bytesPatcher = new SavefileBytesPatcher();
            var recentFiles = new RecentFilesStore(settings);
            var backup = new FileBackupServiceProvider(settings, new NumberedSavefileBackupService(), new DisabledFileBackupService());
            var banishingText = new BanishingTextProvider();
            var messages = new WpfMessageService();
            var fileDialogs = new WpfFileDialogs();
            var about = CreateAboutInfo();

            var homm3 = new Homm3ModuleFactory(heroNameEncoding, backup, bytesPatcher, banishingText, service => service, definitions);
            var homm2 = new H2Runtime.Homm2ModuleFactory(backup, bytesPatcher, banishingText, definitions);

            Logger.LogInformation(
                "Started {App} {Version} on {Os} with {Runtime}",
                about.Name,
                about.Version,
                RuntimeInformation.OSDescription,
                RuntimeInformation.FrameworkDescription);

            return new AppServices(
                loggerFactory,
                settings,
                recentFiles,
                new RecentFilesMenuViewModel(recentFiles),
                backup,
                bytesPatcher,
                banishingText,
                new ShellUrlLauncher(StartWithShell),
                messages,
                fileDialogs,
                new SavegameFileCommands(recentFiles, fileDialogs, messages),
                about,
                new HelpLinks(AppInfo.UserGuideUrl, AppInfo.IssuesUrl, LogPaths.Resolve(settings.Settings, AppInfo.Name)),
                homm3,
                homm2);
        }

        private static AboutInfo CreateAboutInfo()
        {
            var version = typeof(AppServices).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
            var plus = version.IndexOf('+');
            return new AboutInfo(
                AppInfo.Name,
                plus < 0 ? version : version[..plus],
                "A savegame editor for Heroes of Might and Magic II & III.",
                AppInfo.RepositoryUrl,
                AppInfo.ReleasesUrl,
                "Copyright (c) 2026 @dimif. Released under the MIT License.",
                [
                    "Heroes of Might and Magic is a trademark of Ubisoft. This project is not affiliated with or endorsed by Ubisoft.",
                    "The menu and toolbar icons are from the Microsoft Visual Studio Image Library and are not covered by the MIT License. See the README for the third-party notices.",
                    "Always keep a backup of your savegames.",
                ]);
        }

        private static void StartWithShell(string url)
        {
            using var process = Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
}
