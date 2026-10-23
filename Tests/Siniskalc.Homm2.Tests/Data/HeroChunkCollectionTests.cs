namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Heroes.Data;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Tests the heroes of a HoMM2 savegame as the lists of the pages need them.
    /// </summary>
    [TestFixture]
    internal sealed class HeroChunkCollectionTests
    {
        private static IHeroSummaries<PlayerColor> Heroes() => TestServices.LoadBrokenAlliance(10).History.Current.Heroes();

        [Test]
        public void EveryOneOfTheFiftyFourHeroesHasASummary()
        {
            Assert.That(Heroes().GetSummaries(), Has.Count.EqualTo(54));
        }

        [Test]
        public void AHeroIsFoundByItsNumberAndAnUnknownNumberIsNull()
        {
            var heroes = TestServices.LoadBrokenAlliance(10).History.Current.Heroes();
            var summaries = (IHeroSummaries<PlayerColor>)heroes;

            Assert.That(summaries.IndexOfId(7), Is.EqualTo(7));
            Assert.That(summaries.IndexOfId(54), Is.EqualTo(-1));
            Assert.That(summaries.IndexOfId(-1), Is.EqualTo(-1));
            Assert.That(summaries.FindById(7), Is.EqualTo(heroes.GetModel(7).Summary));
            Assert.That(summaries.FindById(54), Is.Null);
            Assert.That(summaries.FindById(-1), Is.Null);
        }
    }
}
