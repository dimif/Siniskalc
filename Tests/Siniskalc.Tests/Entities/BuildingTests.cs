using Siniskalc;
using Siniskalc.Entities;

namespace Siniskalc.Tests.Entities
{
    [TestFixture]
    internal sealed class BuildingTests
    {
        private static readonly Faction[] OriginalFactions =
        [
            Faction.Castle, Faction.Rampart, Faction.Tower, Faction.Inferno, Faction.Necropolis,
            Faction.Dungeon, Faction.Stronghold, Faction.Fortress, Faction.Conflux,
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
            Assert.That(Building.CastleGuardhouse.Code, Is.EqualTo(Building.TowerWorkshop.Code));
            Assert.That(Building.CastleGuardhouse, Is.Not.EqualTo(Building.TowerWorkshop));
            Assert.That(Building.CastleGuardhouse == Building.TowerWorkshop, Is.False);
            Assert.That(Building.All.Distinct().Count(), Is.EqualTo(Building.All.Count));
        }

        [Test]
        public void EveryPrerequisiteBelongsToTheSameFactionOrToAll()
        {
            foreach (var building in Building.All)
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
            foreach (var building in Building.All)
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
            Assert.That(Building.TownHall.Upgrades, Is.EqualTo(Building.VillageHall));
            Assert.That(Building.CityHall.Upgrades, Is.EqualTo(Building.TownHall));
            Assert.That(Building.Capitol.Upgrades, Is.EqualTo(Building.CityHall));
            Assert.That(Building.Capitol.Prerequisites, Is.EquivalentTo(new[] { Building.CityHall, Building.Castle }));
            Assert.That(Building.CityHall.Requires, Is.EquivalentTo(new[] { Building.MagesGuild1, Building.Marketplace, Building.Blacksmith }));
        }

        [Test]
        public void TheCloudTempleIsTheSeventhDwellingOfTheTower()
        {
            Assert.That(Building.TowerCloudTemple.Code, Is.EqualTo(36));
            Assert.That(Building.TowerCloudTemple.Faction, Is.EqualTo(Faction.Tower));
            Assert.That(Building.TowerCloudTemple.Prerequisites, Is.EquivalentTo(new[] { Building.TowerGoldenPavilion, Building.TowerAltarOfWishes }));
            Assert.That(Building.FromBit(Faction.Tower, 36), Is.EqualTo(Building.TowerCloudTemple));
            Assert.That(Building.FromBit(Faction.Castle, 36), Is.EqualTo(Building.CastlePortalOfGlory));
        }

        [Test]
        public void OnlyTheFactionsThatHaveAMageGuildLevelOrShipyardOfferIt()
        {
            Assert.That(Building.MagesGuild5.IsAvailableFor(Faction.Tower), Is.True);
            Assert.That(Building.MagesGuild5.IsAvailableFor(Faction.Castle), Is.False);
            Assert.That(Building.MagesGuild4.IsAvailableFor(Faction.Castle), Is.True);
            Assert.That(Building.MagesGuild4.IsAvailableFor(Faction.Stronghold), Is.False);
            Assert.That(Building.Shipyard.IsAvailableFor(Faction.Castle), Is.True);
            Assert.That(Building.Shipyard.IsAvailableFor(Faction.Tower), Is.False);
        }

        [Test]
        public void ConfluxBuildingsAreFromArmageddonsBlade()
        {
            Assert.That(Building.All.Where(b => b.Faction == Faction.Conflux), Has.All.Matches<Building>(b => b.GameVersion == GameVersion.ArmageddonsBlade));
            Assert.That(Building.All.Where(b => b.Faction != Faction.Conflux && b.GameVersion != GameVersion.HornOfTheAbyss), Has.All.Matches<Building>(b => b.GameVersion == GameVersion.RestorationOfErathia));
            Assert.That(Building.All.Where(b => b.Faction == Faction.Cove), Has.All.Matches<Building>(b => b.GameVersion == GameVersion.HornOfTheAbyss));
        }
    }
}
