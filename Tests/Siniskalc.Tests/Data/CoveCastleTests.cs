using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// The cove_inland_* savefiles hold one Cove castle, Lakenshire, of red. The baseline has a tavern, a nymph waterfall and a shack
    /// (and the fort and village hall the castle starts with). Each other file adds one thing to it: a pub, a nest, an upgraded
    /// nymph waterfall or an upgraded shack. In cove_inland_all_cheat every building was built with a cheat code.
    /// </summary>
    [TestFixture]
    internal sealed class CoveCastleTests
    {
        private static CastleModel Lakenshire(string save)
        {
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "Cove", $"cove_inland_{save}.GM1"));
            var castles = game.History.Current.Castles();
            var index = castles.FindByName("Lakenshire");
            Assert.That(index, Is.GreaterThanOrEqualTo(0), save);
            return TestServices.CastleDeserializer.Deserialize(castles[index]);
        }

        private static IReadOnlySet<Building> Current(string save)
        {
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "Cove", $"cove_inland_{save}.GM1"));
            var castles = game.History.Current.Castles();
            return TestServices.CastleDeserializer.DeserializeCurrentBuildings(castles[castles.FindByName("Lakenshire")]);
        }

        [Test]
        public void TheBaselineCastleIsACoveCastleOfRedWithoutAMageGuild()
        {
            var castle = Lakenshire("baseline");

            Assert.That(castle.Faction, Is.EqualTo(Faction.Cove));
            Assert.That(castle.Owner, Is.EqualTo(PlayerColor.Red));
            Assert.That(castle.Position, Is.EqualTo(new Position(15, 12, 0)));
            Assert.That(castle.MageGuildLevel, Is.EqualTo(0));
            Assert.That(castle.BuiltThisTurn, Is.False);
            Assert.That(castle.Buildings, Is.EquivalentTo(new[]
            {
                Building.Tavern, Building.Fort, Building.VillageHall, Building.CoveNymphWaterfall, Building.CoveShack,
            }));
        }

        [Test]
        public void TheCreaturesWaitingInTheBuiltDwellingsAreStored()
        {
            var castle = Lakenshire("baseline");

            Assert.That(castle.CreaturesAvailable, Is.EqualTo(new[] { 16, 9, 0, 0, 0, 0, 0 }));
            Assert.That(castle.UpgradedCreaturesAvailable, Is.All.EqualTo(0));
        }

        [Test]
        public void ThePubIsBitEighteenAndFlagsTheTurnAndHidesTheShackInTheCurrentBuildings()
        {
            var castle = Lakenshire("pub");

            Assert.That(castle.Buildings.Except(Lakenshire("baseline").Buildings), Is.EquivalentTo(new[] { Building.CovePub }));
            Assert.That(castle.BuiltThisTurn, Is.True);
            Assert.That(Current("pub"), Does.Contain(Building.CovePub).And.Not.Contain(Building.CoveShack));
        }

        [Test]
        public void TheNestIsADwellingAndStartsWithItsGrowth()
        {
            var castle = Lakenshire("nest");

            Assert.That(castle.Buildings.Except(Lakenshire("baseline").Buildings), Is.EquivalentTo(new[] { Building.CoveNest }));
            Assert.That(castle.CreaturesAvailable, Is.EqualTo(new[] { 16, 9, 0, 4, 0, 0, 0 }));
        }

        [Test]
        public void UpgradingADwellingMovesItsCreaturesToTheUpgradedList()
        {
            var nymphs = Lakenshire("upg_nymph_waterfall");
            var shack = Lakenshire("upg_shack");

            Assert.That(nymphs.Buildings.Except(Lakenshire("baseline").Buildings), Is.EquivalentTo(new[] { Building.CoveUpgNymphWaterfall }));
            Assert.That(nymphs.CreaturesAvailable, Is.EqualTo(new[] { 0, 9, 0, 0, 0, 0, 0 }));
            Assert.That(nymphs.UpgradedCreaturesAvailable, Is.EqualTo(new[] { 16, 0, 0, 0, 0, 0, 0 }));
            Assert.That(Current("upg_nymph_waterfall"), Does.Contain(Building.CoveUpgNymphWaterfall).And.Not.Contain(Building.CoveNymphWaterfall));

            Assert.That(shack.Buildings.Except(Lakenshire("baseline").Buildings), Is.EquivalentTo(new[] { Building.CoveUpgShack }));
            Assert.That(shack.UpgradedCreaturesAvailable, Is.EqualTo(new[] { 0, 9, 0, 0, 0, 0, 0 }));
        }

        [Test]
        public void EveryBuildingBuiltWithTheCheatIsABuildingOfCove()
        {
            var castle = Lakenshire("all_cheat");

            // The cheat built everything the faction has. Of the special buildings only the Gunpowder Warehouse (21) is inferred.
            // Bits 19 and 25 are the upgraded pub and roost, which the game builds by itself.
            var expected = new[] { 0, 1, 2, 3, 5, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 18, 19, 21, 22, 23, 24, 25, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43 };
            Assert.That(castle.Buildings.Select(b => (int)b.Code), Is.EquivalentTo(expected));
            Assert.That(castle.MageGuildLevel, Is.EqualTo(4));
            Assert.That(castle.UpgradedCreaturesAvailable, Is.EqualTo(new[] { 16, 9, 7, 4, 3, 2, 1 }));
            Assert.That(castle.Buildings, Does.Contain(Building.CoveUpgMaelstrom).And.Contain(Building.MagesGuild4).And.Not.Contain(Building.MagesGuild5));
        }

        [Test]
        public void TheBuildingsOfTheLaterSavesAreRecognised()
        {
            Assert.That(Lakenshire("nix_fort").Buildings, Is.EquivalentTo(new[]
            {
                Building.Tavern, Building.Fort, Building.VillageHall, Building.CoveNymphWaterfall, Building.CoveShack, Building.CoveNest, Building.CoveNixFort,
            }));
            Assert.That(Lakenshire("tower_of_the_seas").Buildings.Except(Lakenshire("nix_fort").Buildings), Is.EquivalentTo(new[] { Building.MagesGuild1, Building.CoveToweroftheSeas }));
            Assert.That(Lakenshire("tower_of_the_seas").MageGuildLevel, Is.EqualTo(1));
            Assert.That(Lakenshire("roost").Buildings.Except(Lakenshire("tower_of_the_seas").Buildings), Is.EquivalentTo(new[] { Building.CoveRoost }));
            Assert.That(Lakenshire("thieves_guild").Buildings.Except(Lakenshire("roost").Buildings), Is.EquivalentTo(new[] { Building.CoveThievesGuild }));
            Assert.That(Lakenshire("blacksmith").Buildings.Except(Lakenshire("thieves_guild").Buildings), Is.EquivalentTo(new[] { Building.Blacksmith }));
            Assert.That(Lakenshire("frigate").Buildings.Except(Lakenshire("blacksmith").Buildings), Is.EquivalentTo(new[] { Building.CoveFrigate }));
        }

        [Test]
        public void TheRoostReplacesTheNestInTheCurrentBuildings()
        {
            Assert.That(Current("roost"), Does.Contain(Building.CoveRoost).And.Not.Contain(Building.CoveNest));
        }

        [Test]
        public void ADwellingStartsWithItsWeeklyGrowthAndDoublesAfterAWeek()
        {
            Assert.That(Lakenshire("nix_fort").CreaturesAvailable, Is.EqualTo(new[] { 16, 9, 0, 4, 0, 2, 0 }));
            Assert.That(Lakenshire("frigate").CreaturesAvailable, Is.EqualTo(new[] { 32, 18, 7, 11, 6, 4, 0 }));
        }

        [Test]
        public void TheMarketplaceAndTheGrottoThatNeedsItAreRecognised()
        {
            Assert.That(Lakenshire("marketplace").Buildings.Except(Lakenshire("nix_fort").Buildings), Is.EquivalentTo(new[] { Building.Marketplace }));
            Assert.That(Lakenshire("grotto").Buildings.Except(Lakenshire("marketplace").Buildings), Is.EquivalentTo(new[] { Building.CoveGrotto }));
            Assert.That(Building.CoveGrotto.Prerequisites, Is.EquivalentTo(new[] { Building.Marketplace }));
        }

        [Test]
        public void TheMaelstromNeedsTheTowerOfTheSeas()
        {
            Assert.That(Lakenshire("maelstrom").Buildings.Except(Lakenshire("without_maelstrom").Buildings), Is.EquivalentTo(new[] { Building.CoveMaelstrom }));
            Assert.That(Lakenshire("without_maelstrom").Buildings, Does.Contain(Building.CoveToweroftheSeas).And.Contain(Building.MagesGuild1));
            Assert.That(Lakenshire("maelstrom").CreaturesAvailable[6], Is.EqualTo(1));
            Assert.That(Building.CoveMaelstrom.Prerequisites, Is.EquivalentTo(new[] { Building.CoveToweroftheSeas }));
        }

        private static CastleModel Named(string save, string name)
        {
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "Cove", $"{save}.GM1"));
            var castles = game.History.Current.Castles();
            var index = castles.FindByName(name);
            Assert.That(index, Is.GreaterThanOrEqualTo(0), name + " in " + string.Join("; ", castles.Select(c => c.Name)));
            return TestServices.CastleDeserializer.Deserialize(castles[index]);
        }

        [Test]
        public void TheCastlesOfTheFactorySaveAreFoundAndAreNeutral()
        {
            var castle = Named("neutral_cove_and_factory_castles", "Cove Castle");
            var disabled = Named("neutral_cove_and_factory_castles", "Cove Castle D");

            Assert.That(castle.Faction, Is.EqualTo(Faction.Cove));
            Assert.That(castle.Owner, Is.Null);
            Assert.That(castle.Buildings, Is.EquivalentTo(new[] { Building.Fort, Building.VillageHall }));
            Assert.That(castle.ShipyardLocation, Is.EqualTo(new Position(55, 16, 0)));
            Assert.That(castle.EnabledBuildings, Does.Contain(Building.Shipyard));
            Assert.That(Named("neutral_cove_and_factory_castles", "Cove Town").ShipyardLocation, Is.Null);
            Assert.That(Named("neutral_cove_and_factory_castles", "Cove Town").EnabledBuildings, Does.Not.Contain(Building.Shipyard));
            Assert.That(disabled.EnabledBuildings, Is.EquivalentTo(new[] { Building.Fort, Building.VillageHall }));
            Assert.That(Named("neutral_cove_and_factory_castles", "Cove Town").EnabledBuildings, Does.Not.Contain(Building.Fort));
        }

        [Test]
        public void TheSerpentariumAndTheGantryAreTheTwoLevelSevenDwellingsOfTheFactory()
        {
            var serpentarium = Named("neutral_cove_and_factory_castles", "Fac Castle S");
            var gantry = Named("neutral_cove_and_factory_castles", "Fac Castle G");

            Assert.That(serpentarium.Faction, Is.EqualTo(Faction.Factory));
            Assert.That(serpentarium.Buildings, Is.EquivalentTo(new[] { Building.VillageHall, Building.FactorySerpentarium }));
            Assert.That(serpentarium.CreaturesAvailable[6], Is.EqualTo(1));
            Assert.That(gantry.Buildings, Is.EquivalentTo(new[] { Building.VillageHall, Building.FactoryGantry }));
        }

        [Test]
        public void TheHeroInTheCastleIsUfretinAndSitsSeventeenStructsAfterOrrin()
        {
            foreach (var file in new[] { "cove_coastal_05_visiting_hero", "cove_coastal_06_garrisoned_hero" })
            {
                var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "Cove", file + ".GM1"));
                var heroes = game.History.Current.Heroes();
                var orrin = heroes.Single(h => h.Name == "Orrin");
                var ufretin = heroes.Single(h => h.Name == "Ufretin");

                Assert.That(ufretin.HeaderIndex - orrin.HeaderIndex, Is.EqualTo(17 * 1160), file);
            }
        }

        [Test]
        public void TheHeroIdIsInTheHeroStruct()
        {
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "Cove", "cove_coastal_05_visiting_hero.GM1"));
            var heroes = game.History.Current.Heroes();

            Assert.That(heroes.Single(h => h.Name == "Ufretin").GetByteAt(HeroOffsets.HeroIdOffset), Is.EqualTo(17));
            Assert.That(heroes.Single(h => h.Name == "Orrin").GetByteAt(HeroOffsets.HeroIdOffset), Is.EqualTo(0));
        }

        [Test]
        public void TheCastlePageLinksToTheHeroInsideIt()
        {
            foreach (var (file, role) in new[] { ("cove_coastal_05_visiting_hero", "Visiting"), ("cove_coastal_06_garrisoned_hero", "Garrisoned") })
            {
                var path = Path.Combine("TestData", "Hota", "Cove", file + ".GM1");
                var main = TestServices.CreateMainViewModel();
                main.LoadGameFromBytes(TestServices.SavefileCodec.Read(path), path);

                main.ShowCastle(main.LoadedGame().History.Current.Castles().FindByName("Cove Castle"));
                var castle = main.CurrentPageOf<Siniskalc.ViewModels.CastleViewModel>();

                Assert.That(castle.Heroes.Select(h => (h.Role, h.Name)), Is.EqualTo(new[] { (role, "Ufretin") }), file);
                castle.Heroes[0].OpenCommand.Execute(null);
                Assert.That(main.SelectedHero().Name, Is.EqualTo("Ufretin"), file);
            }
        }
    }
}
