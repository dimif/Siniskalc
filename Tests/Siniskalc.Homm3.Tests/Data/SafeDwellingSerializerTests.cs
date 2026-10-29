using Siniskalc.Core.Data;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Tests, on a record of a dwelling made by hand, which bytes the writer of a dwelling changes: the owner and the creatures that can
    /// be hired.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SafeDwellingSerializerTests
    {
        private readonly SafeDwellingSerializer serializer = new();

        private static RecordChunk Blank() => new("Dwelling", new byte[DwellingLayout.RecordSize], 0, 0);

        private static DwellingModel Dwelling(PlayerColor? owner = null, long stock = 0) =>
            new(0, Creature.Centaur.Code, Creature.Centaur, owner ?? PlayerColor.Neutral, new MapPosition(1, 2), stock);

        private static RecordChunk WithBytes(RecordChunk chunk, params (int Offset, byte Value)[] bytes)
        {
            var copy = chunk.ToArray();
            foreach (var (offset, value) in bytes)
            {
                copy[offset] = value;
            }

            return chunk.WithBytes(copy);
        }

        private RecordChunk Write(RecordChunk chunk, DwellingModel dwelling) => this.serializer.Serialize(chunk, dwelling);

        [Test]
        public void TheOwnerIsWrittenAsTheCodeOfItsColourAndNeutralAsFF()
        {
            var blue = this.Write(Blank(), Dwelling(PlayerColor.Blue));
            var neutral = this.Write(blue, Dwelling(PlayerColor.Neutral));

            Assert.That(blue.GetByteAt(DwellingLayout.Owner), Is.EqualTo(PlayerColor.Blue.Code));
            Assert.That(neutral.GetByteAt(DwellingLayout.Owner), Is.EqualTo(DwellingLayout.NoOwner));
        }

        [Test]
        public void AnOwnerByteThatIsNoColourIsKeptWhileTheDwellingStaysNeutralAndReplacedByAColour()
        {
            var odd = WithBytes(Blank(), (DwellingLayout.Owner, 0x09));

            Assert.That(this.Write(odd, Dwelling(PlayerColor.Neutral)).GetByteAt(DwellingLayout.Owner), Is.EqualTo(0x09));
            Assert.That(this.Write(odd, Dwelling(PlayerColor.Red)).GetByteAt(DwellingLayout.Owner), Is.EqualTo(PlayerColor.Red.Code));
        }

        [Test]
        public void TheStockIsWrittenAsALittleEndianInteger()
        {
            var written = this.Write(Blank(), Dwelling(stock: 99_999));

            Assert.That(written.GetBytesAt(DwellingLayout.Stock, 4), Is.EqualTo(new byte[] { 0x9F, 0x86, 0x01, 0 }));
        }

        [Test]
        public void AStockOfNothingAndOfTheMostAreAllowedAndOneBeyondIsRefused()
        {
            Assert.DoesNotThrow(() => this.Write(WithBytes(Blank(), (DwellingLayout.Stock, 5)), Dwelling(stock: 0)));
            Assert.DoesNotThrow(() => this.Write(Blank(), Dwelling(stock: Constants.Army.MaxStackSize)));
            Assert.Throws<ArgumentOutOfRangeException>(() => this.Write(Blank(), Dwelling(stock: Constants.Army.MaxStackSize + 1)));
            Assert.Throws<ArgumentOutOfRangeException>(() => this.Write(WithBytes(Blank(), (DwellingLayout.Stock, 5)), Dwelling(stock: -1)));
        }

        [Test]
        public void AStockBeyondTheLimitThatTheSavefileHoldsIsKeptWhileTheModelStillSaysIt()
        {
            var big = this.Write(Blank(), Dwelling(stock: 1)).ToArray();
            BitConverter.GetBytes(5_000_000u).CopyTo(big, DwellingLayout.Stock);
            var chunk = Blank().WithBytes(big);

            var written = this.Write(chunk, Dwelling(PlayerColor.Neutral, 5_000_000));

            Assert.That(written.GetBytesAt(DwellingLayout.Stock, 4), Is.EqualTo(new byte[] { 0x40, 0x4B, 0x4C, 0 }));
        }

        [Test]
        public void ChangingTheOwnerAndTheStockChangesNothingElse()
        {
            var before = this.Write(Blank(), Dwelling());

            var after = this.Write(before, Dwelling(PlayerColor.Teal, 12));

            var changed = ByteDiff.Of(before.ToArray(), after.ToArray()).Select(d => d.Offset).ToList();
            Assert.That(changed, Is.EquivalentTo(new[] { DwellingLayout.Owner, DwellingLayout.Stock }));
        }

        [Test]
        public void TheCreatureAndTheTileAreNeverWritten()
        {
            var chunk = WithBytes(Blank(), (DwellingLayout.Creature, 0x42), (DwellingLayout.X, 9), (DwellingLayout.Y, 8), (DwellingLayout.Level, 1));

            var written = this.Write(chunk, Dwelling(PlayerColor.Red, 3));

            Assert.That((written.GetByteAt(DwellingLayout.Creature), written.GetByteAt(DwellingLayout.X), written.GetByteAt(DwellingLayout.Y), written.GetByteAt(DwellingLayout.Level)),
                Is.EqualTo(((byte)0x42, (byte)9, (byte)8, (byte)1)));
        }

        [Test]
        public void TheSourceRecordIsNotChangedAndTheSizeIsKept()
        {
            var chunk = Blank();

            var written = this.Write(chunk, Dwelling(PlayerColor.Red, 4));

            Assert.That(chunk.ToArray(), Is.All.EqualTo((byte)0));
            Assert.That(written.Length, Is.EqualTo(DwellingLayout.RecordSize));
        }
    }
}
