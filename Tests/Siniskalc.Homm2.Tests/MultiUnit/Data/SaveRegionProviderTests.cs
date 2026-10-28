namespace Siniskalc.Homm2.Tests.MultiUnit.Data
{
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The regions the hex editor shows for a savegame: the header, then a region for each player, hero and named castle, in the order of
    /// the bytes. The collections are real; what reads and writes their chunks is a fake, because the regions only need the chunks.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class SaveRegionProviderTests
    {
        private static readonly SaveLayout Layout = new(GameVersion.BaseGame, 4, 4, HeroLayout.BaseRecordSize, 50, 1000, 2000, 3000);

        private static SaveDocument Document(IEnumerable<PlayerChunk> players, IEnumerable<HeroChunk> heroes, IEnumerable<CastleChunk> castles)
        {
            var collections = new Dictionary<ChunkKind, IChunkCollection>
            {
                [Homm2ChunkKinds.Player] = new PlayerChunkCollection(players, Mock.Of<IDeserializer<PlayerChunk, PlayerModel>>(), Mock.Of<ISerializer<PlayerChunk, PlayerModel>>(), Mock.Of<IPlayerDiffer>(), id => $"Hero {id}"),
                [Homm2ChunkKinds.Hero] = new HeroChunkCollection(heroes, Mock.Of<IHeroDeserializer>(), Mock.Of<ISerializer<HeroChunk, HeroModel>>(), Mock.Of<IDiffer<HeroModel>>()),
                [Homm2ChunkKinds.Castle] = new CastleChunkCollection(castles, Mock.Of<ICastleDeserializer>(), Mock.Of<ISerializer<CastleChunk, CastleModel>>(), Mock.Of<IDiffer<CastleModel>>()),
            };
            return new SaveDocument(new byte[4000], collections, new SavefileBytesPatcher());
        }

        private static SaveRegionProvider Provider(SaveLayout? layout)
        {
            var locator = new Mock<ISaveLayoutLocator>();
            locator.Setup(l => l.Locate(It.IsAny<byte[]>())).Returns(layout);
            return new SaveRegionProvider(locator.Object);
        }

        private static IReadOnlyList<ByteRegion> Regions(SaveLayout? layout = null) =>
            Provider(layout ?? Layout).GetRegions(
                Document(
                    [new PlayerChunk("Red", new byte[PlayerLayout.RecordSize], 1000 + PlayerLayout.RecordSize), new PlayerChunk("Blue", new byte[PlayerLayout.RecordSize], 1000)],
                    [new HeroChunk("Maximus", new byte[HeroLayout.BaseRecordSize], 2000), new HeroChunk(string.Empty, new byte[HeroLayout.BaseRecordSize], 2000 + HeroLayout.BaseRecordSize)],
                    [new CastleChunk("Xabran", new byte[CastleLayout.RecordSize], 3000), new CastleChunk(string.Empty, new byte[CastleLayout.RecordSize], 3100)]),
                new byte[4000]);

        [Test]
        public void ASavegameWhoseLayoutIsNotFoundHasNoRegions()
        {
            var regions = Provider(null).GetRegions(Document([], [], []), new byte[4000]);

            Assert.That(regions, Is.Empty);
        }

        [Test]
        public void TheHeaderHasTheMapNameTheDescriptionAndTheDateWithItsThreeParts()
        {
            var regions = Regions();

            var date = regions.Single(r => r.Name == "Date");
            Assert.That(regions.Single(r => r.Name == "Map name"), Has.Property(nameof(ByteRegion.Start)).EqualTo(Layout.MapNameStart).And.Property(nameof(ByteRegion.Length)).EqualTo(HeaderLayout.MapNameLength));
            Assert.That(regions.Single(r => r.Name == "Map description").Length, Is.EqualTo(HeaderLayout.MapDescriptionMaxLength));
            Assert.That(date.Parts!.Select(p => (p.Name, p.Start)), Is.EqualTo(new[] { ("Day", 50), ("Week", 52), ("Month", 54) }));
            Assert.That(date.Length, Is.EqualTo((2 * HeaderLayout.DateStep) + 1));
        }

        [Test]
        public void EachPlayerHeroAndNamedCastleHasARegionOfItsWholeRecordAndAnUnnamedCastleHasNone()
        {
            var regions = Regions();

            Assert.That(regions.Where(r => r.Start >= 1000).Select(r => (r.Name, r.Start, r.Length)), Is.EqualTo(new[]
            {
                ("Blue player", 1000, PlayerLayout.RecordSize),
                ("Red player", 1000 + PlayerLayout.RecordSize, PlayerLayout.RecordSize),
                ("Maximus", 2000, HeroLayout.BaseRecordSize),
                ("", 2000 + HeroLayout.BaseRecordSize, HeroLayout.BaseRecordSize),
                ("Xabran", 3000, CastleLayout.RecordSize),
            }));
        }

        [Test]
        public void TheRegionsAreInTheOrderOfTheBytesWhateverOrderTheChunksCameIn()
        {
            var starts = Regions().Select(r => r.Start).ToList();

            Assert.That(starts, Is.Ordered);
        }

        [Test]
        public void TheFieldsOfARecordAreShiftedToWhereTheRecordIsInTheSavegame()
        {
            var hero = Regions().Single(r => r.Name == "Maximus");

            Assert.That(hero.Parts, Is.Not.Empty);
            Assert.That(hero.Parts!.Select(p => p.Start), Is.All.InRange(2000, 2000 + HeroLayout.BaseRecordSize));
            Assert.That(hero.Parts!.Single(p => p.Name == "Experience").Start, Is.EqualTo(2000 + HeroLayout.Experience));
        }
    }
}
