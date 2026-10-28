namespace Siniskalc.Homm2.Tests.Integration.Data
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// What the pending changes of a castle say, and how they are put back.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class CastleDifferSavegameTests
    {
        private const int Xabran = 3;

        private Game game = null!;

        [SetUp]
        public void LoadTheStart() => this.game = TestServices.LoadBrokenAlliance(1);

        [Test]
        public void BuildingAndUpgradingADwellingIsOneChangeThatIsPutBackAsAWhole()
        {
            var original = this.game.History.Current.Castles().GetModel(Xabran);

            this.game.History.CreateCastleSession(Xabran).Update(c => c.WithDwellingLevel(2, 1).WithCreaturesWaiting(2, 5).WithDwellingLevel(2, 2));

            var change = this.game.History.GetPendingChanges().Single();
            Assert.That(change.Change.Description, Is.EqualTo("Dwelling 2"));
            Assert.That(change.Change.Before, Is.EqualTo("not built"));
            Assert.That(change.Change.After, Is.EqualTo("upgraded, 5 waiting"));

            change.Revert(this.game.History);

            Assert.That(this.game.History.Current.Castles().GetModel(Xabran), Is.EqualTo(original));
            Assert.That(this.game.History.GetPendingChanges(), Is.Empty);
        }

        [Test]
        public void ChangingTheCreaturesThatWaitInADwellingIsAChangeOfThatDwelling()
        {
            this.game.History.CreateCastleSession(Xabran).Update(c => c.WithCreaturesWaiting(1, 99));

            var change = this.game.History.GetPendingChanges().Single();

            Assert.That(change.Change.Description, Is.EqualTo("Dwelling 1"));
            Assert.That(change.Change.After, Does.EndWith("99 waiting"));
        }

        [Test]
        public void BuildingTheMageGuildIsOneChangeThatIsPutBackAsAWhole()
        {
            var original = this.game.History.Current.Castles().GetModel(Xabran);

            this.game.History.CreateCastleSession(Xabran).Update(c => c.WithMageGuildLevel(2));

            var change = this.game.History.GetPendingChanges().Single();
            Assert.That(change.Change.Description, Is.EqualTo("Mage Guild"));
            Assert.That(change.Change.After, Is.EqualTo("level 2, spells shown 3/3/0/0/0"));

            change.Revert(this.game.History);

            Assert.That(this.game.History.Current.Castles().GetModel(Xabran), Is.EqualTo(original));
        }

        [Test]
        public void RevertingTheOwnerOfACastlePutsTheCastleTheListsAndTheDaysLeftBackInOneStep()
        {
            var history = this.game.History;
            var original = history.Current.ToBytes();
            var blue = history.Current.Players().GetModel(PlayerColor.Blue.Code);
            var castle = blue.CastleIds[0];

            history.CreateCastleSession(castle).ChangeOwner(PlayerColor.Green);
            Assert.That(history.GetPendingChanges().Select(c => c.Change.Description), Does.Contain("Owner").And.Contain("Castles"));

            history.GetPendingChanges().Single(c => c.Change.Description == "Owner").Revert(history);

            Assert.That(history.GetPendingChanges(), Is.Empty);
            Assert.That(history.Current.ToBytes(), Is.EqualTo(original));

            history.Undo();

            Assert.That(history.GetPendingChanges().Select(c => c.Change.Description), Does.Contain("Owner").And.Contain("Castles"));
        }

        [Test]
        public void RevertingTheOwnerOfTheLastCastleOfAPlayerStopsTheCountdownItStarted()
        {
            var history = this.game.History;
            var blue = history.Current.Players().GetModel(PlayerColor.Blue.Code);
            foreach (var castle in blue.CastleIds.Skip(1))
            {
                history.CreateCastleSession(castle).ChangeOwner(PlayerColor.Green);
            }

            history.MarkSaved();
            history.CreateCastleSession(blue.CastleIds[0]).ChangeOwner(PlayerColor.Neutral);
            Assert.That(history.Current.Players().GetModel(PlayerColor.Blue.Code).DaysLeft, Is.EqualTo(PlayerModel.DaysToOwnACastle));

            history.GetPendingChanges().Single(c => c.Change.Description == "Owner").Revert(history);

            Assert.That(history.Current.Players().GetModel(PlayerColor.Blue.Code).DaysLeft, Is.Null);
            Assert.That(history.GetPendingChanges(), Is.Empty);
        }
    }
}
