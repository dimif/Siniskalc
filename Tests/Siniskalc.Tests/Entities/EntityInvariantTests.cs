using Siniskalc;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;

namespace Siniskalc.Tests.Entities
{
    /// <summary>
    /// Checks that the explicit tags on entities (building type, hero class type, abilities) agree with the data they were derived from.
    /// </summary>
    [TestFixture]
    internal sealed class EntityInvariantTests
    {
        private static readonly Faction[] AllFactions = Faction.All.Where(f => f != Faction.Neutral).ToArray();

        [Test]
        public void ABuildingsTypeAgreesWithItsBit()
        {
            foreach (var building in Building.All)
            {
                var common = building.Faction == null;
                Assert.That(building.IsMageGuild, Is.EqualTo(common && building.Code < 5), building.Name);
                Assert.That(building.IsDwelling, Is.EqualTo(!common && building.Code is >= 30 and <= 36), building.Name);
                Assert.That(building.IsUpgradedDwelling, Is.EqualTo(!common && building.Code is >= 37 and <= 43), building.Name);
                Assert.That(building.IsGrail, Is.EqualTo(!common && building.Code == 26), building.Name);
                if (building.DwellingLevel is { } level)
                {
                    Assert.That(building.Code, Is.EqualTo((building.IsDwelling ? 30 : 37) + level), building.Name);
                }
            }
        }

        [Test]
        public void NoFactionHasTwoBuildingsOfOneType()
        {
            foreach (var faction in AllFactions)
            {
                var buildings = Building.ForFaction(faction).Where(b => b.Faction != null);

                Assert.That(buildings.Select(b => b.Type), Is.Unique, faction.Name);
                Assert.That(Building.ForFaction(faction).Select(b => b.Code), Is.Unique, faction.Name);
            }
        }

        [Test]
        public void EveryFactionHasAllSevenDwellingsAndTheirUpgrades()
        {
            foreach (var faction in AllFactions)
            {
                for (var level = 0; level < Building.DwellingLevelCount; level++)
                {
                    Assert.That(Building.FromDwellingLevel(faction, level, upgraded: false), Is.Not.Null, $"{faction.Name} {level}");
                    Assert.That(Building.FromDwellingLevel(faction, level, upgraded: true), Is.Not.Null, $"{faction.Name} {level}");
                }
            }
        }

        [Test]
        public void ACreatureIsRangedExactlyWhenItHasShots()
        {
            foreach (var creature in Creature.All)
            {
                foreach (var info in new[] { creature.Info }.Concat(creature.VersionInfo.Values))
                {
                    var ranged = info.Special?.Contains(CreatureAbility.Ranged) ?? false;
                    Assert.That(ranged, Is.EqualTo(info.Shots > 0), creature.Name);
                }
            }
        }

        [Test]
        public void EachFactionHasOneMightAndOneMagicClass()
        {
            foreach (var faction in AllFactions)
            {
                var classes = HeroClass.All.Where(c => c.Faction == faction).ToList();

                Assert.That(classes.Select(c => c.Type), Is.EquivalentTo(new[] { HeroClassType.Might, HeroClassType.Magic }), faction.Name);
            }
        }
    }
}
