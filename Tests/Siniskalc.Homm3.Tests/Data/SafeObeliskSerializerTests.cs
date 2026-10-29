using Siniskalc.Heroes.Data;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// The bytes the visits of the obelisks write into the record of the obelisks, on a record made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SafeObeliskSerializerTests
    {
        private readonly SafeObeliskSerializer serializer = new();

        private static ObeliskVisits<PlayerColor> Visits(ObeliskChunk chunk) => new ObeliskDeserializer().Deserialize(chunk);

        [Test]
        public void TheVisitsThatWereReadLeaveEveryByteAlone()
        {
            var chunk = ObeliskDeserializerTests.Obelisks(3, 0x81, 0x24, 0);

            Assert.That(this.serializer.Serialize(chunk, Visits(chunk)).ToArray(), Is.EqualTo(chunk.ToArray()));
        }

        [Test]
        public void ANewVisitSetsTheBitOfTheColourOfThatObeliskOnlyAndTheCountIsNeverWritten()
        {
            var chunk = ObeliskDeserializerTests.Obelisks(3, 0, 0, 0);

            var written = this.serializer.Serialize(chunk, Visits(chunk).WithVisited(2, PlayerColor.Tan, true));

            Assert.That(ByteDiff.Of(chunk.ToArray(), written.ToArray()), Is.EqualTo(new[] { (ObeliskLayout.FlagsOffset + 2, (byte)0, (byte)0b0000_0100) }));
        }

        [Test]
        public void AVisitTakenAwayClearsItsBitAndKeepsTheOthers()
        {
            var chunk = ObeliskDeserializerTests.Obelisks(1, 0b0000_0111);

            var written = this.serializer.Serialize(chunk, Visits(chunk).WithVisited(0, PlayerColor.Blue, false));

            Assert.That(ByteDiff.Of(chunk.ToArray(), written.ToArray()), Is.EqualTo(new[] { (ObeliskLayout.FlagsOffset, (byte)0b0000_0111, (byte)0b0000_0101) }));
        }

        [Test]
        public void VisitsOfMoreObelisksThanTheCountSaysAreNotWritten()
        {
            var chunk = ObeliskDeserializerTests.Obelisks(2, 1, 2, 3, 4);

            var written = this.serializer.Serialize(chunk, new ObeliskVisits<PlayerColor>([9, 9, 9, 9]));

            Assert.That(written.ToArray().Skip(ObeliskLayout.FlagsOffset), Is.EqualTo(new byte[] { 9, 9, 3, 4 }), "the bytes past the count are not the visits of any obelisk");
        }

        [Test]
        public void VisitsOfFewerObelisksThanTheCountSaysLeaveTheRestOfTheBytesAlone()
        {
            var chunk = ObeliskDeserializerTests.Obelisks(3, 5, 6, 7);

            var written = this.serializer.Serialize(chunk, new ObeliskVisits<PlayerColor>([0x10, 0x20]));

            Assert.That(written.ToArray().Skip(ObeliskLayout.FlagsOffset), Is.EqualTo(new byte[] { 0x10, 0x20, 7 }));
        }
    }
}
