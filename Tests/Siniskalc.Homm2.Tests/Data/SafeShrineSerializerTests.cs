namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The bytes a shrine model writes into the record of a shrine, on records made by hand: only the spell number changes, and the low
    /// three bits of the word stay as the game has them.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SafeShrineSerializerTests
    {
        private readonly SafeShrineSerializer serializer = new();

        private static MapObjectChunk Shrine(ushort word, int tag = 1) =>
            new("Shrine", BitConverter.GetBytes(word), 200, new MapPosition(30, 31), 1, tag);

        private static ShrineModel Model(int spell) => new(1, 1, new MapPosition(30, 31), spell, TestCatalog.Standard);

        [Test]
        public void ANewSpellIsWrittenAboveTheLowThreeBitsWhichAreKept()
        {
            var chunk = Shrine((1 << 3) | 5);

            var written = this.serializer.Serialize(chunk, Model(9));

            Assert.That(BitConverter.ToUInt16(written.ToArray()), Is.EqualTo((ushort)((9 << 3) | 5)));
            Assert.That(ByteDiff.Of(chunk.ToArray(), written.ToArray()), Is.EqualTo(new[] { (0, (byte)13, (byte)77) }));
        }

        [Test]
        public void TheSameSpellLeavesTheRecordAlone()
        {
            var chunk = Shrine((9 << 3) | 5);

            Assert.That(this.serializer.Serialize(chunk, Model(9)), Is.SameAs(chunk));
        }

        [Test]
        public void AHiddenShrineIsNeverWritten()
        {
            var chunk = Shrine(8 * 17, 1 | MapObjectsReader.HiddenShrine);

            Assert.That(this.serializer.Serialize(chunk, Model(9)), Is.SameAs(chunk));
        }

        [Test]
        public void AModelWithNoSpellLeavesTheRecordAlone()
        {
            var chunk = Shrine(8 * 17);

            Assert.That(this.serializer.Serialize(chunk, Model(-1)), Is.SameAs(chunk));
        }

        [Test]
        public void ASpellNumberThatDoesNotFitAByteIsRefusedAndTheRecordIsNotChanged()
        {
            var chunk = Shrine(8 * 17);
            var before = chunk.ToArray();

            Assert.That(() => this.serializer.Serialize(chunk, Model(300)), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(chunk.ToArray(), Is.EqualTo(before));
        }
    }
}
