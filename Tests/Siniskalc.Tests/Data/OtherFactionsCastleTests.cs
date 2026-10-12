using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// The savefiles in Hota/OtherFactions hold eight castles of red that start unbuilt, one for each of Tower, Inferno, Necropolis,
    /// Dungeon, Stronghold, Fortress, Conflux and Castle (plus neutral Factory and Rampart castles that never change). Nearly every save
    /// adds one building to each castle, in the order of the files' modification times: the fort, citadel, castle, tavern, blacksmith,
    /// marketplace, resource silo, the mage guilds up to level 3, the dwellings in tier order, the other special buildings, the town hall,
    /// the upgraded dwellings and the city hall. Files named after a state (such as all_dwellings) are saved in between and add nothing.
    /// Only the Castle has a shipyard; its stables had to be built before the training grounds, its level 6 dwelling.
    /// </summary>
    [TestFixture]
    internal sealed class OtherFactionsCastleTests
    {
        private static readonly (string Name, Faction Faction)[] Castles =
        [
            ("Corona", Faction.Tower),
            ("Enkindle", Faction.Inferno),
            ("Grave Raven", Faction.Necropolis),
            ("Castigare", Faction.Dungeon),
            ("Kruber", Faction.Stronghold),
            ("Deadfall", Faction.Fortress),
            ("Wazzar", Faction.Conflux),
            ("Highcastle", Faction.Castle),
        ];

        private static CastleModel Castle(string save, string name)
        {
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "OtherFactions", save + ".GM1"));
            var castles = game.History.Current.Castles();
            var index = castles.FindByName(name);
            Assert.That(index, Is.GreaterThanOrEqualTo(0), name);
            return TestServices.CastleDeserializer.Deserialize(castles[index]);
        }

        // Bits 19 and 25 are set by the game itself once a horde building and the upgrade of its dwelling both exist.
        private static IEnumerable<string> Added(CastleModel after, CastleModel before) =>
            after.Buildings.Except(before.Buildings).Where(b => b.Code != 19 && b.Code != 25).Select(b => b.Name);

        // The additions are written as "Castle=Building|Building;Castle=Building", or "*=Building" when every castle gets the same one.
        // Buildings whose bit is not modelled yet (see the remarks of Building) are not part of the expectations.
        [TestCase("other_factions_01_fort", "other_factions_00_baseline", "*=Fort")]
        [TestCase("other_factions_02_citadel", "other_factions_01_fort", "*=Citadel")]
        [TestCase("other_factions_03_castle", "other_factions_02_citadel", "*=Castle")]
        [TestCase("other_factions_04_tavern", "other_factions_03_castle", "*=Tavern")]
        [TestCase("other_factions_05_castle_and_tavern", "other_factions_04_tavern", "")]
        [TestCase("other_factions_06_blacksmith", "other_factions_05_castle_and_tavern", "*=Blacksmith")]
        [TestCase("other_factions_07_marketplace", "other_factions_06_blacksmith", "*=Marketplace")]
        [TestCase("other_factions_08_blacksmith_and_marketplace", "other_factions_07_marketplace", "")]
        [TestCase("other_factions_09_resource_silo", "other_factions_08_blacksmith_and_marketplace", "*=Resource Silo")]
        [TestCase("other_factions_10_mage_guild_1", "other_factions_09_resource_silo", "*=Mage Guild, level 1")]
        [TestCase("other_factions_11_mage_guild_2", "other_factions_10_mage_guild_1", "*=Mage Guild, level 2")]
        [TestCase("other_factions_12_mage_guild_3", "other_factions_11_mage_guild_2", "*=Mage Guild, level 3")]
        [TestCase("other_factions_13_all_mage_guild_3", "other_factions_12_mage_guild_3", "")]
        [TestCase("other_factions_14_dwelling_1", "other_factions_13_all_mage_guild_3", "Corona=Workshop;Enkindle=Imp Crucible;Grave Raven=Cursed Temple;Castigare=Warren;Kruber=Goblin Barracks;Deadfall=Gnoll Hut;Wazzar=Magic Lantern;Highcastle=Guardhouse")]
        [TestCase("other_factions_15_dwelling_2", "other_factions_14_dwelling_1", "Corona=Parapet;Enkindle=Hall of Sins;Grave Raven=Graveyard;Castigare=Harpy Loft;Kruber=Wolf Pen;Deadfall=Lizard Den;Wazzar=Altar of Air;Highcastle=Archers' Tower")]
        [TestCase("other_factions_16_dwelling_3", "other_factions_15_dwelling_2", "Corona=Golem Factory;Enkindle=Kennels;Grave Raven=Tomb of Souls;Castigare=Pillar of Eyes;Kruber=Orc Tower;Deadfall=Serpent Fly Hive;Wazzar=Altar of Water;Highcastle=Barracks")]
        [TestCase("other_factions_17_dwelling_4", "other_factions_16_dwelling_3", "Corona=Mage Tower;Enkindle=Demon Gate;Grave Raven=Estate;Castigare=Chapel of Stilled Voices;Kruber=Ogre Fort;Deadfall=Basilisk Pit;Wazzar=Altar of Fire;Highcastle=Griffin Tower")]
        [TestCase("other_factions_18_dwelling_5", "other_factions_17_dwelling_4", "Corona=Altar of Wishes;Enkindle=Hell Hole;Grave Raven=Mausoleum;Castigare=Labyrinth;Kruber=Cliff Nest;Deadfall=Gorgon Lair;Wazzar=Altar of Earth;Highcastle=Monastery")]
        [TestCase("other_factions_19_dwelling_6_before_stables", "other_factions_18_dwelling_5", "Corona=Golden Pavilion;Enkindle=Fire Lake;Grave Raven=Hall of Darkness;Castigare=Manticore Lair;Kruber=Cyclops Cave;Deadfall=Wyvern Nest;Wazzar=Altar of Thought")]
        [TestCase("other_factions_20_castle_stables", "other_factions_19_dwelling_6_before_stables", "Highcastle=Stables")]
        [TestCase("other_factions_21_dwelling_7", "other_factions_20_castle_stables", "Corona=Cloud Temple;Enkindle=Forsaken Palace;Grave Raven=Dragon Vault;Castigare=Dragon Cave;Kruber=Behemoth Lair;Deadfall=Hydra Pond;Wazzar=Pyre;Highcastle=Portal of Glory")]
        [TestCase("other_factions_22_all_dwellings", "other_factions_21_dwelling_7", "")]
        [TestCase("other_factions_23_first_special", "other_factions_22_all_dwellings", "Corona=Library;Enkindle=Order of Fire;Grave Raven=Necromancy Amplifier;Castigare=Mana Vortex;Kruber=Hall of Valhalla;Deadfall=Glyphs of Fear;Wazzar=Magic University;Highcastle=Brotherhood of the Sword")]
        [TestCase("other_factions_24_special_2", "other_factions_23_first_special", "")]
        [TestCase("other_factions_25_special_3", "other_factions_24_special_2", "Corona=Wall of Knowledge;Enkindle=Brimstone Stormclouds;Grave Raven=Cover of Darkness;Castigare=Portal of Summoning;Kruber=Freelancer's Guild;Deadfall=Blood Obelisk;Wazzar=Artifact Merchants;Highcastle=Griffin Bastion")]
        [TestCase("other_factions_26_special_4", "other_factions_25_special_3", "Corona=Artifact Merchants;Enkindle=Castle Gate;Grave Raven=Skeleton Transformer;Castigare=Artifact Merchants;Kruber=Ballista Yard;Deadfall=Cage of Warlords;Wazzar=Vault of Ashes;Highcastle=Shipyard")]
        [TestCase("other_factions_27_before_lighthouse", "other_factions_26_special_4", "Corona=Lookout Tower;Enkindle=Birthing Pool;Grave Raven=Unearthed Graves;Castigare=Battle Scholar Academy;Kruber=Mess Hall;Wazzar=Garden of Life")]
        [TestCase("other_factions_28_castle_lighthouse", "other_factions_27_before_lighthouse", "Highcastle=Lighthouse")]
        [TestCase("other_factions_29_all_town_halls", "other_factions_28_castle_lighthouse", "*=Town Hall")]
        [TestCase("other_factions_30_additional_1", "other_factions_29_all_town_halls", "")]
        [TestCase("other_factions_31_all_additional", "other_factions_30_additional_1", "Corona=Sculptor's Wings;Enkindle=Cages;Castigare=Mushroom Rings;Kruber=Escape Tunnel;Deadfall=Captain's Quarters")]
        [TestCase("other_factions_32_additional_2", "other_factions_31_all_additional", "")]
        [TestCase("other_factions_33_upg_dwelling_1", "other_factions_32_additional_2", "Corona=Upg. Workshop;Enkindle=Upg. Imp Crucible;Grave Raven=Upg. Cursed Temple;Castigare=Upg. Warren;Kruber=Upg. Goblin Barracks;Deadfall=Upg. Gnoll Hut;Wazzar=Upg. Magic Lantern;Highcastle=Upg. Guardhouse")]
        [TestCase("other_factions_34_upg_dwelling_2", "other_factions_33_upg_dwelling_1", "Corona=Upg. Parapet;Enkindle=Upg. Hall of Sins;Grave Raven=Upg. Graveyard;Castigare=Upg. Harpy Loft;Kruber=Upg. Wolf Pen;Deadfall=Upg. Lizard Den;Wazzar=Upg. Altar of Air;Highcastle=Upg. Archers' Tower")]
        [TestCase("other_factions_35_upg_dwelling_3", "other_factions_34_upg_dwelling_2", "Corona=Upg. Golem Factory;Enkindle=Upg. Kennels;Grave Raven=Upg. Tomb of Souls;Castigare=Upg. Pillar of Eyes;Kruber=Upg. Orc Tower;Deadfall=Upg. Serpent Fly Hive;Wazzar=Upg. Altar of Water;Highcastle=Upg. Griffin Tower")]
        [TestCase("other_factions_36_upg_dwelling_4", "other_factions_35_upg_dwelling_3", "Corona=Upg. Mage Tower;Enkindle=Upg. Demon Gate;Grave Raven=Upg. Estate;Castigare=Upg. Chapel of Stilled Voices;Kruber=Upg. Ogre Fort;Deadfall=Upg. Basilisk Pit;Wazzar=Upg. Altar of Fire;Highcastle=Upg. Barracks")]
        [TestCase("other_factions_37_upg_dwelling_5", "other_factions_36_upg_dwelling_4", "Corona=Upg. Altar of Wishes;Enkindle=Upg. Hell Hole;Grave Raven=Upg. Mausoleum;Castigare=Upg. Labyrinth;Kruber=Upg. Cliff Nest;Deadfall=Upg. Gorgon Lair;Wazzar=Upg. Altar of Earth;Highcastle=Upg. Monastery")]
        [TestCase("other_factions_38_upg_dwelling_6", "other_factions_37_upg_dwelling_5", "Corona=Upg. Golden Pavilion;Enkindle=Upg. Fire Lake;Grave Raven=Upg. Hall of Darkness;Castigare=Upg. Manticore Lair;Kruber=Upg. Cyclops Cave;Deadfall=Upg. Wyvern Nest;Wazzar=Upg. Altar of Thought;Highcastle=Training Grounds")]
        [TestCase("other_factions_39_upg_dwelling_7", "other_factions_38_upg_dwelling_6", "Corona=Upg. Cloud Temple;Enkindle=Upg. Forsaken Palace;Grave Raven=Upg. Dragon Vault;Castigare=Upg. Dragon Cave;Kruber=Upg. Behemoth Lair;Deadfall=Upg. Hydra Pond;Wazzar=Upg. Pyre;Highcastle=Upg. Training Grounds")]
        [TestCase("other_factions_40_all_upg_dwellings", "other_factions_39_upg_dwelling_7", "Highcastle=Upg. Portal of Glory")]
        [TestCase("other_factions_41_additional_3", "other_factions_40_all_upg_dwellings", "")]
        [TestCase("other_factions_42_city_hall", "other_factions_41_additional_3", "*=City Hall")]
        public void EachSaveAddsTheBuildingsInTheirNames(string after, string before, string additions)
        {
            var expected = additions.Length == 0
                ? []
                : additions.Split(';').Select(a => a.Split('=')).SelectMany(a => a[1].Split('|').Select(b => (Castle: a[0], Building: b))).ToList();

            foreach (var (name, _) in Castles)
            {
                var wanted = expected.Where(e => e.Castle == "*" || e.Castle == name).Select(e => e.Building);
                Assert.That(Added(Castle(after, name), Castle(before, name)), Is.EquivalentTo(wanted), name);
            }
        }

        [Test]
        public void TheCastlesAreOfTheirFactionsAndBelongToRed()
        {
            foreach (var (name, faction) in Castles)
            {
                var castle = Castle("other_factions_00_baseline", name);

                Assert.That(castle.Faction, Is.EqualTo(faction), name);
                Assert.That(castle.Owner, Is.EqualTo(PlayerColor.Red), name);
                Assert.That(castle.MageGuildLevel, Is.EqualTo(0), name);
                Assert.That(castle.Buildings, Does.Contain(Building.VillageHall), name);
            }
        }

        [TestCase("other_factions_10_mage_guild_1", 1)]
        [TestCase("other_factions_11_mage_guild_2", 2)]
        [TestCase("other_factions_12_mage_guild_3", 3)]
        [TestCase("other_factions_42_city_hall", 3)]
        public void TheMageGuildLevelFollowsTheMageGuildBuilt(string save, int level)
        {
            foreach (var (name, _) in Castles)
            {
                Assert.That(Castle(save, name).MageGuildLevel, Is.EqualTo(level), name);
            }
        }

        [Test]
        public void TheUpgradedHordeBuildingsAppearWithTheUpgradeOfTheirDwellings()
        {
            Assert.That(Castle("other_factions_34_upg_dwelling_2", "Corona").Buildings, Does.Contain(Building.TowerUpgSculptorsWings));
            Assert.That(Castle("other_factions_32_additional_2", "Castigare").Buildings, Does.Not.Contain(Building.DungeonUpgMushroomRings));
            Assert.That(Castle("other_factions_33_upg_dwelling_1", "Castigare").Buildings, Does.Contain(Building.DungeonUpgMushroomRings));
            Assert.That(Castle("other_factions_33_upg_dwelling_1", "Enkindle").Buildings, Does.Contain(Building.InfernoUpgBirthingPool));
            Assert.That(Castle("other_factions_33_upg_dwelling_1", "Grave Raven").Buildings, Does.Contain(Building.NecropolisUpgUnearthedGraves));
            Assert.That(Castle("other_factions_33_upg_dwelling_1", "Kruber").Buildings, Does.Contain(Building.StrongholdUpgMessHall));
            Assert.That(Castle("other_factions_33_upg_dwelling_1", "Deadfall").Buildings, Does.Contain(Building.FortressUpgCageOfWarlords));
            Assert.That(Castle("other_factions_33_upg_dwelling_1", "Wazzar").Buildings, Does.Contain(Building.ConfluxUpgGardenOfLife));
            Assert.That(Castle("other_factions_35_upg_dwelling_3", "Highcastle").Buildings, Does.Contain(Building.CastleUpgGriffinBastion));
        }

        [Test]
        public void OnlyTheCastleBuiltTheShipyardAndTheLighthouseThatNeedsIt()
        {
            foreach (var (name, _) in Castles)
            {
                Assert.That(Castle("other_factions_42_city_hall", name).Buildings.Contains(Building.Shipyard), Is.EqualTo(name == "Highcastle"), name);
            }

            Assert.That(Castle("other_factions_26_special_4", "Highcastle").Buildings, Does.Contain(Building.Shipyard).And.Not.Contain(Building.CastleLighthouse));
            Assert.That(Building.CastleLighthouse.Prerequisites, Is.EquivalentTo(new[] { Building.Shipyard }));
        }

        [Test]
        public void TheCastleNeedsItsStablesBeforeItsLevelSixDwelling()
        {
            Assert.That(Building.CastleTrainingGrounds.Prerequisites, Is.EquivalentTo(new[] { Building.CastleStables }));
            Assert.That(Building.CastleStables.Prerequisites, Is.EquivalentTo(new[] { Building.CastleBarracks }));

            // Every other faction had its level 6 dwelling in this save; the Castle could not.
            Assert.That(Castle("other_factions_19_dwelling_6_before_stables", "Highcastle").Buildings, Does.Not.Contain(Building.CastleStables).And.Not.Contain(Building.CastleTrainingGrounds));
            Assert.That(Castle("other_factions_38_upg_dwelling_6", "Highcastle").Buildings, Does.Contain(Building.CastleStables).And.Contain(Building.CastleTrainingGrounds));
        }

        [Test]
        public void EveryBuildingBuiltHasItsPrerequisitesBuilt()
        {
            foreach (var save in Directory.GetFiles(Path.Combine("TestData", "Hota", "OtherFactions"), "*.GM1").Select(Path.GetFileNameWithoutExtension).Where(f => f != "other_factions_editor_dung_necro"))
            {
                foreach (var (name, _) in Castles)
                {
                    var castle = Castle(save!, name);
                    foreach (var building in castle.Buildings)
                    {
                        Assert.That(castle.Buildings, Is.SupersetOf(building.Prerequisites), $"{save} {name} {building.Name}");
                    }
                }
            }
        }

        [Test]
        public void TheDungeonAndNecropolisBuildingsAreAloneInTheirSave()
        {
            // dung_necro holds three Necropolis castles and two Dungeon castles with nothing built but a fort, the dwelling and one more building, placed with the map editor.
            Assert.That(Castle("other_factions_editor_dung_necro", "Nothing").Faction, Is.EqualTo(Faction.Necropolis));
            Assert.That(Castle("other_factions_editor_dung_necro", "Nothing").Buildings, Is.EquivalentTo(new[] { Building.Fort, Building.VillageHall, Building.NecropolisCursedTemple }));
            Assert.That(Added(Castle("other_factions_editor_dung_necro", "Graves"), Castle("other_factions_editor_dung_necro", "Nothing")), Is.EquivalentTo(new[] { "Unearthed Graves" }));
            Assert.That(Added(Castle("other_factions_editor_dung_necro", "Transf"), Castle("other_factions_editor_dung_necro", "Nothing")), Is.EquivalentTo(new[] { "Skeleton Transformer" }));
            Assert.That(Castle("other_factions_editor_dung_necro", "NoRing").Faction, Is.EqualTo(Faction.Dungeon));
            Assert.That(Added(Castle("other_factions_editor_dung_necro", "WithRing"), Castle("other_factions_editor_dung_necro", "NoRing")), Is.EquivalentTo(new[] { "Mushroom Rings" }));

            // The castles of dung_necro were built in the map editor, which ignores prerequisites; in the game the Unearthed Graves need the Skeleton Transformer.
            Assert.That(Building.NecropolisUnearthedGraves.Prerequisites, Is.EquivalentTo(new[] { Building.NecropolisCursedTemple, Building.NecropolisSkeletonTransformer }));
            Assert.That(Building.NecropolisSkeletonTransformer.Prerequisites, Is.EquivalentTo(new[] { Building.NecropolisCursedTemple }));
        }

        [Test]
        public void TheNeutralCastlesNeverChange()
        {
            foreach (var name in new[] { "Fac Castle S", "Fac Castle G", "Strongglen" })
            {
                Assert.That(Castle("other_factions_42_city_hall", name).Buildings, Is.EquivalentTo(Castle("other_factions_00_baseline", name).Buildings), name);
            }
        }
    }
}
