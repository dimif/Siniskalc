namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The bytes the visits of the obelisks write into the record of the obelisks, on a record made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SafeObeliskSerializerTests
    {
        private readonly SafeObeliskSerializer serializer = new();

        private static readonly MapPosition?[] Places = [new MapPosition(3, 4), null, new MapPosition(9, 9)];

        private static ObeliskChunk Obelisks(params byte[] masks) => new("Obelisks", masks, 40, Places);

        private ObeliskVisits<PlayerColor> Visits(ObeliskChunk chunk) => new ObeliskDeserializer().Deserialize(chunk);

        [Test]
        public void TheVisitsThatWereReadLeaveEveryByteAlone()
        {
            var chunk = Obelisks(0x81, 0x24, 0);

            Assert.That(this.serializer.Serialize(chunk, this.Visits(chunk)).ToArray(), Is.EqualTo(chunk.ToArray()));
        }

        [Test]
        public void ANewVisitSetsTheBitOfTheColourOfThatObeliskOnly()
        {
            var chunk = Obelisks(0, 0, 0);

            var written = this.serializer.Serialize(chunk, this.Visits(chunk).WithVisited(2, PlayerColor.Red, true));

            Assert.That(ByteDiff.Of(chunk.ToArray(), written.ToArray()), Is.EqualTo(new[] { (2, (byte)0, (byte)0b0000_0100) }));
        }

        [Test]
        public void AVisitTakenAwayClearsItsBitAndKeepsTheOthers()
        {
            var chunk = Obelisks(0b0000_0111, 0, 0);

            var written = this.serializer.Serialize(chunk, this.Visits(chunk).WithVisited(0, PlayerColor.Green, false));

            Assert.That(ByteDiff.Of(chunk.ToArray(), written.ToArray()), Is.EqualTo(new[] { (0, (byte)0b0000_0111, (byte)0b0000_0101) }));
        }

        [Test]
        public void AllMarkedVisitedByAColourSetsItsBitOnEveryObelisk()
        {
            var chunk = Obelisks(0, 0b1000_0000, 0);

            var written = this.serializer.Serialize(chunk, this.Visits(chunk).WithAll(PlayerColor.Blue, true));

            Assert.That(written.ToArray(), Is.EqualTo(new byte[] { 1, 0b1000_0001, 1 }));
        }

        [Test]
        public void VisitsOfFewerObelisksThanTheChunkHasLeaveTheRestOfTheBytesAlone()
        {
            var chunk = Obelisks(5, 6, 7);

            var written = this.serializer.Serialize(chunk, new ObeliskVisits<PlayerColor>([0x10, 0x20]));

            Assert.That(written.ToArray(), Is.EqualTo(new byte[] { 0x10, 0x20, 7 }));
        }

        [Test]
        public void VisitsOfMoreObelisksThanTheChunkHasTilesAreNotWritten()
        {
            var chunk = Obelisks(5, 6, 7, 8, 9);

            var written = this.serializer.Serialize(chunk, new ObeliskVisits<PlayerColor>([1, 2, 3, 4, 5]));

            Assert.That(written.ToArray(), Is.EqualTo(new byte[] { 1, 2, 3, 8, 9 }), "the bytes past the tiles of the obelisks are not the visits of any obelisk");
        }
    }
}
