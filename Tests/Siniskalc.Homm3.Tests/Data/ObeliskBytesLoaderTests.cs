using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// How the record of the obelisks is found in a savegame, on savegames made by hand: it sits a fixed distance before the record of
    /// Red, which is where the table of players starts (or one record before the first colour's when that is not Red), starts with the
    /// marker FF 00 00, and holds a count and one byte of flags for each obelisk.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ObeliskBytesLoaderTests
    {
        private const int RedStart = 400;

        private readonly ObeliskBytesLoader loader = new();

        private static PlayerChunk Player(PlayerColor color, int start) =>
            new(color, new byte[PlayerLayout.ShadowOfDeath.Stride], start, PlayerLayout.ShadowOfDeath);

        private static PlayerChunk[] Red() => [Player(PlayerColor.Red, RedStart)];

        /// <summary>A savegame with the record of the obelisks in front of the record of Red: the marker, the count and the flags.</summary>
        private static byte[] Savegame(int count, int redStart = RedStart, Action<byte[], int>? change = null)
        {
            var bytes = new byte[redStart + 400];
            var start = redStart - ObeliskLayout.BeforeFirstPlayer;
            bytes[start] = 0xFF;
            bytes[start + ObeliskLayout.CountOffset] = (byte)count;
            for (var i = 0; i < count && start + ObeliskLayout.FlagsOffset + i < redStart; i++)
            {
                bytes[start + ObeliskLayout.FlagsOffset + i] = (byte)(i + 1);
            }

            change?.Invoke(bytes, start);
            return bytes;
        }

        [Test]
        public void NoPlayersMeansNoRecordOfObelisks()
        {
            Assert.That(this.loader.LoadObeliskChunks(Savegame(3), []), Is.Empty);
        }

        [Test]
        public void TheRecordIsTheMarkerTheCountAndOneByteOfFlagsForEachObeliskAtAFixedDistanceBeforeRed()
        {
            var chunk = this.loader.LoadObeliskChunks(Savegame(5), Red()).Single();

            var start = RedStart - ObeliskLayout.BeforeFirstPlayer;
            Assert.That((chunk.Name, chunk.StartIndex, chunk.Length), Is.EqualTo(("Obelisks", start, ObeliskLayout.MarkerLength + 5)));
            Assert.That(chunk.ToArray().Skip(ObeliskLayout.FlagsOffset), Is.EqualTo(new byte[] { 1, 2, 3, 4, 5 }));
            Assert.That(chunk.Positions, Is.Empty, "no table of the objects of the map was found to give them places");
        }

        [Test]
        public void WhenTheFirstPlayerIsNotRedTheRecordOfRedIsFoundOneRecordForEachColourBefore()
        {
            var layout = PlayerLayout.ShadowOfDeath;
            var blue = Player(PlayerColor.Blue, RedStart + layout.Stride);

            var chunk = this.loader.LoadObeliskChunks(Savegame(2), [blue]).Single();

            Assert.That(chunk.StartIndex, Is.EqualTo(RedStart - ObeliskLayout.BeforeFirstPlayer));
        }

        [TestCase(0)]
        [TestCase(49)]
        [TestCase(255)]
        public void ACountOfNoObeliskOrOfMoreThanAMapCanHaveIsNoRecord(int count)
        {
            Assert.That(this.loader.LoadObeliskChunks(Savegame(count), Red()), Is.Empty);
        }

        [Test]
        public void TheMostObelisksAMapCanHaveIsARecord()
        {
            var chunk = this.loader.LoadObeliskChunks(Savegame(48), Red()).Single();

            Assert.That(chunk.Length, Is.EqualTo(ObeliskLayout.MarkerLength + 48));
        }

        [TestCase(0, 0xFE)]
        [TestCase(1, 1)]
        [TestCase(2, 1)]
        public void ABytePatternThatIsNotTheMarkerIsNoRecord(int offset, int value)
        {
            var bytes = Savegame(3, change: (b, start) => b[start + offset] = (byte)value);

            Assert.That(this.loader.LoadObeliskChunks(bytes, Red()), Is.Empty);
        }

        [Test]
        public void ATableOfPlayersTooNearTheStartOfTheSavegameForTheRecordToFitHasNoRecordOfObelisks()
        {
            var bytes = new byte[200];

            Assert.That(this.loader.LoadObeliskChunks(bytes, [Player(PlayerColor.Red, ObeliskLayout.BeforeFirstPlayer - 1)]), Is.Empty);
        }
    }
}
