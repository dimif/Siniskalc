using System.Text;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// How the tiles of the objects of the map are found in a savegame, on bytes made by hand: a table of definitions (a 16-bit length,
    /// the name of a .def file and 33 more bytes, for each), a 16-bit count, and then a definition number and x, y and z for each object.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MapObjectPositionsTests
    {
        private static readonly string[] FiveDefinitions = ["AVWmrnd0.def", "AVLabc00.def", "AVXdef01.def", "AVSghi02.def", "AVMjkl03.def"];

        /// <summary>A savegame: four bytes of zeros, the definitions, the count, the objects as (x, y, z), then some zeros.</summary>
        private static byte[] Savegame(IEnumerable<string> definitions, IEnumerable<(int X, int Y, int Z)> objects, int? count = null)
        {
            var bytes = new List<byte>(new byte[4]);
            foreach (var name in definitions)
            {
                bytes.AddRange(BitConverter.GetBytes((ushort)name.Length));
                bytes.AddRange(Encoding.ASCII.GetBytes(name));
                bytes.AddRange(new byte[33]);
            }

            var list = objects.ToList();
            bytes.AddRange(BitConverter.GetBytes((ushort)(count ?? list.Count)));
            foreach (var (x, y, z) in list)
            {
                bytes.AddRange(BitConverter.GetBytes((ushort)7));
                bytes.AddRange(new[] { (byte)x, (byte)y, (byte)z });
            }

            bytes.AddRange(new byte[40]);
            return [.. bytes];
        }

        private static IReadOnlyList<MapPosition> Find(byte[] bytes, int? searchEnd = null) => MapObjectPositions.Find(bytes, searchEnd ?? bytes.Length);

        [Test]
        public void EveryObjectOfTheListHasItsTileInTheOrderOfTheList()
        {
            var bytes = Savegame(FiveDefinitions, [(1, 2, 0), (30, 40, 1), (251, 250, 0)]);

            Assert.That(Find(bytes), Is.EqualTo(new[] { new MapPosition(1, 2, 0), new MapPosition(30, 40, 1), new MapPosition(251, 250, 0) }));
        }

        [Test]
        public void AMoreThanFiveDefinitionsAreAllSkippedBeforeTheCount()
        {
            var bytes = Savegame(FiveDefinitions.Concat(["AVQnop04.def", "AVRqrs05.def"]), [(9, 8, 0)]);

            Assert.That(Find(bytes), Is.EqualTo(new[] { new MapPosition(9, 8, 0) }));
        }

        [Test]
        public void FewerThanFiveDefinitionsIsNoTable()
        {
            var bytes = Savegame(FiveDefinitions.Take(4), [(1, 2, 0)]);

            Assert.That(Find(bytes), Is.Empty);
        }

        [Test]
        public void BytesWithNoDefinitionsHaveNoPositions()
        {
            Assert.That(Find(new byte[200]), Is.Empty);
            Assert.That(Find([]), Is.Empty);
        }

        [Test]
        public void ACountOfNoObjectGivesNoPositions()
        {
            Assert.That(Find(Savegame(FiveDefinitions, [])), Is.Empty);
        }

        [Test]
        public void OnlyAsManyObjectsAsTheCountSaysAreRead()
        {
            var bytes = Savegame(FiveDefinitions, [(1, 1, 0), (2, 2, 0), (3, 3, 0)], count: 2);

            Assert.That(Find(bytes), Is.EqualTo(new[] { new MapPosition(1, 1, 0), new MapPosition(2, 2, 0) }));
        }

        [Test]
        public void AListCutShortByTheEndOfTheSearchGivesTheObjectsThatFit()
        {
            var bytes = Savegame(FiveDefinitions, [(1, 1, 0), (2, 2, 0), (3, 3, 0)]);
            var listStart = bytes.Length - 40 - (3 * 5);

            Assert.That(Find(bytes, listStart + 9), Is.EqualTo(new[] { new MapPosition(1, 1, 0) }));
            Assert.That(Find(bytes, listStart + 10), Is.EqualTo(new[] { new MapPosition(1, 1, 0), new MapPosition(2, 2, 0) }));
        }

        [TestCase(2, 2, 2)]
        [TestCase(252, 2, 0)]
        [TestCase(2, 252, 0)]
        public void TheListEndsAtTheFirstEntryWhoseTileIsNoTileOfAMap(int x, int y, int z)
        {
            var bytes = Savegame(FiveDefinitions, [(5, 5, 0), (x, y, z), (6, 6, 0)]);

            Assert.That(Find(bytes), Is.EqualTo(new[] { new MapPosition(5, 5, 0) }));
        }

        [Test]
        public void TheUndergroundLevelAndTheTileAtTheCornerOfTheLargestMapAreTiles()
        {
            var bytes = Savegame(FiveDefinitions, [(251, 251, 1), (0, 0, 0)]);

            Assert.That(Find(bytes), Is.EqualTo(new[] { new MapPosition(251, 251, 1), new MapPosition(0, 0, 0) }));
        }

        [Test]
        public void ADefinitionTableThatEndsAfterTheSearchEndIsNotFound()
        {
            var bytes = Savegame(FiveDefinitions, [(1, 2, 0)]);

            Assert.That(Find(bytes, 60), Is.Empty);
        }

        [Test]
        public void TheExtensionOfLaterDefinitionsMayBeInCapitalLetters()
        {
            var bytes = Savegame(["AVWmrnd0.def", "AVLABC00.DEF", "AVXDEF01.DEF", "AVSGHI02.DEF", "AVMJKL03.DEF"], [(4, 5, 0)]);

            Assert.That(Find(bytes), Is.EqualTo(new[] { new MapPosition(4, 5, 0) }));
        }

        [Test]
        public void ANameWithABytePastTheLimitsOfTextIsNoDefinitionSoTheTableIsTooShort()
        {
            var bytes = Savegame(FiveDefinitions, [(1, 2, 0)]);
            bytes[4 + 2 + 3] = 0x1F;

            Assert.That(Find(bytes), Is.Empty);
        }

        [Test]
        public void ANameOfTwentyOneBytesIsNoDefinition()
        {
            var bytes = Savegame(["AVWmrnd0.def", "AVLabc00.def", "AVXdef01.def", "AVSghi02.def", "AVMjkl03abcdefghi.def"], [(1, 2, 0)]);

            Assert.That(Find(bytes), Is.Empty);
        }

        [Test]
        public void ANameOfTwentyBytesIsADefinition()
        {
            var bytes = Savegame(["AVWmrnd0.def", "AVLabc00.def", "AVXdef01.def", "AVSghi02.def", "AVMjkl03abcdefgh.def"], [(1, 2, 0)]);

            Assert.That(Find(bytes), Is.EqualTo(new[] { new MapPosition(1, 2, 0) }));
        }
    }
}
