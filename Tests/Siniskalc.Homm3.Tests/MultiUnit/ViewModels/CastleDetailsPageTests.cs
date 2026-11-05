using Moq;
using Siniskalc.Core.Models;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Tests.Models;
using Siniskalc.Homm3.Tests.ViewModels;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.MultiUnit.ViewModels
{
    /// <summary>
    /// What the castle page shows about the state of a castle, over a castle session with no savefile: the buildings the map disables
    /// or an upgrade replaced, the creatures waiting in the dwellings, the Mage Guild and the shipyard. The castles are Cove towns.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class CastleDetailsPageTests
    {
        private static CastleViewModel Page(CastleModel castle) =>
            new(new FakeCastleSession(castle), new Mock<IByteEditTargetFactory>().Object, new CastleBuildMessageProvider(), new EffectLineProvider(), _ => { });

        private static BuildingRowViewModel Row(CastleViewModel page, string building) =>
            page.BuildingGroups.SelectMany(g => g.Rows).Single(r => r.Name == building);

        /// <summary>A Cove town with its village hall, a built Shack and an upgraded Nymph Waterfall that replaced the Nymph Waterfall.</summary>
        private static CastleModel LakenshireWithAnUpgradedWaterfall()
        {
            var built = CastleModels.With(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Cove")), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("CoveNymphWaterfall"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("CoveUpgNymphWaterfall"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("CoveShack"));
            return built with
            {
                CreaturesAvailable = Numbers.Of(16, 9, 0, 0, 0, 0, 0),
                VisibleBuildings = built.VisibleBuildings.Where(b => b != TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("CoveNymphWaterfall")).ToHashSet(),
            };
        }

        [Test]
        public void BuildingsTheMapDisablesAreMarked()
        {
            var cove = CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Cove"));
            using var page = Page(cove with { EnabledBuildings = cove.EnabledBuildings.Where(b => b != TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Tavern")).ToHashSet() });

            Assert.That(Row(page, "Tavern").IsDisabled, Is.True);
            Assert.That(Row(page, "Tavern").Note, Is.EqualTo("Not allowed by the map"));
            Assert.That(Row(page, "Fort").IsDisabled, Is.False);
            Assert.That(page.BuildingsSummary, Does.Contain(" 1 not allowed"));
        }

        [Test]
        public void WhenTheMapAllowsEverythingNoBuildingIsMarkedAndNoneIsCountedAsNotAllowed()
        {
            using var page = Page(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Cove")));

            Assert.That(page.BuildingGroups.SelectMany(g => g.Rows).Where(r => r.IsDisabled), Is.Empty);
            Assert.That(page.BuildingsSummary, Does.EndWith(" 0 not allowed"));
        }

        [Test]
        public void ABuildingAnUpgradeReplacedIsMarkedAsReplaced()
        {
            using var page = Page(LakenshireWithAnUpgradedWaterfall());

            Assert.That(Row(page, "Nymph Waterfall").IsReplaced, Is.True);
            Assert.That(Row(page, "Upg. Nymph Waterfall").IsReplaced, Is.False);
        }

        [Test]
        public void TheDwellingsShowTheUpgradeAndTheCreaturesWaiting()
        {
            using var page = Page(LakenshireWithAnUpgradedWaterfall());

            Assert.That((page.Dwellings[0].Dwelling, page.Dwellings[0].State, page.Dwellings[0].Creatures), Is.EqualTo(("Upg. Nymph Waterfall", "Upgraded", "16")));
            Assert.That((page.Dwellings[1].Dwelling, page.Dwellings[1].State), Is.EqualTo(("Shack", "Built")));
            Assert.That(page.Dwellings[2].State, Is.EqualTo("Not built"));
            Assert.That(page.Dwellings, Has.Count.EqualTo(7));
            Assert.That(page.DwellingsSummary, Is.EqualTo("25 Total"));
        }

        [Test]
        public void ADwellingTheMapDoesNotAllowSaysSo()
        {
            var cove = LakenshireWithAnUpgradedWaterfall();
            using var page = Page(cove with { EnabledBuildings = cove.EnabledBuildings.Where(b => b != TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("CoveFrigate")).ToHashSet() });

            Assert.That(page.Dwellings[2].State, Is.EqualTo("Not allowed by the map"));
        }

        [Test]
        public void TheDwellingsShowTheBaseWeeklyGrowthOfTheirCreatures()
        {
            using var page = Page(LakenshireWithAnUpgradedWaterfall());

            Assert.That(page.Dwellings[0].WeeklyGrowthText, Is.EqualTo("+16 a week"), "Nymph");
            Assert.That(page.Dwellings[1].WeeklyGrowthText, Is.EqualTo("+9 a week"), "Crew Mate");
            Assert.That(page.Dwellings[2].WeeklyGrowthText, Is.Empty, "not built");
        }

        [Test]
        public void TheMageGuildShowsTheSpellsOfEachBuiltLevel()
        {
            var guild = CastleBuilder.SetMageGuildLevel(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Cove")), 4) with { MageGuildSpellCounts = Numbers.Of(5, 4, 3, 2, 0) };
            using var page = Page(guild);

            Assert.That(page.MageGuildSummary, Is.EqualTo("Level 4 of 4"));
            Assert.That(page.MageGuildLevels.Select(l => l.SpellsText), Is.EqualTo(new[] { "5 spells", "4 spells", "3 spells", "2 spells" }));
        }

        [Test]
        public void WithoutAMageGuildEveryLevelSaysNotBuilt()
        {
            using var page = Page(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Cove")));

            Assert.That(page.MageGuildSummary, Is.EqualTo("No Mage Guild, up to level 4"));
            Assert.That(page.MageGuildLevels.Select(l => l.SpellsText), Has.All.EqualTo("Not built"));
        }

        [Test]
        public void TheCastleSaysWhereItsBoatsAppear()
        {
            using var page = Page(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Cove")) with { ShipyardPosition = new MapPosition(55, 16) });

            Assert.That(page.ShipyardText, Is.EqualTo("Boats appear at X 55, Y 16"));
        }

        [Test]
        public void ACastleOnNoCoastSaysItIsInland()
        {
            using var page = Page(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Cove")));

            Assert.That(page.ShipyardText, Is.EqualTo("Not on the coast, so no boats"));
        }

        [TestCaseSource(nameof(Factions))]
        public void TheUpgradedGrowthBuildingsAreNotListedBecauseTheyAreNoBuildingsOfTheGame(Faction faction)
        {
            var everything = CastleModels.With(CastleModels.Village(faction), Building.ForFaction(faction).ToArray());
            using var page = Page(everything);

            var special = page.BuildingGroups.Single(g => g.Title == "Special buildings").Rows.Select(r => r.Name).ToList();

            Assert.That(special, Has.None.StartsWith("Upg."), faction.Name);
        }

        public static IEnumerable<Faction> Factions => TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.All.Where(f => f != TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Neutral") && f != TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Factory"));

        [Test]
        public void TheUpgradedGantryOfTheFactoryIsARealBuildingAndIsListed()
        {
            var everything = CastleModels.With(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Factory")), Building.ForFaction(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Factory")).ToArray());
            using var page = Page(everything);

            Assert.That(page.BuildingGroups.Single(g => g.Title == "Special buildings").Rows.Select(r => r.Name), Does.Contain("Upg. Gantry"));
        }
    }
}
