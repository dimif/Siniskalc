using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// The Rampart savefiles hold two neutral Rampart castles. "Gladeroot" is a town that never changes. In "Fortune Keep" one building
    /// is built per file, in the order of the file numbers (the first build is the town hall). The files are not named after what was built.
    /// </summary>
    [TestFixture]
    internal sealed class RampartCastleTests
    {
        private static CastleModel FortuneKeep(string save)
        {
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "Rampart", save + ".GM1"));
            var castles = game.History.Current.Castles();
            return TestServices.CastleDeserializer.Deserialize(castles[castles.FindByName("Fortune Keep")]);
        }

        [TestCase("rampart_01_town_hall", "rampart_00_baseline", "Town Hall")]
        [TestCase("rampart_02_citadel", "rampart_01_town_hall", "Citadel")]
        [TestCase("rampart_03_castle", "rampart_02_citadel", "Castle")]
        [TestCase("rampart_04_blacksmith", "rampart_03_castle", "Blacksmith")]
        [TestCase("rampart_05_marketplace", "rampart_04_blacksmith", "Marketplace")]
        [TestCase("rampart_06_resource_silo", "rampart_05_marketplace", "Resource Silo")]
        [TestCase("rampart_07_mage_guild_1", "rampart_06_resource_silo", "Mage Guild, level 1")]
        [TestCase("rampart_08_mystic_pond", "rampart_07_mage_guild_1", "Mystic Pond")]
        [TestCase("rampart_09_city_hall", "rampart_08_mystic_pond", "City Hall")]
        [TestCase("rampart_10_mage_guild_2", "rampart_09_city_hall", "Mage Guild, level 2")]
        [TestCase("rampart_11_mage_guild_3", "rampart_10_mage_guild_2", "Mage Guild, level 3")]
        [TestCase("rampart_12_mage_guild_4", "rampart_11_mage_guild_3", "Mage Guild, level 4")]
        [TestCase("rampart_13_mage_guild_5", "rampart_12_mage_guild_4", "Mage Guild, level 5")]
        [TestCase("rampart_14_capitol", "rampart_13_mage_guild_5", "Capitol")]
        [TestCase("rampart_15_miners_guild", "rampart_14_capitol", "Miners' Guild")]
        [TestCase("rampart_16_treasury", "rampart_15_miners_guild", "Treasury")]
        [TestCase("rampart_17_upg_centaur_stables", "rampart_16_treasury", "Upg. Centaur Stables")]
        [TestCase("rampart_18_upg_dwarf_cottage", "rampart_17_upg_centaur_stables", "Upg. Dwarf Cottage", "Upg. Miners' Guild")]
        [TestCase("rampart_19_homestead", "rampart_18_upg_dwarf_cottage", "Homestead")]
        [TestCase("rampart_20_enchanted_spring", "rampart_19_homestead", "Enchanted Spring")]
        [TestCase("rampart_21_dendroid_arches", "rampart_20_enchanted_spring", "Dendroid Arches")]
        [TestCase("rampart_22_unicorn_glade", "rampart_21_dendroid_arches", "Unicorn Glade")]
        [TestCase("rampart_23_dragon_cliffs", "rampart_22_unicorn_glade", "Dragon Cliffs")]
        [TestCase("rampart_24_dendroid_saplings", "rampart_23_dragon_cliffs", "Dendroid Saplings")]
        [TestCase("rampart_25_fountain_of_fortune", "rampart_24_dendroid_saplings", "Fountain of Fortune")]
        [TestCase("rampart_26_upg_homestead", "rampart_25_fountain_of_fortune", "Upg. Homestead")]
        [TestCase("rampart_27_upg_enchanted_spring", "rampart_26_upg_homestead", "Upg. Enchanted Spring")]
        [TestCase("rampart_28_upg_dendroid_arches", "rampart_27_upg_enchanted_spring", "Upg. Dendroid Arches", "Upg. Dendroid Saplings")]
        [TestCase("rampart_29_upg_unicorn_glade", "rampart_28_upg_dendroid_arches", "Upg. Unicorn Glade")]
        [TestCase("rampart_30_upg_dragon_cliffs", "rampart_29_upg_unicorn_glade", "Upg. Dragon Cliffs")]
        public void EachSaveAddsOneBuilding(string after, string before, string building, string? automatic = null)
        {
            var added = FortuneKeep(after).Buildings.Except(FortuneKeep(before).Buildings).Select(b => b.Name);

            // The game adds the upgraded horde building (bit 19 or 25) by itself when a horde building and the upgrade of its dwelling both exist.
            Assert.That(added, Is.EquivalentTo(automatic == null ? new[] { building } : new[] { building, automatic }));
        }

        [Test]
        public void TheLastSaveHasEveryBuildingOfTheFactionThatCanBeBuilt()
        {
            var castle = FortuneKeep("rampart_30_upg_dragon_cliffs");

            Assert.That(castle.Faction, Is.EqualTo(Faction.Rampart));
            Assert.That(castle.Buildings, Does.Contain(Building.RampartUpgDragonCliffs).And.Contain(Building.Capitol).And.Contain(Building.MagesGuild5));
            Assert.That(castle.MageGuildLevel, Is.EqualTo(5));
        }
    }
}
