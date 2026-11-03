namespace Siniskalc.Core.Tests.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// Tests a region of the bytes of a record: where it ends, its full name, which bytes it holds, its leaves and moving it.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ByteRegionTests
    {
        private static ByteRegion Leaf(string name, int start, int length, string? group = null) => new(name, start, length, ByteRegionKind.Other, group);

        [Test]
        public void ARegionEndsWhereItsLengthTakesIt()
        {
            Assert.That(Leaf("a", 10, 4).End, Is.EqualTo(14));
        }

        [Test]
        public void TheFullNameIsTheNameAloneOrAfterTheGroup()
        {
            Assert.That(Leaf("Slot 1", 0, 1).FullName, Is.EqualTo("Slot 1"));
            Assert.That(Leaf("Slot 1", 0, 1, "Creatures").FullName, Is.EqualTo("Creatures: Slot 1"));
        }

        [Test]
        public void ARegionHoldsTheBytesFromItsStartToJustBeforeItsEnd()
        {
            var region = Leaf("a", 10, 4);

            Assert.That(new[] { 9, 10, 13, 14 }.Select(region.Contains), Is.EqualTo(new[] { false, true, true, false }));
        }

        [Test]
        public void ARegionWithNoPartsHasNoElementsAndIsItsOwnLeaf()
        {
            var region = Leaf("a", 0, 4);

            Assert.That(region.Elements, Is.Empty);
            Assert.That(region.Leaves(), Is.EqualTo(new[] { region }));
        }

        [Test]
        public void TheLeavesOfARegionWithPartsAreTheLeavesOfItsPartsInDepth()
        {
            var deep = new ByteRegion("deep", 4, 2, ByteRegionKind.Other, null, null, [Leaf("c", 4, 1), Leaf("d", 5, 1)]);
            var region = new ByteRegion("all", 0, 6, ByteRegionKind.Other, null, null, [Leaf("a", 0, 2), Leaf("b", 2, 2), deep]);

            Assert.That(region.Leaves().Select(l => l.Name), Is.EqualTo(new[] { "a", "b", "c", "d" }));
            Assert.That(region.Elements, Has.Count.EqualTo(3));
        }

        [Test]
        public void ShiftingMovesTheStartAndKeepsTheLengthAndTheKind()
        {
            var shifted = new ByteRegion("a", 10, 4, ByteRegionKind.Creatures).Shifted(100);

            Assert.That((shifted.Start, shifted.End, shifted.Kind), Is.EqualTo((110, 114, ByteRegionKind.Creatures)));
        }

        [Test]
        public void ShiftingWithAPrefixMakesItTheGroupOrPutsItInFrontOfTheGroupItHas()
        {
            Assert.That(Leaf("a", 0, 1).Shifted(5, "Hero").Group, Is.EqualTo("Hero"));
            Assert.That(Leaf("a", 0, 1, "Slots").Shifted(5, "Hero").Group, Is.EqualTo("Hero / Slots"));
            Assert.That(Leaf("a", 0, 1, "Slots").Shifted(5).Group, Is.EqualTo("Slots"));
            Assert.That(Leaf("a", 0, 1).Shifted(5).Group, Is.Null);
        }

        [Test]
        public void ShiftingMovesTheWholeTreeOfPartsAndGivesThemTheSamePrefix()
        {
            var region = new ByteRegion("all", 0, 4, ByteRegionKind.Other, null, null, [Leaf("a", 0, 2), Leaf("b", 2, 2)]);

            var shifted = region.Shifted(10, "Castle");

            Assert.That(shifted.Elements.Select(e => (e.Name, e.Start, e.Group)), Is.EqualTo(new[] { ("a", 10, "Castle"), ("b", 12, "Castle") }));
            Assert.That(region.Elements.Select(e => e.Start), Is.EqualTo(new[] { 0, 2 }), "the region itself is not changed");
        }

        [Test]
        public void ShiftingARegionWithNoPartsLeavesThemNull()
        {
            Assert.That(Leaf("a", 0, 1).Shifted(3).Parts, Is.Null);
        }
    }
}
