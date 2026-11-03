using Siniskalc.Homm3.Entities;
using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;

namespace Siniskalc.Homm3.Tests.MultiUnit.Data
{
    /// <summary>
    /// Tests the count, every owner/guard record and the following generator table before exposing mines for editing.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class MineBytesLoaderTests
    {
        private readonly MineBytesLoader loader = new(TestCatalog.Hota, new MapObjectTablesLocator());

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
            new byte[junk].Concat(BitConverter.GetBytes(records.Length)).Concat(records.SelectMany(r => r)).Concat(new byte[2]).ToArray();

        [Test]
        public void ARunOfThreeMinesIsFoundInTheOrderOfTheTableWithTheirOffsetsAndIndexes()
        {
            var bytes = Table(13, Record(1, 2, kind: 0), Record(3, 4, kind: 3), Record(7, 8, kind: 6));

            var chunks = this.loader.LoadMineChunks(bytes);

            Assert.That(chunks.Select(c => c.StartIndex), Is.EqualTo(new[] { 17, 17 + 62, 17 + 124 }));
            Assert.That(chunks.Select(c => c.Index), Is.EqualTo(new[] { 0, 1, 2 }));
            Assert.That(chunks.Select(c => c.Length), Is.All.EqualTo(MineLayout.RecordSize));
            Assert.That(chunks[1].ToArray(), Is.EqualTo(Record(3, 4, kind: 3)));
        }

        [Test]
        public void ATableThatEndsWithTheLastByteOfTheSavegameIsFound()
        {
            var bytes = Table(5, Record(1, 1), Record(2, 2), Record(3, 3));

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
        public void OneAndTwoCountedRecordsAreEnoughToIdentifyATable()
        {
            Assert.That(this.loader.LoadMineChunks(Table(3, Record(1, 2))), Has.Count.EqualTo(1));
            Assert.That(this.loader.LoadMineChunks(Table(3, Record(1, 2), Record(3, 4))), Has.Count.EqualTo(2));
        }

        [Test]
        public void BytesThatAreTooShortOrHoldNoRecordGiveNoMines()
        {
            Assert.That(this.loader.LoadMineChunks([]), Is.Empty);
            Assert.That(this.loader.LoadMineChunks(new byte[61]), Is.Empty);
            Assert.That(this.loader.LoadMineChunks(new byte[1000]), Is.Empty);
        }

        [Test]
        public void ARecordOfAnObjectThatIsNoMineIsNotReturnedAndACornerMineIsKept()
        {
            var bytes = Table(0, Record(1, 2, kind: 6), Record(3, 4, kind: 100), Record(0, 0, kind: 2), Record(9, 9, kind: 0));

            var chunks = this.loader.LoadMineChunks(bytes);

            Assert.That(chunks.Select(c => c.StartIndex), Is.EqualTo(new[] { 4, 4 + (62 * 2), 4 + (62 * 3) }));
            Assert.That(chunks.Select(c => c.Index), Is.EqualTo(new[] { 0, 1, 2 }));
        }

        [Test]
        public void AMineWithOnlyOneCoordinateAtZeroIsStillAMine()
        {
            var bytes = Table(0, Record(0, 5), Record(5, 0), Record(5, 5));

            Assert.That(this.loader.LoadMineChunks(bytes), Has.Count.EqualTo(3));
        }

        [Test]
        public void TwoCompetingTablesAreNotExposedForEditing()
        {
            var bytes = Table(0, Record(1, 1), Record(2, 2), Record(3, 3))
                .Concat(Table(7, Record(4, 4), Record(5, 5), Record(6, 6))).ToArray();

            var chunks = this.loader.LoadMineChunks(bytes);

            Assert.That(chunks, Is.Empty);
        }

        [Test]
        public void ShadowOfDeathUsesAOneByteCountAndKeepsTheFirstMinesOwnCoordinates()
        {
            var bytes = new byte[] { 2 }.Concat(Record(4, 8)).Concat(Record(7, 8)).Concat(new byte[2]).ToArray();
            var chunks = new MineBytesLoader(TestCatalog.For(GameVersion.ShadowOfDeath), new MapObjectTablesLocator()).LoadMineChunks(bytes);
            Assert.That(chunks.Select(c => c.StartIndex), Is.EqualTo(new[] { 1, 63 }));
            Assert.That(chunks.Select(c => c.GetByteAt(MineLayout.X)), Is.EqualTo(new[] { 4, 7 }));
        }

        [Test]
        public void ARunWithoutACountAndACountedTableWithoutItsFollowingHeaderAreRefused()
        {
            var run = Record(1, 2).Concat(Record(3, 4)).Concat(Record(5, 6)).ToArray();
            Assert.That(this.loader.LoadMineChunks(run), Is.Empty);
            Assert.That(this.loader.LoadMineChunks(BitConverter.GetBytes(3).Concat(run).ToArray()), Is.Empty);
        }

        [Test]
        public void ATruncatedOrOversizedCountDoesNotProducePartialMines()
        {
            var bytes = Table(0, Record(1, 2), Record(3, 4));
            Assert.That(this.loader.LoadMineChunks(bytes[..^3]), Is.Empty);
            BitConverter.GetBytes(uint.MaxValue).CopyTo(bytes, 0);
            Assert.That(this.loader.LoadMineChunks(bytes), Is.Empty);
        }

        [Test]
        public void AFollowingGeneratorTableMustFitAndHaveValidRecords()
        {
            var bytes = Table(0, Record(1, 2));
            bytes[^2] = 1;
            Assert.That(this.loader.LoadMineChunks(bytes), Is.Empty);
            Assert.That(this.loader.LoadMineChunks(bytes.Concat(new byte[75]).ToArray()), Is.Empty);
        }

        [TestCase(17)]
        [TestCase(18)]
        [TestCase(19)]
        [TestCase(20)]
        public void AFollowingGeneratorTableAcceptsEveryGeneratorKindIncludingMultigenerators(byte kind)
        {
            var bytes = Table(7, Record(4, 8));
            bytes[^2] = 1;
            var generator = new byte[75];
            generator[DwellingLayout.Owner] = 255;
            generator[DwellingLayout.ObjectType] = kind;
            generator[DwellingLayout.X] = 11;
            generator[DwellingLayout.Y] = 8;
            for (var slot = 0; slot < MineLayout.SlotCount; slot++)
            {
                BitConverter.GetBytes(uint.MaxValue).CopyTo(generator, DwellingLayout.Guard.TypeAt(slot));
            }

            Assert.That(this.loader.LoadMineChunks(bytes.Concat(generator).ToArray()).Single().StartIndex, Is.EqualTo(11));
        }

        [Test]
        public void ASuffixThatResemblesASmallerCountedTableDoesNotHideTheWholeTable()
        {
            // The first record's level is also a plausible count of one immediately before the second record.
            var bytes = new byte[] { 2 }.Concat(Record(4, 8, level: 1)).Concat(Record(7, 8)).Concat(new byte[2]).ToArray();
            var chunks = new MineBytesLoader(TestCatalog.For(GameVersion.ShadowOfDeath), new MapObjectTablesLocator()).LoadMineChunks(bytes);
            Assert.That(chunks.Select(c => c.StartIndex), Is.EqualTo(new[] { 1, 63 }));
        }

        [Test]
        public void ACountThatOmitsTheLastRecordIsNotAcceptedAsAPartialTable()
        {
            var bytes = Table(0, Record(1, 2), Record(3, 4));
            bytes[0] = 1;
            Assert.That(this.loader.LoadMineChunks(bytes), Is.Empty);
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
