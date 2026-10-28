using Moq;
using Siniskalc.Homm3.Tests.Data;
using Siniskalc.Core.Data;
using Siniskalc.Core.Models;
using Siniskalc.Heroes.Data;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class CastleDifferTests
    {
        private SaveDocumentHistory history = null!;
        private CastleEditSession still = null!;
        private byte[] original = null!;

        [SetUp]
        public void Load()
        {
            var path = CastleSamples.Folder("Bulwark").Single(p => p.EndsWith("bulwark_13_mage_guild_4.GM1"));
            this.history = CastleSamples.Load(path).History;
            this.original = this.history.Current.ToBytes();
            this.still = this.history.CreateCastleSession(this.history.Current.Castles().FindByName("Still Water"));
        }

        [Test]
        public void NothingIsPendingBeforeAnEdit()
        {
            Assert.That(this.history.GetPendingChanges(), Is.Empty);
        }

        [Test]
        public void EachBuildingBuiltIsOnePendingChangeNamedAfterTheCastle()
        {
            this.still.Update(c => CastleBuilder.Build(c, Building.RampartCentaurStables));
            this.still.Update(c => CastleBuilder.Build(c, Building.RampartDwarfCottage));

            var changes = this.history.GetPendingChanges();

            Assert.That(changes.Select(c => c.Change.Description), Is.EqualTo(new[] { "Centaur Stables", "Dwarf Cottage" }));
            Assert.That(changes, Has.All.Property(nameof(PendingChange.Kind)).EqualTo(Homm3ChunkKinds.Castle));
            Assert.That(changes[0].Path, Is.EqualTo("Still Water › Buildings"));
            Assert.That(changes[0].Change.Before, Is.EqualTo("not built"));
            Assert.That(changes[0].Change.After, Is.EqualTo("built"));
        }

        [Test]
        public void AnAutomaticBuildingIsNotListed()
        {
            this.still.Update(c => CastleBuilder.Build(c, Building.RampartCentaurStables));
            this.still.Update(c => CastleBuilder.Build(c, Building.RampartDwarfCottage));
            this.still.Update(c => CastleBuilder.Build(c, Building.RampartMinersGuild));
            this.still.Update(c => CastleBuilder.Build(c, Building.RampartUpgDwarfCottage));

            Assert.That(this.still.Current.Buildings, Does.Contain(Building.RampartUpgMinersGuild));
            Assert.That(this.history.GetPendingChanges().Select(c => c.Change.Description), Does.Not.Contain("Upg. Miners' Guild"));
        }

        [Test]
        public void ARemovedBuildingIsListedAsBuiltBefore()
        {
            this.still.Update(c => CastleBuilder.Unbuild(c, Building.MagesGuild4));

            var change = this.history.GetPendingChanges().Single();

            Assert.That(change.Change.Description, Is.EqualTo("Mage Guild, level 4"));
            Assert.That(change.Change.Before, Is.EqualTo("built"));
            Assert.That(change.Change.After, Is.EqualTo("not built"));
        }

        [Test]
        public void RevertingABuildingPutsTheBytesBack()
        {
            this.still.Update(c => CastleBuilder.Build(c, Building.RampartCentaurStables));
            this.still.Update(c => CastleBuilder.Unbuild(c, Building.MagesGuild4));

            this.history.GetPendingChanges().Single(c => c.Change.Description == "Centaur Stables").Revert(this.history);

            Assert.That(this.history.GetPendingChanges().Select(c => c.Change.Description), Is.EqualTo(new[] { "Mage Guild, level 4" }));
            Assert.That(this.still.Current.CreaturesAvailable[0], Is.EqualTo(0));

            this.history.GetPendingChanges().Single().Revert(this.history);

            Assert.That(this.history.GetPendingChanges(), Is.Empty);
            Assert.That(this.history.Current.ToBytes(), Is.EqualTo(this.original));
        }

        [Test]
        public void RevertingABuildingAlsoRevertsWhatWasBuiltOnTopOfIt()
        {
            this.still.Update(c => CastleBuilder.Build(c, Building.RampartCentaurStables));
            this.still.Update(c => CastleBuilder.Build(c, Building.RampartUpgCentaurStables));

            this.history.GetPendingChanges().Single(c => c.Change.Description == "Centaur Stables").Revert(this.history);

            Assert.That(this.history.GetPendingChanges(), Is.Empty);
            Assert.That(this.history.Current.ToBytes(), Is.EqualTo(this.original));
        }

        [Test]
        public void RevertingARemovedBuildingBringsBackWhatItNeeded()
        {
            this.still.Update(c => CastleBuilder.Unbuild(c, Building.MagesGuild4));
            this.still.Update(c => CastleBuilder.Unbuild(c, Building.MagesGuild3));

            this.history.GetPendingChanges().Single(c => c.Change.Description == "Mage Guild, level 4").Revert(this.history);

            Assert.That(this.history.GetPendingChanges(), Is.Empty);
            Assert.That(this.history.Current.ToBytes(), Is.EqualTo(this.original));
        }

        [Test]
        public void ARevertCanBeUndone()
        {
            this.still.Update(c => CastleBuilder.Build(c, Building.RampartCentaurStables));
            this.history.GetPendingChanges().Single().Revert(this.history);

            this.history.Undo();

            Assert.That(this.history.GetPendingChanges().Single().Change.Description, Is.EqualTo("Centaur Stables"));
        }

        [Test]
        public void ForbiddingABuildingIsOnePendingChangeFromAllowedToNotAllowed()
        {
            var building = this.AllowedButNotBuilt();
            this.still.Update(c => CastleBuilder.SetAllowed(c, building, false));

            var change = this.history.GetPendingChanges().Single();

            Assert.That(change.Path, Is.EqualTo("Still Water › Buildings"));
            Assert.That(change.Change.Description, Is.EqualTo(building.Name));
            Assert.That(change.Change.Before, Is.EqualTo("allowed"));
            Assert.That(change.Change.After, Is.EqualTo("not allowed"));
        }

        [Test]
        public void AllowingABuildingIsOnePendingChangeFromNotAllowedToAllowed()
        {
            var building = this.AllowedButNotBuilt();
            var forbidden = CastleBuilder.SetAllowed(this.still.Current, building, false);

            var reverts = new List<(Change Change, Func<CastleModel, CastleModel> Revert)>();
            var factory = new Mock<IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits<PlayerColor>>>();
            factory.Setup(f => f.ForCastle(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Change>(), It.IsAny<Func<CastleModel, CastleModel>>()))
                .Callback<string, int, Change, Func<CastleModel, CastleModel>>((_, _, change, revert) => reverts.Add((change, revert)))
                .Returns((PendingChange)null!);

            new CastleDiffer(factory.Object).Diff(0, forbidden, this.still.Current);
            var (change, revert) = reverts.Single();

            Assert.That(change.Before, Is.EqualTo("not allowed"));
            Assert.That(change.After, Is.EqualTo("allowed"));
            Assert.That(revert(this.still.Current).EnabledBuildings, Does.Not.Contain(building));
        }

        [Test]
        public void AnotherCastleIsNotAffected()
        {
            var jotunngard = this.history.CreateCastleSession(this.history.Current.Castles().FindByName("Jotunngard"));
            this.still.Update(c => CastleBuilder.Build(c, Building.RampartCentaurStables));
            jotunngard.Update(c => CastleBuilder.Build(c, Building.BulwarkColliery));

            var changes = this.history.GetPendingChanges();

            Assert.That(changes.Select(c => (c.OwnerName, c.Change.Description)), Is.EquivalentTo(new[] { ("Still Water", "Centaur Stables"), ("Jotunngard", "Colliery") }));
            Assert.That(changes.Select(c => c.Index).Distinct().Count(), Is.EqualTo(2));
        }

        private Building AllowedButNotBuilt()
        {
            var castle = this.still.Current;
            return Building.ForFaction(castle.Faction).First(b => castle.EnabledBuildings.Contains(b) && CastleBuilder.GetSetAllowedFailureReason(castle, b) == CastleBuildFailureReason.None);
        }

        [Test]
        public void OneRevertPutsTheMaskOfAllowedBuildingsBack()
        {
            var building = this.AllowedButNotBuilt();
            this.still.Update(c => CastleBuilder.SetAllowed(c, building, false));

            this.history.GetPendingChanges().Single().Revert(this.history);

            Assert.That(this.history.GetPendingChanges(), Is.Empty);
            Assert.That(this.still.Current.EnabledBuildings, Does.Contain(building));
            Assert.That(this.history.Current.ToBytes(), Is.EqualTo(this.original));
        }

        [Test]
        public void RevertingThePermissionOfABuildingBuiltSinceLeavesItBuilt()
        {
            var building = this.AllowedButNotBuilt();
            this.still.Update(c => CastleBuilder.SetAllowed(c, building, false));
            this.still.Update(c => CastleBuilder.SetAllowed(c, building, true));
            this.still.Update(c => CastleBuilder.SetBuilt(c, building, true));
            this.still.Update(c => CastleBuilder.RestoreAllowed(c, building, false));

            this.history.GetPendingChanges().Single(c => c.Change.After == "not allowed").Revert(this.history);

            Assert.That(this.still.Current.EnabledBuildings, Does.Contain(building));
            Assert.That(this.still.Current.HasBuilt(building), Is.True);
            Assert.That(this.history.GetPendingChanges().Select(c => c.Change.After), Is.EqualTo(new[] { "built" }));
        }
    }
}
