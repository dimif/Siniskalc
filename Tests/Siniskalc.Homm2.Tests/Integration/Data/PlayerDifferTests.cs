namespace Siniskalc.Homm2.Tests.Integration.Data
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// What the pending changes of a player say, including what the player has selected and what the tavern offers.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class PlayerDifferTests
    {
        private Game game = null!;

        [SetUp]
        public void LoadTheStart() => this.game = TestServices.LoadBrokenAlliance(1);

        [Test]
        public void TheSelectedHeroAndCastleAndTheTavernAreListedAndPutBack()
        {
            var original = this.game.History.Current.Players().GetModel(0);

            this.game.History.CreatePlayerSession(0).Update(p => p with
            {
                SelectedHero = p.SelectedHero == 5 ? 6 : 5,
                SelectedCastle = p.SelectedCastle == 5 ? 6 : 5,
                Tavern = new TavernHeroes((p.Tavern.First ?? 0) + 1, (p.Tavern.Second ?? 0) + 1),
            });

            var changes = this.game.History.GetPendingChanges();
            Assert.That(changes.Select(c => c.Change.Description), Is.EquivalentTo(new[] { "Selected hero", "Selected castle", "Tavern hero 1", "Tavern hero 2" }));

            foreach (var change in changes)
            {
                change.Revert(this.game.History);
            }

            Assert.That(this.game.History.Current.Players().GetModel(0), Is.EqualTo(original));
            Assert.That(this.game.History.GetPendingChanges(), Is.Empty);
        }

        [Test]
        public void LosingTheSelectedCastleToAnotherOwnerShowsAsAChangeOfThePlayer()
        {
            var blue = this.game.History.Current.Players().GetModel(0);
            var selected = blue.SelectedCastle ?? blue.CastleIds[0];
            this.game.History.CreatePlayerSession(0).Update(p => p with { SelectedCastle = selected });
            this.game.History.MarkSaved();

            this.game.History.CreateCastleSession(selected).ChangeOwner(PlayerColor.Neutral);

            Assert.That(this.game.History.GetPendingChanges().Select(c => c.Change.Description), Does.Contain("Selected castle"));
        }

        [Test]
        public void RevertingTheHeroesOfAPlayerGivesTheHeroesTheirOwnersBack()
        {
            var history = this.game.History;
            var original = history.Current.ToBytes();
            var hero = history.Current.Players().GetModel(0).HeroIds[0];

            history.CreateHeroSession(hero).ChangeOwner(PlayerColor.Green);
            history.GetPendingChanges().Single(c => c.OwnerName == PlayerColor.Blue.ToString() && c.Change.Description == "Heroes").Revert(history);

            Assert.That(history.Current.Heroes().GetModel(hero).Owner, Is.EqualTo(PlayerColor.Blue));
            Assert.That(history.GetPendingChanges(), Is.Empty);
            Assert.That(history.Current.ToBytes(), Is.EqualTo(original));
        }

        [Test]
        public void RevertingTheCastlesOfAPlayerGivesTheCastlesTheirOwnersBack()
        {
            var history = this.game.History;
            var original = history.Current.ToBytes();
            var castle = history.Current.Players().GetModel(0).CastleIds[0];

            history.CreateCastleSession(castle).ChangeOwner(PlayerColor.Neutral);
            history.GetPendingChanges().Single(c => c.Change.Description == "Castles").Revert(history);

            Assert.That(history.Current.Castles().GetModel(castle).Owner, Is.EqualTo(PlayerColor.Blue));
            Assert.That(history.GetPendingChanges(), Is.Empty);
            Assert.That(history.Current.ToBytes(), Is.EqualTo(original));
        }
    }
}
