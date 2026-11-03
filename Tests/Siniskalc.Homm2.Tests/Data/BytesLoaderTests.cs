namespace Siniskalc.Homm2.Tests.Data
{
    using System.Text;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// How the chunks of the heroes, players, castles and obelisks are cut out of the bytes of a savegame, on a savegame made by hand with
    /// a layout that puts the tables one after the other.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class BytesLoaderTests
    {
        private const int PlayerTable = 100;
        private const int HeroTable = 1500;
        private const int CastleTable = 14500;
        private const int CastleTableEnd = CastleTable + (72 * 100);

        private readonly BytesLoader loader = new();

        private static SaveLayout Layout(int size = 4) =>
            new(GameVersion.BaseGame, size, size, HeroLayout.BaseRecordSize, 50, PlayerTable, HeroTable, CastleTable);

        private static byte[] Savegame(int length = CastleTableEnd + 6000) => new byte[length];

        private static void Text(byte[] bytes, int offset, string text) => Encoding.Latin1.GetBytes(text).CopyTo(bytes, offset);

        [Test]
        public void ThereIsAHeroChunkForEachOfTheFiftyFourHeroesNamedByTheTextOfItsRecordAndCutOutOfTheBytes()
        {
            var bytes = Savegame();
            Text(bytes, HeroTable + (7 * HeroLayout.BaseRecordSize) + HeroLayout.Name, "Maximus\0leftover");
            bytes[HeroTable + (7 * HeroLayout.BaseRecordSize) + HeroLayout.Owner] = 0x42;

            var chunks = this.loader.LoadHeroChunks(bytes, Layout());

            Assert.That(chunks, Has.Count.EqualTo(54));
            Assert.That((chunks[7].Name, chunks[7].StartIndex, chunks[7].Length), Is.EqualTo(("Maximus", HeroTable + (7 * HeroLayout.BaseRecordSize), HeroLayout.BaseRecordSize)));
            Assert.That(chunks[7].GetByteAt(HeroLayout.Owner), Is.EqualTo(0x42));
            Assert.That(chunks[8].Name, Is.Empty);
        }

        [Test]
        public void AChunkHoldsACopyOfItsBytesSoLaterChangesToTheSavegameDoNotReachIt()
        {
            var bytes = Savegame();
            var chunks = this.loader.LoadCastleChunks(bytes, Layout());

            bytes[CastleTable + CastleLayout.X] = 99;

            Assert.That(chunks[0].GetByteAt(CastleLayout.X), Is.Zero);
        }

        [Test]
        public void ThereIsACastleChunkForEachOfTheSeventyTwoPlacesOfTheTableNamedByTheTextAtTheEndOfItsRecord()
        {
            var bytes = Savegame();
            Text(bytes, CastleTable + (3 * CastleLayout.RecordSize) + CastleLayout.Name, "Xabran");

            var chunks = this.loader.LoadCastleChunks(bytes, Layout());

            Assert.That(chunks, Has.Count.EqualTo(72));
            Assert.That((chunks[3].Name, chunks[3].StartIndex, chunks[3].Length), Is.EqualTo(("Xabran", CastleTable + 300, CastleLayout.RecordSize)));
            Assert.That(chunks.Count(c => c.Name.Length > 0), Is.EqualTo(1));
        }

        [Test]
        public void ThePlayerChunksAreInTheOrderOfTheColoursWhateverOrderTheRecordsAreIn()
        {
            var bytes = Savegame();
            var colorsOfTheRecords = new byte[] { 3, 1, 2, 0, 5, 4 };
            for (var record = 0; record < 6; record++)
            {
                bytes[PlayerTable + (record * PlayerLayout.RecordSize) + PlayerLayout.Color] = colorsOfTheRecords[record];
                bytes[PlayerTable + (record * PlayerLayout.RecordSize) + PlayerLayout.HeroCount] = (byte)(10 + record);
            }

            var chunks = this.loader.LoadPlayerChunks(bytes, Layout());

            Assert.That(chunks.Select(c => c.Name), Is.EqualTo(new[] { "Blue", "Green", "Red", "Yellow", "Orange", "Purple" }));
            Assert.That(chunks[0].StartIndex, Is.EqualTo(PlayerTable + (3 * PlayerLayout.RecordSize)), "Blue is the fourth record");
            Assert.That(chunks[0].GetByteAt(PlayerLayout.HeroCount), Is.EqualTo(13));
            Assert.That(chunks.All(c => c.Length == PlayerLayout.RecordSize), Is.True);
        }

        [Test]
        public void RecordsThatDoNotNameTheSixColoursAreTakenInTheOrderOfTheColours()
        {
            var bytes = Savegame();
            for (var record = 0; record < 6; record++)
            {
                bytes[PlayerTable + (record * PlayerLayout.RecordSize) + PlayerLayout.Color] = 2;
            }

            var chunks = this.loader.LoadPlayerChunks(bytes, Layout());

            Assert.That(chunks.Select(c => c.StartIndex), Is.EqualTo(Enumerable.Range(0, 6).Select(r => PlayerTable + (r * PlayerLayout.RecordSize))));
            Assert.That(chunks.Select(c => c.Name), Is.EqualTo(new[] { "Blue", "Green", "Red", "Yellow", "Orange", "Purple" }));
        }

        [Test]
        public void ASavegameWithNoTableOfTilesHasNoObeliskChunk()
        {
            Assert.That(this.loader.LoadObeliskChunks(Savegame(), Layout()), Is.Empty);
        }

        /// <summary>A savegame whose tile table of a map of 4 by 4 tiles follows the flags of the obelisks, with an obelisk on each of the given tiles.</summary>
        private static byte[] SavegameWithObelisks(int size, params int[] tilesWithAnObelisk)
        {
            var flags = CastleTableEnd + ObeliskLayout.FlagsFromCastlesEnd;
            var bytes = Savegame(flags + ObeliskLayout.Capacity + 8 + (size * size * MapTileTable.TileSize) + 100);
            for (var i = 0; i < ObeliskLayout.Capacity; i++)
            {
                bytes[flags + i] = (byte)(i + 1);
            }

            var pair = flags + ObeliskLayout.Capacity + 4;
            BitConverter.GetBytes(size).CopyTo(bytes, pair);
            BitConverter.GetBytes(size).CopyTo(bytes, pair + 4);
            var tiles = pair + 8;
            foreach (var tile in tilesWithAnObelisk)
            {
                bytes[tiles + (tile * MapTileTable.TileSize) + MapTileTable.ObjectOffset] = ObeliskLayout.TileMarker;
            }

            return bytes;
        }

        [Test]
        public void ThereIsOneObeliskChunkWithTheTilesOfTheObelisksAndTheFortyEightBytesOfFlagsAfterTheCastles()
        {
            var bytes = SavegameWithObelisks(4, 1, 6);
            var flags = CastleTableEnd + ObeliskLayout.FlagsFromCastlesEnd;

            var chunk = this.loader.LoadObeliskChunks(bytes, Layout()).Single();

            Assert.That((chunk.Name, chunk.StartIndex, chunk.Length), Is.EqualTo(("Obelisks", flags, ObeliskLayout.Capacity)));
            Assert.That(chunk.GetByteAt(0), Is.EqualTo(1));
            Assert.That(chunk.Positions.Select(p => (p!.X, p.Y)), Is.EqualTo(new[] { (1, 0), (2, 1) }), "the tile 6 of a map 4 wide is column 2 of row 1");
        }

        [Test]
        public void AMapWithNoObeliskTileHasNoObeliskChunk()
        {
            Assert.That(this.loader.LoadObeliskChunks(SavegameWithObelisks(4), Layout()), Is.Empty);
        }

        [Test]
        public void OnlyTheFirstFortyEightObelisksOfAMapAreTaken()
        {
            var bytes = SavegameWithObelisks(8, Enumerable.Range(0, 50).ToArray());

            var chunk = this.loader.LoadObeliskChunks(bytes, Layout(8)).Single();

            Assert.That(chunk.Positions, Has.Count.EqualTo(ObeliskLayout.Capacity));
        }

        [Test]
        public void ASavegameWhoseTilesAreFoundButThatEndsBeforeTheFlagsOfTheObelisksHasNoObeliskChunk()
        {
            var tilesAt = CastleTableEnd + 4 + 8;
            var bytes = Savegame(tilesAt + (16 * MapTileTable.TileSize));
            BitConverter.GetBytes(4).CopyTo(bytes, CastleTableEnd + 4);
            BitConverter.GetBytes(4).CopyTo(bytes, CastleTableEnd + 8);
            bytes[tilesAt + (5 * MapTileTable.TileSize) + MapTileTable.ObjectOffset] = ObeliskLayout.TileMarker;

            Assert.That(this.loader.LoadObeliskChunks(bytes, Layout()), Is.Empty);
        }

        [Test]
        public void ASavegameThatEndsBeforeTheFlagsOfTheObelisksHasNoObeliskChunk()
        {
            var bytes = SavegameWithObelisks(4, 1);
            var cut = bytes[..(CastleTableEnd + ObeliskLayout.FlagsFromCastlesEnd + 10)];

            Assert.That(this.loader.LoadObeliskChunks(cut, Layout()), Is.Empty);
        }
    }
}
