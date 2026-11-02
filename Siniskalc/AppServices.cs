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
        /// <returns>The editor.</returns>
        public Homm3EditorViewModel CreateHomm3Editor() => this.homm3.CreateEditor();

        /// <summary>
        /// Makes the editor of Heroes of Might and Magic II, with nothing loaded.
        /// </summary>
        /// <returns>The editor.</returns>
        public Homm2EditorViewModel CreateHomm2Editor() => this.homm2.CreateEditor();

        /// <summary>
        /// Makes the page of the settings where the user chooses the log level and the folder of the logs.
        /// </summary>
        /// <returns>The page.</returns>
        public LoggingSettingsViewModel CreateLoggingSettings() => new(this.Settings, LogPaths.DefaultFolderFor(AppInfo.Name), this.FileDialogs);

        /// <summary>
        /// Makes the shell of the application, with every game it can edit.
        /// </summary>
        /// <returns>The shell.</returns>
        public ShellViewModel CreateShellViewModel() => new([this.Homm3, this.Homm2], this.Settings, AppInfo.Name);
    }
}
