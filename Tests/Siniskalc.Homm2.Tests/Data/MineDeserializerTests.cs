namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// What the bytes of a mine of the map read as, on records made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MineDeserializerTests
    {
        private static MapObjectChunk Mine(byte owner, byte kind) =>
            new("Mine", [owner, kind, 0, 0, 0, 0, 0], 100, new MapPosition(14, 15), 2);

        [Test]
        public void AMineIsItsKindItsTileItsPlaceInTheTableAndItsOwner()
        {
            var mine = new MineDeserializer(TestCatalog.Standard).Deserialize(Mine(2, (byte)MineKind.GoldMine));

            Assert.That((mine.Index, mine.Kind, mine.Position, mine.Owner), Is.EqualTo((2, MineKind.GoldMine, new MapPosition(14, 15), PlayerColor.Red)));
        }

        [TestCase(0xFF)]
        [TestCase(0x09)]
        [TestCase(0x80)]
        public void AnOwnerByteThatIsTheNeutralByteOrNoColourReadsAsNeutral(int owner)
        {
            var mine = new MineDeserializer(TestCatalog.Standard).Deserialize(Mine((byte)owner, (byte)MineKind.Sawmill));

            Assert.That(mine.Owner, Is.EqualTo(PlayerColor.Neutral));
        }
    }
}
