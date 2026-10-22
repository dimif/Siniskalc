namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Tests.Entities;

    /// <summary>
    /// Tests the setup of a game: who plays each colour.
    /// </summary>
    [TestFixture]
    internal sealed class GameSetupTests
    {
        [Test]
        public void WhoPlaysAColourIsReadByTheCodeOfTheColour()
        {
            var setup = new GameSetup<TestColor>([PlayerType.Human, PlayerType.Computer], GameDifficulty.Queen, TestColor.Red);

            Assert.That((setup.GetControl(TestColor.Red), setup.GetControl(TestColor.Blue)), Is.EqualTo(((PlayerType?)PlayerType.Human, (PlayerType?)PlayerType.Computer)));
        }

        [Test]
        public void AColourTheSetupDoesNotListIsNotKnown()
        {
            Assert.That(new GameSetup<TestColor>([], GameDifficulty.Queen).GetControl(TestColor.Red), Is.Null);
            Assert.That(new GameSetup<TestColor>([PlayerType.Human], GameDifficulty.Queen).GetControl(TestColor.Neutral), Is.Null);
        }
    }
}
