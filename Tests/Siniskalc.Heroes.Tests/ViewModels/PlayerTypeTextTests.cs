namespace Siniskalc.Heroes.Tests.ViewModels
{
    using Moq;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Runtime;
    using Siniskalc.Heroes.Tests.Entities;
    using Siniskalc.Heroes.ViewModels;

    /// <summary>
    /// Tests the words for who plays a colour.
    /// </summary>
    [TestFixture]
    internal sealed class PlayerTypeTextTests
    {
        private static IGameInfo<TestColor> Game(PlayerType? control, TestColor? current)
        {
            var game = new Mock<IGameInfo<TestColor>>();
            game.Setup(g => g.GetControl(TestColor.Red)).Returns(control);
            game.Setup(g => g.CurrentPlayer).Returns(current);
            return game.Object;
        }

        [Test]
        public void AColourIsDescribedByWhoPlaysIt()
        {
            Assert.That(PlayerTypeText.Describe(Game(PlayerType.Computer, null), TestColor.Red), Is.EqualTo(PlayerTypeText.Describe(PlayerType.Computer, false)));
        }

        [Test]
        public void TheColourWhoseTurnItIsSaysSo()
        {
            Assert.That(PlayerTypeText.Describe(Game(PlayerType.Human, TestColor.Red), TestColor.Red), Is.EqualTo(PlayerTypeText.Describe(PlayerType.Human, true)));
        }
    }
}
