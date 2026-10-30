namespace Siniskalc.Wpf.Tests.Views
{
    using System.Linq;
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// Tests the lines of the hex grid, made on demand from the bytes and from a lookup of the known fields that is a fake.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    [Apartment(ApartmentState.STA)]
    internal sealed class HexRowListTests
    {
        private static Mock<IByteRegionLookup> NoRegions()
        {
            var map = new Mock<IByteRegionLookup>();
            map.Setup(m => m.FindAll(It.IsAny<int>())).Returns([]);
            return map;
        }

        private static byte[] Bytes(int count) => [.. Enumerable.Range(0, count).Select(i => (byte)i)];

        private static HexRowList List(byte[] bytes, byte[]? original = null, IByteRegionLookup? map = null) =>
            new(bytes, original ?? bytes, map ?? NoRegions().Object);

        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(16, 1)]
        [TestCase(17, 2)]
        [TestCase(48, 3)]
        public void TheListHasALineForEverySixteenBytesAndOneMoreForTheRest(int length, int rows)
        {
            Assert.That(List(Bytes(length)).Count, Is.EqualTo(rows));
        }

        [Test]
        public void ALineShowsItsAddressInHexAndInDecimalItsBytesInHexAndTheirTextWithDotsForWhatCannotBeShown()
        {
            var bytes = Bytes(40);
            bytes[16] = (byte)'A';
            bytes[17] = 0x7F;

            var row = List(bytes).GetRow(1);

            Assert.That(row.Index, Is.EqualTo(1));
            Assert.That(row.Address, Is.EqualTo("0x00000010 (16)"));
            Assert.That(row.Cells.Select(c => c.Text), Is.EqualTo(new[] { "41", "7F", "12", "13", "14", "15", "16", "17", "18", "19", "1A", "1B", "1C", "1D", "1E", "1F" }));
            Assert.That(row.Ascii, Is.EqualTo("A..............."));
        }

        [Test]
        public void TheLastLineIsPaddedWithEmptyCellsAndBlanks()
        {
            var row = List(Bytes(20)).GetRow(1);

            Assert.That(row.Cells.Select(c => c.Text), Is.EqualTo(new[] { "10", "11", "12", "13", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty }));
            Assert.That(row.Ascii, Is.EqualTo("....            "));
            Assert.That(row.Cells.Skip(4), Is.All.Matches<HexCell>(c => c.Background == null && c.ToolTip == null && !c.IsChanged));
        }

        [Test]
        public void ACellIsMarkedChangedWhereItDiffersFromTheOriginalAndTheToolTipSaysWhatItWas()
        {
            var original = Bytes(16);
            var bytes = Bytes(16);
            bytes[3] = 0xAB;

            var cells = List(bytes, original).GetRow(0).Cells;

            Assert.That(cells.Select(c => c.IsChanged), Is.EqualTo(Enumerable.Range(0, 16).Select(i => i == 3)));
            Assert.That(cells[3].ToolTip, Is.EqualTo("Changed: was 03, now AB"));
            Assert.That(cells[2].ToolTip, Is.Null);
        }

        [Test]
        public void AByteBeyondTheEndOfTheOriginalIsNotChanged()
        {
            var cells = List(Bytes(16), Bytes(10)).GetRow(0).Cells;

            Assert.That(cells.Skip(10), Is.All.Matches<HexCell>(c => !c.IsChanged && c.ToolTip == null));
        }

        [Test]
        public void ACellOfAKnownFieldHasTheBrushOfItsKindAndAToolTipThatNamesTheFieldItsRangeAndItsNote()
        {
            var attack = new ByteRegion("Attack", 2, 1, ByteRegionKind.Stats, Group: "Primary skills", Description: "The base attack");
            var group = new ByteRegion("Primary skills", 0, 4, ByteRegionKind.Stats);
            var map = new Mock<IByteRegionLookup>();
            map.Setup(m => m.FindLeaf(2)).Returns(attack);
            map.Setup(m => m.GetShade(2)).Returns(1);
            map.Setup(m => m.FindAll(2)).Returns([group, attack]);
            map.Setup(m => m.FindAll(It.Is<int>(o => o != 2))).Returns([]);

            var cells = List(Bytes(16), map: map.Object).GetRow(0).Cells;

            Assert.That(cells[2].Background, Is.SameAs(HexPalette.GetBrush(ByteRegionKind.Stats, 1)));
            Assert.That(cells[2].ToolTip, Is.EqualTo(string.Join(Environment.NewLine, "Primary skills (0x0-0x3, 4 bytes)", "Primary skills: Attack (0x2)", "The base attack")));
            Assert.That(cells[1].Background, Is.Null);
        }

        [Test]
        public void ALineIsMadeOnceAndGivenAgain()
        {
            var list = List(Bytes(64));

            Assert.That(list.GetRow(2), Is.SameAs(list.GetRow(2)));
            Assert.That(list[2], Is.SameAs(list.GetRow(2)));
        }

        [Test]
        public void ALineIsStillRightAfterTheCacheOfLinesWasEmptied()
        {
            var list = List(Bytes(1500 * 16));
            var first = list.GetRow(0);
            for (var i = 1; i <= 1100; i++)
            {
                list.GetRow(i);
            }

            var again = list.GetRow(0);

            Assert.That(again, Is.EqualTo(first));
            Assert.That(again.Address, Is.EqualTo(first.Address));
        }

        [TestCase(-1)]
        [TestCase(4)]
        public void ALineThatIsNotInTheListCannotBeAskedFor(int index)
        {
            Assert.That(() => List(Bytes(64)).GetRow(index), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void AnIndexIsFoundOnlyForALineOfTheList()
        {
            var list = List(Bytes(64));
            var row = list.GetRow(3);

            Assert.That(list.IndexOf(row), Is.EqualTo(3));
            Assert.That(list.Contains(row), Is.True);
            Assert.That(list.IndexOf(new HexRow(9, string.Empty, [], string.Empty)), Is.EqualTo(-1));
            Assert.That(list.IndexOf("not a line"), Is.EqualTo(-1));
            Assert.That(list.IndexOf(null), Is.EqualTo(-1));
        }

        [Test]
        public void EnumeratingGivesEveryLineInOrderAndCopyingPutsThemInAnArray()
        {
            var list = List(Bytes(40));

            var array = new object[3];
            list.CopyTo(array, 0);

            Assert.That(list.Cast<HexRow>().Select(r => r.Index), Is.EqualTo(new[] { 0, 1, 2 }));
            Assert.That(array.Cast<HexRow>().Select(r => r.Index), Is.EqualTo(new[] { 0, 1, 2 }));
        }

        [Test]
        public void TheListIsReadOnlyAndFixedSize()
        {
            var list = List(Bytes(40));

            Assert.That((list.IsReadOnly, list.IsFixedSize, list.IsSynchronized), Is.EqualTo((true, true, false)));
            Assert.That(() => list.Add(new object()), Throws.InstanceOf<NotSupportedException>());
            Assert.That(() => list.Clear(), Throws.InstanceOf<NotSupportedException>());
            Assert.That(() => list.Insert(0, new object()), Throws.InstanceOf<NotSupportedException>());
            Assert.That(() => list.Remove(new object()), Throws.InstanceOf<NotSupportedException>());
            Assert.That(() => list.RemoveAt(0), Throws.InstanceOf<NotSupportedException>());
            Assert.That(() => list[0] = new object(), Throws.InstanceOf<NotSupportedException>());
        }
    }
}
