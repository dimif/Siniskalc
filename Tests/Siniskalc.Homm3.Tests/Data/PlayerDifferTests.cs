namespace Siniskalc.Homm3.Tests.Data
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Runtime;

    /// <summary>
    /// What the pending changes of a player say: the resources, the tavern and the days left to own a town.
    /// </summary>
    [TestFixture]
    internal sealed class PlayerDifferTests
    {
        private Game game = null!;

        [SetUp]
        public void LoadASaveInWhichBlueHasDaysLeft() => this.game = TestServices.GameService.LoadGameFromFile(
            Directory.GetFiles(Path.Combine("TestData", "Hota", "Banishing"), "banishing_01_blue_7_days_left.GM?").Single());

        [Test]
        public void ChangingTheDaysLeftIsListedAndPutBack()
        {
            var index = this.game.History.Current.Players().FindByColor(PlayerColor.Blue);
            var original = this.game.History.Current.Players().GetModel(index);

            this.game.History.CreatePlayerSession(index).Update(p => p.WithDaysLeft(3));

            var changes = this.game.History.GetPendingChanges();
            Assert.That(changes.Select(c => (c.Change.Description, c.Change.Before, c.Change.After)), Is.EqualTo(new[] { ("Days left without a castle", "7", "3") }));

            changes.Single().Revert(this.game.History);

            Assert.That(this.game.History.Current.Players().GetModel(index), Is.EqualTo(original));
        }
    }
}
