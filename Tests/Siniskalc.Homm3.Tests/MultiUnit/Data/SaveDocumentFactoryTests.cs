using Moq;
using Siniskalc.Core.Data;
using Siniskalc.Core.Models;
using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Data;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.MultiUnit.Data
{
    /// <summary>
    /// What the factory puts into a document: a collection for each kind of record, the hero reader and writer made for the version of the
    /// offsets, and the name of a hero given to the differ of the players. The collaborators are fakes; the collections are real.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class SaveDocumentFactoryTests
    {
        private Mock<IPlayerDiffer> playerDiffer = null!;
        private Mock<IPlayerDeserializer> playerDeserializer = null!;
        private SaveDocumentFactory factory = null!;

        [SetUp]
        public void SetUp()
        {
            this.playerDiffer = new Mock<IPlayerDiffer>();
            this.playerDiffer
                .Setup(d => d.Diff(It.IsAny<int>(), It.IsAny<PlayerModel>(), It.IsAny<PlayerModel>(), It.IsAny<Func<int, string>>()))
                .Returns([]);
            this.playerDeserializer = new Mock<IPlayerDeserializer>();
            this.playerDeserializer
                .Setup(d => d.Deserialize(It.IsAny<PlayerChunk>()))
                .Returns(new PlayerModel(PlayerColor.Red, ResourceSet.None, new TavernHeroes(0, 1), Numbers.Of(), 0, 0));

            this.factory = new SaveDocumentFactory(
                new VersionSpecificEntitiesProviderFactory(version => new GrantedSpellsResolver(version)),
                Mock.Of<IDiffer<HeroModel>>(),
                Mock.Of<ICastleDeserializer>(),
                Mock.Of<ISerializer<CastleChunk, CastleModel>>(),
                Mock.Of<ICastleDiffer>(),
                this.playerDeserializer.Object,
                Mock.Of<ISerializer<PlayerChunk, PlayerModel>>(),
                this.playerDiffer.Object,
                Mock.Of<IDeserializer<ObeliskChunk, ObeliskVisits<PlayerColor>>>(),
                Mock.Of<ISerializer<ObeliskChunk, ObeliskVisits<PlayerColor>>>(),
                Mock.Of<IDiffer<ObeliskVisits<PlayerColor>>>(),
                new RecordKit<RecordChunk, MineModel>(Mock.Of<IDeserializer<RecordChunk, MineModel>>(), Mock.Of<ISerializer<RecordChunk, MineModel>>(), Mock.Of<IDiffer<MineModel>>()),
                new RecordKit<RecordChunk, DwellingModel>(Mock.Of<IDeserializer<RecordChunk, DwellingModel>>(), Mock.Of<ISerializer<RecordChunk, DwellingModel>>(), Mock.Of<IDiffer<DwellingModel>>()),
                new RecordKit<RecordChunk, MerchantStockModel>(Mock.Of<IDeserializer<RecordChunk, MerchantStockModel>>(), Mock.Of<ISerializer<RecordChunk, MerchantStockModel>>(), Mock.Of<IDiffer<MerchantStockModel>>()),
                new RecordKit<RecordChunk, HeroOwnersModel>(Mock.Of<IDeserializer<RecordChunk, HeroOwnersModel>>(), Mock.Of<ISerializer<RecordChunk, HeroOwnersModel>>(), Mock.Of<IDiffer<HeroOwnersModel>>()),
                new RecordKit<RecordChunk, TurnModel>(Mock.Of<IDeserializer<RecordChunk, TurnModel>>(), Mock.Of<ISerializer<RecordChunk, TurnModel>>(), Mock.Of<IDiffer<TurnModel>>()),
                Mock.Of<ISavefileBytesPatcher>());
        }

        private static HeroChunk Hero(string name, byte id)
        {
            var offsets = new HeroOffsets(GameVersion.ShadowOfDeath);
            var bytes = new byte[1200];
            bytes[offsets[HeroOffsetName.HeroId]] = id;
            System.Text.Encoding.Latin1.GetBytes(name).CopyTo(bytes, offsets[HeroOffsetName.Name]);
            return new HeroChunk(name, bytes, 100);
        }

        private static PlayerChunk Player(PlayerColor color, int start) =>
            new(color, new byte[PlayerLayout.ShadowOfDeath.Stride], start, PlayerLayout.ShadowOfDeath);

        [Test]
        public void ADocumentMadeFromNothingButBytesAndOffsetsHasAnEmptyCollectionOfEveryKind()
        {
            var document = this.factory.Create([1, 2, 3], [], new HeroOffsets(GameVersion.ShadowOfDeath));

            Assert.That(document.Heroes(), Is.Empty);
            Assert.That(document.Players(), Is.Empty);
            Assert.That(document.Castles(), Is.Empty);
            Assert.That(document.Obelisks(), Is.Empty);
            Assert.That(document.Mines(), Is.Empty);
            Assert.That(document.Dwellings(), Is.Empty);
            Assert.That(document.Merchants(), Is.Empty);
            Assert.That(document.HeroOwners(), Is.Empty);
        }

        [Test]
        public void TheChunksGivenForEachKindAreTheOnesInTheCollectionOfThatKind()
        {
            var hero = Hero("Orrin", 0);
            var players = new[] { Player(PlayerColor.Red, 10), Player(PlayerColor.Blue, 200) };
            var mine = new RecordChunk("Mine", new byte[62], 500, 0);
            var dwelling = new RecordChunk("Dwelling", new byte[150], 800, 0);
            var merchant = new RecordChunk("Merchant", new byte[20], 900, 0);
            var owners = new RecordChunk("Owners", new byte[20], 950, 0);

            var document = this.factory.Create([], [hero], new HeroOffsets(GameVersion.ShadowOfDeath), players, null, null, [mine], [dwelling], [merchant], [owners]);

            Assert.That(document.Heroes().Single(), Is.SameAs(hero));
            Assert.That(document.Players(), Is.EqualTo(players));
            Assert.That(document.Mines().Single(), Is.SameAs(mine));
            Assert.That(document.Dwellings().Single(), Is.SameAs(dwelling));
            Assert.That(document.Merchants().Single(), Is.SameAs(merchant));
            Assert.That(document.HeroOwners().Single(), Is.SameAs(owners));
        }

        [TestCase(GameVersion.ShadowOfDeath)]
        [TestCase(GameVersion.HornOfTheAbyss)]
        public void TheGameVersionOfTheDocumentIsTheOneOfTheOffsetsGiven(GameVersion version)
        {
            var document = this.factory.Create([], [], new HeroOffsets(version));

            Assert.That(document.GameVersion(), Is.EqualTo(version));
        }

        [Test]
        public void TheDocumentKeepsTheBytesItWasGivenAndTheSameFactoryMakesIndependentDocuments()
        {
            var first = this.factory.Create([1, 2, 3], [], new HeroOffsets(GameVersion.ShadowOfDeath));
            var second = this.factory.Create([9], [Hero("Orrin", 0)], new HeroOffsets(GameVersion.HornOfTheAbyss));

            Assert.That(first, Is.Not.SameAs(second));
            Assert.That(first.Heroes(), Is.Empty);
            Assert.That(first.GameVersion(), Is.EqualTo(GameVersion.ShadowOfDeath));
            Assert.That(second.Heroes(), Has.Count.EqualTo(1));
        }

        [Test]
        public void TheHeroesAreReadWithTheSkillsOfTheVersionOfTheOffsets()
        {
            var offsets = new HeroOffsets(GameVersion.HornOfTheAbyss);
            var bytes = new byte[1200];
            bytes[offsets[HeroOffsetName.Skills] + SecondarySkill.Interference.Code] = SkillLevel.Intermediate.Code;
            var document = this.factory.Create([], [new HeroChunk("Orrin", bytes, 100)], offsets);

            var skills = document.Heroes().GetModel(0).SecondarySkills;

            Assert.That(skills.GetSkillLevel(SecondarySkill.Interference), Is.EqualTo(SkillLevel.Intermediate));
        }

        private Func<int, string> HeroNameGivenToThePlayerDiffer(params HeroChunk[] heroes)
        {
            Func<int, string>? heroName = null;
            this.playerDiffer
                .Setup(d => d.Diff(It.IsAny<int>(), It.IsAny<PlayerModel>(), It.IsAny<PlayerModel>(), It.IsAny<Func<int, string>>()))
                .Callback<int, PlayerModel, PlayerModel, Func<int, string>>((_, _, _, name) => heroName = name)
                .Returns([]);
            var document = this.factory.Create([], heroes, new HeroOffsets(GameVersion.ShadowOfDeath), [Player(PlayerColor.Red, 10)]);
            var original = document.Players()[0];
            var edited = original.WithBytes(original.ToArray().Select((b, i) => i == 5 ? (byte)1 : b).ToArray());

            document.WithPlayer(0, edited).GetPendingChanges(document);

            Assert.That(heroName, Is.Not.Null, "the differ of the players was not called");
            return heroName!;
        }

        [Test]
        public void ThePlayerDifferIsGivenTheNameOfAHeroByItsId()
        {
            var heroName = this.HeroNameGivenToThePlayerDiffer(Hero("Orrin", 0), Hero("Valeska", 1));

            Assert.That(heroName(1), Is.EqualTo("Valeska"));
            Assert.That(heroName(0), Is.EqualTo("Orrin"));
        }

        [Test]
        public void ThePlayerDifferIsGivenHeroAndTheNumberWhenNoHeroHasThatId()
        {
            var heroName = this.HeroNameGivenToThePlayerDiffer(Hero("Orrin", 0));

            Assert.That(heroName(7), Is.EqualTo("Hero 7"));
        }
    }
}
