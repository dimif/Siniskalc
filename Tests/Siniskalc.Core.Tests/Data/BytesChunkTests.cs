namespace Siniskalc.Core.Tests.Data
{
    using System;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Tests the bytes of a chunk of a savefile and the numbers read from them, through a raw chunk.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class BytesChunkTests
    {
        private static RawBytesChunk Chunk(params byte[] bytes) => new("chunk", bytes, 100);

        [Test]
        public void AChunkKnowsItsNameWhereItStartsWhereItEndsAndHowLongItIs()
        {
            var chunk = Chunk(1, 2, 3, 4);

            Assert.That((chunk.Name, chunk.StartIndex, chunk.End, chunk.Length), Is.EqualTo(("chunk", 100, 104, 4)));
        }

        [Test]
        public void AChunkKeepsACopyOfTheBytesItWasGiven()
        {
            var source = new byte[] { 1, 2, 3 };
            var chunk = new RawBytesChunk("chunk", source, 0);

            source[0] = 9;

            Assert.That(chunk.GetByteAt(0), Is.EqualTo(1));
        }

        [Test]
        public void ToArrayGivesACopyThatDoesNotChangeTheChunk()
        {
            var chunk = Chunk(1, 2, 3);

            var copy = chunk.ToArray();
            copy[0] = 9;

            Assert.That(chunk.ToArray(), Is.EqualTo(new byte[] { 1, 2, 3 }));
        }

        [Test]
        public void GetBytesAtGivesACopyOfTheBytesFromAnOffset()
        {
            var chunk = Chunk(1, 2, 3, 4, 5);

            var part = chunk.GetBytesAt(1, 3);
            part[0] = 9;

            Assert.That(part, Is.EqualTo(new byte[] { 9, 3, 4 }));
            Assert.That(chunk.GetBytesAt(1, 3), Is.EqualTo(new byte[] { 2, 3, 4 }));
        }

        [Test]
        public void NumbersAreReadLittleEndianFromAnOffset()
        {
            var chunk = Chunk(0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09);

            Assert.That(chunk.GetUInt16At(1), Is.EqualTo((ushort)0x0201));
            Assert.That(chunk.GetUInt32At(1), Is.EqualTo(0x04030201u));
            Assert.That(chunk.GetInt32At(1), Is.EqualTo(0x04030201));
            Assert.That(chunk.GetUInt64At(1), Is.EqualTo(0x0807060504030201UL));
            Assert.That(chunk.GetInt16At(0), Is.EqualTo((short)0x0100));
        }

        [Test]
        public void SignedNumbersAreReadAsNegativeWhenTheirTopBitIsSet()
        {
            var chunk = Chunk(0xFF, 0xFF, 0xFF, 0xFF, 0xFE, 0xFF);

            Assert.That(chunk.GetInt32At(0), Is.EqualTo(-1));
            Assert.That(chunk.GetInt16At(4), Is.EqualTo((short)-2));
            Assert.That(chunk.GetUInt16At(4), Is.EqualTo((ushort)0xFFFE));
        }

        [Test]
        public void ReadingPastTheEndOfTheChunkIsRefused()
        {
            var chunk = Chunk(1, 2, 3);

            Assert.That(() => chunk.GetByteAt(3), Throws.InstanceOf<IndexOutOfRangeException>());
            Assert.That(() => chunk.GetUInt32At(0), Throws.InstanceOf<ArgumentException>());
            Assert.That(() => chunk.GetBytesAt(2, 2), Throws.InstanceOf<ArgumentException>());
        }

        [Test]
        public void CopyToPutsTheBytesInTheSavefileAtTheStartOfTheChunk()
        {
            var chunk = new RawBytesChunk("chunk", [7, 8, 9], 2);
            var savefile = new byte[6];

            chunk.CopyTo(savefile);

            Assert.That(savefile, Is.EqualTo(new byte[] { 0, 0, 7, 8, 9, 0 }));
        }

        [Test]
        public void TwoChunksHaveTheSameBytesWhateverTheirNamesAndPlaces()
        {
            var one = new RawBytesChunk("one", [1, 2, 3], 0);
            var other = new RawBytesChunk("other", [1, 2, 3], 50);

            Assert.That(one.HasSameBytes(other), Is.True);
            Assert.That(one.HasSameBytes(new RawBytesChunk("one", [1, 2, 4], 0)), Is.False);
            Assert.That(one.HasSameBytes(new RawBytesChunk("one", [1, 2], 0)), Is.False);
        }

        [Test]
        public void WithBytesGivesANewChunkOfTheSameNameAndPlaceAndLeavesTheOldOneAlone()
        {
            var chunk = Chunk(1, 2, 3);

            var changed = chunk.WithBytes([4, 5, 6]);

            Assert.That(changed, Is.Not.SameAs(chunk));
            Assert.That((changed.Name, changed.StartIndex), Is.EqualTo(("chunk", 100)));
            Assert.That(changed.ToArray(), Is.EqualTo(new byte[] { 4, 5, 6 }));
            Assert.That(chunk.ToArray(), Is.EqualTo(new byte[] { 1, 2, 3 }));
        }

        [Test]
        public void WithBytesRefusesAnotherLengthAndSaysBoth()
        {
            var chunk = Chunk(1, 2, 3);

            var thrown = Assert.Throws<ArgumentException>(() => chunk.WithBytes([1, 2]));

            Assert.That(thrown!.Message, Does.Contain("(3)").And.Contain("2"));
            Assert.That(() => chunk.WithBytes([1, 2, 3, 4]), Throws.InstanceOf<ArgumentException>(), "longer is refused as well");
        }

        [Test]
        public void ARawChunkIsPatchedAsItself()
        {
            var chunk = Chunk(1, 2, 3);

            Assert.That(chunk.PatchParts, Is.EqualTo(new[] { chunk }));
        }
    }
}
