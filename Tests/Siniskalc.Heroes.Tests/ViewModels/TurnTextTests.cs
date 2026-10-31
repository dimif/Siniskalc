namespace Siniskalc.Heroes.Tests.ViewModels
{
    using Siniskalc.Heroes.ViewModels;

    /// <summary>
    /// Tests the words for whose turn it is.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class TurnTextTests
    {
        [Test]
        public void TheOverviewNamesThePlayerWhoseTurnItIsAndSaysNothingWhenTheSavegameDoesNotTell()
        {
            Assert.That(TurnText.Overview("Red"), Is.EqualTo("Current turn: Red"));
            Assert.That(TurnText.Overview(null), Is.Empty);
        }

        [Test]
        public void ThePlayerIsMarkedAfterWhoPlaysItOrOnItsOwnWhenThatIsNotKnown()
        {
            Assert.That(TurnText.Beside("Human", isCurrent: true), Is.EqualTo("Human · current turn"));
            Assert.That(TurnText.Beside(string.Empty, isCurrent: true), Is.EqualTo("Current turn"));
            Assert.That(TurnText.Beside("Computer", isCurrent: false), Is.EqualTo("Computer"));
        }
    }
}
