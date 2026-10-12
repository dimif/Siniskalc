using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// The second half of the Restoration of Erathia series (saves 12 to 29, see <see cref="RoeCastleTests"/> for the first): unique buildings, dwellings,
    /// Mage Guilds, the Resource Silo, horde buildings and grail buildings, built in all eight castles of Red between two saves. Save 28 does not exist.
    /// The unique buildings that the notes of the series call "cage of warlords" and "captain's quarters" for Fortress are the other way round:
    /// Captain's Quarters is the unique one (save 13) and Cage of Warlords is the horde building (save 23).
    /// </summary>
    [TestFixture]
    internal sealed class RoeCastleBuildingsTests
    {
        private static readonly string[] RedCastles = ["1_Castle", "2_Rampart", "3_Tower", "4_Inferno", "5_necropolis", "6_Dung", "7_Strong", "8_Fort"];

        private static Game Load(string save) => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Roe", save + ".GM1"));

        private static CastleModel Castle(string save, string name)
        {
            var castles = Load(save).History.Current.Castles();
            return TestServices.CastleDeserializer.Deserialize(castles[castles.FindByName(name)]);
        }

        // The buildings each castle added since the previous save: "castle=Building|Building;castle=Building".
        [TestCase("roe_12_unique_1", "roe_11_city_hall", "6_Dung=Mana Vortex;7_Strong=Hall of Valhalla;8_Fort=Glyphs of Fear;1_Castle=Brotherhood of the Sword;5_necropolis=Necromancy Amplifier;4_Inferno=Order of Fire;3_Tower=Library;2_Rampart=Mystic Pond")]
        [TestCase("roe_13_unique_2", "roe_12_unique_1", "6_Dung=Portal of Summoning;7_Strong=Escape Tunnel;8_Fort=Captain's Quarters;1_Castle=Lighthouse;5_necropolis=Cover of Darkness;4_Inferno=Brimstone Stormclouds;3_Tower=Wall of Knowledge;2_Rampart=Fountain of Fortune")]
        [TestCase("roe_14_dwelling_1", "roe_13_unique_2", "6_Dung=Warren;7_Strong=Goblin Barracks;8_Fort=Gnoll Hut;1_Castle=Guardhouse;5_necropolis=Cursed Temple;4_Inferno=Imp Crucible;3_Tower=Workshop;2_Rampart=Centaur Stables")]
        [TestCase("roe_15_dwelling_2", "roe_14_dwelling_1", "6_Dung=Harpy Loft;7_Strong=Wolf Pen;8_Fort=Lizard Den;1_Castle=Archers' Tower;5_necropolis=Graveyard;4_Inferno=Hall of Sins;3_Tower=Parapet;2_Rampart=Dwarf Cottage")]
        [TestCase("roe_15b_dwelling_3", "roe_15_dwelling_2", "6_Dung=Pillar of Eyes;7_Strong=Orc Tower;8_Fort=Serpent Fly Hive;1_Castle=Barracks;5_necropolis=Tomb of Souls;4_Inferno=Kennels;3_Tower=Golem Factory;2_Rampart=Homestead")]
        [TestCase("roe_16_dwelling_4", "roe_15b_dwelling_3", "6_Dung=Chapel of Stilled Voices;7_Strong=Ogre Fort;8_Fort=Basilisk Pit;1_Castle=Griffin Tower;5_necropolis=Estate;4_Inferno=Demon Gate;3_Tower=Mage Tower;2_Rampart=Enchanted Spring")]
        [TestCase("roe_17_dwelling_5", "roe_16_dwelling_4", "6_Dung=Labyrinth;7_Strong=Cliff Nest;8_Fort=Gorgon Lair;1_Castle=Monastery;5_necropolis=Mausoleum;4_Inferno=Hell Hole;3_Tower=Altar of Wishes;2_Rampart=Dendroid Arches")]
        [TestCase("roe_18_unique_3", "roe_17_dwelling_5", "6_Dung=Artifact Merchants;7_Strong=Freelancer's Guild;8_Fort=Blood Obelisk;1_Castle=Stables;5_necropolis=Skeleton Transformer;4_Inferno=Castle Gate;3_Tower=Lookout Tower;2_Rampart=Miners' Guild")]
        [TestCase("roe_19_dwelling_6", "roe_18_unique_3", "6_Dung=Manticore Lair;7_Strong=Cyclops Cave;8_Fort=Wyvern Nest;1_Castle=Training Grounds;5_necropolis=Hall of Darkness;4_Inferno=Fire Lake;3_Tower=Golden Pavilion;2_Rampart=Unicorn Glade")]
        [TestCase("roe_20_mage_guild_2", "roe_19_dwelling_6", "6_Dung=Mage Guild, level 2;7_Strong=Mage Guild, level 2;8_Fort=Mage Guild, level 2;1_Castle=Mage Guild, level 2;5_necropolis=Mage Guild, level 2;4_Inferno=Mage Guild, level 2;3_Tower=Mage Guild, level 2;2_Rampart=Mage Guild, level 2")]
        [TestCase("roe_21_dwelling_7", "roe_20_mage_guild_2", "6_Dung=Dragon Cave;7_Strong=Behemoth Lair;8_Fort=Hydra Pond;1_Castle=Portal of Glory;5_necropolis=Dragon Vault;4_Inferno=Forsaken Palace;3_Tower=Cloud Temple;2_Rampart=Dragon Cliffs")]
        [TestCase("roe_22_resource_silo", "roe_21_dwelling_7", "6_Dung=Resource Silo;7_Strong=Resource Silo;8_Fort=Resource Silo;1_Castle=Resource Silo;5_necropolis=Resource Silo;4_Inferno=Resource Silo;3_Tower=Resource Silo;2_Rampart=Resource Silo")]
        [TestCase("roe_23_horde_1", "roe_22_resource_silo", "6_Dung=Mushroom Rings;7_Strong=Mess Hall;8_Fort=Cage of Warlords;1_Castle=Griffin Bastion;5_necropolis=Unearthed Graves;4_Inferno=Birthing Pool;3_Tower=Sculptor's Wings;2_Rampart=Dendroid Saplings")]
        [TestCase("roe_24_unique_4", "roe_23_horde_1", "6_Dung=Battle Scholar Academy;7_Strong=Ballista Yard;4_Inferno=Cages;3_Tower=Artifact Merchants;2_Rampart=Treasury")]
        [TestCase("roe_25_mage_guild_3", "roe_24_unique_4", "6_Dung=Mage Guild, level 3;7_Strong=Mage Guild, level 3;8_Fort=Mage Guild, level 3;1_Castle=Mage Guild, level 3;5_necropolis=Mage Guild, level 3;4_Inferno=Mage Guild, level 3;3_Tower=Mage Guild, level 3;2_Rampart=Mage Guild, level 3")]
        [TestCase("roe_26_grail_1", "roe_25_mage_guild_3", "1_Castle=Colossus;5_necropolis=Soul Prison")]
        [TestCase("roe_27_grail_2", "roe_26_grail_1", "6_Dung=Guardian of Earth;7_Strong=Warlords' Monument;8_Fort=Carnivorous Plant;4_Inferno=Deity of Fire;3_Tower=Skyship")]
        [TestCase("roe_29_all_grails", "roe_27_grail_2", "2_Rampart=Spirit Guardian")]
        [TestCase("roe_30_upg_dwelling_1", "roe_29_all_grails", "6_Dung=Upg. Mushroom Rings|Upg. Warren;7_Strong=Upg. Goblin Barracks|Upg. Mess Hall;8_Fort=Upg. Cage of Warlords|Upg. Gnoll Hut;1_Castle=Upg. Guardhouse;5_necropolis=Upg. Cursed Temple|Upg. Unearthed Graves;4_Inferno=Upg. Birthing Pool|Upg. Imp Crucible;3_Tower=Upg. Workshop;2_Rampart=Upg. Centaur Stables")]
        [TestCase("roe_31_upg_dwelling_2", "roe_30_upg_dwelling_1", "6_Dung=Upg. Harpy Loft;7_Strong=Upg. Wolf Pen;8_Fort=Upg. Lizard Den;1_Castle=Upg. Archers' Tower;5_necropolis=Upg. Graveyard;4_Inferno=Upg. Hall of Sins;3_Tower=Upg. Parapet|Upg. Sculptor's Wings;2_Rampart=Upg. Dwarf Cottage|Upg. Miners' Guild")]
        [TestCase("roe_35_upg_dwelling_3", "roe_34_tazar_all_spell_tomes", "6_Dung=Upg. Pillar of Eyes;7_Strong=Upg. Orc Tower;8_Fort=Upg. Serpent Fly Hive;1_Castle=Upg. Griffin Bastion|Upg. Griffin Tower;5_necropolis=Upg. Tomb of Souls;4_Inferno=Upg. Cages|Upg. Kennels;3_Tower=Upg. Golem Factory;2_Rampart=Upg. Homestead")]
        [TestCase("roe_39_upg_dwelling_4", "roe_38_damacon_angel_wings", "6_Dung=Upg. Chapel of Stilled Voices;7_Strong=Upg. Ogre Fort;8_Fort=Upg. Basilisk Pit;1_Castle=Upg. Barracks;5_necropolis=Upg. Estate;4_Inferno=Upg. Demon Gate;3_Tower=Upg. Mage Tower;2_Rampart=Upg. Enchanted Spring")]
        [TestCase("roe_40_upg_dwelling_5", "roe_39_upg_dwelling_4", "6_Dung=Upg. Labyrinth;7_Strong=Upg. Cliff Nest;8_Fort=Upg. Gorgon Lair;1_Castle=Upg. Monastery;5_necropolis=Upg. Mausoleum;4_Inferno=Upg. Hell Hole;3_Tower=Upg. Altar of Wishes;2_Rampart=Upg. Dendroid Arches|Upg. Dendroid Saplings")]
        [TestCase("roe_43_mage_guild_4", "roe_42_tazar_garrisoned", "1_Castle=Mage Guild, level 4;2_Rampart=Mage Guild, level 4;3_Tower=Mage Guild, level 4;4_Inferno=Mage Guild, level 4;5_necropolis=Mage Guild, level 4;6_Dung=Mage Guild, level 4")]
        [TestCase("roe_44_upg_dwelling_6", "roe_43_mage_guild_4", "6_Dung=Upg. Manticore Lair;7_Strong=Upg. Cyclops Cave;8_Fort=Upg. Wyvern Nest;1_Castle=Upg. Training Grounds;5_necropolis=Upg. Hall of Darkness;4_Inferno=Upg. Fire Lake;3_Tower=Upg. Golden Pavilion;2_Rampart=Upg. Unicorn Glade")]
        [TestCase("roe_45_upg_dwelling_7", "roe_44_upg_dwelling_6", "6_Dung=Upg. Dragon Cave;7_Strong=Upg. Behemoth Lair;8_Fort=Upg. Hydra Pond;1_Castle=Upg. Portal of Glory;5_necropolis=Upg. Dragon Vault;4_Inferno=Upg. Forsaken Palace;3_Tower=Upg. Cloud Temple;2_Rampart=Upg. Dragon Cliffs")]
        [TestCase("roe_46_mage_guild_5", "roe_45_upg_dwelling_7", "6_Dung=Mage Guild, level 5;5_necropolis=Mage Guild, level 5;4_Inferno=Mage Guild, level 5;3_Tower=Mage Guild, level 5;2_Rampart=Mage Guild, level 5")]
        [TestCase("roe_47_garrisons", "roe_46_mage_guild_5", "")]
        [TestCase("roe_48_castle_capitol", "roe_47_garrisons", "1_Castle=Capitol")]
        [TestCase("roe_48b_rampart_capitol", "roe_47_garrisons", "2_Rampart=Capitol")]
        [TestCase("roe_49_tazar_fly", "roe_48b_rampart_capitol", "")]
        public void EachSaveAddsTheBuildingsOfTheNotesToTheCastlesThatBuiltThem(string after, string before, string added)
        {
            var expected = added.Length == 0 ? new Dictionary<string, string[]>() : added.Split(';').ToDictionary(p => p.Split('=')[0], p => p.Split('=')[1].Split('|'));

            foreach (var name in RedCastles)
            {
                var actual = Castle(after, name).Buildings.Except(Castle(before, name).Buildings).Select(b => b.Name);

                Assert.That(actual, Is.EquivalentTo(expected.GetValueOrDefault(name) ?? []), $"{name} in {after}");
            }
        }

        // What an upgrade, a horde building or a unique building replaced: it is built, but no longer in the castle.
        [TestCase("roe_12_unique_1", "1_Castle", "Tavern")]
        [TestCase("roe_13_unique_2", "2_Rampart", "Mystic Pond")]
        [TestCase("roe_18_unique_3", "2_Rampart", "Dwarf Cottage")]
        [TestCase("roe_20_mage_guild_2", "6_Dung", "Mage Guild, level 1")]
        [TestCase("roe_20_mage_guild_2", "7_Strong", "Mage Guild, level 1")]
        [TestCase("roe_20_mage_guild_2", "8_Fort", "Mage Guild, level 1")]
        [TestCase("roe_20_mage_guild_2", "1_Castle", "Mage Guild, level 1")]
        [TestCase("roe_20_mage_guild_2", "5_necropolis", "Mage Guild, level 1")]
        [TestCase("roe_20_mage_guild_2", "4_Inferno", "Mage Guild, level 1")]
        [TestCase("roe_20_mage_guild_2", "3_Tower", "Mage Guild, level 1")]
        [TestCase("roe_20_mage_guild_2", "2_Rampart", "Mage Guild, level 1")]
        [TestCase("roe_23_horde_1", "6_Dung", "Warren")]
        [TestCase("roe_23_horde_1", "7_Strong", "Goblin Barracks")]
        [TestCase("roe_23_horde_1", "8_Fort", "Gnoll Hut")]
        [TestCase("roe_23_horde_1", "1_Castle", "Griffin Tower")]
        [TestCase("roe_23_horde_1", "5_necropolis", "Cursed Temple")]
        [TestCase("roe_23_horde_1", "4_Inferno", "Imp Crucible")]
        [TestCase("roe_23_horde_1", "3_Tower", "Parapet")]
        [TestCase("roe_23_horde_1", "2_Rampart", "Dendroid Arches")]
        [TestCase("roe_24_unique_4", "4_Inferno", "Kennels")]
        [TestCase("roe_25_mage_guild_3", "6_Dung", "Mage Guild, level 2")]
        [TestCase("roe_25_mage_guild_3", "7_Strong", "Mage Guild, level 2")]
        [TestCase("roe_25_mage_guild_3", "8_Fort", "Mage Guild, level 2")]
        [TestCase("roe_25_mage_guild_3", "1_Castle", "Mage Guild, level 2")]
        [TestCase("roe_25_mage_guild_3", "5_necropolis", "Mage Guild, level 2")]
        [TestCase("roe_25_mage_guild_3", "4_Inferno", "Mage Guild, level 2")]
        [TestCase("roe_25_mage_guild_3", "3_Tower", "Mage Guild, level 2")]
        [TestCase("roe_25_mage_guild_3", "2_Rampart", "Mage Guild, level 2")]
        [TestCase("roe_30_upg_dwelling_1", "6_Dung", "Mushroom Rings")]
        [TestCase("roe_30_upg_dwelling_1", "7_Strong", "Mess Hall")]
        [TestCase("roe_30_upg_dwelling_1", "8_Fort", "Cage of Warlords")]
        [TestCase("roe_30_upg_dwelling_1", "1_Castle", "Guardhouse")]
        [TestCase("roe_30_upg_dwelling_1", "5_necropolis", "Unearthed Graves")]
        [TestCase("roe_30_upg_dwelling_1", "4_Inferno", "Birthing Pool")]
        [TestCase("roe_30_upg_dwelling_1", "3_Tower", "Workshop")]
        [TestCase("roe_30_upg_dwelling_1", "2_Rampart", "Centaur Stables")]
        [TestCase("roe_31_upg_dwelling_2", "6_Dung", "Harpy Loft")]
        [TestCase("roe_31_upg_dwelling_2", "7_Strong", "Wolf Pen")]
        [TestCase("roe_31_upg_dwelling_2", "8_Fort", "Lizard Den")]
        [TestCase("roe_31_upg_dwelling_2", "1_Castle", "Archers' Tower")]
        [TestCase("roe_31_upg_dwelling_2", "5_necropolis", "Graveyard")]
        [TestCase("roe_31_upg_dwelling_2", "4_Inferno", "Hall of Sins")]
        [TestCase("roe_31_upg_dwelling_2", "3_Tower", "Sculptor's Wings")]
        [TestCase("roe_31_upg_dwelling_2", "2_Rampart", "Miners' Guild")]
        [TestCase("roe_35_upg_dwelling_3", "6_Dung", "Pillar of Eyes")]
        [TestCase("roe_35_upg_dwelling_3", "7_Strong", "Orc Tower")]
        [TestCase("roe_35_upg_dwelling_3", "8_Fort", "Serpent Fly Hive")]
        [TestCase("roe_35_upg_dwelling_3", "1_Castle", "Griffin Bastion")]
        [TestCase("roe_35_upg_dwelling_3", "5_necropolis", "Tomb of Souls")]
        [TestCase("roe_35_upg_dwelling_3", "4_Inferno", "Cages")]
        [TestCase("roe_35_upg_dwelling_3", "3_Tower", "Golem Factory")]
        [TestCase("roe_35_upg_dwelling_3", "2_Rampart", "Homestead")]
        [TestCase("roe_39_upg_dwelling_4", "6_Dung", "Chapel of Stilled Voices")]
        [TestCase("roe_39_upg_dwelling_4", "7_Strong", "Ogre Fort")]
        [TestCase("roe_39_upg_dwelling_4", "8_Fort", "Basilisk Pit")]
        [TestCase("roe_39_upg_dwelling_4", "1_Castle", "Barracks")]
        [TestCase("roe_39_upg_dwelling_4", "5_necropolis", "Estate")]
        [TestCase("roe_39_upg_dwelling_4", "4_Inferno", "Demon Gate")]
        [TestCase("roe_39_upg_dwelling_4", "3_Tower", "Mage Tower")]
        [TestCase("roe_39_upg_dwelling_4", "2_Rampart", "Enchanted Spring")]
        [TestCase("roe_40_upg_dwelling_5", "6_Dung", "Labyrinth")]
        [TestCase("roe_40_upg_dwelling_5", "7_Strong", "Cliff Nest")]
        [TestCase("roe_40_upg_dwelling_5", "8_Fort", "Gorgon Lair")]
        [TestCase("roe_40_upg_dwelling_5", "1_Castle", "Monastery")]
        [TestCase("roe_40_upg_dwelling_5", "5_necropolis", "Mausoleum")]
        [TestCase("roe_40_upg_dwelling_5", "4_Inferno", "Hell Hole")]
        [TestCase("roe_40_upg_dwelling_5", "3_Tower", "Altar of Wishes")]
        [TestCase("roe_40_upg_dwelling_5", "2_Rampart", "Dendroid Saplings")]
        [TestCase("roe_43_mage_guild_4", "1_Castle", "Mage Guild, level 3")]
        [TestCase("roe_43_mage_guild_4", "2_Rampart", "Mage Guild, level 3")]
        [TestCase("roe_43_mage_guild_4", "3_Tower", "Mage Guild, level 3")]
        [TestCase("roe_43_mage_guild_4", "4_Inferno", "Mage Guild, level 3")]
        [TestCase("roe_43_mage_guild_4", "5_necropolis", "Mage Guild, level 3")]
        [TestCase("roe_43_mage_guild_4", "6_Dung", "Mage Guild, level 3")]
        [TestCase("roe_46_mage_guild_5", "6_Dung", "Mage Guild, level 4")]
        [TestCase("roe_46_mage_guild_5", "5_necropolis", "Mage Guild, level 4")]
        [TestCase("roe_46_mage_guild_5", "4_Inferno", "Mage Guild, level 4")]
        [TestCase("roe_46_mage_guild_5", "3_Tower", "Mage Guild, level 4")]
        [TestCase("roe_46_mage_guild_5", "2_Rampart", "Mage Guild, level 4")]
        [TestCase("roe_44_upg_dwelling_6", "6_Dung", "Manticore Lair")]
        [TestCase("roe_44_upg_dwelling_6", "7_Strong", "Cyclops Cave")]
        [TestCase("roe_44_upg_dwelling_6", "8_Fort", "Wyvern Nest")]
        [TestCase("roe_44_upg_dwelling_6", "1_Castle", "Training Grounds")]
        [TestCase("roe_44_upg_dwelling_6", "5_necropolis", "Hall of Darkness")]
        [TestCase("roe_44_upg_dwelling_6", "4_Inferno", "Fire Lake")]
        [TestCase("roe_44_upg_dwelling_6", "3_Tower", "Golden Pavilion")]
        [TestCase("roe_44_upg_dwelling_6", "2_Rampart", "Unicorn Glade")]
        [TestCase("roe_45_upg_dwelling_7", "6_Dung", "Dragon Cave")]
        [TestCase("roe_45_upg_dwelling_7", "7_Strong", "Behemoth Lair")]
        [TestCase("roe_45_upg_dwelling_7", "8_Fort", "Hydra Pond")]
        [TestCase("roe_45_upg_dwelling_7", "1_Castle", "Portal of Glory")]
        [TestCase("roe_45_upg_dwelling_7", "5_necropolis", "Dragon Vault")]
        [TestCase("roe_45_upg_dwelling_7", "4_Inferno", "Forsaken Palace")]
        [TestCase("roe_45_upg_dwelling_7", "3_Tower", "Cloud Temple")]
        [TestCase("roe_45_upg_dwelling_7", "2_Rampart", "Dragon Cliffs")]
        [TestCase("roe_48_castle_capitol", "1_Castle", "City Hall")]
        [TestCase("roe_48b_rampart_capitol", "2_Rampart", "City Hall")]
        public void AnUpgradeReplacesTheBuildingsItListsInTheCurrentBuildings(string save, string castle, string replaced)
        {
            var model = Castle(save, castle);

            foreach (var name in replaced.Split('|'))
            {
                Assert.That(model.Buildings.Select(b => b.Name), Does.Contain(name));
                Assert.That(model.CurrentBuildings.Select(b => b.Name), Does.Not.Contain(name));
            }
        }

        [TestCase("roe_20_mage_guild_2", 2, "5,4,0,0,0", "6,5,0,0,0")]
        [TestCase("roe_25_mage_guild_3", 3, "5,4,3,0,0", "6,5,4,0,0")]
        public void TheMageGuildHasOneSpellMoreInEachLevelOfTheTowerBecauseOfTheLibrary(string save, int level, string spells, string towerSpells)
        {
            foreach (var name in RedCastles)
            {
                var castle = Castle(save, name);

                Assert.That(castle.MageGuildLevel, Is.EqualTo(level), name);
                Assert.That(string.Join(",", castle.SpellCounts), Is.EqualTo(name == "3_Tower" ? towerSpells : spells), name);
            }
        }

        [TestCase("roe_14_dwelling_1", "2_Rampart", "14,0,0,0,0,0,0")]
        [TestCase("roe_15b_dwelling_3", "1_Castle", "42,27,0,4,0,0,0")]
        [TestCase("roe_16_dwelling_4", "1_Castle", "42,27,7,4,0,0,0")]
        [TestCase("roe_21_dwelling_7", "6_Dung", "42,24,7,4,3,2,1")]
        public void TheCreaturesWaitingInTheDwellingsAreByCreatureLevel(string save, string castle, string expected)
        {
            Assert.That(string.Join(",", Castle(save, castle).CreaturesAvailable), Is.EqualTo(expected));
        }

        [Test]
        public void AnUpgradedDwellingMovesItsCreaturesToTheUpgradedList()
        {
            var before = Castle("roe_29_all_grails", "6_Dung");
            var after = Castle("roe_30_upg_dwelling_1", "6_Dung");

            Assert.That(before.UpgradedCreaturesAvailable.Sum(), Is.EqualTo(0));
            Assert.That(after.UpgradedCreaturesAvailable, Is.EqualTo(new[] { 122, 0, 0, 0, 0, 0, 0 }));
            Assert.That(after.CreaturesAvailable[0], Is.EqualTo(0));
        }

        [Test]
        public void TheLastSaveOfTheBuildingSeriesHasEveryDwellingOfEveryCastleUpgradedExceptTheSeventh()
        {
            foreach (var name in RedCastles)
            {
                var castle = Castle("roe_40_upg_dwelling_5", name);

                Assert.That(castle.UpgradedCreaturesAvailable.Take(5).All(c => c > 0), Is.True, name);
                Assert.That(castle.UpgradedCreaturesAvailable.Skip(5), Is.All.EqualTo(0), name);
            }
        }

        [TestCase("roe_43_mage_guild_4", "5,4,3,2,0", "6,5,4,3,0", 4)]
        [TestCase("roe_46_mage_guild_5", "5,4,3,2,1", "6,5,4,3,2", 5)]
        public void TheMageGuildsOfFourAndFiveFillTheHigherSpellLevels(string save, string spells, string towerSpells, int level)
        {
            string[] capped = ["7_Strong", "8_Fort"];
            foreach (var name in RedCastles.Where(n => !capped.Contains(n) && !(level == 5 && n == "1_Castle")))
            {
                Assert.That(Castle(save, name).MageGuildLevel, Is.EqualTo(level), name);
                Assert.That(string.Join(",", Castle(save, name).SpellCounts), Is.EqualTo(name == "3_Tower" ? towerSpells : spells), name);
            }
        }

        [Test]
        public void StrongholdAndFortressStopAtMageGuildLevelThreeAndCastleAtFour()
        {
            foreach (var name in new[] { "7_Strong", "8_Fort" })
            {
                Assert.That(Castle("roe_46_mage_guild_5", name).MageGuildLevel, Is.EqualTo(3), name);
                Assert.That(Castle("roe_46_mage_guild_5", name).Buildings, Does.Not.Contain(Building.MagesGuild4), name);
            }

            Assert.That(Castle("roe_46_mage_guild_5", "1_Castle").MageGuildLevel, Is.EqualTo(4));
            Assert.That(Castle("roe_46_mage_guild_5", "1_Castle").Buildings, Does.Not.Contain(Building.MagesGuild5));
        }

        [Test]
        public void TheUpgradedSeventhDwellingsHoldTheirCreaturesInTheUpgradedList()
        {
            var dungeon = Castle("roe_45_upg_dwelling_7", "6_Dung");

            Assert.That(dungeon.CreaturesAvailable, Is.All.EqualTo(0));
            Assert.That(dungeon.UpgradedCreaturesAvailable, Is.EqualTo(new[] { 174, 88, 63, 36, 27, 18, 9 }));
        }

        [Test]
        public void TheGarrisonsListTheStrongestCreatureFirstAndTheTowerHoldsTheThreeStacksThatWerePutThere()
        {
            var dungeon = Castle("roe_47_garrisons", "6_Dung");
            var tower = Castle("roe_47_garrisons", "3_Tower");

            Assert.That(dungeon.Garrison.Slots.Select(s => (s.SlotNumber, s.CreatureCode, s.CreatureCount)), Is.EqualTo(new[] { (1, (byte)83, 9u), (2, (byte)81, 18u), (3, (byte)79, 27u), (4, (byte)77, 36u), (5, (byte)75, 63u), (6, (byte)73, 88u), (7, (byte)71, 174u) }));
            Assert.That(tower.Garrison.Slots.Select(s => (s.SlotNumber, s.CreatureCode, s.CreatureCount)), Is.EqualTo(new[] { (1, (byte)41, 9u), (2, (byte)39, 18u), (3, (byte)37, 27u) }));
            Assert.That(Castle("roe_46_mage_guild_5", "6_Dung").Garrison.Slots, Is.Empty);
        }

        [Test]
        public void RecruitingTheCreaturesEmptiesTheDwellingsOfThoseCastles()
        {
            Assert.That(Castle("roe_47_garrisons", "6_Dung").UpgradedCreaturesAvailable, Is.All.EqualTo(0));
            Assert.That(Castle("roe_47_garrisons", "1_Castle").UpgradedCreaturesAvailable, Is.EqualTo(new[] { 154, 99, 71, 36, 27, 18, 9 }));
        }

        [TestCase("roe_48_castle_capitol", "1_Castle", "2_Rampart")]
        [TestCase("roe_48b_rampart_capitol", "2_Rampart", "1_Castle")]
        public void OnlyTheCastleThatBuiltTheCapitolHasItAndItReplacesTheCityHall(string save, string withCapitol, string without)
        {
            Assert.That(Castle(save, withCapitol).Buildings, Does.Contain(Building.Capitol));
            Assert.That(Castle(save, withCapitol).CurrentBuildings, Does.Contain(Building.Capitol).And.Not.Contain(Building.CityHall));
            Assert.That(Castle(save, without).Buildings, Does.Not.Contain(Building.Capitol));
            Assert.That(Castle(save, without).CurrentBuildings, Does.Contain(Building.CityHall));
        }

        [Test]
        public void TazarIsAtTheGateOfTheDungeonInTheGarrisonsSave()
        {
            var dungeon = Castle("roe_47_garrisons", "6_Dung");

            Assert.That(dungeon.GarrisonedHeroId, Is.Null);
            Assert.That(dungeon.VisitingHeroId, Is.EqualTo(115));
        }

        [Test]
        public void EveryCastleOfEverySaveFromTwelveSerializesToTheBytesItWasReadFrom()
        {
            foreach (var path in Directory.GetFiles(Path.Combine("TestData", "Roe"), "*.GM1"))
            {
                foreach (var chunk in TestServices.GameService.LoadGameFromFile(path).History.Current.Castles())
                {
                    Assert.That(TestServices.SerializeCastle(chunk, TestServices.CastleDeserializer.Deserialize(chunk)).HasSameBytes(chunk), Is.True, $"{chunk.Name} in {path}");
                }
            }
        }
    }
}
