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
        private readonly SafeObeliskSerializer<PlayerColor> serializer = new(ObeliskLayout.Flags);

        private static ObeliskVisits<PlayerColor> Visits(ObeliskChunk chunk) => new ObeliskDeserializer<PlayerColor>(ObeliskLayout.Flags).Deserialize(chunk);

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
    }
}
