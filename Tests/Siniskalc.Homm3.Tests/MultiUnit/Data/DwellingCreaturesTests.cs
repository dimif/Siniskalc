using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;

namespace Siniskalc.Homm3.Tests.MultiUnit.Data
{
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class DwellingCreaturesTests
    {
        private static readonly Faction[] Factions =
            [TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Castle"), TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Rampart"), TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Tower"), TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Inferno"), TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Necropolis"), TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Dungeon"), TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Stronghold"), TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Fortress"), TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Conflux"),
             TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Cove"), TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Factory"), TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Bulwark")];

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
            Assert.That(DwellingCreatures.NamesOf(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Stronghold"), 6, hasUpgrade: true), Is.EqualTo(@"Behemoth\Ancient Behemoth"));
            Assert.That(DwellingCreatures.NamesOf(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Stronghold"), 6, hasUpgrade: false), Is.EqualTo("Behemoth"));
            Assert.That(DwellingCreatures.Of(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("StrongholdUpgBehemothLair"))!.Name, Is.EqualTo("Ancient Behemoth"));
            Assert.That(DwellingCreatures.Of(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("TownHall")), Is.Null);
        }

        [Test]
        public void TheQuirksOfHotaAreResolvedByTheData()
        {
            Assert.That(DwellingCreatures.NamesOf(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Cove"), 2, hasUpgrade: true), Is.EqualTo(@"Pirate\Corsair"));
            Assert.That(DwellingCreatures.NamesOf(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Factory"), 6, hasUpgrade: true), Is.EqualTo(@"Couatl\Crimson Couatl"));
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("SeaDog").Dwelling, Is.Null);
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Gantry").Dwelling, Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("FactoryGantry")));
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.All.Where(c => c.Dwelling != null).GroupBy(c => c.Dwelling).Where(g => g.Count() > 1), Is.Empty, "one creature per building");
        }

        [Test]
        public void NeutralCreaturesHaveNoDwelling()
        {
            Assert.That(Creature.ForFaction(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Neutral")).Select(c => c.Dwelling), Is.All.Null);
        }
    }
}
