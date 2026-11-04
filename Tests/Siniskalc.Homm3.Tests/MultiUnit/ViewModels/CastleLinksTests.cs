using System.Text;
using Moq;
using Siniskalc.Core.Data;
using Siniskalc.Heroes.Data;
using Siniskalc.Heroes.Models;
using Siniskalc.Heroes.Wpf.ViewModels;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Tests.Models;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.MultiUnit.ViewModels
{
    /// <summary>
    /// The castles a player owns as the player page and the overview list them: the links to the castles, the heroes inside them, the
    /// pills of the Capitol and the Grail structure, and the settlement and fort of each. The document is made by hand: real hero
    /// records and collections, castle records that a fake reader turns into the castles of the test.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class CastleLinksTests
    {
        private readonly Dictionary<string, CastleModel> castles = [];
        private readonly List<int> openedCastles = [];
        private readonly List<int> openedHeroes = [];

        [SetUp]
        public void StartWithNoCastle()
        {
            this.castles.Clear();
            this.openedCastles.Clear();
            this.openedHeroes.Clear();
        }

        private static HeroChunk Hero(string name, byte id)
        {
            var offsets = new HeroOffsets(GameVersion.ShadowOfDeath);
            var bytes = new byte[1200];
            bytes[offsets[HeroOffsetName.HeroId]] = id;
            Encoding.Latin1.GetBytes(name).CopyTo(bytes, offsets[HeroOffsetName.Name]);
            return new HeroChunk(name, bytes, 100);
        }

        private static CastleModel Castle(string name, PlayerColor owner, Faction? faction = null) =>
            CastleModels.Village(faction ?? TestCatalog.For(GameVersion.ShadowOfDeath).Factions.ById("Castle")) with { Name = name, Owner = owner };

        private IReadOnlyList<CastleLinkViewModel> OwnedBy(PlayerColor owner, params HeroChunk[] heroes)
        {
            var deserializer = new Mock<ICastleDeserializer>();
            deserializer.Setup(d => d.Deserialize(It.IsAny<CastleChunk>())).Returns<CastleChunk>(c => this.castles[c.Name]);
            var factory = new SaveDocumentFactory(
                new VersionSpecificEntitiesProviderFactory(TestCatalog.Catalogs),
                Mock.Of<IDiffer<HeroModel>>(),
                _ => deserializer.Object,
                Mock.Of<ISerializer<CastleChunk, CastleModel>>(),
                Mock.Of<ICastleDiffer>(),
                Mock.Of<IPlayerDeserializer>(),
                Mock.Of<ISerializer<PlayerChunk, PlayerModel>>(),
                Mock.Of<IDeserializer<PlayerChunk, PlayerLists>>(),
                Mock.Of<ISerializer<PlayerChunk, PlayerLists>>(),
                Mock.Of<IPlayerDiffer>(),
                Mock.Of<IDeserializer<ObeliskChunk, ObeliskVisits<PlayerColor>>>(),
                Mock.Of<ISerializer<ObeliskChunk, ObeliskVisits<PlayerColor>>>(),
                Mock.Of<IDiffer<ObeliskVisits<PlayerColor>>>(),
                _ => Kit<MineModel>(),
                _ => Kit<DwellingModel>(),
                _ => Kit<MerchantStockModel>(),
                _ => Kit<MerchantStockModel>(),
                Kit<HeroOwnersModel>(),
                Kit<TurnModel>(),
                Mock.Of<IDocumentReconciler>(),
                Mock.Of<ISavefileBytesPatcher>(),
                Mock.Of<ICreatureBankLoader>(l => l.Load(It.IsAny<byte[]>(), It.IsAny<GameVersion>(), It.IsAny<IReadOnlyList<HeroChunk>>(), It.IsAny<int>()) == new List<CreatureBankChunk>()),
                _ => new RecordKit<CreatureBankChunk, CreatureBankModel>(Mock.Of<IDeserializer<CreatureBankChunk, CreatureBankModel>>(), Mock.Of<ISerializer<CreatureBankChunk, CreatureBankModel>>(), Mock.Of<IDiffer<CreatureBankModel>>()));
            var chunks = this.castles.Keys.Select((name, i) => new CastleChunk(name, new byte[10], 1000 + (i * 100))).ToList();

            var document = factory.Create([], heroes, new HeroOffsets(GameVersion.ShadowOfDeath), null, chunks);

            return CastleLinks.OwnedBy(document, owner, this.openedCastles.Add, this.openedHeroes.Add);
        }

        private static RecordKit<RecordChunk, TModel> Kit<TModel>() =>
            new(Mock.Of<IDeserializer<RecordChunk, TModel>>(), Mock.Of<ISerializer<RecordChunk, TModel>>(), Mock.Of<IDiffer<TModel>>());

        [Test]
        public void OnlyTheCastlesOfTheOwnerAreListedByNameAndOpenByTheirPlaceAmongAllCastles()
        {
            this.castles["Zeta"] = Castle("Zeta", PlayerColor.Red);
            this.castles["Alpha"] = Castle("Alpha", PlayerColor.Blue);
            this.castles["Beta"] = Castle("Beta", PlayerColor.Red);

            var links = this.OwnedBy(PlayerColor.Red);
            links[0].OpenCommand.Execute(null);
            links[1].OpenCommand.Execute(null);

            Assert.That(links.Select(l => l.Name), Is.EqualTo(new[] { "Beta", "Zeta" }));
            Assert.That(this.openedCastles, Is.EqualTo(new[] { 2, 0 }));
        }

        [Test]
        public void ARecordWithNoNameIsNoCastleAndIsNotListed()
        {
            this.castles["Used"] = Castle("Used", PlayerColor.Red);
            this.castles["Unused"] = Castle(string.Empty, PlayerColor.Red);

            Assert.That(this.OwnedBy(PlayerColor.Red).Select(l => l.Name), Is.EqualTo(new[] { "Used" }));
        }

        [Test]
        public void ThePlayerOfNoCastleHasNoLinks()
        {
            this.castles["Alpha"] = Castle("Alpha", PlayerColor.Blue);

            Assert.That(this.OwnedBy(PlayerColor.Red), Is.Empty);
        }

        [Test]
        public void ACastleListsItsGarrisonedAndItsVisitingHeroAsLinksThatOpenTheirPageByPlaceAmongTheHeroes()
        {
            this.castles["Inferno"] = Castle("Inferno", PlayerColor.Red, TestCatalog.For(GameVersion.ShadowOfDeath).Factions.ById("Inferno")) with { GarrisonedHeroId = 3, VisitingHeroId = 9 };

            var link = this.OwnedBy(PlayerColor.Red, Hero("Crag Hack", 7), Hero("Nymus", 3), Hero("Tazar", 9)).Single();
            link.Heroes[1].OpenCommand.Execute(null);

            Assert.That(link.Heroes.Select(h => (h.Role, h.Name)), Is.EqualTo(new[] { ("Garrisoned", "Nymus"), ("Visiting", "Tazar") }));
            Assert.That(this.openedHeroes, Is.EqualTo(new[] { 2 }));
        }

        [Test]
        public void ACastleWithNoHeroInsideHasNoHeroLinksAndAHeroNoOneHasTheIdOfIsNamedByItsId()
        {
            this.castles["Empty"] = Castle("Empty", PlayerColor.Red);
            this.castles["Ghost"] = Castle("Ghost", PlayerColor.Red) with { VisitingHeroId = 40 };

            var links = this.OwnedBy(PlayerColor.Red, Hero("Nymus", 3));

            Assert.That(links.Single(l => l.Name == "Empty").Heroes, Is.Empty);
            Assert.That(links.Single(l => l.Name == "Ghost").Heroes.Select(h => h.Name), Is.EqualTo(new[] { "Unknown hero 40" }));
        }

        [Test]
        public void ACastleWithTheGrailStructureIsMarkedAndItsStructureIsNamed()
        {
            var grail = TestCatalog.For(GameVersion.ShadowOfDeath).Buildings.All.First(b => b.IsGrail && b.Faction == TestCatalog.For(GameVersion.ShadowOfDeath).Factions.ById("Conflux"));
            this.castles["Conflux"] = CastleModels.With(Castle("Conflux", PlayerColor.Red, TestCatalog.For(GameVersion.ShadowOfDeath).Factions.ById("Conflux")), grail);
            this.castles["Necropolis"] = Castle("Necropolis", PlayerColor.Red, TestCatalog.For(GameVersion.ShadowOfDeath).Factions.ById("Necropolis"));

            var links = this.OwnedBy(PlayerColor.Red);

            Assert.That(links.Single(l => l.Name == "Conflux").HasGrail, Is.True);
            Assert.That(links.Single(l => l.Name == "Conflux").GrailText, Is.EqualTo(grail.Name));
            Assert.That(links.Single(l => l.Name == "Necropolis").HasGrail, Is.False);
        }

        [Test]
        public void OnlyACastleWithTheCapitolIsTheCapital()
        {
            this.castles["Capital"] = CastleModels.With(Castle("Capital", PlayerColor.Red), TestCatalog.For(GameVersion.ShadowOfDeath).Buildings.ById("Capitol"));
            this.castles["Town"] = CastleModels.With(Castle("Town", PlayerColor.Red), TestCatalog.For(GameVersion.ShadowOfDeath).Buildings.ById("TownHall"));

            var links = this.OwnedBy(PlayerColor.Red);

            Assert.That(links.Single(l => l.Name == "Capital").IsCapital, Is.True);
            Assert.That(links.Single(l => l.Name == "Town").IsCapital, Is.False);
        }

        [TestCase("Village", "Village")]
        [TestCase("Town", "Town")]
        [TestCase("City", "City")]
        [TestCase("Capitol", "Capitol")]
        public void TheStatusNamesTheBiggestHallBuilt(string hall, string expected)
        {
            var building = hall switch
            {
                "Town" => TestCatalog.For(GameVersion.ShadowOfDeath).Buildings.ById("TownHall"),
                "City" => TestCatalog.For(GameVersion.ShadowOfDeath).Buildings.ById("CityHall"),
                "Capitol" => TestCatalog.For(GameVersion.ShadowOfDeath).Buildings.ById("Capitol"),
                _ => TestCatalog.For(GameVersion.ShadowOfDeath).Buildings.ById("VillageHall"),
            };
            this.castles["C"] = CastleModels.With(Castle("C", PlayerColor.Red), building);

            Assert.That(this.OwnedBy(PlayerColor.Red).Single().Status, Does.StartWith(expected));
        }

        [TestCase("none", "Village")]
        [TestCase("Fort", "Village · Fort")]
        [TestCase("Citadel", "Village · Citadel")]
        [TestCase("Castle", "Village · Castle")]
        public void TheStatusNamesTheBiggestFortBuiltAfterTheHall(string fort, string expected)
        {
            var building = fort switch
            {
                "Fort" => TestCatalog.For(GameVersion.ShadowOfDeath).Buildings.ById("Fort"),
                "Citadel" => TestCatalog.For(GameVersion.ShadowOfDeath).Buildings.ById("Citadel"),
                "Castle" => TestCatalog.For(GameVersion.ShadowOfDeath).Buildings.ById("Castle"),
                _ => null,
            };
            this.castles["C"] = building == null ? Castle("C", PlayerColor.Red) : CastleModels.With(Castle("C", PlayerColor.Red), building);

            Assert.That(this.OwnedBy(PlayerColor.Red).Single().Status, Does.StartWith(expected));
        }

        [Test]
        public void TheStatusAlsoTellsTheMageGuildAndTheDwellings()
        {
            var guild = CastleBuilder.SetMageGuildLevel(CastleModels.Village(TestCatalog.For(GameVersion.ShadowOfDeath).Factions.ById("Castle")), 2) with { Name = "C", Owner = PlayerColor.Red };
            this.castles["C"] = CastleModels.With(guild, Building.FromDwellingLevel(TestCatalog.For(GameVersion.ShadowOfDeath).Factions.ById("Castle"), 0, upgraded: false)!);

            Assert.That(this.OwnedBy(PlayerColor.Red).Single().Status, Is.EqualTo("Village · Mage Guild 2 · 1 dwelling"));
        }
    }
}
