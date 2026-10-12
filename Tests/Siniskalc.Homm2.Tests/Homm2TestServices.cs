namespace Siniskalc.Homm2.Tests
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// The real components of the HoMM2 data layer, built the way the application builds them, for the tests that load savegames.
    /// </summary>
    internal static class Homm2TestServices
    {
        public static IHomm2SaveLayoutLocator Locator { get; } = new Homm2SaveLayoutLocator();

        public static IHomm2HeroDeserializer HeroDeserializer { get; } = new Homm2HeroDeserializer();

        public static IHomm2HeroSerializer HeroSerializer { get; } = new SafeHomm2HeroSerializer();

        public static IHomm2PlayerDeserializer PlayerDeserializer { get; } = new Homm2PlayerDeserializer();

        public static IHomm2PlayerSerializer PlayerSerializer { get; } = new SafeHomm2PlayerSerializer();

        public static IHomm2CastleDeserializer CastleDeserializer { get; } = new Homm2CastleDeserializer();

        public static IHomm2CastleSerializer CastleSerializer { get; } = new SafeHomm2CastleSerializer();

        public static IHomm2SavegameParser Parser { get; } = new Homm2SavegameParser(
            Locator,
            new Homm2BytesLoader(),
            new Homm2HeaderReader(),
            new Homm2SaveDocumentFactory(
                HeroDeserializer,
                HeroSerializer,
                new Homm2HeroDiffer(),
                PlayerDeserializer,
                PlayerSerializer,
                new Homm2PlayerDiffer(),
                CastleDeserializer,
                CastleSerializer,
                new Homm2CastleDiffer(),
                new SavefileBytesPatcher()));

        /// <summary>
        /// Loads the savegame at <paramref name="path"/>.
        /// </summary>
        public static Homm2Game Load(string path) => Parser.Parse(File.ReadAllBytes(path), path);

        /// <summary>
        /// Loads the save of Broken Alliance whose number is <paramref name="number"/>.
        /// </summary>
        public static Homm2Game LoadBrokenAlliance(int number) => Load(Homm2Saves.BrokenAllianceSave(number));
    }
}
