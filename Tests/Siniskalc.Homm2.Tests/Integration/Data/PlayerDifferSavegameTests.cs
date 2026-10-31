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
    internal sealed class PlayerDifferSavegameTests
    {
        private Game game = null!;

        [SetUp]
        public void LoadTheStart() => this.game = TestServices.LoadBrokenAlliance(1);

        [Test]
        public void TheTavernIsListedAndPutBack()
        {
            var original = this.game.History.Current.Players().GetModel(0);

            this.game.History.CreatePlayerSession(0).Update(p => p with
            {
                Tavern = new TavernHeroes((p.Tavern.First ?? 0) + 1, (p.Tavern.Second ?? 0) + 1),
            });

            var changes = this.game.History.GetPendingChanges();
            Assert.That(changes.Select(c => c.Change.Description), Is.EquivalentTo(new[] { "Tavern hero 1", "Tavern hero 2" }));

            foreach (var change in changes)
            {
                change.Revert(this.game.History);
            }

            Assert.That(this.game.History.Current.Players().GetModel(0), Is.EqualTo(original));
            Assert.That(this.game.History.GetPendingChanges(), Is.Empty);
        }

        [Test]
        public void LosingTheSelectedCastleClearsTheSelectionWithoutAChangeOfThePlayer()
        {
            var history = this.game.History;
            var players = history.Current.Players();
            var blue = players.GetLists(0);
            var selected = blue.SelectedCastle ?? blue.CastleIds[0];
            history.ApplyAsIs(history.Current.WithChunk(Homm2ChunkKinds.Player, 0, players.ListsSerializer.Serialize(players[0], blue with { SelectedCastle = selected })));
            history.MarkSaved();

            history.CreateCastleSession(selected).Update(c => c with { Owner = PlayerColor.Neutral });

            Assert.That(history.Current.Players().GetLists(0).SelectedCastle, Is.Null);
            Assert.That(history.GetPendingChanges().Select(c => c.Change.Description), Does.Not.Contain("Selected castle"));
        }

        [Test]
        public void TheOwnerOfTheHeroIsTheOnlyChangeOfAHeroGivenToAnotherPlayer()
        {
            var history = this.game.History;
            var hero = history.Current.Players().GetLists(0).HeroIds[0];

            history.CreateHeroSession(hero).Update(h => h.WithOwner(PlayerColor.Green));

            Assert.That(history.GetPendingChanges().Select(c => c.Change.Description), Is.EqualTo(new[] { "Owner" }));
            Assert.That(history.Current.Players().GetLists(PlayerColor.Green.Code).HeroIds, Does.Contain(hero));
            Assert.That(history.CreatePlayerSession(0).IsDirty, Is.False, "the lists follow the owner and are not the player's own change");
        }

        [Test]
        public void ACastleGivenBackToItsOwnerByHandGoesBackToItsPlaceAndTheSavegameIsAsItWas()
        {
            var history = this.game.History;
            var original = history.Current.ToBytes();
            var castle = history.Current.Players().GetLists(0).CastleIds[0];

            history.CreateCastleSession(castle).Update(c => c with { Owner = PlayerColor.Neutral });
            history.CreateCastleSession(castle).Update(c => c with { Owner = PlayerColor.Blue });

            Assert.That(history.GetPendingChanges(), Is.Empty);
            Assert.That(history.Current.ToBytes(), Is.EqualTo(original));
        }
    }
}
