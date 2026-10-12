using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// The Factory savefiles hold two neutral Factory castles. "Fac Castle S" never changes. In "Fac Castle G" one building is built
    /// per file, in the order of the file names (the file named after the building has just built it). It starts with the gantry.
    /// </summary>
    [TestFixture]
    internal sealed class FactoryCastleTests
    {
        private static CastleModel Gantry(string save)
        {
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "Factory", save + ".GM1"));
            var castles = game.History.Current.Castles();
            return TestServices.CastleDeserializer.Deserialize(castles[castles.FindByName("Fac Castle G")]);
        }

        [TestCase("factory_08_bank", "factory_07_baseline", "Bank")]
        [TestCase("factory_09_fort", "factory_08_bank", "Fort")]
        [TestCase("factory_10_adobe", "factory_09_fort", "Halfling Adobe")]
        [TestCase("factory_11_foundry", "factory_10_adobe", "Foundry")]
        [TestCase("factory_12_ranch", "factory_11_foundry", "Ranch")]
        [TestCase("factory_13_catacombs", "factory_12_ranch", "Catacombs")]
        [TestCase("factory_14_mage_guild", "factory_13_catacombs", "Mage Guild, level 1")]
        [TestCase("factory_15_blacksmith", "factory_14_mage_guild", "Blacksmith")]
        [TestCase("factory_16_manufactory", "factory_15_blacksmith", "Manufactory")]
        [TestCase("factory_17_watchtower", "factory_16_manufactory", "Watchtower")]
        [TestCase("factory_18_serpentarium", "factory_17_watchtower", "Serpentarium")]
        [TestCase("factory_19_pen", "factory_18_serpentarium", "Pen")]
        [TestCase("factory_20_mana_generator", "factory_19_pen", "Mana Generator")]
        [TestCase("factory_21_marketplace", "factory_20_mana_generator", "Marketplace")]
        [TestCase("factory_22_upg_gantry", "factory_21_marketplace", "Upg. Gantry")]
        [TestCase("factory_26_upg_ranch", "factory_25_upg_manufactory", "Upg. Ranch")]
        [TestCase("factory_27_upg_foundry", "factory_26_upg_ranch", "Upg. Foundry")]
        [TestCase("factory_28_upg_adobe", "factory_27_upg_foundry", "Upg. Halfling Adobe")]
        [TestCase("factory_30_upg_serpentarium", "factory_29_mage_guild_2", "Upg. Serpentarium")]
        public void EachSaveAddsTheBuildingInItsName(string after, string before, string building)
        {
            // The game builds the upgraded pen (bit 19) by itself once the pen and the upgraded ranch both exist.
            var added = Gantry(after).Buildings.Except(Gantry(before).Buildings).Where(b => b.Code != 19).Select(b => b.Name);

            Assert.That(added, Is.EquivalentTo(new[] { building }));
        }

        [Test]
        public void TheUpgradedDwellingsOfTheFactoryAreRecognised()
        {
            var castle = Gantry("factory_22b_upg_catacombs");

            Assert.That(castle.Buildings, Does.Contain(Building.FactoryUpgCatacombs).And.Contain(Building.FactoryUpgGantry));
            Assert.That(castle.UpgradedCreaturesAvailable[4], Is.EqualTo(6));
            Assert.That(castle.CreaturesAvailable[4], Is.EqualTo(0));
        }

        [Test]
        public void TheFactoryHasAManaGeneratorInsteadOfAShipyard()
        {
            Assert.That(Building.ForFaction(Faction.Factory).Where(b => b.Code == 6), Is.EquivalentTo(new[] { Building.FactoryManaGenerator }));
        }

        [Test]
        public void TheMageGuildLevelFollowsTheMageGuildBuilt()
        {
            Assert.That(Gantry("factory_14_mage_guild").MageGuildLevel, Is.EqualTo(1));
            Assert.That(Gantry("factory_29_mage_guild_2").MageGuildLevel, Is.EqualTo(2));
        }
    }
}
