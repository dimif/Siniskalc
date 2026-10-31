using Siniskalc.Core.Data;
using Siniskalc.Homm3.Data;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Where the hex editor is told each field of the record of a mine lives.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MineRegionProviderTests
    {
        private readonly MineRegionProvider provider = new();

        private static RecordChunk Record(int length) => new("Mine", new byte[length], 500, 0);

        [Test]
        public void TheRecordOfAMineIsDescribedFieldByFieldInTheOrderOfItsBytes()
        {
            var regions = this.provider.GetRegions(Record(62));

            Assert.That(regions.Select(r => (r.Name, r.Start, r.Length)), Is.EqualTo(new[]
            {
                ("Pos X", 0, 1),
                ("Pos Y", 1, 1),
                ("Pos Lvl", 2, 1),
                ("Owner", 3, 1),
                ("Kind", 4, 1),
                ("Abandoned", 5, 1),
                ("Guard creatures", 6, 28),
                ("Guard count", 34, 28),
            }));
        }

        [Test]
        public void TheGuardIsSevenSlotsOfFourBytesForTheCreatureAndFourForTheCount()
        {
            var regions = this.provider.GetRegions(Record(62));

            var creatures = regions.Single(r => r.Name == "Guard creatures");
            var counts = regions.Single(r => r.Name == "Guard count");
            Assert.That(creatures.Parts!.Select(p => (p.Name, p.Start, p.Length)), Is.EqualTo(Enumerable.Range(0, 7).Select(i => ($"Slot {i + 1}", 6 + (i * 4), 4))));
            Assert.That(counts.Parts!.Select(p => p.Start), Is.EqualTo(Enumerable.Range(0, 7).Select(i => 34 + (i * 4))));
            Assert.That(counts.Parts!.Select(p => p.Group), Has.All.EqualTo("Guard count"));
        }

        [Test]
        public void ARecordCutShortLeavesOutTheFieldsThatWouldRunPastItsEnd()
        {
            var regions = this.provider.GetRegions(Record(20));

            Assert.That(regions.Select(r => r.Name), Is.EqualTo(new[] { "Pos X", "Pos Y", "Pos Lvl", "Owner", "Kind", "Abandoned" }));
        }

        [Test]
        public void AGuardThatEndsOneByteAfterTheRecordIsLeftOutWhole()
        {
            var regions = this.provider.GetRegions(Record(61));

            Assert.That(regions.Select(r => r.Name), Does.Not.Contain("Guard count"));
            Assert.That(regions.Select(r => r.Name), Does.Contain("Guard creatures"));
        }

        [Test]
        public void EveryByteOfARecordBelongsToAtMostOneField()
        {
            var leaves = this.provider.GetRegions(Record(62)).SelectMany(r => r.Leaves()).ToList();

            Assert.That(leaves.SelectMany(l => Enumerable.Range(l.Start, l.Length)), Is.Unique);
        }
    }
}
