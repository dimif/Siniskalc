using System.Text;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.MultiUnit.Data
{
    /// <summary>
    /// Tests how the places of the map (learning stones, merchants and so on) are found in a savegame made by hand: the cells of the map come
    /// first, then the table of definitions, a count and the list of the tiles of the objects. The loader uses the real finder of that list.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class MapPlacesLoaderTests
    {
        private const int MerchantType = 7;
        private const int LearningStoneType = 100;

        private static readonly string[] Definitions = ["AVWmrnd0.def", "AVLabc00.def", "AVXdef01.def", "AVSghi02.def", "AVMjkl03.def"];

        private static readonly MapPosition[] Tiles = [new(1, 1, 0), new(10, 11, 0), new(20, 21, 0), new(30, 31, 1)];

        private readonly MapPlacesLoader loader = new();

        /// <summary>A cell of the map: the type of the object (32 bits), its place in the list (16 bits), its number and three more bytes.</summary>
        private static byte[] Cell(int type, int place, int number, byte tail1 = 0, byte tail2 = 0, byte tail3 = 0, byte head = 0, byte head2 = 0, byte head3 = 0) =>
            [(byte)type, head, head2, head3, (byte)place, (byte)(place >> 8), (byte)number, tail1, tail2, tail3];

        private static byte[] Savegame(IEnumerable<byte[]> cells, IEnumerable<MapPosition>? objects = null, bool withDefinitions = true)
        {
            var bytes = new List<byte>(cells.SelectMany(c => c));
            if (withDefinitions)
            {
                foreach (var name in Definitions)
                {
                    bytes.AddRange(BitConverter.GetBytes((ushort)name.Length));
                    bytes.AddRange(Encoding.ASCII.GetBytes(name));
                    bytes.AddRange(new byte[33]);
                }
            }

            var list = (objects ?? Tiles).ToList();
            bytes.AddRange(BitConverter.GetBytes((ushort)list.Count));
            foreach (var tile in list)
            {
                bytes.AddRange(BitConverter.GetBytes((ushort)7));
                bytes.AddRange(new[] { (byte)tile.X, (byte)tile.Y, (byte)tile.Level });
            }

            bytes.AddRange(new byte[40]);
            return [.. bytes];
        }

        [Test]
        public void BytesWithoutDefinitionsOrWithoutObjectsHaveNoPlaces()
        {
            var cell = Cell(MerchantType, 2, 5);

            Assert.That(this.loader.Load(Savegame([cell], withDefinitions: false), TestCatalog.Hota), Is.SameAs(MapPlaces.None));
            Assert.That(this.loader.Load(Savegame([cell], objects: []), TestCatalog.Hota), Is.SameAs(MapPlaces.None));
            Assert.That(this.loader.Load([], TestCatalog.For(GameVersion.HornOfTheAbyss)), Is.SameAs(MapPlaces.None));
        }

        [Test]
        public void AMerchantCellGivesTheNumberAndTheTileOfItsPlaceInTheList()
        {
            var places = this.loader.Load(Savegame([Cell(MerchantType, 2, 5)]), TestCatalog.Hota);

            Assert.That(places.Merchants, Is.EqualTo(new[] { (5, Tiles[2]) }));
            Assert.That(places.Of(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitedPlaces.ById("LearningStone")), Is.Empty);
        }

        [Test]
        public void ACellOfAKindGivesItsPlaceAndTheOtherKindsHaveNone()
        {
            var places = this.loader.Load(Savegame([Cell(LearningStoneType, 3, 1), Cell(MerchantType, 1, 0)]), TestCatalog.Hota);

            Assert.That(places.Of(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitedPlaces.ById("LearningStone")), Is.EqualTo(new[] { (1, Tiles[3]) }));
            Assert.That(places.Of(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitedPlaces.ById("MarlettoTower")), Is.Empty);
            Assert.That(places.Merchants, Is.EqualTo(new[] { (0, Tiles[1]) }));
        }

        [Test]
        public void ThePlacesComeInTheOrderOfTheirNumbers()
        {
            var places = this.loader.Load(Savegame([Cell(MerchantType, 3, 9), Cell(MerchantType, 1, 2), Cell(MerchantType, 2, 4)]), TestCatalog.Hota);

            Assert.That(places.Merchants.Select(m => m.Number), Is.EqualTo(new[] { 2, 4, 9 }));
        }

        [Test]
        public void APlaceOfTheListIsTakenOnceByItsFirstCell()
        {
            var places = this.loader.Load(Savegame([Cell(MerchantType, 2, 7), Cell(MerchantType, 2, 3)]), TestCatalog.Hota);

            Assert.That(places.Merchants, Is.EqualTo(new[] { (7, Tiles[2]) }));
        }

        [Test]
        public void ANumberIsTakenOnceAndTheFirstPlaceKeepsIt()
        {
            var places = this.loader.Load(Savegame([Cell(MerchantType, 1, 4), Cell(MerchantType, 2, 4)]), TestCatalog.Hota);

            Assert.That(places.Merchants, Is.EqualTo(new[] { (4, Tiles[1]) }));
        }

        [Test]
        public void ACellThatPointsOutsideTheListOrAtItsFirstEntryOrNumbersTooHighIsNotAPlace()
        {
            var places = this.loader.Load(Savegame([
                Cell(MerchantType, 0, 1),
                Cell(MerchantType, Tiles.Length, 2),
                Cell(MerchantType, 1, 32),
                Cell(MerchantType, 2, 31),
            ]), TestCatalog.Hota);

            Assert.That(places.Merchants, Is.EqualTo(new[] { (31, Tiles[2]) }));
        }

        [Test]
        public void ACellWithAnythingInTheHighBytesOfItsTypeIsNotACellOfThatType()
        {
            var places = this.loader.Load(Savegame([Cell(MerchantType, 1, 1, head: 1), Cell(MerchantType, 2, 2)]), TestCatalog.Hota);

            Assert.That(places.Merchants, Is.EqualTo(new[] { (2, Tiles[2]) }));
        }

        [TestCase(1, 0)]
        [TestCase(0, 1)]
        public void ACellWhoseTypeHasAnythingInItsThirdOrFourthByteIsNotACellOfThatType(byte head2, byte head3)
        {
            var places = this.loader.Load(Savegame([Cell(MerchantType, 1, 1, head2: head2, head3: head3), Cell(MerchantType, 2, 2)]), TestCatalog.Hota);

            Assert.That(places.Merchants, Is.EqualTo(new[] { (2, Tiles[2]) }));
        }

        [Test]
        public void AMerchantCellMustHaveZerosAfterItsNumberLikeAKindWithoutFlags()
        {
            var places = this.loader.Load(Savegame([Cell(MerchantType, 1, 1, 1), Cell(MerchantType, 2, 2)]), TestCatalog.Hota);

            Assert.That(places.Merchants, Is.EqualTo(new[] { (2, Tiles[2]) }));
        }

        [Test]
        public void APlaceBeyondTheFirstTwoHundredAndFiftySixOfTheListIsRead()
        {
            var tiles = Enumerable.Range(0, 300).Select(i => new MapPosition(i % 200, i / 200, 0)).ToList();

            var places = this.loader.Load(Savegame([Cell(MerchantType, 280, 3)], tiles), TestCatalog.Hota);

            Assert.That(places.Merchants, Is.EqualTo(new[] { (3, tiles[280]) }));
        }

        [Test]
        public void ADefinitionsMarkAtTheVeryStartOfTheBytesIsNotTheTableOfDefinitions()
        {
            var bytes = Encoding.ASCII.GetBytes(".def").Concat(Savegame([Cell(MerchantType, 2, 5)])).ToArray();

            Assert.That(this.loader.Load(bytes, TestCatalog.For(GameVersion.HornOfTheAbyss)).Merchants, Is.EqualTo(new[] { (5, Tiles[2]) }));
        }

        [TestCase(1, 0, 0)]
        [TestCase(0, 1, 0)]
        [TestCase(0, 0, 1)]
        public void ACellOfAKindWithoutFlagsMustHaveZerosAfterItsNumber(byte tail1, byte tail2, byte tail3)
        {
            var places = this.loader.Load(Savegame([Cell(LearningStoneType, 1, 1, tail1, tail2, tail3), Cell(LearningStoneType, 2, 2)]), TestCatalog.Hota);

            Assert.That(places.Of(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitedPlaces.ById("LearningStone")), Is.EqualTo(new[] { (2, Tiles[2]) }));
        }

        [Test]
        public void ACellOfAKindWithFlagsMayHaveAnythingAfterItsNumberAndTheNumberIsMasked()
        {
            var places = this.loader.Load(Savegame([Cell(102, 1, 0x25, 1, 2, 3)]), TestCatalog.Hota);

            Assert.That(places.Of(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitedPlaces.ById("TreeOfKnowledge")), Is.EqualTo(new[] { (5, Tiles[1]) }));
        }

        [Test]
        public void ACellOfOtherTypeIsIgnored()
        {
            var places = this.loader.Load(Savegame([Cell(99, 1, 1), Cell(MerchantType + 1, 2, 2)]), TestCatalog.Hota);

            Assert.That(places.Merchants, Is.Empty);
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitedPlaces.All.Select(k => places.Of(k)), Is.All.Empty);
        }
    }
}
