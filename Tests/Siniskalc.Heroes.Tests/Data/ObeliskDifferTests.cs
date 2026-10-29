namespace Siniskalc.Heroes.Tests.Data
{
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Tests.Entities;

    /// <summary>
    /// Tests the changes the visited obelisks of any game are listed with.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ObeliskDifferTests
    {
        private static readonly ChunkKind Kind = new("Obelisks", 3);

        private readonly List<(int Index, Change Change, Func<ObeliskVisits<TestColor>, ObeliskVisits<TestColor>> Revert)> made = [];

        [SetUp]
        public void ForgetWhatWasMade() => this.made.Clear();

        private ObeliskDiffer<TestColor> NewDiffer()
        {
            var factory = new Mock<Func<int, Change, Func<ObeliskVisits<TestColor>, ObeliskVisits<TestColor>>, PendingChange>>();
            factory
                .Setup(f => f(It.IsAny<int>(), It.IsAny<Change>(), It.IsAny<Func<ObeliskVisits<TestColor>, ObeliskVisits<TestColor>>>()))
                .Returns((int index, Change change, Func<ObeliskVisits<TestColor>, ObeliskVisits<TestColor>> revert) =>
                {
                    this.made.Add((index, change, revert));
                    return new PendingChange("Obelisks", change, Kind, index, _ => { });
                });
            return new ObeliskDiffer<TestColor>([TestColor.Red, TestColor.Blue], factory.Object);
        }

        [Test]
        public void EachObeliskAndColourThatChangedIsOneChangeWithItsWords()
        {
            var changes = this.NewDiffer().Diff(0, new ObeliskVisits<TestColor>(new byte[] { 0b00, 0b01 }), new ObeliskVisits<TestColor>(new byte[] { 0b10, 0b00 }));

            Assert.That(
                changes.Select(c => (c.Change.Description, c.Change.Before, c.Change.After)),
                Is.EqualTo(new[] { ("Obelisk 1, Blue", "not visited", "visited"), ("Obelisk 2, Red", "visited", "not visited") }));
        }

        [Test]
        public void ObeliskVisitsThatAreTheSameHaveNoChanges()
        {
            var visits = new ObeliskVisits<TestColor>(new byte[] { 0b11 });

            Assert.That(this.NewDiffer().Diff(0, visits, visits), Is.Empty);
            Assert.That(this.made, Is.Empty);
        }

        [Test]
        public void PuttingAChangeBackRestoresOnlyThatObeliskAndColour()
        {
            var original = new ObeliskVisits<TestColor>(new byte[] { 0b00, 0b01 });
            var current = new ObeliskVisits<TestColor>(new byte[] { 0b11, 0b01 });
            this.NewDiffer().Diff(0, original, current);

            var revertRed = this.made.Single(m => m.Change.Description == "Obelisk 1, Red").Revert;

            Assert.That(revertRed(current).Masks, Is.EqualTo(new byte[] { 0b10, 0b01 }));
        }

        [Test]
        public void TheChangeIsMadeForTheRecordTheDifferWasGiven()
        {
            this.NewDiffer().Diff(4, new ObeliskVisits<TestColor>(new byte[] { 0 }), new ObeliskVisits<TestColor>(new byte[] { 1 }));

            Assert.That(this.made.Select(m => m.Index), Is.EqualTo(new[] { 4 }));
        }
    }
}
