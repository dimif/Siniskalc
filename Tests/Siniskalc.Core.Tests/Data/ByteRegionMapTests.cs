namespace Siniskalc.Core.Tests.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// Tests the map from a byte of a record to the region it belongs to and the shade it is drawn in.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ByteRegionMapTests
    {
        private static ByteRegion Leaf(string name, int start, int length) => new(name, start, length, ByteRegionKind.Other);

        [Test]
        public void EveryByteOfARegionMapsToIt()
        {
            var map = new ByteRegionMap([Leaf("a", 2, 3)], 10);

            Assert.That(Enumerable.Range(0, 7).Select(i => map.FindLeaf(i)?.Name), Is.EqualTo(new[] { null, null, "a", "a", "a", null, null }));
        }

        [Test]
        public void AnOffsetOutsideTheMapHasNoRegionAndShadeZero()
        {
            var map = new ByteRegionMap([Leaf("a", 0, 10)], 10);

            Assert.That(map.FindLeaf(-1), Is.Null);
            Assert.That(map.FindLeaf(10), Is.Null);
            Assert.That(map.FindAll(-1), Is.Empty);
            Assert.That(map.FindAll(10), Is.Empty);
            Assert.That((map.GetShade(-1), map.GetShade(10)), Is.EqualTo((0, 0)));
        }

        [Test]
        public void TheRegionsAreKeptAsTheyWereGiven()
        {
            var regions = new[] { Leaf("a", 0, 1) };

            Assert.That(new ByteRegionMap(regions, 4).Regions, Is.SameAs(regions));
        }

        [Test]
        public void ARegionPastTheLengthOfTheMapIsCutAtIt()
        {
            var map = new ByteRegionMap([Leaf("a", 6, 10)], 8);

            Assert.That(map.FindLeaf(7)?.Name, Is.EqualTo("a"));
            Assert.That(map.FindLeaf(8), Is.Null);
        }

        [Test]
        public void ARegionThatStartsBeforeTheFirstByteMapsOnlyItsBytesInsideTheMap()
        {
            var map = new ByteRegionMap([Leaf("a", -2, 4)], 8);

            Assert.That(map.FindLeaf(0)?.Name, Is.EqualTo("a"));
            Assert.That(map.FindLeaf(1)?.Name, Is.EqualTo("a"));
            Assert.That(map.FindLeaf(2), Is.Null);
        }

        [Test]
        public void ThePartsOfARegionAreMappedAndNotTheRegionThatHoldsThem()
        {
            var group = new ByteRegion("Army", 0, 8, ByteRegionKind.Creatures, null, null, [Leaf("Slot 1", 0, 4), Leaf("Slot 2", 4, 4)]);
            var map = new ByteRegionMap([group], 8);

            Assert.That(map.FindLeaf(3)?.Name, Is.EqualTo("Slot 1"));
            Assert.That(map.FindLeaf(4)?.Name, Is.EqualTo("Slot 2"));
        }

        [Test]
        public void NeighbouringRegionsAlternateShadeAndTheBytesOfOneRegionShareIt()
        {
            var map = new ByteRegionMap([Leaf("a", 0, 2), Leaf("b", 2, 2), Leaf("c", 4, 2)], 6);

            Assert.That(Enumerable.Range(0, 6).Select(map.GetShade), Is.EqualTo(new[] { 0, 0, 1, 1, 0, 0 }));
        }

        [Test]
        public void TheShadeFollowsTheOrderOfTheBytesNotTheOrderTheRegionsWereGivenIn()
        {
            var map = new ByteRegionMap([Leaf("b", 2, 2), Leaf("a", 0, 2)], 4);

            Assert.That((map.FindLeaf(0)?.Name, map.GetShade(0), map.GetShade(2)), Is.EqualTo(("a", 0, 1)));
        }

        [Test]
        public void AByteThatTwoRegionsShareMapsToTheFirstAndFindAllListsBoth()
        {
            var map = new ByteRegionMap([Leaf("ballista", 4, 4), Leaf("cannon", 4, 4), Leaf("other", 8, 1)], 10);

            Assert.That(map.FindLeaf(5)?.Name, Is.EqualTo("ballista"));
            Assert.That(map.FindAll(5).Select(r => r.Name), Is.EqualTo(new[] { "ballista", "cannon" }));
            Assert.That(map.FindAll(8).Select(r => r.Name), Is.EqualTo(new[] { "other" }));
            Assert.That(map.FindAll(0), Is.Empty);
        }

        [Test]
        public void ARegionThatOverlapsKeepsTheShadeOfTheRegionThatOwnsTheByte()
        {
            var map = new ByteRegionMap([Leaf("a", 0, 4), Leaf("b", 2, 4)], 6);

            Assert.That((map.GetShade(2), map.GetShade(4)), Is.EqualTo((0, 1)));
        }

        [Test]
        public void AMapWithNoRegionsMapsNothing()
        {
            var map = new ByteRegionMap([], 4);

            Assert.That(Enumerable.Range(0, 4).Select(i => map.FindLeaf(i)), Is.All.Null);
        }
    }
}
