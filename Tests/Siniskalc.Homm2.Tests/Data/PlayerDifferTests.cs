namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// What the pending changes of a player say, including what the player has selected and what the tavern offers.
    /// </summary>
    [TestFixture]
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
                Tavern = new TavernHeroes(1, 2) == p.Tavern ? new TavernHeroes(3, 4) : new TavernHeroes(1, 2),
            });

            var changes = this.game.History.GetPendingChanges();
            Assert.That(changes.Select(c => c.Change.Description), Is.EquivalentTo(new[] { "Selected hero", "Selected castle", "Tavern" }));

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

            this.game.History.CreateCastleSession(selected).ChangeOwner(null);

            Assert.That(this.game.History.GetPendingChanges().Select(c => c.Change.Description), Does.Contain("Selected castle"));
        }
    }
}
