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
            var castle = CastleModels.Village(Faction.Bulwark);

            Assert.That(this.messages.DescribeBuild(castle, Building.Citadel), Is.EqualTo("Requires Fort"));
            Assert.That(this.messages.DescribeBuild(castle, Building.Fort), Is.Null);
            Assert.That(this.messages.DescribeBuild(castle, Building.VillageHall), Is.EqualTo("Already built"));
            Assert.That(this.messages.DescribeBuild(castle, Building.CastleGuardhouse), Is.EqualTo("This town cannot have it"));
        }

        [Test]
        public void ABuildingTheMapForbidsIsSaidToBeNotAllowed()
        {
            var castle = CastleModels.Village(Faction.Bulwark);
            var forbidden = castle with { EnabledBuildings = castle.EnabledBuildings.Except([Building.Fort]).ToHashSet() };

            Assert.That(this.messages.DescribeBuild(forbidden, Building.Fort), Is.EqualTo("Not allowed by the map"));
        }

        [Test]
        public void AHordeBuildingIsSaidToComeFromItsDwelling()
        {
            var castle = CastleModels.Village(Faction.Rampart);

            Assert.That(this.messages.DescribeBuild(castle, Building.RampartUpgMinersGuild), Does.StartWith("The game builds it by itself"));
            Assert.That(this.messages.DescribeBuild(castle, Building.RampartUpgDwarfCottage), Is.EqualTo("Requires Dwarf Cottage"));
        }

        [Test]
        public void ABuildingAnotherNeedsIsSaidToBeNeededByIt()
        {
            var castle = CastleModels.With(CastleModels.Village(Faction.Bulwark), Building.Fort, Building.Citadel);

            Assert.That(this.messages.DescribeUnbuild(castle, Building.Fort), Is.EqualTo("Needed by Citadel"));
            Assert.That(this.messages.DescribeUnbuild(castle, Building.VillageHall), Is.EqualTo("Every town has a village hall"));
            Assert.That(this.messages.DescribeUnbuild(castle, Building.Capitol), Is.EqualTo("Not built"));
            Assert.That(this.messages.DescribeUnbuild(castle, Building.Citadel), Is.Null);
        }

        [Test]
        public void ADwellingNeededByItsUpgradeNamesTheUpgrade()
        {
            var castle = CastleModels.With(CastleModels.Village(Faction.Rampart), Building.RampartDwarfCottage, Building.RampartUpgDwarfCottage);

            Assert.That(this.messages.DescribeUnbuild(castle, Building.RampartDwarfCottage), Is.EqualTo("Needed by Upg. Dwarf Cottage"));
        }
    }
}
