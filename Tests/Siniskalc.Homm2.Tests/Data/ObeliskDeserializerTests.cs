namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// What the bytes of the obelisks of the map read as, on a record made by hand: one byte for each obelisk whose bits tell which
    /// players have visited it.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ObeliskDeserializerTests
    {
        private static readonly MapPosition?[] Places = [new MapPosition(3, 4), null, new MapPosition(9, 9)];

        private static ObeliskChunk Obelisks(params byte[] masks) => new("Obelisks", masks, 40, Places);

        [Test]
        public void EachObeliskHasOneByteWithABitForEachColourThatVisitedIt()
        {
            var visits = new ObeliskDeserializer().Deserialize(Obelisks(0b0000_0001, 0b0010_0100, 0));

            Assert.That(visits.Masks, Is.EqualTo(new byte[] { 1, 0x24, 0 }));
            Assert.That((visits.IsVisited(0, PlayerColor.Blue), visits.IsVisited(0, PlayerColor.Green)), Is.EqualTo((true, false)));
            Assert.That((visits.IsVisited(1, PlayerColor.Red), visits.IsVisited(1, PlayerColor.Purple)), Is.EqualTo((true, true)));
            Assert.That(visits.VisitedCount(PlayerColor.Blue), Is.EqualTo(1));
        }

        [Test]
        public void TheTilesOfTheObelisksAreTheOnesTheChunkWasFoundWithAndAMissingTileStaysMissing()
        {
            var visits = new ObeliskDeserializer().Deserialize(Obelisks(0, 0, 0));

            Assert.That(Enumerable.Range(0, 3).Select(visits.PositionOf), Is.EqualTo(Places));
        }

        [Test]
        public void OnlyAsManyBytesAsTheChunkHasTilesAreRead()
        {
            var visits = new ObeliskDeserializer().Deserialize(Obelisks(1, 2, 4, 8, 16));

            Assert.That(visits.Masks, Is.EqualTo(new byte[] { 1, 2, 4 }));
        }

        [Test]
        public void BitsOfColoursTheGameDoesNotHaveAreReadAsTheyAreAndNotAnError()
        {
            var visits = new ObeliskDeserializer().Deserialize(Obelisks(0xC0, 0, 0));

            Assert.That(visits.Masks[0], Is.EqualTo(0xC0));
            Assert.That(PlayerColor.All.Select(c => visits.IsVisited(0, c)), Has.All.False);
        }
    }
}
