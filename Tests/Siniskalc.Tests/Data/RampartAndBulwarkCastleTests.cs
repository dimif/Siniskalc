using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// The bulwark_* savefiles hold two castles of red that start completely unbuilt: the Rampart "Still Water" and
    /// the Bulwark "Jotunngard". One building is built in each per file where there is one to build: first the buildings that are
    /// not dwellings, then the dwellings, the upgrades, the support buildings and finally the grail.
    /// </summary>
    [TestFixture]
    internal sealed class RampartAndBulwarkCastleTests
    {
        private static CastleModel Castle(string save, string name)
        {
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "Bulwark", save + ".GM1"));
            var castles = game.History.Current.Castles();
            return TestServices.CastleDeserializer.Deserialize(castles[castles.FindByName(name)]);
        }

        // Bits 19 and 25 are set by the game itself once a horde building and the upgrade of its dwelling both exist.
        private static IEnumerable<string> Added(CastleModel after, CastleModel before) =>
            after.Buildings.Except(before.Buildings).Where(b => b.Code != 19 && b.Code != 25).Select(b => b.Name);

        [TestCase("bulwark_01_fort", "bulwark_00_baseline", new string[] { "Fort" }, new string[] { "Fort" })]
        [TestCase("bulwark_02_citadel", "bulwark_01_fort", new string[] { "Citadel" }, new string[] { "Citadel" })]
        [TestCase("bulwark_03_castle", "bulwark_02_citadel", new string[] { "Castle" }, new string[] { "Castle" })]
        [TestCase("bulwark_04_tavern", "bulwark_03_castle", new string[] { "Tavern" }, new string[] { "Tavern" })]
        [TestCase("bulwark_05_town_hall", "bulwark_04_tavern", new string[] { "Town Hall" }, new string[] { "Town Hall" })]
        [TestCase("bulwark_06_blacksmith", "bulwark_05_town_hall", new string[] { "Blacksmith" }, new string[] { "Blacksmith" })]
        [TestCase("bulwark_07_marketplace", "bulwark_06_blacksmith", new string[] { "Marketplace" }, new string[] { "Marketplace" })]
        [TestCase("bulwark_08_resource_silo", "bulwark_07_marketplace", new string[] { "Resource Silo" }, new string[] { "Resource Silo" })]
        [TestCase("bulwark_09_mage_guild_1", "bulwark_08_resource_silo", new string[] { "Mage Guild, level 1" }, new string[] { "Mage Guild, level 1" })]
        [TestCase("bulwark_10_city_hall", "bulwark_09_mage_guild_1", new string[] { "City Hall" }, new string[] { "City Hall" })]
        [TestCase("bulwark_11_mage_guild_2", "bulwark_10_city_hall", new string[] { "Mage Guild, level 2" }, new string[] { "Mage Guild, level 2" })]
        [TestCase("bulwark_12_mage_guild_3", "bulwark_11_mage_guild_2", new string[] { "Mage Guild, level 3" }, new string[] { "Mage Guild, level 3" })]
        [TestCase("bulwark_13_mage_guild_4", "bulwark_12_mage_guild_3", new string[] { "Mage Guild, level 4" }, new string[] { "Mage Guild, level 4" })]
        [TestCase("bulwark_14_centaur_stables__colliery", "bulwark_13_mage_guild_4", new string[] { "Centaur Stables" }, new string[] { "Colliery" })]
        [TestCase("bulwark_15_dwarf_cottage__frigid_spur", "bulwark_14_centaur_stables__colliery", new string[] { "Dwarf Cottage" }, new string[] { "Frigid Spur" })]
        [TestCase("bulwark_16_homestead__hall_of_the_bold", "bulwark_15_dwarf_cottage__frigid_spur", new string[] { "Homestead" }, new string[] { "Hall of the Bold" })]
        [TestCase("bulwark_17_enchanted_spring__mountain_embassy", "bulwark_16_homestead__hall_of_the_bold", new string[] { "Enchanted Spring" }, new string[] { "Mountain Embassy" })]
        [TestCase("bulwark_18_dendroid_arches__shaman_fane", "bulwark_17_enchanted_spring__mountain_embassy", new string[] { "Dendroid Arches" }, new string[] { "Shaman Fane" })]
        [TestCase("bulwark_19_unicorn_glade__mammoth_stalls", "bulwark_18_dendroid_arches__shaman_fane", new string[] { "Unicorn Glade" }, new string[] { "Mammoth Stalls" })]
        [TestCase("bulwark_20_dragon_cliffs__frosthome", "bulwark_19_unicorn_glade__mammoth_stalls", new string[] { "Dragon Cliffs" }, new string[] { "Frosthome" })]
        [TestCase("bulwark_21_dendroid_saplings__sieidi_of_the_runes", "bulwark_20_dragon_cliffs__frosthome", new string[] { "Dendroid Saplings" }, new string[] { "Sieidi of the Runes" })]
        [TestCase("bulwark_22_miners_guild__hay_bins", "bulwark_21_dendroid_saplings__sieidi_of_the_runes", new string[] { "Miners' Guild" }, new string[] { "Hay Bins" })]
        [TestCase("bulwark_23_treasury__altar_of_the_runes", "bulwark_22_miners_guild__hay_bins", new string[] { "Treasury" }, new string[] { "Altar of the Runes" })]
        [TestCase("bulwark_24_nothing__fair", "bulwark_23_treasury__altar_of_the_runes", new string[] {  }, new string[] { "Fair" })]
        [TestCase("bulwark_25_mystic_pond__upg_colliery", "bulwark_24_nothing__fair", new string[] { "Mystic Pond" }, new string[] { "Upg. Colliery" })]
        [TestCase("bulwark_26_upg_centaur_stables__upg_frigid_spur", "bulwark_25_mystic_pond__upg_colliery", new string[] { "Upg. Centaur Stables" }, new string[] { "Upg. Frigid Spur" })]
        [TestCase("bulwark_27_upg_dwarf_cottage__upg_hall_of_the_bold", "bulwark_26_upg_centaur_stables__upg_frigid_spur", new string[] { "Upg. Dwarf Cottage" }, new string[] { "Upg. Hall of the Bold" })]
        [TestCase("bulwark_28_upg_homestead__upg_mountain_embassy", "bulwark_27_upg_dwarf_cottage__upg_hall_of_the_bold", new string[] { "Upg. Homestead" }, new string[] { "Upg. Mountain Embassy" })]
        [TestCase("bulwark_29_upg_dendroid_arches__upg_shaman_fane", "bulwark_28_upg_homestead__upg_mountain_embassy", new string[] { "Upg. Dendroid Arches" }, new string[] { "Upg. Shaman Fane" })]
        [TestCase("bulwark_30_upg_unicorn_glade__upg_mammoth_stalls", "bulwark_29_upg_dendroid_arches__upg_shaman_fane", new string[] { "Upg. Unicorn Glade" }, new string[] { "Upg. Mammoth Stalls" })]
        [TestCase("bulwark_31_upg_enchanted_spring__upg_frosthome", "bulwark_30_upg_unicorn_glade__upg_mammoth_stalls", new string[] { "Upg. Enchanted Spring" }, new string[] { "Upg. Frosthome" })]
        [TestCase("bulwark_32_upg_dragon_cliffs__nothing", "bulwark_31_upg_enchanted_spring__upg_frosthome", new string[] { "Upg. Dragon Cliffs" }, new string[] {  })]
        [TestCase("bulwark_33_mage_guild_5__nothing", "bulwark_32_upg_dragon_cliffs__nothing", new string[] { "Mage Guild, level 5" }, new string[] {  })]
        [TestCase("bulwark_milestone_before_grail_after_33", "bulwark_33_mage_guild_5__nothing", new string[] { "Fountain of Fortune" }, new string[] { })]
        [TestCase("bulwark_34_spirit_guardian__nothing", "bulwark_milestone_before_grail_after_33", new string[] { "Spirit Guardian" }, new string[] {  })]
        [TestCase("bulwark_35_nothing__spirits_of_the_forebears", "bulwark_34_spirit_guardian__nothing", new string[] {  }, new string[] { "Spirits of the Forebears" })]
        public void EachSaveAddsTheBuildingsInTheirNames(string after, string before, string[] rampart, string[] bulwark)
        {
            Assert.That(Added(Castle(after, "Still Water"), Castle(before, "Still Water")), Is.EquivalentTo(rampart));
            Assert.That(Added(Castle(after, "Jotunngard"), Castle(before, "Jotunngard")), Is.EquivalentTo(bulwark));
        }

        [Test]
        public void TheCastlesStartWithoutBuildings()
        {
            var rampart = Castle("bulwark_00_baseline", "Still Water");
            var bulwark = Castle("bulwark_00_baseline", "Jotunngard");

            Assert.That(rampart.Faction, Is.EqualTo(Faction.Rampart));
            Assert.That(bulwark.Faction, Is.EqualTo(Faction.Bulwark));
            Assert.That(rampart.Owner, Is.EqualTo(PlayerColor.Red));
            Assert.That(rampart.Buildings, Is.EquivalentTo(new[] { Building.VillageHall }));
            Assert.That(bulwark.Buildings, Is.EquivalentTo(new[] { Building.VillageHall }));
        }

        [TestCase("Still Water")]
        [TestCase("Jotunngard")]
        public void InTheLastSaveEveryBuildingTheMapAllowsIsBuiltExceptTheCapitol(string name)
        {
            var castle = Castle("bulwark_35_nothing__spirits_of_the_forebears", name);

            Assert.That(castle.EnabledBuildings.Except(castle.Buildings), Is.EquivalentTo(new[] { Building.Capitol }));
        }
    }
}
