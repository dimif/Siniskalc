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
    using Siniskalc.Homm2.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Homm3.Runtime;
    using Siniskalc.Homm3.ViewModels;
    using Siniskalc.Wpf.ViewModels;
    using Siniskalc.Wpf.Views;
    using H2Data = Siniskalc.Homm2.Data;
    using H2Runtime = Siniskalc.Homm2.Runtime;

    /// <summary>
    /// The composition root: builds each component once and hands it to the ones that use it. What the games share is built here;
    /// what only a game uses is built by the factory of that game. Nothing else creates a component that another class depends on.
    /// </summary>
    public sealed class AppServices
    {
        private readonly Homm3ModuleFactory homm3;
        private readonly H2Runtime.Homm2ModuleFactory homm2;

        /// <param name="heroNameEncoding">How hero names are written in the savefile.</param>
        /// <param name="settingsPath">Where the settings of the user are kept; tests give a file of their own.</param>
        /// <param name="loggerFactory">Makes the loggers; the application leaves it out and gets the log file of the user, tests give a silent one.</param>
        /// <exception cref="SettingsException">The settings file exists but cannot be read.</exception>
        /// <exception cref="LoggingException">The log folder the user chose cannot be used.</exception>
        public AppServices(Encoding? heroNameEncoding = null, string? settingsPath = null, ILoggerFactory? loggerFactory = null)
        {
            this.BytesPatcher = new SavefileBytesPatcher();
            this.Settings = settingsPath is null
                ? new SettingsStore(UserSettings.PathFor("Siniskalc"), UserSettings.PathFor("Heroes3Editor"))
                : new SettingsStore(settingsPath);
            this.LoggerFactory = loggerFactory ?? LoggingSetup.Create(this.Settings.Settings, AppInfo.Name);
            this.RecentFiles = new RecentFilesStore(this.Settings);
            this.RecentFilesMenu = new RecentFilesMenuViewModel(this.RecentFiles);
            this.Backup = new FileBackupServiceProvider(this.Settings, new NumberedSavefileBackupService(), new DisabledFileBackupService());
            this.BanishingText = new BanishingTextProvider();
            this.UrlLauncher = new ShellUrlLauncher(StartWithShell);
            this.Messages = new WpfMessageService();
            this.FileDialogs = new WpfFileDialogs();
            this.FileCommands = new SavegameFileCommands(this.RecentFiles, this.FileDialogs, this.Messages, this.LoggerFactory.CreateLogger<SavegameFileCommands>());
            this.About = CreateAboutInfo();
            this.Help = new HelpLinks(AppInfo.UserGuideUrl, AppInfo.IssuesUrl);

            this.homm3 = new Homm3ModuleFactory(heroNameEncoding ?? Encoding.Latin1, this.Backup, this.BytesPatcher, this.BanishingText, this.LoggerFactory);
            this.homm2 = new H2Runtime.Homm2ModuleFactory(this.Backup, this.BytesPatcher, this.BanishingText, this.LoggerFactory);

            this.LoggerFactory.CreateLogger<AppServices>().LogInformation(
                "Started {App} {Version} on {Os} with {Runtime}",
                this.About.Name,
                this.About.Version,
                RuntimeInformation.OSDescription,
                RuntimeInformation.FrameworkDescription);
        }

        /// <summary>
        /// Gets the factory of the loggers. The application disposes it when it exits, which flushes the log file.
        /// </summary>
        public ILoggerFactory LoggerFactory { get; }

        public SettingsStore Settings { get; }

        public RecentFilesStore RecentFiles { get; }

        public RecentFilesMenuViewModel RecentFilesMenu { get; }

        public IFileBackupServiceProvider Backup { get; }

        public ISavefileBytesPatcher BytesPatcher { get; }

        public BanishingTextProvider BanishingText { get; }

        public IUrlLauncher UrlLauncher { get; }

        public IMessageService Messages { get; }

        public IFileDialogs FileDialogs { get; }

        public SavegameFileCommands FileCommands { get; }

        public AboutInfo About { get; }

        public HelpLinks Help { get; }

        /// <summary>
        /// Gets the game module of Heroes of Might and Magic III.
        /// </summary>
        public IGameModule<Homm3EditorViewModel> Homm3 => this.homm3.Module;

        /// <summary>
        /// Gets the game module of Heroes of Might and Magic II.
        /// </summary>
        public IGameModule<Homm2EditorViewModel> Homm2 => this.homm2.Module;

        public IGameService GameService => this.homm3.GameService;

        public ISavefileCodec SavefileCodec => this.homm3.SavefileCodec;

        public ISaveDocumentFactory DocumentFactory => this.homm3.DocumentFactory;

        public ICastleDeserializer CastleDeserializer => this.homm3.CastleDeserializer;

        public ISerializer<CastleChunk, CastleModel> CastleSerializer => this.homm3.CastleSerializer;

        public IPlayerDeserializer PlayerDeserializer => this.homm3.PlayerDeserializer;

        public ISaveRegionProvider SaveRegionProvider => this.homm3.SaveRegionProvider;

        public IVersionSpecificEntitiesProviderFactory VersionEntities => this.homm3.VersionEntities;

        public IHeroRegionProvider HeroRegions => this.homm3.HeroRegions;

        public IPlayerRegionProvider PlayerRegions => this.homm3.PlayerRegions;

        public ICastleRegionProvider CastleRegions => this.homm3.CastleRegions;

        public IByteEditTargetFactory ByteEditTargets => this.homm3.ByteEditTargets;

        public EffectLineProvider EffectLines => this.homm3.EffectLines;

        public CastleBuildMessageProvider CastleBuildMessages => this.homm3.CastleBuildMessages;

        public ISpellPointsCalculatorFactory SpellPoints => this.homm3.SpellPoints;

        public H2Runtime.IGameService Homm2GameService => this.homm2.GameService;

        public ISaveRegionProvider Homm2SaveRegionProvider => this.homm2.SaveRegionProvider;

        public H2Data.IByteEditTargetFactory Homm2ByteEditTargets => this.homm2.ByteEditTargets;

        /// <summary>
        /// Makes the editor of Heroes of Might and Magic III, with nothing loaded.
        /// </summary>
        public Homm3EditorViewModel CreateHomm3Editor() => this.homm3.CreateEditor();

        /// <summary>
        /// Makes the editor of Heroes of Might and Magic II, with nothing loaded.
        /// </summary>
        public Homm2EditorViewModel CreateHomm2Editor() => this.homm2.CreateEditor();

        /// <summary>
        /// Makes the shell of the application, with every game it can edit.
        /// </summary>
        public ShellViewModel CreateShellViewModel() => new([this.Homm3, this.Homm2], this.Settings, AppInfo.Name);

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
                "Copyright (c) 2026 Dimitri Fadda. Released under the MIT License.",
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
