using Siniskalc.Core.Data;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    /// <summary>
    /// The session of a player: putting back the list of its heroes as the savefile had it, which also gives its heroes their owners back.
    /// Replayed on the baseline save of <c>TestData/Hota/HeroOwners</c>.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class PlayerEditSessionTests
    {
        private const int Clancy = 22;

        private static SaveDocumentHistory Load() =>
            TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "HeroOwners", "hero_owners_00_baseline.GM2")).History;

        private static PlayerModel Player(SaveDocument document, PlayerColor color) => document.Players().GetModel(document.Players().FindByColor(color));

        private static HeroModel Hero(SaveDocument document, int heroId) => document.Heroes().GetModel(document.Heroes().IndexOfId(heroId));

        private static PlayerEditSession SessionOf(SaveDocumentHistory history, PlayerColor color) =>
            history.CreatePlayerSession(history.Current.Players().FindByColor(color));

        [Test]
        public void ThePlayerSessionIsOfThePlayerItWasOpenedOn()
        {
            var history = Load();
            var index = history.Current.Players().Count - 1;

            var session = history.CreatePlayerSession(index);

            Assert.That(session.PlayerIndex, Is.EqualTo(index));
            Assert.That(session.Current, Is.EqualTo(history.Current.Players().GetModel(index)));
        }

        [Test]
        public void RestoringAPlayerWhoseHeroesWereNotTouchedChangesNothing()
        {
            var history = Load();
            var before = history.Current;

            SessionOf(history, Player(before, Hero(before, Clancy).Owner).Color).RestoreHeroes();

            Assert.That(history.Current, Is.SameAs(before));
        }

        [Test]
        public void RestoringThePlayerWhoLostAHeroGivesTheHeroItsOwnerAndItsPlaceInTheListBack()
        {
            var history = Load();
            var original = history.Current;
            var owner = Hero(original, Clancy).Owner;
            history.CreateHeroSession(original.Heroes().IndexOfId(Clancy)).ChangeOwner(PlayerColor.Neutral);
            Assert.That(Player(history.Current, owner).HeroIds, Is.Not.EqualTo(Player(original, owner).HeroIds), "the hero left the list");

            SessionOf(history, owner).RestoreHeroes();

            Assert.That(Hero(history.Current, Clancy).Owner, Is.EqualTo(owner));
            Assert.That(Player(history.Current, owner).HeroIds, Is.EqualTo(Player(original, owner).HeroIds));
            Assert.That(Player(history.Current, owner).SelectedHero, Is.EqualTo(Player(original, owner).SelectedHero));
            Assert.That(history.Current.HeroOwners()[0].ToArray(), Is.EqualTo(original.HeroOwners()[0].ToArray()));
        }

        [Test]
        public void RestoringIsOneStepOfTheHistoryThatCanBeUndone()
        {
            var history = Load();
            var owner = Hero(history.Current, Clancy).Owner;
            history.CreateHeroSession(history.Current.Heroes().IndexOfId(Clancy)).ChangeOwner(PlayerColor.Neutral);
            var defeated = history.Current;

            SessionOf(history, owner).RestoreHeroes();
            history.Undo();

            Assert.That(history.Current, Is.SameAs(defeated));
        }

        [Test]
        public void RestoringAPlayerWhoGainedTheHeroTakesItBackToItsFirstOwner()
        {
            var history = Load();
            var original = history.Current;
            var first = Hero(original, Clancy).Owner;
            var other = PlayerColor.All.First(c => c != first && original.Players().FindByColor(c) >= 0);
            history.CreateHeroSession(original.Heroes().IndexOfId(Clancy)).ChangeOwner(other);
            Assert.That(Player(history.Current, other).HeroIds, Does.Contain(Clancy));

            SessionOf(history, other).RestoreHeroes();

            Assert.That(Hero(history.Current, Clancy).Owner, Is.EqualTo(first));
            Assert.That(Player(history.Current, other).HeroIds, Is.EqualTo(Player(original, other).HeroIds));
            Assert.That(Player(history.Current, first).HeroIds, Is.EqualTo(Player(original, first).HeroIds));
        }

        [Test]
        public void RestoringAPlayerBringsBackTheHeroThatWasSelectedOnTheMap()
        {
            var history = Load();
            var original = history.Current;
            var owner = Hero(original, Clancy).Owner;
            var selected = Player(original, owner).SelectedHero;
            history.CreatePlayerSession(original.Players().FindByColor(owner)).Update(p => p with { SelectedHero = null });
            Assert.That(Player(history.Current, owner).SelectedHero, Is.Null);

            SessionOf(history, owner).RestoreHeroes();

            Assert.That(Player(history.Current, owner).SelectedHero, Is.EqualTo(selected));
        }

        [Test]
        public void RestoringAPlayerBringsBackAListThatWasEditedWithoutTouchingTheOwnersOfItsHeroes()
        {
            var history = Load();
            var original = history.Current;
            var owner = Hero(original, Clancy).Owner;
            history.CreatePlayerSession(original.Players().FindByColor(owner)).Update(p => p.WithoutHero(Clancy));
            Assert.That(Player(history.Current, owner).HeroIds, Is.Not.EqualTo(Player(original, owner).HeroIds));

            SessionOf(history, owner).RestoreHeroes();

            Assert.That(Player(history.Current, owner).HeroIds, Is.EqualTo(Player(original, owner).HeroIds));
        }
    }
}
