namespace Siniskalc.Homm2.Tests
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// The real components of the HoMM2 data layer, built the way the application builds them, for the tests that load savegames.
    /// </summary>
    internal static class TestServices
    {
        public static ISaveLayoutLocator Locator { get; } = new SaveLayoutLocator();

        public static IHeroDeserializer HeroDeserializer { get; } = new HeroDeserializer();

        public static IHeroSerializer HeroSerializer { get; } = new SafeHeroSerializer();

        public static IPlayerDeserializer PlayerDeserializer { get; } = new PlayerDeserializer();

        public static IPlayerSerializer PlayerSerializer { get; } = new SafePlayerSerializer();

        public static ICastleDeserializer CastleDeserializer { get; } = new CastleDeserializer();

        public static ICastleSerializer CastleSerializer { get; } = new SafeCastleSerializer();

        public static ISavegameParser Parser { get; } = new SavegameParser(
            Locator,
            new BytesLoader(),
            new HeaderReader(),
            new SaveDocumentFactory(
                HeroDeserializer,
                HeroSerializer,
                new HeroDiffer(),
                PlayerDeserializer,
                PlayerSerializer,
                new PlayerDiffer(),
                CastleDeserializer,
                CastleSerializer,
                new CastleDiffer(),
                new ObeliskDeserializer(),
                new SafeObeliskSerializer(),
                new ObeliskDiffer(),
                new SavefileBytesPatcher()),
            new MapPlacesReader());

        /// <summary>
        /// Loads the savegame at <paramref name="path"/>.
        /// </summary>
        public static Game Load(string path) => Parser.Parse(File.ReadAllBytes(path), path);

        /// <summary>
        /// Loads the save of Broken Alliance whose number is <paramref name="number"/>.
        /// </summary>
        public static Game LoadBrokenAlliance(int number) => Load(Saves.BrokenAllianceSave(number));

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
