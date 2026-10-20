namespace Siniskalc.Homm3.Runtime
{
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Heroes.Data;
    using System.Text;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// Builds the components of Heroes of Might and Magic III, once, from what the games share, and the module that holds them.
    /// </summary>
    public sealed class Homm3ModuleFactory
    {
        /// <param name="heroNameEncoding">How hero names are written in the savefile.</param>
        /// <param name="backup">Keeps a copy of a savefile before it is replaced.</param>
        /// <param name="bytesPatcher">Applies the edits of a document to the bytes of a savefile.</param>
        /// <param name="banishingText">Words the countdown of a player with no town is shown with.</param>
        public Homm3ModuleFactory(Encoding heroNameEncoding, IFileBackupServiceProvider backup, ISavefileBytesPatcher bytesPatcher, BanishingTextProvider banishingText)
        {
            this.BytesPatcher = bytesPatcher;
            this.BanishingText = banishingText;

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
            var pendingChanges = new Homm3PendingChangesFactory();
            this.DocumentFactory = new SaveDocumentFactory(
                this.VersionEntities,
                new HeroDiffer(pendingChanges),
                this.CastleDeserializer,
                this.CastleSerializer,
                new CastleDiffer(pendingChanges),
                this.PlayerDeserializer,
                new SafePlayerSerializer(),
                new PlayerDiffer(pendingChanges),
                new ObeliskDeserializer(),
                new SafeObeliskSerializer(),
                new ObeliskDiffer<PlayerColor>(PlayerColor.All, pendingChanges.ForObelisks),
                this.BytesPatcher);

            this.SavefileCodec = new GzipSavefileCodec();
            this.GameService = new GameService(
                new SavegameParser(
                    new SavegameVersionDetector(),
                    new HeroBytesLoader(heroNameEncoding),
                    new CastleBytesLoader(heroNameEncoding),
                    new PlayerBytesLoader(),
                    new ObeliskBytesLoader(),
                    dateLocator,
                    setupLocator,
                    mapHeaderLocator,
                    this.DocumentFactory),
                this.SavefileCodec,
                backup);
            this.SaveRegionProvider = new SaveRegionProvider(mapHeaderLocator, setupLocator, dateLocator, this.HeroRegions, this.PlayerRegions, this.CastleRegions);
            this.ByteEditTargets = new ByteEditTargetFactory(this.HeroRegions, this.PlayerRegions, this.CastleRegions);
            this.EffectLines = new EffectLineProvider();
            this.CastleBuildMessages = new CastleBuildMessageProvider();
            this.SpellPoints = new SpellPointsCalculatorFactory();
            this.Module = new Homm3Module(this.CreateEditor);
        }

        /// <summary>
        /// Gets the game module of Heroes of Might and Magic III.
        /// </summary>
        public IGameModule<Homm3EditorViewModel> Module { get; }

        public IGameService GameService { get; }

        public ISavefileCodec SavefileCodec { get; }

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

        public ISpellPointsCalculatorFactory SpellPoints { get; }

        /// <summary>
        /// Makes the editor of Heroes of Might and Magic III, with nothing loaded.
        /// </summary>
        public Homm3EditorViewModel CreateEditor() => new(this.GameService, this.EffectLines, this.BanishingText, this.CastleBuildMessages, this.SpellPoints, this.SaveRegionProvider, this.ByteEditTargets, new NavigationBuilder(this.BanishingText));
    }
}
