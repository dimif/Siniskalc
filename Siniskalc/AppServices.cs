namespace Siniskalc
{
    using Siniskalc.Heroes.Wpf.ViewModels;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
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
    /// The composition root: builds each component once and hands it to the ones that use it. Nothing else creates a component
    /// that another class depends on.
    /// </summary>
    public sealed class AppServices
    {
        /// <param name="heroNameEncoding">How hero names are written in the savefile.</param>
        /// <param name="settingsPath">Where the settings of the user are kept; tests give a file of their own.</param>
        public AppServices(Encoding? heroNameEncoding = null, string? settingsPath = null)
        {
            var encoding = heroNameEncoding ?? Encoding.Latin1;

            var dateLocator = new GameDateLocator();
            var setupLocator = new PlayerControlLocator();
            var mapHeaderLocator = new MapHeaderLocator();
            this.VersionEntities = new VersionSpecificEntitiesProviderFactory(version => new GrantedSpellsResolver(version));
            this.HeroRegions = new HeroRegionProvider(this.VersionEntities);
            this.PlayerRegions = new PlayerRegionProvider();
            this.CastleRegions = new CastleRegionProvider();

            this.CastleDeserializer = new CastleDeserializer();
            this.CastleSerializer = new SafeCastleSerializer();
            this.PlayerDeserializer = new PlayerDeserializer();
            this.BytesPatcher = new SavefileBytesPatcher();
            this.DocumentFactory = new SaveDocumentFactory(
                this.VersionEntities,
                new HeroDiffer(),
                this.CastleDeserializer,
                this.CastleSerializer,
                new CastleDiffer(),
                this.PlayerDeserializer,
                new SafePlayerSerializer(),
                new PlayerDiffer(),
                new ObeliskDeserializer(),
                new SafeObeliskSerializer(),
                new ObeliskDiffer(),
                this.BytesPatcher);

            this.SavefileCodec = new GzipSavefileCodec();
            this.Settings = settingsPath is null
                ? new SettingsStore(UserSettings.PathFor("Siniskalc"), UserSettings.PathFor("Heroes3Editor"))
                : new SettingsStore(settingsPath);
            this.RecentFiles = new RecentFilesStore(this.Settings);
            this.RecentFilesMenu = new RecentFilesMenuViewModel(this.RecentFiles);
            this.Backup = new FileBackupServiceProvider(this.Settings, new NumberedSavefileBackupService(), new DisabledFileBackupService());
            this.GameService = new GameService(
                new SavegameParser(
                new SavegameVersionDetector(),
                new HeroBytesLoader(encoding),
                new CastleBytesLoader(encoding),
                new PlayerBytesLoader(),
                new ObeliskBytesLoader(),
                dateLocator,
                setupLocator,
                mapHeaderLocator,
                this.DocumentFactory),
                this.SavefileCodec,
                this.Backup);
            this.SaveRegionProvider = new SaveRegionProvider(mapHeaderLocator, setupLocator, dateLocator, this.HeroRegions, this.PlayerRegions, this.CastleRegions);
            this.ByteEditTargets = new ByteEditTargetFactory(this.HeroRegions, this.PlayerRegions, this.CastleRegions);
            this.EffectLines = new EffectLineProvider();
            this.BanishingText = new BanishingTextProvider();
            this.CastleBuildMessages = new CastleBuildMessageProvider();
            this.SpellPoints = new SpellPointsCalculator();
            this.UrlLauncher = new ShellUrlLauncher(StartWithShell);
            this.Messages = new WpfMessageService();
            this.FileDialogs = new WpfFileDialogs();
            this.FileCommands = new SavegameFileCommands(this.RecentFiles, this.FileDialogs, this.Messages);
            this.About = CreateAboutInfo();
            this.Help = new HelpLinks(AppInfo.UserGuideUrl, AppInfo.IssuesUrl);
            this.Homm3 = new Homm3Module(this.CreateHomm3Editor);

            var homm2Locator = new H2Data.SaveLayoutLocator();
            this.Homm2GameService = new H2Runtime.GameService(
                new H2Runtime.SavegameParser(
                    homm2Locator,
                    new H2Data.BytesLoader(),
                    new H2Data.HeaderReader(),
                    new H2Data.SaveDocumentFactory(
                        new H2Data.HeroDeserializer(),
                        new H2Data.SafeHeroSerializer(),
                        new H2Data.HeroDiffer(),
                        new H2Data.PlayerDeserializer(),
                        new H2Data.SafePlayerSerializer(),
                        new H2Data.PlayerDiffer(),
                        new H2Data.CastleDeserializer(),
                        new H2Data.SafeCastleSerializer(),
                        new H2Data.CastleDiffer(),
                        new H2Data.ObeliskDeserializer(),
                        new H2Data.SafeObeliskSerializer(),
                        new H2Data.ObeliskDiffer(),
                        this.BytesPatcher),
                    new H2Data.MapPlacesReader()),
                new RawSavefileCodec(),
                this.Backup);
            this.Homm2SaveRegionProvider = new H2Data.SaveRegionProvider(homm2Locator);
            this.Homm2ByteEditTargets = new H2Data.ByteEditTargetFactory();
            this.Homm2 = new H2Runtime.Homm2Module(homm2Locator, this.CreateHomm2Editor);
        }

        public IGameService GameService { get; }

        public ISavefileCodec SavefileCodec { get; }

        public SettingsStore Settings { get; }

        public RecentFilesStore RecentFiles { get; }

        public RecentFilesMenuViewModel RecentFilesMenu { get; }

        public IFileBackupServiceProvider Backup { get; }

        public ISaveDocumentFactory DocumentFactory { get; }

        public ICastleDeserializer CastleDeserializer { get; }

        public ICastleSerializer CastleSerializer { get; }

        public IPlayerDeserializer PlayerDeserializer { get; }

        public ISavefileBytesPatcher BytesPatcher { get; }

        public ISaveRegionProvider SaveRegionProvider { get; }

        public IVersionSpecificEntitiesProviderFactory VersionEntities { get; }

        public IHeroRegionProvider HeroRegions { get; }

        public IPlayerRegionProvider PlayerRegions { get; }

        public ICastleRegionProvider CastleRegions { get; }

        public IByteEditTargetFactory ByteEditTargets { get; }

        public EffectLineProvider EffectLines { get; }

        public BanishingTextProvider BanishingText { get; }

        public CastleBuildMessageProvider CastleBuildMessages { get; }

        public ISpellPointsCalculator SpellPoints { get; }

        public IUrlLauncher UrlLauncher { get; }

        public IMessageService Messages { get; }

        public IFileDialogs FileDialogs { get; }

        public SavegameFileCommands FileCommands { get; }

        public AboutInfo About { get; }

        public HelpLinks Help { get; }

        /// <summary>
        /// Gets the game module of Heroes of Might and Magic III.
        /// </summary>
        public IGameModule<Homm3EditorViewModel> Homm3 { get; }

        public H2Runtime.IGameService Homm2GameService { get; }

        public ISaveRegionProvider Homm2SaveRegionProvider { get; }

        public H2Data.IByteEditTargetFactory Homm2ByteEditTargets { get; }

        /// <summary>
        /// Gets the game module of Heroes of Might and Magic II.
        /// </summary>
        public IGameModule<Homm2EditorViewModel> Homm2 { get; }

        /// <summary>
        /// Makes the editor of Heroes of Might and Magic III, with nothing loaded.
        /// </summary>
        public Homm3EditorViewModel CreateHomm3Editor() => new(this.GameService, this.EffectLines, this.BanishingText, this.CastleBuildMessages, this.SpellPoints, this.SaveRegionProvider, this.ByteEditTargets, new Siniskalc.Homm3.ViewModels.NavigationBuilder(this.BanishingText));

        /// <summary>
        /// Makes the shell of the application, with every game it can edit.
        /// </summary>
        public ShellViewModel CreateShellViewModel() => new([this.Homm3, this.Homm2], this.Settings, AppInfo.Name);

        /// <summary>
        /// Makes the editor of Heroes of Might and Magic II, with nothing loaded.
        /// </summary>
        public Homm2EditorViewModel CreateHomm2Editor() => new(this.Homm2GameService, this.Homm2SaveRegionProvider, this.Homm2ByteEditTargets, new Siniskalc.Homm2.ViewModels.NavigationBuilder(), this.BanishingText);

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
