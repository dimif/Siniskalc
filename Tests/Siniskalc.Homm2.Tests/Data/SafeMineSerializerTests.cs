namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The bytes a mine model writes into the record of a mine and into the flags drawn on the tiles of the mine, on records made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SafeMineSerializerTests
    {
        private readonly SafeMineSerializer serializer = new();

        private static MapObjectChunk Mine(byte owner, params byte[][] parts) =>
            new("Mine", [owner, (byte)MineKind.GoldMine, 0, 0, 0, 0, 0], 100, new MapPosition(14, 15), 2, 0, parts.Select((bytes, i) => new MapObjectPart(500 + (i * 10), bytes)).ToList());

        private static MineModel Model(MineKind kind, PlayerColor owner) => new(2, kind, new MapPosition(14, 15), owner, TestCatalog.Standard);

        [Test]
        public void AnOwnerThatIsNotChangedLeavesTheRecordAndTheFlagsAsTheyAre()
        {
            var chunk = Mine(0xFF, [0x12, 0x34]);

            var written = this.serializer.Serialize(chunk, Model(MineKind.GoldMine, PlayerColor.Neutral));

            Assert.That(written, Is.SameAs(chunk), "a mine whose flag the game never drew stays so");
        }

        [Test]
        public void AnOwnerByteThatIsNoColourIsKeptWhileTheModelSaysNeutralAndReplacedWhenItSaysAColour()
        {
            var chunk = Mine(0x09, [0x12, 0x34]);

            Assert.That(this.serializer.Serialize(chunk, Model(MineKind.GoldMine, PlayerColor.Neutral)), Is.SameAs(chunk));
            Assert.That(this.serializer.Serialize(chunk, Model(MineKind.GoldMine, PlayerColor.Blue)).GetByteAt(0), Is.EqualTo(PlayerColor.Blue.Code));
        }

        [Test]
        public void ANewOwnerIsWrittenToTheRecordAndASeizedFlagIsDrawnOnEveryTileOfTheMine()
        {
            var chunk = Mine(0xFF, [MapObjectLayout.NoClass, MapObjectLayout.NoPicture, 0x77], [MapObjectLayout.NoClass, MapObjectLayout.NoPicture]);

            var written = this.serializer.Serialize(chunk, Model(MineKind.GoldMine, PlayerColor.Red));

            Assert.That(written.GetByteAt(0), Is.EqualTo(2));
            Assert.That(written.Parts.Select(p => p.Bytes), Is.EqualTo(new[] { new byte[] { 0x38, 16 }, new byte[] { 0x38, 16 } }), "the flag of a gold mine is picture 14 plus the colour");
            Assert.That(written.Parts.Select(p => p.StartIndex), Is.EqualTo(new[] { 500, 510 }));
        }

        [Test]
        public void TheFlagOfASawmillAndOfAnAlchemistLabHaveTheirOwnPictures()
        {
            var parts = new[] { MapObjectLayout.NoClass, MapObjectLayout.NoPicture };

            var sawmill = this.serializer.Serialize(Mine(0xFF, parts), Model(MineKind.Sawmill, PlayerColor.Green));
            var lab = this.serializer.Serialize(Mine(0xFF, parts), Model(MineKind.AlchemistLab, PlayerColor.Green));

            Assert.That(sawmill.Parts[0].Bytes, Is.EqualTo(new byte[] { 0x38, 29 }));
            Assert.That(lab.Parts[0].Bytes, Is.EqualTo(new byte[] { 0x38, 22 }));
        }

        [Test]
        public void AFlagThatAlreadyShowsTheNewOwnerIsLeftAsItIs()
        {
            var showing = new byte[] { 0x38, 16, 0xAB };
            var chunk = Mine(0xFF, showing, [MapObjectLayout.NoClass, MapObjectLayout.NoPicture]);

            var written = this.serializer.Serialize(chunk, Model(MineKind.GoldMine, PlayerColor.Red));

            Assert.That(written.Parts[0].Bytes, Is.EqualTo(showing), "its extra byte is not touched");
            Assert.That(written.Parts[1].Bytes, Is.EqualTo(new byte[] { 0x38, 16 }));
        }

        [Test]
        public void ANeutralMineHasTheNeutralByteAndNoFlagOnItsTiles()
        {
            var chunk = Mine(2, [0x38, 16], [0x38, 16]);

            var written = this.serializer.Serialize(chunk, Model(MineKind.GoldMine, PlayerColor.Neutral));

            Assert.That(written.GetByteAt(0), Is.EqualTo(0xFF));
            Assert.That(written.Parts.Select(p => p.Bytes), Is.EqualTo(new[] { new byte[] { 0x00, 0xFF }, new byte[] { 0x00, 0xFF } }));
        }

        [Test]
        public void TheKindTheTileAndThePlaceInTheTableAreNeverWritten()
        {
            var chunk = Mine(0xFF, [MapObjectLayout.NoClass, MapObjectLayout.NoPicture]);

            var written = this.serializer.Serialize(chunk, new MineModel(9, MineKind.Sawmill, new MapPosition(1, 1), PlayerColor.Blue, TestCatalog.Standard));

            Assert.That(ByteDiff.Of(chunk.ToArray(), written.ToArray()), Is.EqualTo(new[] { (0, (byte)0xFF, (byte)0) }));
        }
    }
}
