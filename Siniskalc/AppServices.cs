namespace Siniskalc
{
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Data;
    using Siniskalc.Models;
    using Siniskalc.Runtime;
    using Siniskalc.ViewModels;
    using Siniskalc.Wpf.ViewModels;
    using Siniskalc.Wpf.Views;

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
            this.Backup = new NumberedSavefileBackup(this.Settings);
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
            this.CastleBuildMessages = new CastleBuildMessageProvider();
            this.SpellPoints = new SpellPointsCalculator();
            this.UrlLauncher = new ShellUrlLauncher(StartWithShell);
            this.Messages = new WpfMessageService();
            this.FileDialogs = new WpfFileDialogs();
            this.FileCommands = new SavegameFileCommands(this.RecentFiles, this.FileDialogs, this.Messages);
            this.About = CreateAboutInfo();
        }

        public IGameService GameService { get; }

        public ISavefileCodec SavefileCodec { get; }

        public SettingsStore Settings { get; }

        public RecentFilesStore RecentFiles { get; }

        public RecentFilesMenuViewModel RecentFilesMenu { get; }

        public ISavefileBackup Backup { get; }

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

        public CastleBuildMessageProvider CastleBuildMessages { get; }

        public ISpellPointsCalculator SpellPoints { get; }

        public IUrlLauncher UrlLauncher { get; }

        public IMessageService Messages { get; }

        public IFileDialogs FileDialogs { get; }

        public SavegameFileCommands FileCommands { get; }

        public AboutInfo About { get; }

        public MainViewModel CreateMainViewModel() => new(this.GameService, this.EffectLines, this.CastleBuildMessages, this.SpellPoints, this.SaveRegionProvider, this.ByteEditTargets, this.Settings);

        private static AboutInfo CreateAboutInfo()
        {
            var version = typeof(AppServices).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
            var plus = version.IndexOf('+');
            return new AboutInfo(
                Constants.App.Name,
                plus < 0 ? version : version[..plus],
                "A savegame editor for Heroes of Might and Magic III.",
                Constants.App.RepositoryUrl,
                Constants.App.ReleasesUrl,
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
