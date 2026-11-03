namespace Siniskalc.Heroes.Tests.Data
{
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Tests.Entities;

    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SafeObeliskSerializerTests
    {
        [Test]
        public void TheNumberOfPositionsLimitsWritesWhenTheChunkStoresNoCount()
        {
            var chunk = new ObeliskChunk("Obelisks", [0x11, 0x22, 0x33], 0, [new MapPosition(1, 2), new MapPosition(3, 4)]);
            var visits = new ObeliskVisits<TestColor>([0x44, 0x55, 0x66]);
            var serializer = new SafeObeliskSerializer<TestColor>(new ObeliskFlagsLayout(0, null));

            var result = serializer.Serialize(chunk, visits);

            Assert.That(result.ToArray(), Is.EqualTo(new byte[] { 0x44, 0x55, 0x33 }));
        }

        [Test]
        public void TheStoredCountLimitsWritesAndUnrelatedBytesStayAlone()
        {
            var chunk = new ObeliskChunk("Obelisks", [0x99, 0x02, 0x11, 0x22, 0x33, 0x88], 0);
            var visits = new ObeliskVisits<TestColor>([0x44, 0x55, 0x66]);
            var serializer = new SafeObeliskSerializer<TestColor>(new ObeliskFlagsLayout(2, 1));

            var result = serializer.Serialize(chunk, visits);

            Assert.That(result.ToArray(), Is.EqualTo(new byte[] { 0x99, 0x02, 0x44, 0x55, 0x33, 0x88 }));
        }

        [Test]
        public void AStoredZeroCountPreservesAllBytesEvenWhenTheModelHasFlags()
        {
            var chunk = new ObeliskChunk("Obelisks", [0x99, 0x00, 0x11], 0, [new MapPosition(1, 2)]);
            var serializer = new SafeObeliskSerializer<TestColor>(new ObeliskFlagsLayout(2, 1));

            var result = serializer.Serialize(chunk, new ObeliskVisits<TestColor>([0x44]));

            Assert.That(result.ToArray(), Is.EqualTo(chunk.ToArray()));
        }

        [Test]
        public void AShorterModelLeavesTheOtherFlagBytesAlone()
        {
            var chunk = new ObeliskChunk("Obelisks", [0x99, 0x03, 0x11, 0x22, 0x33], 0);
            var visits = new ObeliskVisits<TestColor>([0x44]);
            var serializer = new SafeObeliskSerializer<TestColor>(new ObeliskFlagsLayout(2, 1));

            var result = serializer.Serialize(chunk, visits);

            Assert.That(result.ToArray(), Is.EqualTo(new byte[] { 0x99, 0x03, 0x44, 0x22, 0x33 }));
        }
    }
}
