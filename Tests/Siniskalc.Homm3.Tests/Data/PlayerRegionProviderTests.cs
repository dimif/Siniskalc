using Siniskalc.Homm3.Data;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Where the hex editor is told each field of a player's record lives, for both layouts.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class PlayerRegionProviderTests
    {
        private readonly PlayerRegionProvider provider = new();

        private static IEnumerable<PlayerLayout> Layouts => PlayerRecords.Layouts;

        [TestCaseSource(nameof(Layouts))]
        public void TheFieldsBeforeTheResourcesSitAtTheSameBytesInEveryLayout(PlayerLayout layout)
        {
            var regions = this.provider.GetRegions(layout, layout.Stride);

            Assert.That(regions.Take(7).Select(r => (r.Name, r.Start, r.Length)), Is.EqualTo(new[]
            {
                ("Hero count", 0, 1),
                ("Selected hero", 1, 1),
                ("Hero ids", 2, 8),
                ("Tavern heroes", 10, 2),
                ("Days left", 22, 1),
                ("Town count", 23, 1),
                ("Selected town", 24, 1),
            }));
        }

        [TestCaseSource(nameof(Layouts))]
        public void TheResourcesAreOneFourByteIntegerForEachOfTheSevenAfterThe97ByteHead(PlayerLayout layout)
        {
            var resources = this.provider.GetRegions(layout, layout.Stride).Single(r => r.Name == "Resources");

            Assert.That((resources.Start, resources.Length), Is.EqualTo((97, 28)));
            Assert.That(resources.Parts!.Select(p => (p.Start, p.Length)), Is.EqualTo(Enumerable.Range(0, 7).Select(i => (97 + (i * 4), 4))));
            Assert.That(resources.Parts!.Select(p => p.Group), Has.All.EqualTo("Resources"));
        }

        [TestCaseSource(nameof(Layouts))]
        public void TheMarkerAndTheColourSitWhereTheLayoutPutsThemAfterTheOtherFields(PlayerLayout layout)
        {
            var regions = this.provider.GetRegions(layout, layout.Stride);

            Assert.That(regions.TakeLast(2).Select(r => (r.Name, r.Start, r.Length)), Is.EqualTo(new[]
            {
                ("Marker", layout.MarkerOffset, layout.Marker.Length),
                ("Colour", layout.ColorOffset, 1),
            }));
        }

        [Test]
        public void TheMarkerAndColourOfShadowOfDeathAndOfHornOfTheAbyssAreAtTheirOwnBytes()
        {
            var sod = this.provider.GetRegions(PlayerLayout.ShadowOfDeath, 145).TakeLast(2).Select(r => (r.Start, r.Length));
            var hota = this.provider.GetRegions(PlayerLayout.HornOfTheAbyss, 149).TakeLast(2).Select(r => (r.Start, r.Length));

            Assert.That(sod, Is.EqualTo(new[] { (141, 3), (144, 1) }));
            Assert.That(hota, Is.EqualTo(new[] { (142, 4), (148, 1) }));
        }

        [TestCaseSource(nameof(Layouts))]
        public void NoByteBelongsToTwoFields(PlayerLayout layout)
        {
            var leaves = this.provider.GetRegions(layout, layout.Stride).SelectMany(r => r.Leaves()).ToList();

            Assert.That(leaves.SelectMany(l => Enumerable.Range(l.Start, l.Length)), Is.Unique);
        }

        [Test]
        public void AChunkCutShortLeavesOutTheFieldsThatWouldRunPastItsEnd()
        {
            var regions = this.provider.GetRegions(PlayerLayout.ShadowOfDeath, 110);

            Assert.That(regions.Select(r => r.Name), Is.EqualTo(new[] { "Hero count", "Selected hero", "Hero ids", "Tavern heroes", "Days left", "Town count", "Selected town" }));
        }

        [Test]
        public void WithoutALengthEveryFieldIsGiven()
        {
            Assert.That(this.provider.GetRegions(PlayerLayout.ShadowOfDeath).Select(r => r.Name), Does.Contain("Colour"));
        }
    }
}
