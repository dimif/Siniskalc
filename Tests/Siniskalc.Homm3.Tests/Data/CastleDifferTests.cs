using Siniskalc.Homm3.Tests.Models;
using Siniskalc.Core.Data;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// What the differ of a castle lists for two versions of the record and how it puts a building back, on towns made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class CastleDifferTests
    {
        private PendingChangesFake fake = null!;

        [SetUp]
        public void StartWithNoReverts() => this.fake = new();

        private static CastleModel Rampart() => CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Rampart"));

        private IReadOnlyList<PendingChange> Diff(CastleModel original, CastleModel current) => new CastleDiffer(this.fake.Factory).Diff(5, original, current);

        [Test]
        public void TwoEqualCastlesHaveNoChanges()
        {
            Assert.That(this.Diff(Rampart(), Rampart()), Is.Empty);
        }

        [Test]
        public void EachBuildingBuiltIsOnePendingChangeNamedAfterTheCastle()
        {
            var current = CastleBuilder.Build(CastleBuilder.Build(Rampart(), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables")), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartDwarfCottage"));

            var changes = this.Diff(Rampart(), current);

            Assert.That(changes.Select(c => c.Change.Description), Is.EquivalentTo(new[] { "Centaur Stables", "Dwarf Cottage" }));
            Assert.That(changes, Has.All.Property(nameof(PendingChange.Kind)).EqualTo(Homm3ChunkKinds.Castle));
            var stables = changes.Single(c => c.Change.Description == "Centaur Stables");
            Assert.That((stables.Path, stables.Index, stables.Change.Before, stables.Change.After), Is.EqualTo(("Test Town › Buildings", 5, "not built", "built")));
        }

        [Test]
        public void AnAutomaticBuildingIsNotListed()
        {
            var current = Rampart();
            foreach (var building in new[] { TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartDwarfCottage"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartMinersGuild"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgDwarfCottage") })
            {
                current = CastleBuilder.Build(current, building);
            }

            Assert.That(current.Buildings, Does.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgMinersGuild")));
            Assert.That(this.Diff(Rampart(), current).Select(c => c.Change.Description), Does.Not.Contain("Upg. Miners' Guild"));
        }

        [Test]
        public void ARemovedBuildingIsListedAsBuiltBefore()
        {
            var original = CastleBuilder.Build(Rampart(), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables"));

            var change = this.Diff(original, CastleBuilder.Unbuild(original, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables"))).Single();

            Assert.That((change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Centaur Stables", "built", "not built")));
        }

        [Test]
        public void ForbiddingABuildingIsOnePendingChangeFromAllowedToNotAllowed()
        {
            var building = AllowedButNotBuilt(Rampart());

            var change = this.Diff(Rampart(), CastleBuilder.SetAllowed(Rampart(), building, false)).Single();

            Assert.That((change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo((building.Name, "allowed", "not allowed")));
        }

        [Test]
        public void AllowingABuildingIsOnePendingChangeAndItsRevertForbidsItAgain()
        {
            var building = AllowedButNotBuilt(Rampart());
            var forbidden = CastleBuilder.SetAllowed(Rampart(), building, false);

            var change = this.Diff(forbidden, Rampart()).Single();

            Assert.That((change.Change.Before, change.Change.After), Is.EqualTo(("not allowed", "allowed")));
            Assert.That(this.fake.CastleReverts[building.Name](Rampart()).EnabledBuildings, Does.Not.Contain(building));
        }

        [Test]
        public void PuttingBackABuildingAlsoPutsBackWhatWasBuiltOnTopOfIt()
        {
            var original = Rampart();
            var current = CastleBuilder.Build(CastleBuilder.Build(original, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables")), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgCentaurStables"));

            var restored = new CastleDiffer(this.fake.Factory).RestoreBuilding(current, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables"), original);

            Assert.That(this.Diff(original, restored), Is.Empty);
        }

        [Test]
        public void PuttingBackARemovedBuildingBringsBackWhatItNeeded()
        {
            var original = CastleBuilder.Build(CastleBuilder.Build(Rampart(), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables")), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgCentaurStables"));
            var current = CastleBuilder.Unbuild(CastleBuilder.Unbuild(original, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgCentaurStables")), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables"));

            var restored = new CastleDiffer(this.fake.Factory).RestoreBuilding(current, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgCentaurStables"), original);

            Assert.That(this.Diff(original, restored), Is.Empty);
        }

        [Test]
        public void PuttingBackABuildingThatIsNotInTheOriginalTakesItDown()
        {
            var original = Rampart();
            var current = CastleBuilder.Build(original, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables"));

            var restored = new CastleDiffer(this.fake.Factory).RestoreBuilding(current, TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables"), original);

            Assert.That(restored.HasBuilt(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables")), Is.False);
            Assert.That(restored.CreaturesAvailable[0], Is.EqualTo(original.CreaturesAvailable[0]));
        }

        private static Building AllowedButNotBuilt(CastleModel castle) =>
            Building.ForFaction(castle.Faction).First(b => castle.EnabledBuildings.Contains(b) && CastleBuilder.GetSetAllowedFailureReason(castle, b) == CastleBuildFailureReason.None);
    }
}
