namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Homm2.Data;

    /// <summary>
    /// What the nine bytes of the castles that built something this turn read as, on records made by hand: one bit for each of the 72
    /// castles of the table, the first castle in the lowest bit of the first byte.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class BuiltTodayDeserializerTests
    {
        private static MapObjectChunk BuiltToday(Action<byte[]>? set = null)
        {
            var bytes = new byte[MapObjectLayout.BuiltTodaySize];
            set?.Invoke(bytes);
            return new MapObjectChunk("Built this turn", bytes, 700, null, 0);
        }

        [Test]
        public void NoBitMeansNoCastleBuiltAnything()
        {
            Assert.That(new BuiltTodayDeserializer().Deserialize(BuiltToday()).Castles, Is.Empty);
        }

        [Test]
        public void EachSetBitIsTheCastleAtThatPlaceOfTheTableInTheOrderOfTheTable()
        {
            var built = new BuiltTodayDeserializer().Deserialize(BuiltToday(b =>
            {
                b[0] = 0b0000_0101;
                b[1] = 0b1000_0000;
                b[8] = 0b1000_0001;
            }));

            Assert.That(built.Castles, Is.EqualTo(new[] { 0, 2, 15, 64, 71 }));
        }

        [Test]
        public void TheIndexOfTheRecordIsKept()
        {
            Assert.That(new BuiltTodayDeserializer().Deserialize(new MapObjectChunk("Built", new byte[9], 700, null, 5)).Index, Is.EqualTo(5));
        }
    }
}
