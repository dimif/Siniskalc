using Siniskalc.Data;
using Siniskalc.Runtime;
using Siniskalc.ViewModels;

namespace Siniskalc.Tests.ViewModels
{
    /// <summary>
    /// Tests what the castle page shows about the state of a castle: the buildings the map disables or an upgrade replaced,
    /// the creatures waiting in the dwellings, the Mage Guild and the shipyard.
    /// </summary>
    [TestFixture]
    internal sealed class CastleDetailsViewModelTests
    {
        private static CastleViewModel Show(string folder, string save, string castle)
        {
            var path = Path.Combine("TestData", "Hota", folder, save + ".GM1");
            var main = TestServices.CreateMainViewModel();
            main.LoadGameFromBytes(TestServices.SavefileCodec.Read(path), path);
            main.ShowCastle(main.LoadedGame().History.Current.Castles().FindByName(castle));
            return main.CurrentPageOf<CastleViewModel>();
        }

        private static BuildingRowViewModel Row(CastleViewModel castle, string building) =>
            castle.BuildingGroups.SelectMany(g => g.Rows).Single(r => r.Name == building);

        [Test]
        public void BuildingsTheMapDisablesAreMarked()
        {
            var castle = Show("Cove", "neutral_cove_and_factory_castles", "Cove Castle D");

            Assert.That(Row(castle, "Tavern").IsDisabled, Is.True);
            Assert.That(Row(castle, "Tavern").Note, Is.EqualTo("Not allowed by the map"));
            Assert.That(Row(castle, "Fort").IsDisabled, Is.False);
            Assert.That(castle.BuildingsSummary, Does.Contain(" not allowed").And.Not.Contain(" 0 not allowed"));
        }

        [Test]
        public void ABuildingAnUpgradeReplacedIsMarkedAsReplaced()
        {
            var castle = Show("Cove", "cove_inland_upg_nymph_waterfall", "Lakenshire");

            Assert.That(Row(castle, "Nymph Waterfall").IsReplaced, Is.True);
            Assert.That(Row(castle, "Upg. Nymph Waterfall").IsReplaced, Is.False);
        }

        [Test]
        public void TheDwellingsShowTheUpgradeAndTheCreaturesWaiting()
        {
            var castle = Show("Cove", "cove_inland_upg_nymph_waterfall", "Lakenshire");

            Assert.That(castle.Dwellings[0].Dwelling, Is.EqualTo("Upg. Nymph Waterfall"));
            Assert.That(castle.Dwellings[0].State, Is.EqualTo("Upgraded"));
            Assert.That(castle.Dwellings[0].Creatures, Is.EqualTo("16"));
            Assert.That(castle.Dwellings[1].Dwelling, Is.EqualTo("Shack"));
            Assert.That(castle.Dwellings[1].State, Is.EqualTo("Built"));
            Assert.That(castle.Dwellings[2].State, Is.EqualTo("Not built"));
            Assert.That(castle.Dwellings, Has.Count.EqualTo(7));
            Assert.That(castle.DwellingsSummary, Is.EqualTo("25 Total"));
        }

        [Test]
        public void TheMageGuildShowsTheSpellsOfEachBuiltLevel()
        {
            var castle = Show("Cove", "cove_inland_all_cheat", "Lakenshire");

            Assert.That(castle.MageGuildSummary, Is.EqualTo("Level 4 of 4"));
            Assert.That(castle.MageGuildLevels.Select(l => l.SpellsText), Is.EqualTo(new[] { "5 spells", "4 spells", "3 spells", "2 spells" }));

            var none = Show("Cove", "cove_inland_baseline", "Lakenshire");
            Assert.That(none.MageGuildSummary, Is.EqualTo("No Mage Guild, up to level 4"));
            Assert.That(none.MageGuildLevels.Select(l => l.SpellsText), Has.All.EqualTo("Not built"));
        }

        [Test]
        public void TheCastleSaysWhereItsBoatsAppearOrThatItIsInland()
        {
            Assert.That(Show("Cove", "neutral_cove_and_factory_castles", "Cove Castle").ShipyardText, Is.EqualTo("Boats appear at X 55, Y 16"));
            Assert.That(Show("Cove", "neutral_cove_and_factory_castles", "Cove Town").ShipyardText, Is.EqualTo("Not on the coast, so no boats"));
        }

        [TestCase("1_Castle")]
        [TestCase("2_Rampart")]
        [TestCase("3_Tower")]
        [TestCase("4_Inferno")]
        [TestCase("5_necropolis")]
        [TestCase("6_Dung")]
        [TestCase("7_Strong")]
        [TestCase("8_Fort")]
        public void TheUpgradedGrowthBuildingsAreNotListedBecauseTheyAreNoBuildingsOfTheGame(string castle)
        {
            var path = Path.Combine("TestData", "Roe", "roe_42_tazar_garrisoned.GM1");
            var main = TestServices.CreateMainViewModel();
            main.LoadGameFromBytes(TestServices.SavefileCodec.Read(path), path);
            main.ShowCastle(main.LoadedGame().History.Current.Castles().FindByName(castle));
            var page = main.CurrentPageOf<CastleViewModel>();

            var special = page.BuildingGroups.Single(g => g.Title == "Special buildings").Rows.Select(r => r.Name).ToList();

            Assert.That(special, Has.None.StartsWith("Upg."), castle);
        }
    }
}
