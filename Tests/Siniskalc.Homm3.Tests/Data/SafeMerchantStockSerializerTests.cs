using Siniskalc.Core.Data;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Tests, on a list of seven slots made by hand, which bytes the writer of the stock of a merchant changes.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SafeMerchantStockSerializerTests
    {
        private readonly SafeMerchantStockSerializer serializer = new();

        private static RecordChunk Empty()
        {
            var bytes = new byte[MerchantLayout.ListSize];
            Array.Fill(bytes, (byte)0xFF);
            return new RecordChunk("Merchant", bytes, 0, 0);
        }

        private static MerchantStockModel Stock(params Artifact?[] slots) =>
            new(0, "Merchant", slots.Concat(Enumerable.Repeat<Artifact?>(null, MerchantLayout.SlotCount - slots.Length)).ToList());

        private static int UnknownCode() => Enumerable.Range(0, 256).First(c => !Artifact.All.ContainsKey((byte)c));

        private static RecordChunk WithCode(RecordChunk chunk, int slot, uint code)
        {
            var bytes = chunk.ToArray();
            BitConverter.GetBytes(code).CopyTo(bytes, MerchantLayout.SlotAt(slot));
            return chunk.WithBytes(bytes);
        }

        private RecordChunk Write(RecordChunk chunk, MerchantStockModel stock) => this.serializer.Serialize(chunk, stock);

        [Test]
        public void AnArtifactIsWrittenAsItsCodeInALittleEndianIntegerInItsSlot()
        {
            var written = this.Write(Empty(), Stock(null, Artifact.HelmOfTheAlabasterUnicorn, null, null, null, null, Artifact.OrbOfInhibition));

            Assert.That(written.GetBytesAt(4, 4), Is.EqualTo(new byte[] { 0x13, 0, 0, 0 }));
            Assert.That(written.GetBytesAt(24, 4), Is.EqualTo(new byte[] { 0x7E, 0, 0, 0 }));
            Assert.That(written.GetBytesAt(0, 4), Is.EqualTo(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }));
        }

        [Test]
        public void AnEmptiedSlotGoesBackToFFFFFFFF()
        {
            var filled = this.Write(Empty(), Stock(Artifact.HelmOfTheAlabasterUnicorn));

            var emptied = this.Write(filled, Stock());

            Assert.That(emptied.ToArray(), Is.EqualTo(Empty().ToArray()));
        }

        [Test]
        public void ChangingOneSlotChangesOnlyItsFourBytes()
        {
            var before = this.Write(Empty(), Stock(Artifact.HelmOfTheAlabasterUnicorn, Artifact.OrbOfInhibition));

            var after = this.Write(before, Stock(Artifact.HelmOfTheAlabasterUnicorn, Artifact.SkullHelmet));

            Assert.That(ByteDiff.Of(before.ToArray(), after.ToArray()).Select(d => d.Offset), Is.SubsetOf(Enumerable.Range(4, 4)));
            Assert.That(after.GetUInt32At(4), Is.EqualTo((uint)Artifact.SkullHelmet.Code));
        }

        [Test]
        public void ACodeThatIsNoArtifactIsKeptWhileTheModelStillHoldsWhatItReadsAs()
        {
            var odd = WithCode(Empty(), 2, (uint)UnknownCode());
            var read = new MerchantStockDeserializer().Deserialize(odd);

            Assert.That(this.Write(odd, read).ToArray(), Is.EqualTo(odd.ToArray()));
        }

        [Test]
        public void ACodeAboveAByteIsKeptTheSameWayAndReplacedWhenTheModelChanges()
        {
            var odd = WithCode(Empty(), 2, 0x3FF);
            var read = new MerchantStockDeserializer().Deserialize(odd);

            Assert.That(this.Write(odd, read).GetUInt32At(8), Is.EqualTo(0x3FFu));
            Assert.That(this.Write(odd, read.WithSlot(2, Artifact.OrbOfInhibition)).GetUInt32At(8), Is.EqualTo(0x7Eu));
            Assert.That(this.Write(odd, read.WithSlot(2, null)).GetUInt32At(8), Is.EqualTo(uint.MaxValue));
        }

        [TestCase(6)]
        [TestCase(8)]
        public void AListThatIsNotSevenSlotsLongIsRefused(int count)
        {
            var stock = new MerchantStockModel(0, "Merchant", Enumerable.Repeat<Artifact?>(null, count).ToList());

            Assert.Throws<ArgumentOutOfRangeException>(() => this.Write(Empty(), stock));
        }

        [Test]
        public void TheSourceIsNotChangedAndTheSizeIsKept()
        {
            var chunk = Empty();

            var written = this.Write(chunk, Stock(Artifact.OrbOfInhibition));

            Assert.That(chunk.ToArray(), Is.All.EqualTo((byte)0xFF));
            Assert.That(written.Length, Is.EqualTo(MerchantLayout.ListSize));
        }
    }
}
