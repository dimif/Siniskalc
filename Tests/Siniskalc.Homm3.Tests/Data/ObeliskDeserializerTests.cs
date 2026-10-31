using Siniskalc.Heroes.Data;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// What the bytes of the obelisks of the map read as, on a record made by hand: a count of obelisks in the fourth byte, then one byte
    /// for each with a bit for each colour that has visited it.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ObeliskDeserializerTests
    {
        private static readonly MapPosition?[] Places = [new MapPosition(3, 4), null, new MapPosition(9, 9)];

        /// <summary>The record: four bytes of marker, the flags from the fifth byte, and the count in the fourth.</summary>
        internal static ObeliskChunk Obelisks(byte count, params byte[] flags)
        {
            var bytes = new byte[ObeliskLayout.FlagsOffset + flags.Length];
            bytes[ObeliskLayout.CountOffset] = count;
            flags.CopyTo(bytes, ObeliskLayout.FlagsOffset);
            return new ObeliskChunk("Obelisks", bytes, 40, Places);
        }

        [Test]
        public void EachObeliskHasOneByteWithABitForEachColourThatVisitedItStartingWithRed()
        {
            var visits = new ObeliskDeserializer<PlayerColor>(ObeliskLayout.Flags).Deserialize(Obelisks(3, 0b0000_0001, 0b1000_0010, 0));

            Assert.That(visits.Masks, Is.EqualTo(new byte[] { 1, 0x82, 0 }));
            Assert.That((visits.IsVisited(0, PlayerColor.Red), visits.IsVisited(0, PlayerColor.Blue)), Is.EqualTo((true, false)));
            Assert.That((visits.IsVisited(1, PlayerColor.Blue), visits.IsVisited(1, PlayerColor.Pink)), Is.EqualTo((true, true)));
            Assert.That(visits.VisitedCount(PlayerColor.Red), Is.EqualTo(1));
        }

        [Test]
        public void OnlyAsManyBytesAsTheCountSaysAreReadWhateverElseTheRecordHolds()
        {
            var visits = new ObeliskDeserializer<PlayerColor>(ObeliskLayout.Flags).Deserialize(Obelisks(2, 5, 6, 7, 8));

            Assert.That(visits.Masks, Is.EqualTo(new byte[] { 5, 6 }));
        }

        [Test]
        public void ARecordWithNoObeliskHasNoVisits()
        {
            Assert.That(new ObeliskDeserializer<PlayerColor>(ObeliskLayout.Flags).Deserialize(Obelisks(0, 5, 6)).Count, Is.Zero);
        }

        [Test]
        public void TheTilesOfTheObelisksAreTheOnesTheChunkWasFoundWithAndAMissingTileStaysMissing()
        {
            var visits = new ObeliskDeserializer<PlayerColor>(ObeliskLayout.Flags).Deserialize(Obelisks(3, 0, 0, 0));

            Assert.That(Enumerable.Range(0, 3).Select(visits.PositionOf), Is.EqualTo(Places));
        }
    }
}
