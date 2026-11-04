namespace Siniskalc.Homm2.Tests.ViewModels
{
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// Tests the wording of the map of HoMM2.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MapTextProviderTests
    {
        [TestCase(13, 1, "13 shrines, 1 sphinx")]
        [TestCase(1, 2, "1 shrine, 2 sphinxes")]
        [TestCase(0, 3, "3 sphinxes")]
        [TestCase(4, 0, "4 shrines")]
        [TestCase(0, 0, "")]
        public void TheSummaryLeavesOutWhatTheMapDoesNotHave(int shrines, int sphinxes, string expected)
        {
            Assert.That(new MapTextProvider().Summarize(shrines, sphinxes), Is.EqualTo(expected));
        }
    }
}
