using Moq;
using Siniskalc.Core.Data;
using Siniskalc.Core.Runtime;
using Siniskalc.Heroes.Data;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Runtime;
using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Runtime
{
    /// <summary>
    /// Tests how a savegame is put together from the bytes: the version is found first, every finder gets what the earlier ones found, and the
    /// game carries the document the factory made. The finders are fakes, so the order and the arguments can be checked without a savefile;
    /// the document handed back is a real one.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SavegameParserTests
    {
        private static readonly byte[] Bytes = [1, 2, 3, 4];

        private Mock<ISavegameVersionDetector> detector = null!;
        private Mock<IHeroBytesLoader> heroes = null!;
        private Mock<ICastleBytesLoader> castles = null!;
        private Mock<IPlayerBytesLoader> players = null!;
        private Mock<IObeliskBytesLoader> obelisks = null!;
        private Mock<IMineBytesLoader> mines = null!;
        private Mock<IDwellingBytesLoader> dwellings = null!;
        private Mock<IMerchantBytesLoader> merchants = null!;
        private Mock<IHeroOwnersBytesLoader> heroOwners = null!;
        private Mock<ITurnBytesLoader> turn = null!;
        private Mock<IMapPlacesLoader> places = null!;
        private Mock<IGameDateLocator> date = null!;
        private Mock<ITownMerchantStockLocator> townMerchant = null!;
        private Mock<IPlayerControlLocator> setup = null!;
        private Mock<IMapHeaderLocator> map = null!;
        private Mock<ISaveDocumentFactory> documents = null!;
        private SaveDocument document = null!;

        [SetUp]
        public void BuildFakesThatFindNothing()
        {
            this.detector = new Mock<ISavegameVersionDetector>();
            this.detector.Setup(d => d.Detect(Bytes)).Returns(GameVersion.ShadowOfDeath);
            this.heroes = new Mock<IHeroBytesLoader>();
            this.heroes.Setup(l => l.LoadHeroChunks(It.IsAny<byte[]>(), It.IsAny<GameVersion>())).Returns([]);
            this.castles = new Mock<ICastleBytesLoader>();
            this.castles.Setup(l => l.LoadCastleChunks(It.IsAny<byte[]>(), It.IsAny<int>())).Returns([]);
            this.players = new Mock<IPlayerBytesLoader>();
            this.players.Setup(l => l.LoadPlayerChunks(It.IsAny<byte[]>(), It.IsAny<GameVersion>(), It.IsAny<IEnumerable<BytesChunk>>(), true)).Returns([]);
            this.obelisks = new Mock<IObeliskBytesLoader>();
            this.obelisks.Setup(l => l.LoadObeliskChunks(It.IsAny<byte[]>(), It.IsAny<IReadOnlyList<PlayerChunk>>())).Returns([]);
            this.mines = new Mock<IMineBytesLoader>();
            this.mines.Setup(l => l.LoadMineChunks(It.IsAny<byte[]>())).Returns([]);
            this.dwellings = new Mock<IDwellingBytesLoader>();
            this.dwellings.Setup(l => l.LoadDwellingChunks(It.IsAny<byte[]>(), It.IsAny<IEnumerable<BytesChunk>>())).Returns([]);
            this.merchants = new Mock<IMerchantBytesLoader>();
            this.merchants.Setup(l => l.LoadMerchantChunks(It.IsAny<byte[]>())).Returns([]);
            this.heroOwners = new Mock<IHeroOwnersBytesLoader>();
            this.heroOwners.Setup(l => l.LoadHeroOwnerChunks(It.IsAny<byte[]>(), It.IsAny<GameVersion>(), It.IsAny<int>())).Returns([]);
            this.turn = new Mock<ITurnBytesLoader>();
            this.turn.Setup(l => l.LoadTurnChunks(It.IsAny<byte[]>(), It.IsAny<GameVersion>())).Returns([]);
            this.places = new Mock<IMapPlacesLoader>();
            this.places.Setup(l => l.Load(It.IsAny<byte[]>(), It.IsAny<EntityCatalog>())).Returns(MapPlaces.None);
            this.date = new Mock<IGameDateLocator>();
            this.townMerchant = new Mock<ITownMerchantStockLocator>();
            this.townMerchant.Setup(l => l.FindOffset(It.IsAny<byte[]>(), It.IsAny<GameVersion>(), It.IsAny<int>())).Returns(-1);
            this.setup = new Mock<IPlayerControlLocator>();
            this.map = new Mock<IMapHeaderLocator>();
            this.document = CastleSamples.Load(CastleSamples.Folder("Bulwark").First()).History.Current;
            this.documents = new Mock<ISaveDocumentFactory>();
            this.documents
                .Setup(f => f.Create(It.IsAny<byte[]>(), It.IsAny<IEnumerable<HeroChunk>>(), It.IsAny<HeroOffsets>(), It.IsAny<IEnumerable<PlayerChunk>?>(), It.IsAny<IEnumerable<CastleChunk>?>(), It.IsAny<IEnumerable<ObeliskChunk>?>(), It.IsAny<IEnumerable<RecordChunk>?>(), It.IsAny<IEnumerable<RecordChunk>?>(), It.IsAny<IEnumerable<RecordChunk>?>(), It.IsAny<IEnumerable<RecordChunk>?>(), It.IsAny<IEnumerable<RecordChunk>?>(), It.IsAny<IEnumerable<PlayerChunk>?>(), It.IsAny<IEnumerable<RecordChunk>?>()))
                .Returns(() => this.document);
        }

        private SavegameParser Parser() => new(
            this.detector.Object, this.heroes.Object, _ => this.castles.Object, this.players.Object, this.obelisks.Object, _ => this.mines.Object, _ => this.dwellings.Object,
            _ => this.merchants.Object, this.heroOwners.Object, this.turn.Object, this.places.Object, this.date.Object, this.townMerchant.Object, this.setup.Object, this.map.Object, this.documents.Object);

        [Test]
        public void ABytesArrayOfNoKnownVersionIsRefusedBeforeAnythingIsLookedFor()
        {
            this.detector.Setup(d => d.Detect(Bytes)).Returns(GameVersion.Unknown);

            Assert.Throws<UnsupportedSavefileException>(() => this.Parser().Parse(Bytes, "x.GM1"));

            this.heroes.VerifyNoOtherCalls();
            this.castles.VerifyNoOtherCalls();
            this.documents.VerifyNoOtherCalls();
        }

        [Test]
        public void TheGameCarriesTheFileTheNameTheBytesTheVersionAndTheDocument()
        {
            var game = this.Parser().Parse(Bytes, "x.GM1", "My game");

            Assert.That((game.FileName, game.Name, game.GameVersion), Is.EqualTo(("x.GM1", "My game", GameVersion.ShadowOfDeath)));
            Assert.That(game.Bytes, Is.SameAs(Bytes));
            Assert.That(game.History.Current, Is.SameAs(this.document));
        }

        [Test]
        public void TheFindersOfTheHeroesAndTheMerchantsAreGivenTheBytesAndTheVersion()
        {
            this.Parser().Parse(Bytes, "x.GM1");

            this.heroes.Verify(l => l.LoadHeroChunks(Bytes, GameVersion.ShadowOfDeath), Times.Once);
            this.merchants.Verify(l => l.LoadMerchantChunks(Bytes), Times.Once);
            this.castles.Verify(l => l.LoadCastleChunks(Bytes, 0), Times.Once);
            this.mines.Verify(l => l.LoadMineChunks(Bytes), Times.Once);
            this.places.Verify(l => l.Load(Bytes, It.IsAny<EntityCatalog>()), Times.Once);
        }

        [Test]
        public void TheCastleSearchStartsAfterTheMapHeaderRatherThanTreatingItsNameAsATown()
        {
            var header = new MapHeaderLocation(95, 18, 115, 40);
            this.map.Setup(l => l.Locate(Bytes, GameVersion.ShadowOfDeath)).Returns(header);

            this.Parser().Parse(Bytes, "x.GM1");

            this.castles.Verify(l => l.LoadCastleChunks(Bytes, header.DifficultyAt + 1), Times.Once);
        }

        [Test]
        public void ThePlayersAreLookedForAroundTheHeroesAndTheCastlesAlreadyFound()
        {
            var hero = new HeroChunk("Orrin", [1, 2, 3], 10);
            var castle = new CastleChunk("Haret", new byte[CastleLayout.FixedLength], 50);
            this.heroes.Setup(l => l.LoadHeroChunks(Bytes, GameVersion.ShadowOfDeath)).Returns([hero]);
            this.castles.Setup(l => l.LoadCastleChunks(Bytes, 0)).Returns([castle]);

            this.Parser().Parse(Bytes, "x.GM1");

            this.players.Verify(l => l.LoadPlayerChunks(Bytes, GameVersion.ShadowOfDeath, It.Is<IEnumerable<BytesChunk>>(o => o.SequenceEqual(new BytesChunk[] { hero, castle })), true), Times.Once);
        }

        [Test]
        public void TheObelisksAreLookedForWithThePlayersAndTheDwellingsAfterTheMinesAndAroundEveryChunkFoundSoFar()
        {
            var hero = new HeroChunk("Orrin", [1, 2, 3], 10);
            var mine = new RecordChunk("Mine", new byte[5], 30, 0);
            var player = PlayerRecords.Blank(PlayerLayout.ShadowOfDeath, PlayerColor.Red, 0);
            var obelisk = new ObeliskChunk("Obelisks", new byte[4], 40);
            this.heroes.Setup(l => l.LoadHeroChunks(Bytes, GameVersion.ShadowOfDeath)).Returns([hero]);
            this.players.Setup(l => l.LoadPlayerChunks(Bytes, GameVersion.ShadowOfDeath, It.IsAny<IEnumerable<BytesChunk>>(), true)).Returns([player]);
            this.obelisks.Setup(l => l.LoadObeliskChunks(Bytes, It.Is<IReadOnlyList<PlayerChunk>>(p => p.Single() == player))).Returns([obelisk]);
            this.mines.Setup(l => l.LoadMineChunks(Bytes)).Returns([mine]);

            this.Parser().Parse(Bytes, "x.GM1");

            this.dwellings.Verify(l => l.LoadDwellingChunks(Bytes, It.Is<IEnumerable<BytesChunk>>(o => o.SequenceEqual(new BytesChunk[] { hero, player, obelisk, mine }))), Times.Once);
        }

        [Test]
        public void TheDateAndTheHeroOwnersAreLookedForFromTheEndOfTheLastPlayerOrFromTheStart()
        {
            this.Parser().Parse(Bytes, "x.GM1");
            this.date.Verify(l => l.Find(Bytes, GameVersion.ShadowOfDeath, 0), Times.Once);
            this.heroOwners.Verify(l => l.LoadHeroOwnerChunks(Bytes, GameVersion.ShadowOfDeath, 0), Times.Once);
            this.townMerchant.Verify(l => l.FindOffset(Bytes, GameVersion.ShadowOfDeath, 0), Times.Once);

            var first = PlayerRecords.Blank(PlayerLayout.ShadowOfDeath, PlayerColor.Red, 0);
            var last = new PlayerChunk(PlayerColor.Blue, first.ToArray(), 5000, PlayerLayout.ShadowOfDeath);
            this.players.Setup(l => l.LoadPlayerChunks(Bytes, GameVersion.ShadowOfDeath, It.IsAny<IEnumerable<BytesChunk>>(), true)).Returns([first, last]);

            this.Parser().Parse(Bytes, "x.GM1");

            this.date.Verify(l => l.Find(Bytes, GameVersion.ShadowOfDeath, last.End), Times.Once);
            this.heroOwners.Verify(l => l.LoadHeroOwnerChunks(Bytes, GameVersion.ShadowOfDeath, last.End), Times.Once);
            this.townMerchant.Verify(l => l.FindOffset(Bytes, GameVersion.ShadowOfDeath, last.End), Times.Once);
        }

        [Test]
        public void TheGameHoldsWhatTheLocatorsFound()
        {
            var found = new GameDate(2, 3, 4);
            this.date.Setup(l => l.Find(Bytes, GameVersion.ShadowOfDeath, 0)).Returns(found);

            var game = this.Parser().Parse(Bytes, "x.GM1");

            Assert.That(game.Date, Is.EqualTo(found));
            Assert.That(game.Metadata.Setup, Is.Null);
            Assert.That(game.Map, Is.Null);
        }

        [Test]
        public void TheDocumentIsMadeFromTheChunksTheFindersReturnedAndTheOffsetsOfTheVersion()
        {
            var hero = new HeroChunk("Orrin", [1, 2, 3], 10);
            var merchant = new RecordChunk("Black market", new byte[28], 70, 0);
            this.heroes.Setup(l => l.LoadHeroChunks(Bytes, GameVersion.ShadowOfDeath)).Returns([hero]);
            this.merchants.Setup(l => l.LoadMerchantChunks(Bytes)).Returns([merchant]);

            this.Parser().Parse(Bytes, "x.GM1");

            this.documents.Verify(
                f => f.Create(
                    Bytes,
                    It.Is<IEnumerable<HeroChunk>>(h => h.Single() == hero),
                    It.Is<HeroOffsets>(o => o.GameVersion == GameVersion.ShadowOfDeath),
                    It.IsAny<IEnumerable<PlayerChunk>?>(),
                    It.IsAny<IEnumerable<CastleChunk>?>(),
                    It.IsAny<IEnumerable<ObeliskChunk>?>(),
                    It.IsAny<IEnumerable<RecordChunk>?>(),
                    It.IsAny<IEnumerable<RecordChunk>?>(),
                    It.Is<IEnumerable<RecordChunk>?>(m => m!.Single() == merchant),
                    It.IsAny<IEnumerable<RecordChunk>?>(),
                    It.IsAny<IEnumerable<RecordChunk>?>(),
                    It.IsAny<IEnumerable<PlayerChunk>?>(),
                    It.Is<IEnumerable<RecordChunk>?>(s => !s!.Any())),
                Times.Once);
        }
    }
}
