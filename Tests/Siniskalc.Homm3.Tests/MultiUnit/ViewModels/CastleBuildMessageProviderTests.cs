using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Tests.Models;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.MultiUnit.ViewModels
{
    /// <summary>The words <see cref="CastleBuildMessageProvider"/> gives for the reasons of <see cref="CastleBuilder"/>.</summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class CastleBuildMessageProviderTests
    {
        private readonly CastleBuildMessageProvider messages = new();

        [Test]
        public void AnUpgradeNamesTheBuildingItNeedsAndAnAllowedBuildingHasNoMessage()
        {
            var castle = CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Bulwark"));

            Assert.That(this.messages.DescribeBuild(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Citadel")), Is.EqualTo("Requires Fort"));
            Assert.That(this.messages.DescribeBuild(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Fort")), Is.Null);
            Assert.That(this.messages.DescribeBuild(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("VillageHall")), Is.EqualTo("Already built"));
            Assert.That(this.messages.DescribeBuild(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("CastleGuardhouse")), Is.EqualTo("This town cannot have it"));
        }

        [Test]
        public void ABuildingTheMapForbidsIsSaidToBeNotAllowed()
        {
            var castle = CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Bulwark"));
            var forbidden = castle with { EnabledBuildings = castle.EnabledBuildings.Except([TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Fort")]).ToHashSet() };

            Assert.That(this.messages.DescribeBuild(forbidden, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Fort")), Is.EqualTo("Not allowed by the map"));
        }

        [Test]
        public void AHordeBuildingIsSaidToComeFromItsDwelling()
        {
            var castle = CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Rampart"));

            Assert.That(this.messages.DescribeBuild(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgMinersGuild")), Does.StartWith("The game builds it by itself"));
            Assert.That(this.messages.DescribeBuild(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgDwarfCottage")), Is.EqualTo("Requires Dwarf Cottage"));
        }

        [Test]
        public void ABuildingAnotherNeedsIsSaidToBeNeededByIt()
        {
            var castle = CastleModels.With(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Bulwark")), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Fort"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Citadel"));

            Assert.That(this.messages.DescribeUnbuild(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Fort")), Is.EqualTo("Needed by Citadel"));
            Assert.That(this.messages.DescribeUnbuild(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("VillageHall")), Is.EqualTo("Every town has a village hall"));
            Assert.That(this.messages.DescribeUnbuild(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Capitol")), Is.EqualTo("Not built"));
            Assert.That(this.messages.DescribeUnbuild(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Citadel")), Is.Null);
        }

        [Test]
        public void ADwellingNeededByItsUpgradeNamesTheUpgrade()
        {
            var castle = CastleModels.With(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Rampart")), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartDwarfCottage"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgDwarfCottage"));

            Assert.That(this.messages.DescribeUnbuild(castle, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartDwarfCottage")), Is.EqualTo("Needed by Upg. Dwarf Cottage"));
        }
    }
}
