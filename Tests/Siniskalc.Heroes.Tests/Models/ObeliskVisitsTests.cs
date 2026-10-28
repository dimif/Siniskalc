namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Tests.Entities;

    /// <summary>
    /// Tests the obelisks each colour has visited, which both games keep as a byte for each obelisk with a bit for every colour.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ObeliskVisitsTests
    {
        private static ObeliskVisits<TestColor> Visits(params byte[] masks) => new(masks);

        [Test]
        public void TheBitOfEachColourSaysWhetherItVisitedTheObelisk()
        {
            var visits = Visits(0b01, 0b10, 0b11);

            Assert.That(Enumerable.Range(0, 3).Select(i => visits.IsVisited(i, TestColor.Red)), Is.EqualTo(new[] { true, false, true }));
            Assert.That(Enumerable.Range(0, 3).Select(i => visits.IsVisited(i, TestColor.Blue)), Is.EqualTo(new[] { false, true, true }));
        }

        [Test]
        public void MarkingAnObeliskLeavesTheBitsOfTheOtherColoursAndTheOriginalAlone()
        {
            var visits = Visits(0b10, 0b00);

            var marked = visits.WithVisited(0, TestColor.Red, true);

            Assert.That(marked.Masks, Is.EqualTo(new byte[] { 0b11, 0b00 }));
            Assert.That(visits.Masks, Is.EqualTo(new byte[] { 0b10, 0b00 }));
        }

        [Test]
        public void MarkingEveryObeliskForAColourAndClearingThemKeepsTheOthers()
        {
            var visits = Visits(0b10, 0b00, 0b10).WithAll(TestColor.Red, true);
            Assert.That(visits.AllVisited(TestColor.Red), Is.True);
            Assert.That(visits.Masks, Is.EqualTo(new byte[] { 0b11, 0b01, 0b11 }));

            Assert.That(visits.WithAll(TestColor.Red, false).Masks, Is.EqualTo(new byte[] { 0b10, 0b00, 0b10 }));
        }

        [Test]
        public void ColoursAreCountedByTheObelisksTheyVisited()
        {
            var visits = Visits(0b01, 0b11, 0b10);

            Assert.That((visits.VisitedCount(TestColor.Red), visits.VisitedCount(TestColor.Blue)), Is.EqualTo((2, 2)));
        }

        [Test]
        public void ObeliskVisitsAreEqualWhenTheirBytesAreAndTheirPositionsAreNotCompared()
        {
            var here = new ObeliskVisits<TestColor>(new byte[] { 1 }, new MapPosition?[] { new MapPosition(1, 2) });

            Assert.That(here, Is.EqualTo(Visits(1)));
            Assert.That(here.GetHashCode(), Is.EqualTo(Visits(1).GetHashCode()));
            Assert.That(here, Is.Not.EqualTo(Visits(3)));
        }

        [Test]
        public void ThePositionOfAnObeliskIsNullWhenItWasNotFound()
        {
            var visits = new ObeliskVisits<TestColor>(new byte[] { 0, 0 }, new MapPosition?[] { new MapPosition(4, 5), null });

            Assert.That((visits.PositionOf(0), visits.PositionOf(1), visits.PositionOf(7)), Is.EqualTo(((MapPosition?)new MapPosition(4, 5), (MapPosition?)null, (MapPosition?)null)));
        }

        [Test]
        public void AnObeliskThatIsNotThereIsRefused()
        {
            Assert.That(() => Visits(0).IsVisited(1, TestColor.Red), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void AMapWithNobodyVisitingHasEmptyBytes()
        {
            Assert.That(ObeliskVisits<TestColor>.NoneVisited(3).Masks, Is.EqualTo(new byte[] { 0, 0, 0 }));
        }
    }
}
