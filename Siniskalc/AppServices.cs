namespace Siniskalc
{
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
    /// The components of the application, built by <see cref="AppCompositionRoot"/> and handed in. It creates none of them: it holds them and
    /// makes the view models that are made on demand.
    /// </summary>
    public sealed class AppServices
    {
        private readonly Homm3ModuleFactory homm3;
        private readonly H2Runtime.Homm2ModuleFactory homm2;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServices"/> class.
        /// </summary>
        /// <param name="loggerFactory">The factory of the loggers.</param>
        /// <param name="settings">The store of the settings of the user.</param>
        /// <param name="recentFiles">The list of the savegames opened last.</param>
        /// <param name="recentFilesMenu">The view model of the menu of the recent files.</param>
        /// <param name="backup">The provider of the service that backs a savefile up before it is replaced.</param>
        /// <param name="bytesPatcher">What writes the chunks of a savefile over its bytes.</param>
        /// <param name="banishingText">The provider of the text shown for a banished creature.</param>
        /// <param name="urlLauncher">What opens web addresses and folders for the user.</param>
        /// <param name="messages">The service that shows messages to the user.</param>
        /// <param name="fileDialogs">The service that shows the dialogs to pick files and folders.</param>
        /// <param name="fileCommands">The commands to open and save a savegame, shared by the games.</param>
        /// <param name="about">What the About window shows.</param>
        /// <param name="help">The links and folders of the Help menu.</param>
        /// <param name="homm3">The factory of the components of Heroes of Might and Magic III.</param>
        /// <param name="homm2">The factory of the components of Heroes of Might and Magic II.</param>
        public AppServices(
            ILoggerFactory loggerFactory,
            SettingsStore settings,
            RecentFilesStore recentFiles,
            RecentFilesMenuViewModel recentFilesMenu,
            IFileBackupServiceProvider backup,
            ISavefileBytesPatcher bytesPatcher,
            BanishingTextProvider banishingText,
            IUrlLauncher urlLauncher,
            IMessageService messages,
            IFileDialogs fileDialogs,
            SavegameFileCommands fileCommands,
            AboutInfo about,
            HelpLinks help,
            Homm3ModuleFactory homm3,
            H2Runtime.Homm2ModuleFactory homm2)
        {
            this.LoggerFactory = loggerFactory;
            this.Settings = settings;
            this.RecentFiles = recentFiles;
            this.RecentFilesMenu = recentFilesMenu;
            this.Backup = backup;
            this.BytesPatcher = bytesPatcher;
            this.BanishingText = banishingText;
            this.UrlLauncher = urlLauncher;
            this.Messages = messages;
            this.FileDialogs = fileDialogs;
            this.FileCommands = fileCommands;
            this.About = about;
            this.Help = help;
            this.homm3 = homm3;
            this.homm2 = homm2;
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

        public ISavegameService<Game> GameService => this.homm3.GameService;

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

        public ISavegameService<H2Runtime.Game> Homm2GameService => this.homm2.GameService;

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
        /// Makes the page of the settings where the user chooses the log level and the folder of the logs.
        /// </summary>
        public LoggingSettingsViewModel CreateLoggingSettings() => new(this.Settings, LogPaths.DefaultFolderFor(AppInfo.Name), this.FileDialogs);

        /// <summary>
        /// Makes the shell of the application, with every game it can edit.
        /// </summary>
        public ShellViewModel CreateShellViewModel() => new([this.Homm3, this.Homm2], this.Settings, AppInfo.Name);
    }
}
