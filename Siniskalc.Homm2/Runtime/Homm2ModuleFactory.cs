namespace Siniskalc.Homm2.Runtime
{
    using System;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// Builds the components of Heroes of Might and Magic II, once, from what the games share, and the module that holds them.
    /// </summary>
    public sealed class Homm2ModuleFactory
    {
        private readonly BanishingTextProvider banishingText;

        /// <param name="backup">Keeps a copy of a savefile before it is replaced.</param>
        /// <param name="bytesPatcher">Applies the edits of a document to the bytes of a savefile.</param>
        /// <param name="banishingText">Words the countdown of a player with no town is shown with.</param>
        /// <param name="definitions">Reads the game definitions the catalogs are built from.</param>
        public Homm2ModuleFactory(IFileBackupServiceProvider backup, ISavefileBytesPatcher bytesPatcher, BanishingTextProvider banishingText, IGameDefinitionReader definitions)
        {
            this.banishingText = banishingText;
            this.Catalogs = new GameCatalogs(definitions);

            var locator = new SaveLayoutLocator();
            var pendingChanges = new Homm2PendingChangesFactory();
            var factories = Enum.GetValues<GameVersion>().ToDictionary(version => version, version => CreateDocumentFactory(this.Catalogs.For(version)));
            this.GameService = new SavegameService<Game>(
                new SavegameParser(
                    locator,
                    new BytesLoader(),
                    new HeaderReader(),
                    factories[GameVersion.BaseGame],
                    new MapPlacesReader(this.Catalogs),
                    new MapObjectsReader(this.Catalogs),
                    this.Catalogs,
                    version => factories[version]),
                new RawSavefileCodec(),
                backup);
            this.SaveRegionProvider = new SaveRegionProvider(locator);
            this.ByteEditTargets = new ByteEditTargetFactory();
            this.MaxSpellPointsCalculator = new MaxSpellPointsCalculator();
            this.Module = new Homm2Module(locator, this.CreateEditor);

            ISaveDocumentFactory CreateDocumentFactory(GameCatalog catalog) =>
                new SaveDocumentFactory(
                        new HeroDeserializer(catalog, new CreatureCodes(catalog)),
                        new SafeHeroSerializer(catalog),
                        new HeroDiffer(pendingChanges, catalog),
                        new PlayerDeserializer(),
                        new SafePlayerSerializer(),
                        new PlayerListsDeserializer(),
                        new SafePlayerListsSerializer(),
                        new PlayerDiffer(pendingChanges),
                        new CastleDeserializer(catalog),
                        new SafeCastleSerializer(catalog),
                        new CastleDiffer(pendingChanges, catalog),
                        new ObeliskDeserializer<PlayerColor>(ObeliskLayout.Flags),
                        new SafeObeliskSerializer<PlayerColor>(ObeliskLayout.Flags),
                        new ObeliskDiffer<PlayerColor>(PlayerColor.All, pendingChanges.ForObelisks),
                        new RecordKit<MapObjectChunk, MineModel>(new MineDeserializer(catalog), new SafeMineSerializer(), new MineDiffer()),
                        new RecordKit<MapObjectChunk, ShrineModel>(new ShrineDeserializer(catalog), new SafeShrineSerializer(), new ShrineDiffer()),
                        new RecordKit<MapObjectChunk, SphinxModel>(new SphinxDeserializer(catalog), new SafeSphinxSerializer(), new SphinxDiffer()),
                        new RecordKit<MapObjectChunk, TurnModel>(new TurnDeserializer(), new SafeTurnSerializer(), new TurnDiffer()),
                        new RecordKit<MapObjectChunk, BuiltTodayModel>(new BuiltTodayDeserializer(), new SafeBuiltTodaySerializer(), new BuiltTodayDiffer()),
                        new PlayerListsReconciler(),
                        bytesPatcher);
        }

        /// <summary>
        /// Gets the catalogs of the versions of the game.
        /// </summary>
        public GameCatalogs Catalogs { get; }

        /// <summary>
        /// Gets the game module of Heroes of Might and Magic II.
        /// </summary>
        public IGameModule<Homm2EditorViewModel> Module { get; }

        /// <summary>
        /// Gets the service that opens, edits and saves a savegame.
        /// </summary>
        public ISavegameService<Game> GameService { get; }

        /// <summary>
        /// Gets what lists the regions of a savegame for the hex editor.
        /// </summary>
        public ISaveRegionProvider SaveRegionProvider { get; }

        /// <summary>
        /// Gets what makes the targets of the hex editor.
        /// </summary>
        public IByteEditTargetFactory ByteEditTargets { get; }

        /// <summary>
        /// Gets what works out the most spell points of a hero.
        /// </summary>
        public IMaxSpellPointsCalculator<SecondarySkill, SecondarySkills> MaxSpellPointsCalculator { get; }

        /// <summary>
        /// Makes the editor of Heroes of Might and Magic II, with nothing loaded.
        /// </summary>
        /// <returns>The view model of the editor.</returns>
        public Homm2EditorViewModel CreateEditor() => new(this.GameService, this.SaveRegionProvider, this.ByteEditTargets, new NavigationBuilder(this.banishingText), this.banishingText, this.MaxSpellPointsCalculator);
    }
}
