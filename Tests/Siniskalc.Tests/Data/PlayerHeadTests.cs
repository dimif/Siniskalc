using Siniskalc.Data;
using Siniskalc.Models;
using Siniskalc.Runtime;
using Siniskalc.ViewModels;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// The first 97 bytes of the struct of a player: its heroes, the two heroes of its tavern and its towns. Found in the Restoration of Erathia
    /// saves (the tavern in 56 to 77, where Damacon fled and was defeated), and checked against every savefile of the repository for every colour.
    /// </summary>
    [TestFixture]
    internal sealed class PlayerHeadTests
    {
        private static Game Load(string save) => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Roe", save + ".GM1"));

        private static IEnumerable<string> EverySavefile() => Directory.GetFiles("TestData", "*.GM?", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal);

        private static PlayerModel Red(Game game) => game.History.Current.Players().GetModel(game.History.Current.Players().FindByColor(Siniskalc.Entities.PlayerColor.Red));

        [TestCaseSource(nameof(EverySavefile))]
        public void TheHeroesOfEveryPlayerOfEverySavefileAreAListOfDistinctIdsThatTheCountMatches(string path)
        {
            var game = TestServices.GameService.LoadGameFromFile(path);
            var heroCount = game.History.Current.Heroes().Count;

            foreach (var chunk in game.History.Current.Players())
            {
                var count = chunk.GetByteAt(PlayerLayout.HeroCount);
                var ids = Enumerable.Range(0, PlayerLayout.HeroIdPlaces).Select(i => (int)chunk.GetByteAt(PlayerLayout.HeroIds + i)).ToList();
                var used = ids.Where(id => id != 0xFF).ToList();

                Assert.That(count, Is.LessThanOrEqualTo(PlayerLayout.HeroIdPlaces), chunk.Name);
                Assert.That(used, Has.Count.EqualTo(count), chunk.Name);
                Assert.That(ids.Take(count), Is.EqualTo(used), $"{chunk.Name}: the ids come first");
                Assert.That(used.Distinct().Count(), Is.EqualTo(used.Count), chunk.Name);
                Assert.That(used, Has.All.LessThan(heroCount), chunk.Name);

                var tavern = TestServices.PlayerDeserializer.Deserialize(chunk).Tavern;
                Assert.That(tavern.Ids.Where(i => i != null), Has.All.LessThan(heroCount), chunk.Name);
                Assert.That(tavern.First == null || tavern.First != tavern.Second, Is.True, chunk.Name);
            }
        }

        [TestCase("roe_55_fortress_capitol", 65, 32)]
        [TestCase("roe_65_damacon_before_surrender", 65, 32)]
        [TestCase("roe_66_damacon_surrender", 65, 84)]
        [TestCase("roe_67_damacon_defeated", 65, 45)]
        [TestCase("roe_76_tazar_before_dig", 77, 107)]
        public void TheTavernOfRedOffersTheHeroesTheGameShowed(string save, int first, int second)
        {
            Assert.That(Red(Load(save)).Tavern, Is.EqualTo(new TavernHeroes(first, second)));
        }

        [Test]
        public void TheHeroesTheTavernOffersAreNamedByTheirId()
        {
            var game = Load("roe_66_damacon_surrender");
            var heroes = game.History.Current.Heroes();

            Assert.That(heroes.FindById(Red(game).Tavern.Second!.Value)!.Name, Is.EqualTo("Damacon"));
            Assert.That(heroes.FindById(65)!.Name, Is.EqualTo("Vokial"));
            Assert.That(heroes.FindById(45)!.Name, Is.EqualTo("Solmyr"));
        }

        [Test]
        public void AnEditedTavernIsSavedPendingAndRevertedLikeAnyOtherChange()
        {
            var game = Load("roe_66_damacon_surrender");
            var original = game.History.Current.ToBytes();
            var session = game.History.CreatePlayerSession(game.History.Current.Players().FindByColor(Siniskalc.Entities.PlayerColor.Red));

            session.Update(p => p with { Tavern = p.Tavern.With(1, 45) });

            var change = game.History.GetPendingChanges().Single();
            Assert.That((change.OwnerName, change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Red", "Tavern hero 2", "Damacon", "Solmyr")));
            var saved = TestServices.GameService.LoadGameFromBytes(game.History.Current.ToBytes(), "edited.GM1");
            Assert.That(Red(saved).Tavern, Is.EqualTo(new TavernHeroes(65, 45)));

            session.Update(p => p with { Tavern = p.Tavern.With(0, null) });
            Assert.That(Red(TestServices.GameService.LoadGameFromBytes(game.History.Current.ToBytes(), "edited.GM1")).Tavern, Is.EqualTo(new TavernHeroes(null, 45)));
            Assert.That(game.History.GetPendingChanges().Select(c => (c.Change.Description, c.Change.After)), Is.EquivalentTo(new[] { ("Tavern hero 1", "(none)"), ("Tavern hero 2", "Solmyr") }));

            foreach (var pending in game.History.GetPendingChanges())
            {
                pending.Revert(game.History);
            }

            Assert.That(game.History.Current.ToBytes(), Is.EqualTo(original));
        }

        [Test]
        public void EditingTheTavernOnlyChangesTheTwoBytesOfTheTavern()
        {
            var game = Load("roe_66_damacon_surrender");
            var before = game.History.Current.ToBytes();
            var player = game.History.Current.Players()[game.History.Current.Players().FindByColor(Siniskalc.Entities.PlayerColor.Red)];
            var session = game.History.CreatePlayerSession(game.History.Current.Players().FindByColor(Siniskalc.Entities.PlayerColor.Red));

            session.Update(p => p with { Tavern = new TavernHeroes(10, 11) });

            var after = game.History.Current.ToBytes();
            var changed = Enumerable.Range(0, before.Length).Where(i => before[i] != after[i]).ToList();
            Assert.That(changed, Is.EqualTo(new[] { player.StartIndex + PlayerLayout.TavernHeroes, player.StartIndex + PlayerLayout.TavernHeroes + 1 }));
        }

        [Test]
        public void AHeroIdThatIsNoByteOrIsTheMarkerOfNoHeroIsRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TavernHeroes.Empty.With(0, 255));
            Assert.Throws<ArgumentOutOfRangeException>(() => TavernHeroes.Empty.With(0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => TavernHeroes.Empty.With(2, 1));
        }

        [Test]
        public void TheTavernOfThePlayerPageOffersTheHeroesNoPlayerOwnsAndNotTheOneInTheOtherPlace()
        {
            var game = Load("roe_67_damacon_defeated");
            var session = game.History.CreatePlayerSession(game.History.Current.Players().FindByColor(Siniskalc.Entities.PlayerColor.Red));
            using var view = new PlayerViewModel(session, TestServices.ByteEditTargets);
            var first = view.TavernPlaces[0];
            var second = view.TavernPlaces[1];

            Assert.That(first.Selected!.Name, Is.EqualTo("Vokial"));
            Assert.That(second.Selected!.Name, Is.EqualTo("Solmyr"));
            Assert.That(first.Choices[0].HeroId, Is.Null);
            Assert.That(first.Choices.Select(c => c.HeroId), Does.Not.Contain(45), "Solmyr is in the other place");
            Assert.That(second.Choices.Select(c => c.HeroId), Does.Not.Contain(65), "Vokial is in the other place");
            Assert.That(first.Choices.Select(c => c.Name), Does.Contain("Damacon"), "Damacon is no player's hero any more");
            Assert.That(first.Choices.Select(c => c.Name), Does.Not.Contain("Tazar"), "Tazar is a hero of Red");

            first.Selected = first.Choices.Single(c => c.Name == "Damacon");

            Assert.That(session.Current.Tavern, Is.EqualTo(new TavernHeroes(84, 45)));
            Assert.That(first.Selected!.Name, Is.EqualTo("Damacon"));
            Assert.That(second.Choices.Select(c => c.HeroId), Does.Not.Contain(84));
        }
    }
}
