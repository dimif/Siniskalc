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
        private static readonly ILogger<AppServices> Logger = LogFactory.For<AppServices>();

        private readonly Homm3ModuleFactory homm3;
        private readonly H2Runtime.Homm2ModuleFactory homm2;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServices"/> class: reads the settings, starts logging and builds every component.
        /// </summary>
        /// <param name="heroNameEncoding">How hero names are written in the savefile.</param>
        /// <param name="settingsPath">Where the settings of the user are kept; tests give a file of their own.</param>
        /// <param name="loggerFactory">Makes the loggers; the application leaves it out and gets the log file of the user, tests give a silent one.</param>
        /// <param name="definitions">Reads the shipped game definitions; tests can supply definitions of their own.</param>
        /// <exception cref="SettingsException">The settings file exists but cannot be read.</exception>
        /// <exception cref="LoggingException">The log folder the user chose cannot be used, or the Serilog configuration cannot be read or built.</exception>
        public AppServices(Encoding? heroNameEncoding = null, string? settingsPath = null, ILoggerFactory? loggerFactory = null, IGameDefinitionReader? definitions = null)
        {
            this.BytesPatcher = new SavefileBytesPatcher();
            var settingsFile = settingsPath ?? UserSettings.PathFor(AppInfo.Name);
            this.Settings = new SettingsStore(settingsFile);
            this.Settings.EnsureCreated();
            this.LoggerFactory = loggerFactory ?? LoggingSetup.Create(this.Settings.Settings, AppInfo.Name, LogPaths.SerilogConfigPathFor(settingsFile));
            LogFactory.Install(this.LoggerFactory);
            this.RecentFiles = new RecentFilesStore(this.Settings);
            this.RecentFilesMenu = new RecentFilesMenuViewModel(this.RecentFiles);
            this.Backup = new FileBackupServiceProvider(this.Settings, new NumberedSavefileBackupService(), new DisabledFileBackupService());
            this.BanishingText = new BanishingTextProvider();
            this.UrlLauncher = new ShellUrlLauncher(StartWithShell);
            this.Messages = new WpfMessageService();
            this.FileDialogs = new WpfFileDialogs();
            this.FileCommands = new SavegameFileCommands(this.RecentFiles, this.FileDialogs, this.Messages);
            this.About = CreateAboutInfo();
            this.Help = new HelpLinks(AppInfo.UserGuideUrl, AppInfo.IssuesUrl, LogPaths.Resolve(this.Settings.Settings, AppInfo.Name));

            var definitionReader = definitions ?? new JsonGameDefinitionReader();
            this.homm3 = new Homm3ModuleFactory(heroNameEncoding ?? Encoding.Latin1, this.Backup, this.BytesPatcher, this.BanishingText, definitions: definitionReader);
            this.homm2 = new H2Runtime.Homm2ModuleFactory(this.Backup, this.BytesPatcher, this.BanishingText, definitionReader);
            Logger.LogDebug("Built the modules of both games");

            Logger.LogInformation(
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

        /// <summary>
        /// Gets the store of the settings of the user.
        /// </summary>
        public SettingsStore Settings { get; }

        /// <summary>
        /// Gets the list of the savegames opened last.
        /// </summary>
        public RecentFilesStore RecentFiles { get; }

        /// <summary>
        /// Gets the view model of the menu of the recent files.
        /// </summary>
        public RecentFilesMenuViewModel RecentFilesMenu { get; }

        /// <summary>
        /// Gets the provider of the service that backs a savefile up before it is replaced.
        /// </summary>
        public IFileBackupServiceProvider Backup { get; }

        /// <summary>
        /// Gets what writes the chunks of a savefile over its bytes.
        /// </summary>
        public ISavefileBytesPatcher BytesPatcher { get; }

        /// <summary>
        /// Gets the provider of the text shown for a banished creature.
        /// </summary>
        public BanishingTextProvider BanishingText { get; }

        /// <summary>
        /// Gets what opens web addresses and folders for the user.
        /// </summary>
        public IUrlLauncher UrlLauncher { get; }

        /// <summary>
        /// Gets the service that shows messages to the user.
        /// </summary>
        public IMessageService Messages { get; }

        /// <summary>
        /// Gets the service that shows the dialogs to pick files and folders.
        /// </summary>
        public IFileDialogs FileDialogs { get; }

        /// <summary>
        /// Gets the commands to open and save a savegame, shared by the games.
        /// </summary>
        public SavegameFileCommands FileCommands { get; }

        /// <summary>
        /// Gets what the About window shows.
        /// </summary>
        public AboutInfo About { get; }

        /// <summary>
        /// Gets the links and folders of the Help menu.
        /// </summary>
        public HelpLinks Help { get; }

        /// <summary>
        /// Gets the game module of Heroes of Might and Magic III.
        /// </summary>
        public IGameModule<Homm3EditorViewModel> Homm3 => this.homm3.Module;

        /// <summary>
        /// Gets the game module of Heroes of Might and Magic II.
        /// </summary>
        public IGameModule<Homm2EditorViewModel> Homm2 => this.homm2.Module;

        /// <summary>
        /// Gets the service that loads, reloads and saves Heroes of Might and Magic III games.
        /// </summary>
        public ISavegameService<Game> GameService => this.homm3.GameService;

        /// <summary>
        /// Gets the codec that reads and writes Heroes of Might and Magic III savefiles on disk.
        /// </summary>
        public ISavefileCodec SavefileCodec => this.homm3.SavefileCodec;

        /// <summary>
        /// Gets the factory of the documents of Heroes of Might and Magic III savefiles.
        /// </summary>
        public ISaveDocumentFactory DocumentFactory => this.homm3.DocumentFactory;

        /// <summary>
        /// Gets what reads a Heroes of Might and Magic III castle from its chunk.
        /// </summary>
        public ICastleDeserializer CastleDeserializer => this.homm3.CastleDeserializer;

        /// <summary>
        /// Gets what writes a Heroes of Might and Magic III castle into its chunk.
        /// </summary>
        public ISerializer<CastleChunk, CastleModel> CastleSerializer => this.homm3.CastleSerializer;

        /// <summary>
        /// Gets what reads a Heroes of Might and Magic III player from its chunk.
        /// </summary>
        public IPlayerDeserializer PlayerDeserializer => this.homm3.PlayerDeserializer;

        /// <summary>
        /// Gets what says where each known field lives in a Heroes of Might and Magic III savefile.
        /// </summary>
        public ISaveRegionProvider SaveRegionProvider => this.homm3.SaveRegionProvider;

        /// <summary>
        /// Gets the factory of the provider of the entities that depend on the game version of a Heroes of Might and Magic III savefile.
        /// </summary>
        public IVersionSpecificEntitiesProviderFactory VersionEntities => this.homm3.VersionEntities;

        /// <summary>
        /// Gets what says where the known fields of a Heroes of Might and Magic III hero are.
        /// </summary>
        public IHeroRegionProvider HeroRegions => this.homm3.HeroRegions;

        /// <summary>
        /// Gets what says where the known fields of a Heroes of Might and Magic III player are.
        /// </summary>
        public IPlayerRegionProvider PlayerRegions => this.homm3.PlayerRegions;

        /// <summary>
        /// Gets what says where the known fields of a Heroes of Might and Magic III castle are.
        /// </summary>
        public ICastleRegionProvider CastleRegions => this.homm3.CastleRegions;

        /// <summary>
        /// Gets the factory of the targets the hex editor edits in a Heroes of Might and Magic III savefile.
        /// </summary>
        public IByteEditTargetFactory ByteEditTargets => this.homm3.ByteEditTargets;

        /// <summary>
        /// Gets the provider of the effect lines Heroes of Might and Magic III pages show.
        /// </summary>
        public EffectLineProvider EffectLines => this.homm3.EffectLines;

        /// <summary>
        /// Gets the provider of the messages about building in a Heroes of Might and Magic III castle.
        /// </summary>
        public CastleBuildMessageProvider CastleBuildMessages => this.homm3.CastleBuildMessages;

        /// <summary>
        /// Gets the factory of the calculators of the spell points of a Heroes of Might and Magic III hero.
        /// </summary>
        public ISpellPointsCalculatorFactory SpellPoints => this.homm3.SpellPoints;

        /// <summary>
        /// Gets the service that loads, reloads and saves Heroes of Might and Magic II games.
        /// </summary>
        public ISavegameService<H2Runtime.Game> Homm2GameService => this.homm2.GameService;

        /// <summary>
        /// Gets what says where each known field lives in a Heroes of Might and Magic II savefile.
        /// </summary>
        public ISaveRegionProvider Homm2SaveRegionProvider => this.homm2.SaveRegionProvider;

        /// <summary>
        /// Gets the factory of the targets the hex editor edits in a Heroes of Might and Magic II savefile.
        /// </summary>
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
        /// Makes the page of the settings where the user chooses the log level and the folder of the logs.
        /// </summary>
        public LoggingSettingsViewModel CreateLoggingSettings() => new(this.Settings, LogPaths.DefaultFolderFor(AppInfo.Name), this.FileDialogs);

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
