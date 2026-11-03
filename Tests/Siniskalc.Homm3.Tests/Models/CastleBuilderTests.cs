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
            var castle = CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Bulwark"));

            Assert.That(CastleBuilder.GetBuildFailureReason(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Citadel")), Is.EqualTo(CastleBuildFailureReason.UpgradedBuildingMissing));
            Assert.That(CastleBuilder.GetBuildFailureReason(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Fort")), Is.EqualTo(CastleBuildFailureReason.None));
            Assert.Throws<GameRuleException>(() => CastleBuilder.Build(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Citadel")));
            Assert.That(CastleBuilder.Build(CastleBuilder.Build(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Fort")), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Citadel")).VisibleBuildings,
                Does.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Citadel")).And.Not.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Fort")));
        }

        [Test]
        public void ABuildingOfAnotherFactionCannotBeBuilt()
        {
            var castle = CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Bulwark"));

            Assert.That(CastleBuilder.GetBuildFailureReason(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("CastleGuardhouse")), Is.Not.EqualTo(CastleBuildFailureReason.None));
        }

        [Test]
        public void ABuildingTheMapDoesNotAllowCannotBeBuilt()
        {
            var castle = CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Bulwark"));
            var forbidden = castle with { EnabledBuildings = castle.EnabledBuildings.Except([TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Fort")]).ToHashSet() };

            Assert.That(CastleBuilder.GetBuildFailureReason(forbidden, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Fort")), Is.Not.EqualTo(CastleBuildFailureReason.None));
        }

        [Test]
        public void AVillageHallAndABuildingAnotherOneNeedsCannotBeRemoved()
        {
            var castle = CastleModels.With(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Bulwark")), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Fort"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Citadel"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Tavern"));

            Assert.That(CastleBuilder.GetUnbuildFailureReason(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Fort")), Is.EqualTo(CastleBuildFailureReason.NeededByUpgrade));
            Assert.That(CastleBuilder.UpgradesBuiltOn(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Fort")), Is.EqualTo(new[] { TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Citadel") }));
            Assert.That(CastleBuilder.GetUnbuildFailureReason(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("VillageHall")), Is.Not.EqualTo(CastleBuildFailureReason.None));
            Assert.That(CastleBuilder.GetUnbuildFailureReason(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Tavern")), Is.EqualTo(CastleBuildFailureReason.None));
            Assert.Throws<GameRuleException>(() => CastleBuilder.Unbuild(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Fort")));
        }

        [Test]
        public void TheLighthouseAndTheShipyardCanBeBuiltAndRemovedInEitherOrder()
        {
            var castle = CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Castle"));

            var built = CastleBuilder.Build(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("CastleLighthouse"));

            Assert.That(built.HasBuilt(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("CastleLighthouse")), Is.True);
            Assert.That(CastleBuilder.UnmetRequirements(built, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("CastleLighthouse")), Is.EqualTo(new[] { TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Shipyard") }));
            var both = CastleBuilder.Build(built, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Shipyard"));
            Assert.That(CastleBuilder.GetUnbuildFailureReason(both, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Shipyard")), Is.EqualTo(CastleBuildFailureReason.None));
            Assert.That(CastleBuilder.Unbuild(both, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Shipyard")).HasBuilt(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("CastleLighthouse")), Is.True);
        }

        [Test]
        public void ADwellingDoesNotNeedTheFort()
        {
            Assert.That(CastleBuilder.GetBuildFailureReason(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Castle")), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("CastleGuardhouse")), Is.EqualTo(CastleBuildFailureReason.None));
        }

        [Test]
        public void AnUpgradedDwellingNeedsItsDwellingAndTheDwellingStaysWhileTheUpgradeStands()
        {
            var village = CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Rampart"));
            var both = CastleModels.With(village, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartDwarfCottage"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgDwarfCottage"));

            Assert.That(CastleBuilder.GetBuildFailureReason(village, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgDwarfCottage")), Is.EqualTo(CastleBuildFailureReason.UpgradedBuildingMissing));
            Assert.Throws<GameRuleException>(() => CastleBuilder.Build(village, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgDwarfCottage")));
            Assert.That(CastleBuilder.GetUnbuildFailureReason(both, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartDwarfCottage")), Is.EqualTo(CastleBuildFailureReason.NeededByUpgrade));
            Assert.That(CastleBuilder.GetUnbuildFailureReason(both, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgDwarfCottage")), Is.EqualTo(CastleBuildFailureReason.None));
        }

        [Test]
        public void TheUpgradedHordeBuildingComesWithTheUpgradeOfItsDwellingInEitherOrder()
        {
            var castle = CastleModels.With(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Rampart")), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartDwarfCottage"));

            var hordeFirst = CastleBuilder.Build(CastleBuilder.Build(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartMinersGuild")), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgDwarfCottage"));
            Assert.That(hordeFirst.Buildings, Does.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgMinersGuild")));
            Assert.That(hordeFirst.VisibleBuildings, Does.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgMinersGuild")).And.Not.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartMinersGuild")).And.Not.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgDwarfCottage")));

            var upgradeFirst = CastleBuilder.SetBuilt(CastleBuilder.SetBuilt(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgDwarfCottage"), true), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartMinersGuild"), true);
            Assert.That(upgradeFirst.VisibleBuildings, Is.EquivalentTo(hordeFirst.VisibleBuildings));

            var removed = CastleBuilder.Unbuild(hordeFirst, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartMinersGuild"));
            Assert.That(removed.Buildings, Does.Not.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgMinersGuild")));
            Assert.That(removed.VisibleBuildings, Does.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgDwarfCottage")));
        }

        [Test]
        public void TheMageGuildCanBeBuiltInOneGoAndRemovedAgain()
        {
            var baseline = CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Rampart"));

            var guild = CastleBuilder.SetMageGuildLevel(baseline, 5);
            Assert.That(guild.MageGuildLevel, Is.EqualTo(5));
            Assert.That(guild.MageGuildSpellCounts, Is.EqualTo(new[] { 5, 4, 3, 2, 1 }));
            Assert.That(guild.VisibleBuildings, Does.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("MagesGuild5")).And.Not.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("MagesGuild4")));

            var smaller = CastleBuilder.SetMageGuildLevel(guild, 2);
            Assert.That(smaller.MageGuildLevel, Is.EqualTo(2));
            Assert.That(smaller.MageGuildSpellCounts, Is.EqualTo(new[] { 5, 4, 0, 0, 0 }));
            Assert.That(smaller.VisibleBuildings, Does.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("MagesGuild2")).And.Not.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("MagesGuild3")));

            var none = CastleBuilder.SetMageGuildLevel(smaller, 0);
            Assert.That(none.VisibleBuildings, Is.EquivalentTo(baseline.VisibleBuildings));
            Assert.That(none.MageGuildSpellCounts, Is.All.EqualTo(0));
        }

        [Test]
        public void TheMageGuildOfBulwarkStopsAtLevelFour()
        {
            var castle = CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Bulwark"));

            Assert.That(CastleBuilder.SetMageGuildLevel(castle, 4).MageGuildLevel, Is.EqualTo(4));
            Assert.Throws<GameRuleException>(() => CastleBuilder.SetMageGuildLevel(castle, 5));
        }

        [Test]
        public void ADwellingStartsWithOneWeekOfCreaturesAndEmptiesWhenRemoved()
        {
            var built = CastleBuilder.Build(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Rampart")), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables"));

            Assert.That(built.CreaturesAvailable[0], Is.EqualTo(14));

            var removed = CastleBuilder.Unbuild(built, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables"));
            Assert.That(removed.CreaturesAvailable[0], Is.EqualTo(0));
        }
    }
}
