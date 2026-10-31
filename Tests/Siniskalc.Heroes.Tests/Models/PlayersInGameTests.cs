namespace Siniskalc.Heroes.Tests.Models
{
    using Moq;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests the shared inclusion of player records and owners in the overview and navigation.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class PlayersInGameTests
    {
        [Test]
        public void PlayerRecordsAndOwnersAreIncludedOnceInCodeOrder()
        {
            var blue = Color(0);
            var red = Color(1);
            var neutral = Color(255);

            Assert.That(
                PlayersInGame.Of(new[] { red }, new[] { neutral, blue, red }, new[] { blue }),
                Is.EqualTo(new[] { blue, red, neutral }));
        }

        [Test]
        public void NoRecordsOrOwnersGivesNoPlayers()
        {
            Assert.That(PlayersInGame.Of<IPlayerColor>([], [], []), Is.Empty);
        }

        private static IPlayerColor Color(byte code)
        {
            var color = new Mock<IPlayerColor>();
            color.SetupGet(c => c.Code).Returns(code);
            return color.Object;
        }
    }
}
