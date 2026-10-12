using Siniskalc.Data;
using Siniskalc.Entities;

namespace Siniskalc.Tests.Data
{
    [TestFixture]
    internal sealed class DwellingCreaturesTests
    {
        private static readonly Faction[] Factions =
            [Faction.Castle, Faction.Rampart, Faction.Tower, Faction.Inferno, Faction.Necropolis, Faction.Dungeon, Faction.Stronghold, Faction.Fortress, Faction.Conflux,
             Faction.Cove, Faction.Factory, Faction.Bulwark];

        [Test]
        public void EveryDwellingOfEveryFactionServesACreatureOfItsLevel([ValueSource(nameof(Factions))] Faction faction)
        {
            foreach (var building in Building.ForFaction(faction).Where(b => b.IsDwelling || (b.Faction != null && b.Code is >= 37 and <= 43)))
            {
                var creature = DwellingCreatures.Of(building);

                Assert.That(creature, Is.Not.Null, $"{faction.Name}: {building.Name}");
                Assert.That(creature!.Faction, Is.EqualTo(faction));
            }
        }

        [Test]
        public void NamesAreBothCreaturesWhenTheUpgradeIsBuilt()
        {
            Assert.That(DwellingCreatures.NamesOf(Faction.Stronghold, 6, hasUpgrade: true), Is.EqualTo(@"Behemoth\Ancient Behemoth"));
            Assert.That(DwellingCreatures.NamesOf(Faction.Stronghold, 6, hasUpgrade: false), Is.EqualTo("Behemoth"));
            Assert.That(DwellingCreatures.Of(Building.StrongholdUpgBehemothLair)!.Name, Is.EqualTo("Ancient Behemoth"));
            Assert.That(DwellingCreatures.Of(Building.TownHall), Is.Null);
        }

        [Test]
        public void TheQuirksOfHotaAreResolvedByTheData()
        {
            Assert.That(DwellingCreatures.NamesOf(Faction.Cove, 2, hasUpgrade: true), Is.EqualTo(@"Pirate\Corsair"));
            Assert.That(DwellingCreatures.NamesOf(Faction.Factory, 6, hasUpgrade: true), Is.EqualTo(@"Couatl\Crimson Couatl"));
            Assert.That(Creature.SeaDog.Dwelling, Is.Null);
            Assert.That(Creature.Gantry.Dwelling, Is.EqualTo(Building.FactoryGantry));
            Assert.That(Creature.All.Where(c => c.Dwelling != null).GroupBy(c => c.Dwelling).Where(g => g.Count() > 1), Is.Empty, "one creature per building");
        }

        [Test]
        public void NeutralCreaturesHaveNoDwelling()
        {
            Assert.That(Creature.ForFaction(Faction.Neutral).Select(c => c.Dwelling), Is.All.Null);
        }
    }
}
