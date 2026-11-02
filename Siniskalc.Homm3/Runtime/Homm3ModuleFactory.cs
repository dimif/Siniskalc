namespace Siniskalc.Homm3.Runtime
{
    using System;
    using System.Collections.Frozen;
    using System.Text;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
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
        /// <param name="decorateGameService">
        /// Wraps the service that loads and saves games, for the editor and everything else the factory builds with it; the application
        /// returns the service as it is. The tests use it to serve a savefile they have already parsed.
        /// </param>
        /// <param name="definitions">Reads the game definitions the catalogs are built from.</param>
        public Homm3ModuleFactory(
            Encoding heroNameEncoding,
            IFileBackupServiceProvider backup,
            ISavefileBytesPatcher bytesPatcher,
            BanishingTextProvider banishingText,
            Func<ISavegameService<Game>, ISavegameService<Game>> decorateGameService,
            IGameDefinitionReader definitions)
        {
            this.BytesPatcher = bytesPatcher;
            this.BanishingText = banishingText;

            var dateLocator = new GameDateLocator();
            var setupLocator = new PlayerControlLocator();
            var mapHeaderLocator = new MapHeaderLocator();
            var catalogs = new GameCatalogLoader(definitions).Load();
            this.VersionEntities = new VersionSpecificEntitiesProviderFactory(catalogs);
            var castleLoaders = catalogs.ToFrozenDictionary(pair => pair.Key, pair => new CastleBytesLoader(heroNameEncoding, pair.Value));
            var mineLoaders = catalogs.ToFrozenDictionary(pair => pair.Key, pair => new MineBytesLoader(pair.Value));
            var dwellingLoaders = catalogs.ToFrozenDictionary(pair => pair.Key, pair => new DwellingBytesLoader(pair.Value));
            var merchantLoaders = catalogs.ToFrozenDictionary(pair => pair.Key, pair => new MerchantBytesLoader(pair.Value));
            var catalog = this.VersionEntities.For(GameVersion.HornOfTheAbyss).Catalog;
            this.HeroRegions = new HeroRegionProvider(this.VersionEntities);
            this.PlayerRegions = new PlayerRegionProvider();
            this.CastleRegions = new CastleRegionProvider();

            this.CastleDeserializer = new CastleDeserializer(catalog);
            this.CastleSerializer = new SafeCastleSerializer();
            this.PlayerDeserializer = new PlayerDeserializer();
            var pendingChanges = new Homm3PendingChangesFactory();
            this.DocumentFactory = new SaveDocumentFactory(
                this.VersionEntities,
                new HeroDiffer(pendingChanges),
                entities => new CastleDeserializer(entities),
                this.CastleSerializer,
                new CastleDiffer(pendingChanges),
                this.PlayerDeserializer,
                new SafePlayerSerializer(),
                new PlayerListsDeserializer(),
                new SafePlayerListsSerializer(),
                new PlayerDiffer(pendingChanges),
                new ObeliskDeserializer<PlayerColor>(ObeliskLayout.Flags),
                new SafeObeliskSerializer<PlayerColor>(ObeliskLayout.Flags),
                new ObeliskDiffer<PlayerColor>(PlayerColor.All, pendingChanges.ForObelisks),
                entities => new RecordKit<RecordChunk, MineModel>(new MineDeserializer(new CreatureCodes(entities)), new SafeMineSerializer(new CreatureCodes(entities)), new MineDiffer()),
                entities => new RecordKit<RecordChunk, DwellingModel>(new DwellingDeserializer(entities), new SafeDwellingSerializer(), new DwellingDiffer()),
                entities => new RecordKit<RecordChunk, MerchantStockModel>(new MerchantStockDeserializer(entities), new SafeMerchantStockSerializer(entities), new MerchantStockDiffer()),
                new RecordKit<RecordChunk, HeroOwnersModel>(new HeroOwnersDeserializer(), new SafeHeroOwnersSerializer(), new HeroOwnersDiffer()),
                new RecordKit<RecordChunk, TurnModel>(new TurnDeserializer(), new SafeTurnSerializer(), new TurnDiffer()),
                new PlayerListsReconciler(),
                this.BytesPatcher);

            this.SavefileCodec = new GzipSavefileCodec();
            ISavegameService<Game> gameService = new SavegameService<Game>(
                new SavegameParser(
                    new SavegameVersionDetector(),
                    new HeroBytesLoader(heroNameEncoding),
                    version => castleLoaders[version],
                    new PlayerBytesLoader(),
                    new ObeliskBytesLoader(),
                    version => mineLoaders[version],
                    version => dwellingLoaders[version],
                    version => merchantLoaders[version],
                    new HeroOwnersBytesLoader(dateLocator),
                    new TurnBytesLoader(setupLocator),
                    new MapPlacesLoader(),
                    dateLocator,
                    setupLocator,
                    mapHeaderLocator,
                    this.DocumentFactory),
                this.SavefileCodec,
                backup);
            this.GameService = decorateGameService(gameService);
            this.SaveRegionProvider = new SaveRegionProvider(mapHeaderLocator, setupLocator, dateLocator, this.HeroRegions, this.PlayerRegions, this.CastleRegions);
            this.ByteEditTargets = new ByteEditTargetFactory(this.HeroRegions, this.PlayerRegions, this.CastleRegions, new MineRegionProvider(), new DwellingRegionProvider());
            this.EffectLines = new EffectLineProvider();
            this.CastleBuildMessages = new CastleBuildMessageProvider();
            this.SpellPoints = new SpellPointsCalculatorFactory();
            this.Module = new Homm3Module(this.CreateEditor);
        }

        /// <summary>
        /// Gets the game module of Heroes of Might and Magic III.
        /// </summary>
        public IGameModule<Homm3EditorViewModel> Module { get; }

        public ISavegameService<Game> GameService { get; }

        public ISavefileCodec SavefileCodec { get; }

        public ISaveDocumentFactory DocumentFactory { get; }

        public ICastleDeserializer CastleDeserializer { get; }

        public ISerializer<CastleChunk, CastleModel> CastleSerializer { get; }

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
