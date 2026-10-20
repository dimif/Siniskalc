namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Heroes.Data;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Tests the heroes of a HoMM2 savegame as the lists of the pages need them.
    /// </summary>
    [TestFixture]
    internal sealed class HeroSummariesTests
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

            Assert.That(((IHeroSummaries<PlayerColor>)heroes).FindById(7), Is.EqualTo(heroes.GetModel(7).Summary));
            Assert.That(((IHeroSummaries<PlayerColor>)heroes).FindById(54), Is.Null);
            Assert.That(((IHeroSummaries<PlayerColor>)heroes).FindById(-1), Is.Null);
        }
    }
}
