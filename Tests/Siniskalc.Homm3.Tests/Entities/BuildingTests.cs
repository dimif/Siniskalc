using Siniskalc.Homm3.Entities;

namespace Siniskalc.Homm3.Tests.Entities
{
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class BuildingTests
    {
        private static readonly Faction[] OriginalFactions =
        [
            TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Castle"), TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Rampart"), TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Tower"), TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Inferno"), TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Necropolis"),
            TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Dungeon"), TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Stronghold"), TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Fortress"), TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Conflux"),
        ];

        [Test]
        public void NoTwoBuildingsOfAFactionShareABit()
        {
            foreach (var faction in OriginalFactions)
            {
                var buildings = Building.ForFaction(faction);

                Assert.That(buildings.Select(b => b.Code), Is.Unique, faction.Name);
            }
        }

        [Test]
        public void BuildingsOfDifferentFactionsAreNotEqualEvenWithTheSameBit()
        {
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("CastleGuardhouse").Code, Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("TowerWorkshop").Code));
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("CastleGuardhouse"), Is.Not.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("TowerWorkshop")));
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("CastleGuardhouse") == TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("TowerWorkshop"), Is.False);
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.All.Distinct().Count(), Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.All.Count));
        }

        [Test]
        public void EveryPrerequisiteBelongsToTheSameFactionOrToAll()
        {
            foreach (var building in TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.All)
            {
                foreach (var prerequisite in building.Prerequisites)
                {
                    Assert.That(prerequisite, Is.Not.Null, building.Name);
                    Assert.That(prerequisite.Faction == null || prerequisite.Faction == building.Faction, Is.True, $"{building.Name} requires {prerequisite.Name}");
                    Assert.That(building.Faction == null ? prerequisite.Faction : null, Is.Null, $"{building.Name} is common but requires {prerequisite.Name}");
                }
            }
        }

        [Test]
        public void EveryPrerequisiteIsAvailableWhereTheBuildingIs()
        {
            foreach (var faction in OriginalFactions)
            {
                var available = Building.ForFaction(faction);
                foreach (var building in available)
                {
                    Assert.That(building.Prerequisites, Is.SubsetOf(available), $"{faction.Name}: {building.Name}");
                }
            }
        }

        [Test]
        public void PrerequisitesDoNotLoopBack()
        {
            foreach (var building in TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.All)
            {
                var seen = new HashSet<Building>();
                var pending = new Stack<Building>(building.Prerequisites);
                while (pending.Count > 0)
                {
                    var next = pending.Pop();
                    Assert.That(next, Is.Not.EqualTo(building), building.Name);
                    if (seen.Add(next))
                    {
                        foreach (var prerequisite in next.Prerequisites)
                        {
                            pending.Push(prerequisite);
                        }
                    }
                }
            }
        }

        [Test]
        public void TheHallsUpgradeOneAnother()
        {
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("TownHall").Upgrades, Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("VillageHall")));
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("CityHall").Upgrades, Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("TownHall")));
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Capitol").Upgrades, Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("CityHall")));
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Capitol").Prerequisites, Is.EquivalentTo(new[] { TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("CityHall"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Castle") }));
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("CityHall").Requires, Is.EquivalentTo(new[] { TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("MagesGuild1"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Marketplace"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Blacksmith") }));
        }

        [Test]
        public void TheCloudTempleIsTheSeventhDwellingOfTheTower()
        {
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("TowerCloudTemple").Code, Is.EqualTo(36));
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("TowerCloudTemple").Faction, Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Tower")));
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("TowerCloudTemple").Prerequisites, Is.EquivalentTo(new[] { TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("TowerGoldenPavilion"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("TowerAltarOfWishes") }));
            Assert.That(Building.FromBit(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Tower"), 36), Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("TowerCloudTemple")));
            Assert.That(Building.FromBit(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Castle"), 36), Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("CastlePortalOfGlory")));
        }

        [Test]
        public void OnlyTheFactionsThatHaveAMageGuildLevelOrShipyardOfferIt()
        {
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("MagesGuild5").IsAvailableFor(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Tower")), Is.True);
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("MagesGuild5").IsAvailableFor(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Castle")), Is.False);
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("MagesGuild4").IsAvailableFor(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Castle")), Is.True);
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("MagesGuild4").IsAvailableFor(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Stronghold")), Is.False);
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Shipyard").IsAvailableFor(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Castle")), Is.True);
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Shipyard").IsAvailableFor(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Tower")), Is.False);
        }

        [Test]
        public void ConfluxBuildingsAreFromArmageddonsBlade()
        {
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.All.Where(b => b.Faction == TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Conflux")), Has.All.Matches<Building>(b => b.GameVersion == GameVersion.ArmageddonsBlade));
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.All.Where(b => b.Faction != TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Conflux") && b.GameVersion != GameVersion.HornOfTheAbyss), Has.All.Matches<Building>(b => b.GameVersion == GameVersion.RestorationOfErathia));
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.All.Where(b => b.Faction == TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Cove")), Has.All.Matches<Building>(b => b.GameVersion == GameVersion.HornOfTheAbyss));
        }
    }
}
