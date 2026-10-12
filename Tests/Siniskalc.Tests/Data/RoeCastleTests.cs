using Siniskalc;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// Shadow of Death savefiles (header 42.2) of the Restoration of Erathia map. Red owns one castle of each of the eight RoE factions, named
    /// after their number (1_Castle ... 8_Fort), and starts with a village hall only. One thing is built in all of them between two saves, in
    /// the order of the file numbers. Save 1 spent resources and built nothing. Elfwind is a neutral Rampart town that never changes.
    /// </summary>
    [TestFixture]
    internal sealed class RoeCastleTests
    {
        private static readonly string[] RedCastles = ["1_Castle", "2_Rampart", "3_Tower", "4_Inferno", "5_necropolis", "6_Dung", "7_Strong", "8_Fort"];

        // Only these two are on the coast.
        private static readonly string[] CoastalCastles = ["1_Castle", "5_necropolis"];

        private static readonly string[] Saves =
        [
            "roe_00_baseline", "roe_01_resources", "roe_02_shipyard", "roe_03_fort", "roe_04_citadel", "roe_05_castle", "roe_06_tavern",
            "roe_07_blacksmith", "roe_08_marketplace", "roe_09_town_hall", "roe_10_mage_guild_1", "roe_11_city_hall",
        ];

        private static Game Load(string save) => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Roe", save + ".GM1"));

        private static CastleModel Castle(string save, string name)
        {
            var castles = Load(save).History.Current.Castles();
            return TestServices.CastleDeserializer.Deserialize(castles[castles.FindByName(name)]);
        }

        [TestCaseSource(nameof(Saves))]
        public void TheSavefileIsDetectedAsShadowOfDeathWithNineCastles(string save)
        {
            var game = Load(save);

            Assert.That(game.GameVersion, Is.EqualTo(GameVersion.ShadowOfDeath));
            Assert.That(game.History.Current.Castles(), Has.Count.EqualTo(9));
        }

        [TestCase("1_Castle", "Castle")]
        [TestCase("2_Rampart", "Rampart")]
        [TestCase("3_Tower", "Tower")]
        [TestCase("4_Inferno", "Inferno")]
        [TestCase("5_necropolis", "Necropolis")]
        [TestCase("6_Dung", "Dungeon")]
        [TestCase("7_Strong", "Stronghold")]
        [TestCase("8_Fort", "Fortress")]
        public void RedOwnsOneCastleOfEachFactionWithOnlyTheVillageHall(string name, string faction)
        {
            var castle = Castle(Saves[0], name);

            Assert.That(castle.Faction?.Name, Is.EqualTo(faction));
            Assert.That(castle.Owner, Is.EqualTo(PlayerColor.Red));
            Assert.That(castle.Buildings, Is.EquivalentTo(new[] { Building.VillageHall }));
            Assert.That(castle.MageGuildLevel, Is.EqualTo(0));
        }

        [TestCase("roe_01_resources", "roe_00_baseline")]
        [TestCase("roe_02_shipyard", "roe_01_resources", "Shipyard")]
        [TestCase("roe_03_fort", "roe_02_shipyard", "Fort")]
        [TestCase("roe_04_citadel", "roe_03_fort", "Citadel")]
        [TestCase("roe_05_castle", "roe_04_citadel", "Castle")]
        [TestCase("roe_06_tavern", "roe_05_castle", "Tavern")]
        [TestCase("roe_07_blacksmith", "roe_06_tavern", "Blacksmith")]
        [TestCase("roe_08_marketplace", "roe_07_blacksmith", "Marketplace")]
        [TestCase("roe_09_town_hall", "roe_08_marketplace", "Town Hall")]
        [TestCase("roe_10_mage_guild_1", "roe_09_town_hall", "Mage Guild, level 1")]
        [TestCase("roe_11_city_hall", "roe_10_mage_guild_1", "City Hall")]
        public void EachSaveAddsOneBuildingToEveryCastleOfRed(string after, string before, string? building = null)
        {
            foreach (var name in RedCastles)
            {
                var added = Castle(after, name).Buildings.Except(Castle(before, name).Buildings).Select(b => b.Name);

                var expected = building == null || (building == "Shipyard" && !CoastalCastles.Contains(name)) ? [] : new[] { building };
                Assert.That(added, Is.EquivalentTo(expected), name);
            }
        }

        [Test]
        public void TheTownHallReplacesTheVillageHallAndTheCityHallReplacesTheTownHall()
        {
            var townHall = Castle("roe_09_town_hall", "2_Rampart");
            var cityHall = Castle("roe_11_city_hall", "2_Rampart");

            Assert.That(townHall.CurrentBuildings, Does.Contain(Building.TownHall).And.Not.Contain(Building.VillageHall));
            Assert.That(cityHall.CurrentBuildings, Does.Contain(Building.CityHall).And.Not.Contain(Building.TownHall));
            Assert.That(cityHall.Buildings, Does.Contain(Building.VillageHall).And.Contain(Building.TownHall));
        }

        [Test]
        public void TheMageGuildHasLevelOneInEveryCastleOfRedFromSaveTen()
        {
            foreach (var name in RedCastles)
            {
                Assert.That(Castle("roe_09_town_hall", name).MageGuildLevel, Is.EqualTo(0), name);
                Assert.That(Castle("roe_10_mage_guild_1", name).MageGuildLevel, Is.EqualTo(1), name);
                Assert.That(Castle("roe_11_city_hall", name).MageGuildLevel, Is.EqualTo(1), name);
            }
        }

        [TestCaseSource(nameof(Saves))]
        public void ElfwindStaysANeutralRampartWithItsFourStartingBuildings(string save)
        {
            var castle = Castle(save, "Elfwind");

            Assert.That(castle.Faction, Is.EqualTo(Faction.Rampart));
            Assert.That(castle.Owner, Is.Null);
            Assert.That(castle.Buildings, Has.Count.EqualTo(4));
        }

        [Test]
        public void EveryCastleOfEverySaveSerializesToTheBytesItWasReadFrom()
        {
            foreach (var save in Saves)
            {
                foreach (var chunk in Load(save).History.Current.Castles())
                {
                    var written = TestServices.SerializeCastle(chunk, TestServices.CastleDeserializer.Deserialize(chunk));

                    Assert.That(written.HasSameBytes(chunk), Is.True, $"{chunk.Name} in {save}");
                }
            }
        }
    }
}
