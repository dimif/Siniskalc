using Siniskalc.Core.Data;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;

namespace Siniskalc.Tests.Data
{
    [TestFixture]
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
        public void AnotherCastleIsNotAffected()
        {
            var jotunngard = this.history.CreateCastleSession(this.history.Current.Castles().FindByName("Jotunngard"));
            this.still.Update(c => CastleBuilder.Build(c, Building.RampartCentaurStables));
            jotunngard.Update(c => CastleBuilder.Build(c, Building.BulwarkColliery));

            var changes = this.history.GetPendingChanges();

            Assert.That(changes.Select(c => (c.OwnerName, c.Change.Description)), Is.EquivalentTo(new[] { ("Still Water", "Centaur Stables"), ("Jotunngard", "Colliery") }));
            Assert.That(changes.Select(c => c.Index).Distinct().Count(), Is.EqualTo(2));
        }
    }
}
