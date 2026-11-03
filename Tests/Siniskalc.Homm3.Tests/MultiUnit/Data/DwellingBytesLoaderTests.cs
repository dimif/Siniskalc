using Siniskalc.Homm3.Entities;
using Siniskalc.Core.Data;
using Siniskalc.Homm3.Data;

namespace Siniskalc.Homm3.Tests.MultiUnit.Data
{
    /// <summary>
    /// Tests how dwellings are found in complete counted tables made by hand, each
    /// naming the type of a creature generator.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class DwellingBytesLoaderTests
    {
        private const int MinesEnd = 40;

        private readonly DwellingBytesLoader loader = new(TestCatalog.For(GameVersion.HornOfTheAbyss), new MapObjectTablesLocator());

        private static byte[] Record(int creature = 14, int x = 5, int y = 6, int level = 0, int owner = 0xFF, int type = DwellingLayout.CreatureGeneratorType)
        {
            var bytes = new byte[DwellingLayout.RecordSize];
            bytes[DwellingLayout.Creature] = (byte)creature;
            bytes[DwellingLayout.X] = (byte)x;
            bytes[DwellingLayout.Y] = (byte)y;
            bytes[DwellingLayout.Level] = (byte)level;
            bytes[DwellingLayout.Owner] = (byte)owner;
            bytes[DwellingLayout.ObjectType] = (byte)type;
            for (var slot = 0; slot < MineLayout.SlotCount; slot++)
            {
                BitConverter.GetBytes(uint.MaxValue).CopyTo(bytes, DwellingLayout.Guard.TypeAt(slot));
            }

            return bytes;
        }

        private static byte[] Savegame(params byte[][] records) =>
            new byte[MinesEnd].Concat(BitConverter.GetBytes((ushort)records.Length)).Concat(records.SelectMany(r => r)).Concat(new byte[20]).ToArray();

        private IReadOnlyList<RecordChunk> Load(byte[] bytes, params BytesChunk[] others) =>
            this.loader.LoadDwellingChunks(bytes, others);

        [Test]
        public void ACountedGeneratorTableIsFoundWithOffsetsAndIndexesWithoutAnyMines()
        {
            var chunks = this.Load(Savegame(Record(), Record(), Record()));

            Assert.That(chunks.Select(c => c.StartIndex), Is.EqualTo(new[] { MinesEnd + 2, MinesEnd + 77, MinesEnd + 152 }));
            Assert.That(chunks.Select(c => c.Index), Is.EqualTo(new[] { 0, 1, 2 }));
            Assert.That(chunks[2].ToArray(), Is.EqualTo(Record()[..DwellingLayout.EditableSize]));
        }

        [Test]
        public void TheTableCanBeLocatedAnywhereAfterAnExplicitEmptyOwnerGuardCount()
        {
            var bytes = new byte[MinesEnd + 30].Concat(BitConverter.GetBytes((ushort)3)).Concat(new[] { Record(), Record(), Record() }.SelectMany(r => r)).ToArray();

            var chunks = this.Load(bytes);

            Assert.That(chunks.Select(c => c.StartIndex), Is.EqualTo(new[] { MinesEnd + 32, MinesEnd + 107, MinesEnd + 182 }));
        }

        [Test]
        public void ATableThatEndsWithTheLastByteOfTheSavegameIsFound()
        {
            var bytes = Savegame(Record(), Record(), Record())[..^20];

            Assert.That(this.Load(bytes), Has.Count.EqualTo(3));
        }

        [Test]
        public void AChunkThatEndsWhereTheFirstRecordStartsDoesNotOverlapIt()
        {
            var before = new RecordChunk("Other", new byte[10], MinesEnd + 2 - 10, 0);

            Assert.That(this.Load(Savegame(Record(), Record(), Record()), before), Has.Count.EqualTo(3));
        }

        [Test]
        public void ARunThatStartsAnywhereElseIsNotATable()
        {
            var bytes = new byte[MinesEnd + 1].Concat(new[] { Record(), Record(), Record() }.SelectMany(r => r)).ToArray();

            Assert.That(this.Load(bytes), Is.Empty);
        }

        [Test]
        public void ASingleCountedDwellingIsFoundWithoutMines()
        {
            Assert.That(this.Load(Savegame(Record())), Has.Count.EqualTo(1));
        }

        [Test]
        public void TwoCountedDwellingsAreFoundWithoutMines()
        {
            Assert.That(this.Load(Savegame(Record(), Record())), Has.Count.EqualTo(2));
        }

        [Test]
        public void UnsupportedGeneratorsArePreservedWhileSingleCreatureDwellingsOnBothSidesAreFound()
        {
            var bytes = Savegame(Record(creature: 54), Record(type: 20), Record(creature: 176));
            var chunks = this.Load(bytes);
            Assert.That(chunks.Select(c => c.StartIndex), Is.EqualTo(new[] { MinesEnd + 2, MinesEnd + 152 }));
            Assert.That(chunks.Select(c => c.Index), Is.EqualTo(new[] { 0, 1 }));
            Assert.That(chunks.Select(c => c.GetByteAt(DwellingLayout.Creature)), Is.EqualTo(new[] { 54, 176 }));
        }

        [Test]
        public void ARecordThatDoesNotFitInTheBytesIsNotRead()
        {
            var bytes = Savegame(Record(), Record(), Record()).Take(MinesEnd + 2 + (3 * DwellingLayout.RecordSize) - 2).ToArray();

            Assert.That(this.Load(bytes), Is.Empty);
        }

        [Test]
        public void AChunkIsNamedAfterItsCreatureAndTile()
        {
            var chunks = this.Load(Savegame(Record(creature: 14, x: 3, y: 4), Record(creature: 250, x: 7, y: 8), Record()));

            Assert.That(chunks[0].Name, Is.EqualTo("Centaur dwelling 3, 4"));
            Assert.That(chunks[1].Name, Is.EqualTo("Creature 250 dwelling 7, 8"));
        }

        [TestCaseSource(nameof(Invalid))]
        public void ARecordThatBreaksOneCheckEndsTheRun(string reason, byte[] record)
        {
            Assert.That(this.Load(Savegame(Record(), record, Record())), Is.Empty, reason);
            Assert.That(this.Load(Savegame(Record(), Record(), Record(), record)), Is.Empty, reason);
        }

        [TestCaseSource(nameof(Valid))]
        public void ARecordAtTheEdgeOfEveryCheckStillFormsATable(string reason, byte[] record)
        {
            Assert.That(this.Load(Savegame(Record(), record, Record())), Has.Count.EqualTo(3), reason);
        }

        [Test]
        public void AnyOverlapRefusesTheWholeTableRatherThanExposingAPartialTable()
        {
            var bytes = Savegame(Record(), Record(), Record(), Record());
            var overlapsTheThird = new RecordChunk("Other", new byte[10], MinesEnd + 2 + (2 * DwellingLayout.RecordSize) + 10, 0);
            var overlapsTheFourth = new RecordChunk("Other", new byte[10], MinesEnd + 2 + (3 * DwellingLayout.RecordSize) + 10, 0);
            var startsWhereTheFourthEnds = new RecordChunk("Other", new byte[10], MinesEnd + 2 + (4 * DwellingLayout.RecordSize) - 1, 0);

            Assert.That(this.Load(bytes, overlapsTheThird), Is.Empty);
            Assert.That(this.Load(bytes, overlapsTheFourth), Is.Empty);
            Assert.That(this.Load(bytes, startsWhereTheFourthEnds), Has.Count.EqualTo(4));
        }

        private static IEnumerable<object[]> Invalid()
        {
            var tooFar = (int)Constants.Map.MaxCoordinate + 1;
            yield return ["a type that is no creature generator", Record(type: 16)];
            yield return ["an owner that is no colour", Record(owner: 8)];
            yield return ["a level above 1", Record(level: 2)];
            yield return ["x beyond the map", Record(x: tooFar)];
            yield return ["y beyond the map", Record(y: tooFar)];
        }

        private static IEnumerable<object[]> Valid()
        {
            yield return ["no owner", Record(owner: 0xFF)];
            yield return ["the last colour", Record(owner: 7)];
            yield return ["level 1", Record(level: 1)];
            yield return ["the last tile of the map", Record(x: (int)Constants.Map.MaxCoordinate, y: (int)Constants.Map.MaxCoordinate)];
            yield return ["the last column with another row", Record(x: (int)Constants.Map.MaxCoordinate, y: 1)];
            yield return ["the last row with another column", Record(x: 1, y: (int)Constants.Map.MaxCoordinate)];
        }
    }
}
