using Siniskalc.Heroes.Entities;
using Siniskalc.Core.Data;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Tests, on a record of a mine made by hand, which bytes the writer of a mine changes: the owner and the seven slots of the guard.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SafeMineSerializerTests
    {
        private readonly SafeMineSerializer serializer = new();

        private static RecordChunk Blank() => new("Mine", new byte[MineLayout.RecordSize], 0, 0);

        private static MineModel Mine(PlayerColor? owner = null, IArmy<Creature>? guard = null) =>
            new(0, MineKind.GoldMine, owner ?? PlayerColor.Neutral, new MapPosition(1, 2), guard ?? new Army<Creature>(Constants.CreatureSlotCount));

        private RecordChunk Write(RecordChunk chunk, MineModel mine) => this.serializer.Serialize(chunk, mine);

        [Test]
        public void TheOwnerIsWrittenAsTheCodeOfItsColourAndNeutralAsFF()
        {
            var blue = this.Write(Blank(), Mine(PlayerColor.Blue));
            var neutral = this.Write(blue, Mine(PlayerColor.Neutral));

            Assert.That(blue.GetByteAt(MineLayout.Owner), Is.EqualTo(PlayerColor.Blue.Code));
            Assert.That(neutral.GetByteAt(MineLayout.Owner), Is.EqualTo(MineLayout.NoOwner));
        }

        [Test]
        public void AnOwnerByteThatIsNoColourIsKeptWhileTheMineStaysNeutralAndReplacedByAColour()
        {
            var bytes = Blank().ToArray();
            bytes[MineLayout.Owner] = 0x09;
            var odd = Blank().WithBytes(bytes);

            Assert.That(this.Write(odd, Mine(PlayerColor.Neutral)).GetByteAt(MineLayout.Owner), Is.EqualTo(0x09));
            Assert.That(this.Write(odd, Mine(PlayerColor.Red)).GetByteAt(MineLayout.Owner), Is.EqualTo(PlayerColor.Red.Code));
        }

        [Test]
        public void TheGuardIsWrittenAsLittleEndianTypeAndCountIntegersInTheirOwnTables()
        {
            var guard = new Army<Creature>(Constants.CreatureSlotCount).WithSlot(1, Creature.Centaur, 1234).WithSlot(6, Creature.GoldDragon, 3);

            var written = this.Write(Blank(), Mine(guard: guard));

            Assert.That(written.GetBytesAt(MineLayout.Guard.TypeOffset + 4, 4), Is.EqualTo(new byte[] { 0x0E, 0, 0, 0 }));
            Assert.That(written.GetBytesAt(MineLayout.Guard.CountOffset + 4, 4), Is.EqualTo(new byte[] { 0xD2, 0x04, 0, 0 }));
            Assert.That(written.GetBytesAt(MineLayout.Guard.TypeOffset + 24, 4), Is.EqualTo(new byte[] { 0x1B, 0, 0, 0 }));
            Assert.That(written.GetBytesAt(MineLayout.Guard.CountOffset + 24, 4), Is.EqualTo(new byte[] { 3, 0, 0, 0 }));
        }

        [Test]
        public void AnEmptiedSlotGoesBackToTheEmptyMark()
        {
            var filled = this.Write(Blank(), Mine(guard: new Army<Creature>(Constants.CreatureSlotCount).WithSlot(2, Creature.Angel, 5)));

            var emptied = this.Write(filled, Mine());

            Assert.That(emptied.GetBytesAt(MineLayout.Guard.TypeOffset + 8, 4), Is.EqualTo(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }));
        }

        [Test]
        public void ChangingTheOwnerAndTheGuardChangesNothingElse()
        {
            var before = this.Write(Blank(), Mine());

            var after = this.Write(before, Mine(PlayerColor.Teal, new Army<Creature>(Constants.CreatureSlotCount).WithSlot(0, Creature.Centaur, 7)));

            var inFirstSlot = Enumerable.Range(MineLayout.Guard.TypeOffset, 4).Concat(Enumerable.Range(MineLayout.Guard.CountOffset, 4)).Append(MineLayout.Owner);
            var changed = ByteDiff.Of(before.ToArray(), after.ToArray()).Select(d => d.Offset).ToList();
            Assert.That(changed, Is.SubsetOf(inFirstSlot));
            Assert.That(changed, Does.Contain(MineLayout.Owner).And.Contain(MineLayout.Guard.TypeOffset).And.Contain(MineLayout.Guard.CountOffset));
        }

        [Test]
        public void TheKindAndTheTileAreNeverWritten()
        {
            var bytes = Blank().ToArray();
            bytes[MineLayout.X] = 9;
            bytes[MineLayout.Y] = 8;
            bytes[MineLayout.Level] = 1;
            bytes[MineLayout.Kind] = 3;
            var chunk = Blank().WithBytes(bytes);

            var written = this.Write(chunk, Mine());

            Assert.That(written.GetBytesAt(0, 5), Is.EqualTo(new byte[] { 9, 8, 1, 0xFF, 3 }));
        }

        [Test]
        public void TheSourceRecordIsNotChangedAndTheSizeIsKept()
        {
            var chunk = Blank();

            var written = this.Write(chunk, Mine(PlayerColor.Red));

            Assert.That(chunk.ToArray(), Is.All.EqualTo((byte)0));
            Assert.That(written.Length, Is.EqualTo(MineLayout.RecordSize));
        }
    }
}
