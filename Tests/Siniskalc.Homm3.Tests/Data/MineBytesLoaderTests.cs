using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Tests how the table of mines is found in bytes made by hand: a run of records of 62 bytes that look like mines, with the mines among
    /// them cut out.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MineBytesLoaderTests
    {
        private readonly MineBytesLoader loader = new();

        /// <summary>A record that passes every check of the loader: no creature in the guard, a tile, an owner, a kind.</summary>
        private static byte[] Record(int x = 5, int y = 6, int level = 0, int owner = 0xFF, int kind = 6, int abandoned = 0)
        {
            var bytes = new byte[MineLayout.RecordSize];
            bytes[MineLayout.X] = (byte)x;
            bytes[MineLayout.Y] = (byte)y;
            bytes[MineLayout.Level] = (byte)level;
            bytes[MineLayout.Owner] = (byte)owner;
            bytes[MineLayout.Kind] = (byte)kind;
            bytes[MineLayout.Abandoned] = (byte)abandoned;
            for (var slot = 0; slot < MineLayout.SlotCount; slot++)
            {
                BitConverter.GetBytes(uint.MaxValue).CopyTo(bytes, MineLayout.Guard.TypeAt(slot));
            }

            return bytes;
        }

        private static byte[] WithGuard(byte[] record, int slot, uint type, uint count)
        {
            BitConverter.GetBytes(type).CopyTo(record, MineLayout.Guard.TypeAt(slot));
            BitConverter.GetBytes(count).CopyTo(record, MineLayout.Guard.CountAt(slot));
            return record;
        }

        private static byte[] Table(int junk, params byte[][] records) =>
            new byte[junk].Concat(records.SelectMany(r => r)).Concat(new byte[10]).ToArray();

        [Test]
        public void ARunOfThreeMinesIsFoundInTheOrderOfTheTableWithTheirOffsetsAndIndexes()
        {
            var bytes = Table(13, Record(1, 2, kind: 0), Record(3, 4, kind: 3), Record(7, 8, kind: 6));

            var chunks = this.loader.LoadMineChunks(bytes);

            Assert.That(chunks.Select(c => c.StartIndex), Is.EqualTo(new[] { 13, 13 + 62, 13 + 124 }));
            Assert.That(chunks.Select(c => c.Index), Is.EqualTo(new[] { 0, 1, 2 }));
            Assert.That(chunks.Select(c => c.Length), Is.All.EqualTo(MineLayout.RecordSize));
            Assert.That(chunks[1].ToArray(), Is.EqualTo(Record(3, 4, kind: 3)));
        }

        [Test]
        public void ATableThatEndsWithTheLastByteOfTheSavegameIsFound()
        {
            var bytes = new byte[5].Concat(new[] { Record(1, 1), Record(2, 2), Record(3, 3) }.SelectMany(r => r)).ToArray();

            Assert.That(this.loader.LoadMineChunks(bytes), Has.Count.EqualTo(3));
        }

        [Test]
        public void AChunkIsNamedAfterTheKindOfTheMineAndItsTile()
        {
            var bytes = Table(0, Record(1, 2, 0, kind: 0), Record(3, 4, 1, kind: 6), Record(5, 6, 0, kind: 6));

            var names = this.loader.LoadMineChunks(bytes).Select(c => c.Name).ToList();

            Assert.That(names[1], Is.EqualTo(MineKinds.NameAt(MineKind.GoldMine, new MapPosition(3, 4, 1), hasLevels: true)));
            Assert.That(names[0], Is.EqualTo(MineKinds.NameAt(MineKind.Sawmill, new MapPosition(1, 2, 0), hasLevels: true)));
        }

        [Test]
        public void ARunOfTwoRecordsIsNotATable()
        {
            Assert.That(this.loader.LoadMineChunks(Table(3, Record(1, 2), Record(3, 4))), Is.Empty);
        }

        [Test]
        public void BytesThatAreTooShortOrHoldNoRecordGiveNoMines()
        {
            Assert.That(this.loader.LoadMineChunks([]), Is.Empty);
            Assert.That(this.loader.LoadMineChunks(new byte[61]), Is.Empty);
            Assert.That(this.loader.LoadMineChunks(new byte[1000]), Is.Empty);
        }

        [Test]
        public void ARecordOfAnObjectThatIsNoMineOrSitsAtTheCornerIsInTheRunButNotAMine()
        {
            var bytes = Table(0, Record(1, 2, kind: 6), Record(3, 4, kind: 7), Record(0, 0, kind: 2), Record(9, 9, kind: 0));

            var chunks = this.loader.LoadMineChunks(bytes);

            Assert.That(chunks.Select(c => c.StartIndex), Is.EqualTo(new[] { 0, 62 * 3 }));
            Assert.That(chunks.Select(c => c.Index), Is.EqualTo(new[] { 0, 1 }));
        }

        [Test]
        public void AMineWithOnlyOneCoordinateAtZeroIsStillAMine()
        {
            var bytes = Table(0, Record(0, 5), Record(5, 0), Record(5, 5));

            Assert.That(this.loader.LoadMineChunks(bytes), Has.Count.EqualTo(3));
        }

        [Test]
        public void TwoTablesAreBothReadAndTheIndexesGoOn()
        {
            var bytes = Table(0, Record(1, 1), Record(2, 2), Record(3, 3), new byte[7], Record(4, 4), Record(5, 5), Record(6, 6));

            var chunks = this.loader.LoadMineChunks(bytes);

            Assert.That(chunks.Select(c => c.Index), Is.EqualTo(new[] { 0, 1, 2, 3, 4, 5 }));
            Assert.That(chunks[3].StartIndex, Is.EqualTo((62 * 3) + 7));
        }

        [TestCaseSource(nameof(Invalid))]
        public void ARecordThatBreaksOneCheckEndsTheRunSoNoTableIsFound(string reason, byte[] record)
        {
            var bytes = Table(0, Record(1, 1), record, Record(3, 3));

            Assert.That(this.loader.LoadMineChunks(bytes), Is.Empty, reason);
        }

        [TestCaseSource(nameof(Valid))]
        public void ARecordAtTheEdgeOfEveryCheckStillFormsATable(string reason, byte[] record)
        {
            var bytes = Table(0, Record(1, 1), record, Record(3, 3));

            Assert.That(this.loader.LoadMineChunks(bytes), Has.Count.EqualTo(3), reason);
        }

        private static IEnumerable<object[]> Invalid()
        {
            var tooFar = (int)Constants.Map.MaxCoordinate + 1;
            yield return ["x beyond the map", Record(x: tooFar)];
            yield return ["y beyond the map", Record(y: tooFar)];
            yield return ["a level above 1", Record(level: 2)];
            yield return ["an owner that is no colour", Record(owner: 8)];
            yield return ["abandoned above 1", Record(abandoned: 2)];
            yield return ["a used slot with no count", WithGuard(Record(), 0, 14, 0)];
            yield return ["an empty slot with a count", WithGuard(Record(), 2, uint.MaxValue, 5)];
            yield return ["a creature code above the limit", WithGuard(Record(), 0, MineLayout.MaxCreatureCode + 1, 5)];
            yield return ["a count above the limit", WithGuard(Record(), 0, 14, MineLayout.MaxCount + 1)];
            yield return ["bytes that are no record at all", new byte[MineLayout.RecordSize]];
        }

        private static IEnumerable<object[]> Valid()
        {
            yield return ["the last tile of the map", Record(x: (int)Constants.Map.MaxCoordinate, y: (int)Constants.Map.MaxCoordinate)];
            yield return ["the last column with another row", Record(x: (int)Constants.Map.MaxCoordinate, y: 1)];
            yield return ["the last row with another column", Record(x: 1, y: (int)Constants.Map.MaxCoordinate)];
            yield return ["level 1", Record(level: 1)];
            yield return ["the last colour", Record(owner: 7)];
            yield return ["no owner", Record(owner: 0xFF)];
            yield return ["abandoned 1", Record(abandoned: 1)];
            yield return ["the highest creature code", WithGuard(Record(), 6, MineLayout.MaxCreatureCode, 1)];
            yield return ["the highest count", WithGuard(Record(), 3, 14, MineLayout.MaxCount)];
        }
    }
}
