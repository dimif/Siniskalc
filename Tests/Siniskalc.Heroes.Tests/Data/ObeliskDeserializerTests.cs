namespace Siniskalc.Heroes.Tests.Data
{
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Tests.Entities;

    [TestFixture]
    internal sealed class ObeliskDeserializerTests
    {
        [Test]
        public void TheNumberOfPositionsIsTheCountWhenTheChunkStoresNoCount()
        {
            var chunk = new ObeliskChunk("Obelisks", [0x11, 0x22, 0x33], 0, [new MapPosition(1, 2), new MapPosition(3, 4)]);
            var deserializer = new ObeliskDeserializer<TestColor>(new ObeliskFlagsLayout(0, null));

            var visits = deserializer.Deserialize(chunk);

            Assert.That(visits.Masks, Is.EqualTo(new byte[] { 0x11, 0x22 }));
            Assert.That(visits.PositionOf(0), Is.EqualTo(new MapPosition(1, 2)));
            Assert.That(visits.PositionOf(1), Is.EqualTo(new MapPosition(3, 4)));
        }

        [Test]
        public void TheStoredCountLimitsHowManyFlagsAreRead()
        {
            var chunk = new ObeliskChunk("Obelisks", [0x99, 0x02, 0x11, 0x22, 0x33], 0);
            var deserializer = new ObeliskDeserializer<TestColor>(new ObeliskFlagsLayout(2, 1));

            var visits = deserializer.Deserialize(chunk);

            Assert.That(visits.Masks, Is.EqualTo(new byte[] { 0x11, 0x22 }));
        }
    }
}
