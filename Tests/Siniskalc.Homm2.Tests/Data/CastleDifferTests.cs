namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// What the pending changes of a castle say, and how they are put back.
    /// </summary>
    [TestFixture]
    internal sealed class CastleDifferTests
    {
        private const int Xabran = 3;

        private Game game = null!;

        [SetUp]
        public void LoadTheStart() => this.game = TestServices.LoadBrokenAlliance(1);

        [Test]
        public void BuildingAndUpgradingADwellingIsOneChangeThatIsPutBackAsAWhole()
        {
            var original = this.game.History.Current.Castles().GetModel(Xabran);

            this.game.History.CreateCastleSession(Xabran).Update(c => c.WithDwellingLevel(2, 1).WithWaiting(2, 5).WithDwellingLevel(2, 2));

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
            this.game.History.CreateCastleSession(Xabran).Update(c => c.WithWaiting(1, 99));

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
    }
}
