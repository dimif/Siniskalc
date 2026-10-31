namespace Siniskalc.Homm2.Tests.Integration.Data
{
    using Siniskalc.Heroes.Data;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Tests the heroes of a HoMM2 savegame as the lists of the pages need them.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class HeroChunkCollectionTests
    {
        private static IHeroSummaries<PlayerColor> Heroes() => TestServices.LoadBrokenAlliance(10).History.Current.Heroes();

        [Test]
        public void EveryOneOfTheFiftyFourHeroesHasASummary()
        {
            Assert.That(Heroes().GetSummaries(), Has.Count.EqualTo(54));
        }

        [Test]
        public void EveryHeroIdResolvesToItsActualPageIndexInEverySave()
        {
            foreach (var path in Saves.All())
            {
                var heroes = TestServices.Load(path).History.Current.Heroes();
                foreach (var (summary, index) in heroes.GetSummaries().Select((summary, index) => (summary, index)))
                {
                    Assert.That(summary.Index, Is.EqualTo(index), path);
                    Assert.That(summary.HeroId, Is.EqualTo(index), path);
                    Assert.That(heroes.IndexOfId(summary.HeroId), Is.EqualTo(summary.Index), path);
                    Assert.That(heroes.FindById(summary.HeroId), Is.EqualTo(summary), path);
                }
            }
        }

        [Test]
        public void ScenarioPortraitBytesDoNotChangeTheHeroIdsUsedByPlayersAndPages()
        {
            var document = TestServices.Load(Saves.Path("MapDifficultySize", "map_difficulty_size_07_map_hard_extra_large")).History.Current;
            var heroes = document.Heroes();
            var index = heroes.GetModels().Select((hero, index) => (hero, index)).First(entry => entry.hero.Id != entry.index).index;
            var hero = heroes.GetSummaries()[index];

            Assert.That(heroes.GetModel(index).Id, Is.EqualTo(55));
            Assert.That(index, Is.EqualTo(8));
            Assert.That(hero.HeroId, Is.EqualTo(index));
            Assert.That(hero.Index, Is.EqualTo(index));
            Assert.That(heroes.GetModel(heroes.IndexOfId(hero.HeroId)).Name, Is.EqualTo(hero.Name));
            foreach (var player in document.Players().GetModels())
            {
                foreach (var heroId in player.HeroIds)
                {
                    Assert.That(heroes.FindById(heroId)?.Owner, Is.EqualTo(player.Color), $"Hero {heroId} listed by {player.Color}");
                }
            }
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
