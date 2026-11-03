namespace Siniskalc.Homm2.Tests
{
    using Siniskalc.Heroes.Data;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// The real components of the HoMM2 data layer, built the way the application builds them, for the tests that load savegames.
    /// </summary>
    internal static class TestServices
    {
        public static ISaveLayoutLocator Locator { get; } = new SaveLayoutLocator();

        public static IMaxSpellPointsCalculator<SecondarySkill, SecondarySkills> SpellPoints { get; } = new MaxSpellPointsCalculator();

        public static BanishingTextProvider BanishingText { get; } = new();

        public static MineGuardianTextProvider MineGuardianText { get; } = new();

        public static IHeroDeserializer HeroDeserializer { get; } = new HeroDeserializer(TestCatalog.Standard, new CreatureCodes(TestCatalog.Standard));

        public static ISerializer<HeroChunk, HeroModel> HeroSerializer { get; } = new SafeHeroSerializer(TestCatalog.Standard);

        public static IDeserializer<PlayerChunk, PlayerModel> PlayerDeserializer { get; } = new PlayerDeserializer();

        public static ISerializer<PlayerChunk, PlayerModel> PlayerSerializer { get; } = new SafePlayerSerializer();

        public static IDeserializer<PlayerChunk, PlayerLists> PlayerListsDeserializer { get; } = new PlayerListsDeserializer();

        public static ISerializer<PlayerChunk, PlayerLists> PlayerListsSerializer { get; } = new SafePlayerListsSerializer();

        public static ICastleDeserializer CastleDeserializer { get; } = new CastleDeserializer(TestCatalog.Standard);

        public static ISerializer<CastleChunk, CastleModel> CastleSerializer { get; } = new SafeCastleSerializer(TestCatalog.Standard);

        public static MapObjectDeserializerAndSerializer<MineModel> Mines { get; } = new(new MineDeserializer(TestCatalog.Standard), new SafeMineSerializer());

        public static MapObjectDeserializerAndSerializer<ShrineModel> Shrines { get; } = new(new ShrineDeserializer(TestCatalog.Standard), new SafeShrineSerializer());

        public static MapObjectDeserializerAndSerializer<SphinxModel> Sphinxes { get; } = new(new SphinxDeserializer(TestCatalog.Standard), new SafeSphinxSerializer());

        private static readonly Homm2PendingChangesFactory PendingChanges = new();

        public static ISavegameParser<Game> Parser { get; } = new SavegameParser(
            Locator,
            new BytesLoader(),
            new HeaderReader(),
            new SaveDocumentFactory(
                HeroDeserializer,
                HeroSerializer,
                new HeroDiffer(PendingChanges, TestCatalog.Standard),
                PlayerDeserializer,
                PlayerSerializer,
                PlayerListsDeserializer,
                PlayerListsSerializer,
                new PlayerDiffer(PendingChanges),
                CastleDeserializer,
                CastleSerializer,
                new CastleDiffer(PendingChanges, TestCatalog.Standard),
                new ObeliskDeserializer<PlayerColor>(ObeliskLayout.Flags),
                new SafeObeliskSerializer<PlayerColor>(ObeliskLayout.Flags),
                new ObeliskDiffer<PlayerColor>(PlayerColor.All, PendingChanges.ForObelisks),
                new RecordKit<MapObjectChunk, MineModel>(new MineDeserializer(TestCatalog.Standard), new SafeMineSerializer(), new MineDiffer()),
                new RecordKit<MapObjectChunk, ShrineModel>(new ShrineDeserializer(TestCatalog.Standard), new SafeShrineSerializer(), new ShrineDiffer()),
                new RecordKit<MapObjectChunk, SphinxModel>(new SphinxDeserializer(TestCatalog.Standard), new SafeSphinxSerializer(), new SphinxDiffer()),
                new RecordKit<MapObjectChunk, TurnModel>(new TurnDeserializer(), new SafeTurnSerializer(), new TurnDiffer()),
                new RecordKit<MapObjectChunk, BuiltTodayModel>(new BuiltTodayDeserializer(), new SafeBuiltTodaySerializer(), new BuiltTodayDiffer()),
                new PlayerListsReconciler(),
                new SavefileBytesPatcher()),
            new MapPlacesReader(TestCatalog.Catalogs),
            new MapObjectsReader(TestCatalog.Catalogs), TestCatalog.Catalogs);

        /// <summary>
        /// Loads the savegame at <paramref name="path"/>.
        /// </summary>
        public static Game Load(string path) => Parser.Parse(File.ReadAllBytes(path), path);

        /// <summary>
        /// Loads the save of Broken Alliance whose number is <paramref name="number"/>.
        /// </summary>
        public static Game LoadBrokenAlliance(int number) => Load(Saves.BrokenAllianceSave(number));

        /// <summary>
        /// Loads the save of the folder <paramref name="folder"/> of TestData/Homm2 with this name (without the extension).
        /// </summary>
        public static Game LoadNamed(string folder, string name) => Load(Saves.Named(folder, name));

        /// <summary>
        /// Loads the save of Threedom whose number is <paramref name="number"/>.
        /// </summary>
        public static Game LoadThreedom(int number) => Load(Saves.ThreedomSave(number));

        /// <summary>
        /// Loads the save of MarkerFacing whose number is <paramref name="number"/>.
        /// </summary>
        public static Game LoadMarkerFacing(int number) => Load(Saves.MarkerFacingSave(number));
    }
}
