using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Models
{
    /// <summary>The rules of <see cref="CastleBuilder"/> on castles built in memory.</summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class CastleBuilderTests
    {
        [Test]
        public void AnUpgradeNeedsTheBuildingItUpgrades()
        {
            var castle = CastleModels.Village(Faction.Bulwark);

            Assert.That(CastleBuilder.GetBuildFailureReason(castle, Building.Citadel), Is.EqualTo(CastleBuildFailureReason.UpgradedBuildingMissing));
            Assert.That(CastleBuilder.GetBuildFailureReason(castle, Building.Fort), Is.EqualTo(CastleBuildFailureReason.None));
            Assert.Throws<GameRuleException>(() => CastleBuilder.Build(castle, Building.Citadel));
            Assert.That(CastleBuilder.Build(CastleBuilder.Build(castle, Building.Fort), Building.Citadel).VisibleBuildings,
                Does.Contain(Building.Citadel).And.Not.Contain(Building.Fort));
        }

        [Test]
        public void ABuildingOfAnotherFactionCannotBeBuilt()
        {
            var castle = CastleModels.Village(Faction.Bulwark);

            Assert.That(CastleBuilder.GetBuildFailureReason(castle, Building.CastleGuardhouse), Is.Not.EqualTo(CastleBuildFailureReason.None));
        }

        [Test]
        public void ABuildingTheMapDoesNotAllowCannotBeBuilt()
        {
            var castle = CastleModels.Village(Faction.Bulwark);
            var forbidden = castle with { EnabledBuildings = castle.EnabledBuildings.Except([Building.Fort]).ToHashSet() };

            Assert.That(CastleBuilder.GetBuildFailureReason(forbidden, Building.Fort), Is.Not.EqualTo(CastleBuildFailureReason.None));
        }

        [Test]
        public void AVillageHallAndABuildingAnotherOneNeedsCannotBeRemoved()
        {
            var castle = CastleModels.With(CastleModels.Village(Faction.Bulwark), Building.Fort, Building.Citadel, Building.Tavern);

            Assert.That(CastleBuilder.GetUnbuildFailureReason(castle, Building.Fort), Is.EqualTo(CastleBuildFailureReason.NeededByUpgrade));
            Assert.That(CastleBuilder.UpgradesBuiltOn(castle, Building.Fort), Is.EqualTo(new[] { Building.Citadel }));
            Assert.That(CastleBuilder.GetUnbuildFailureReason(castle, Building.VillageHall), Is.Not.EqualTo(CastleBuildFailureReason.None));
            Assert.That(CastleBuilder.GetUnbuildFailureReason(castle, Building.Tavern), Is.EqualTo(CastleBuildFailureReason.None));
            Assert.Throws<GameRuleException>(() => CastleBuilder.Unbuild(castle, Building.Fort));
        }

        [Test]
        public void TheLighthouseAndTheShipyardCanBeBuiltAndRemovedInEitherOrder()
        {
            var castle = CastleModels.Village(Faction.Castle);

            var built = CastleBuilder.Build(castle, Building.CastleLighthouse);

            Assert.That(built.HasBuilt(Building.CastleLighthouse), Is.True);
            Assert.That(CastleBuilder.UnmetRequirements(built, Building.CastleLighthouse), Is.EqualTo(new[] { Building.Shipyard }));
            var both = CastleBuilder.Build(built, Building.Shipyard);
            Assert.That(CastleBuilder.GetUnbuildFailureReason(both, Building.Shipyard), Is.EqualTo(CastleBuildFailureReason.None));
            Assert.That(CastleBuilder.Unbuild(both, Building.Shipyard).HasBuilt(Building.CastleLighthouse), Is.True);
        }

        [Test]
        public void ADwellingDoesNotNeedTheFort()
        {
            Assert.That(CastleBuilder.GetBuildFailureReason(CastleModels.Village(Faction.Castle), Building.CastleGuardhouse), Is.EqualTo(CastleBuildFailureReason.None));
        }

        [Test]
        public void AnUpgradedDwellingNeedsItsDwellingAndTheDwellingStaysWhileTheUpgradeStands()
        {
            var village = CastleModels.Village(Faction.Rampart);
            var both = CastleModels.With(village, Building.RampartDwarfCottage, Building.RampartUpgDwarfCottage);

            Assert.That(CastleBuilder.GetBuildFailureReason(village, Building.RampartUpgDwarfCottage), Is.EqualTo(CastleBuildFailureReason.UpgradedBuildingMissing));
            Assert.Throws<GameRuleException>(() => CastleBuilder.Build(village, Building.RampartUpgDwarfCottage));
            Assert.That(CastleBuilder.GetUnbuildFailureReason(both, Building.RampartDwarfCottage), Is.EqualTo(CastleBuildFailureReason.NeededByUpgrade));
            Assert.That(CastleBuilder.GetUnbuildFailureReason(both, Building.RampartUpgDwarfCottage), Is.EqualTo(CastleBuildFailureReason.None));
        }

        [Test]
        public void TheUpgradedHordeBuildingComesWithTheUpgradeOfItsDwellingInEitherOrder()
        {
            var castle = CastleModels.With(CastleModels.Village(Faction.Rampart), Building.RampartDwarfCottage);

            var hordeFirst = CastleBuilder.Build(CastleBuilder.Build(castle, Building.RampartMinersGuild), Building.RampartUpgDwarfCottage);
            Assert.That(hordeFirst.Buildings, Does.Contain(Building.RampartUpgMinersGuild));
            Assert.That(hordeFirst.VisibleBuildings, Does.Contain(Building.RampartUpgMinersGuild).And.Not.Contain(Building.RampartMinersGuild).And.Not.Contain(Building.RampartUpgDwarfCottage));

            var upgradeFirst = CastleBuilder.SetBuilt(CastleBuilder.SetBuilt(castle, Building.RampartUpgDwarfCottage, true), Building.RampartMinersGuild, true);
            Assert.That(upgradeFirst.VisibleBuildings, Is.EquivalentTo(hordeFirst.VisibleBuildings));

            var removed = CastleBuilder.Unbuild(hordeFirst, Building.RampartMinersGuild);
            Assert.That(removed.Buildings, Does.Not.Contain(Building.RampartUpgMinersGuild));
            Assert.That(removed.VisibleBuildings, Does.Contain(Building.RampartUpgDwarfCottage));
        }

        [Test]
        public void TheMageGuildCanBeBuiltInOneGoAndRemovedAgain()
        {
            var baseline = CastleModels.Village(Faction.Rampart);

            var guild = CastleBuilder.SetMageGuildLevel(baseline, 5);
            Assert.That(guild.MageGuildLevel, Is.EqualTo(5));
            Assert.That(guild.MageGuildSpellCounts, Is.EqualTo(new[] { 5, 4, 3, 2, 1 }));
            Assert.That(guild.VisibleBuildings, Does.Contain(Building.MagesGuild5).And.Not.Contain(Building.MagesGuild4));

            var smaller = CastleBuilder.SetMageGuildLevel(guild, 2);
            Assert.That(smaller.MageGuildLevel, Is.EqualTo(2));
            Assert.That(smaller.MageGuildSpellCounts, Is.EqualTo(new[] { 5, 4, 0, 0, 0 }));
            Assert.That(smaller.VisibleBuildings, Does.Contain(Building.MagesGuild2).And.Not.Contain(Building.MagesGuild3));

            var none = CastleBuilder.SetMageGuildLevel(smaller, 0);
            Assert.That(none.VisibleBuildings, Is.EquivalentTo(baseline.VisibleBuildings));
            Assert.That(none.MageGuildSpellCounts, Is.All.EqualTo(0));
        }

        [Test]
        public void TheMageGuildOfBulwarkStopsAtLevelFour()
        {
            var castle = CastleModels.Village(Faction.Bulwark);

            Assert.That(CastleBuilder.SetMageGuildLevel(castle, 4).MageGuildLevel, Is.EqualTo(4));
            Assert.Throws<GameRuleException>(() => CastleBuilder.SetMageGuildLevel(castle, 5));
        }

        [Test]
        public void ADwellingStartsWithOneWeekOfCreaturesAndEmptiesWhenRemoved()
        {
            var built = CastleBuilder.Build(CastleModels.Village(Faction.Rampart), Building.RampartCentaurStables);

            Assert.That(built.CreaturesAvailable[0], Is.EqualTo(14));

            var removed = CastleBuilder.Unbuild(built, Building.RampartCentaurStables);
            Assert.That(removed.CreaturesAvailable[0], Is.EqualTo(0));
        }
    }
}
