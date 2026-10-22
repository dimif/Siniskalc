namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests the reducers of a player of any game.
    /// </summary>
    [TestFixture]
    internal sealed class PlayerModelExtensionsTests
    {
        [Test]
        public void GivingAPlayerDaysKeepsItsTypeAndEverythingElseItHas()
        {
            var player = new TestPlayer(7, "kept");

            var changed = player.WithDaysLeft(3);

            Assert.That(changed, Is.EqualTo(player with { DaysLeft = 3 }));
            Assert.That(changed.Extra, Is.EqualTo("kept"));
        }

        [Test]
        public void ABanishedPlayerIsNotGivenDays()
        {
            var banished = new TestPlayer(Constants.Player.MinDaysLeft, "out");

            Assert.That(banished.WithDaysLeft(5), Is.SameAs(banished));
        }

        [TestCase(Constants.Player.MinDaysLeft)]
        [TestCase(Constants.Player.MaxDaysLeft + 1)]
        public void AnAmountOfDaysThatTheGameDoesNotCountIsRefused(int days)
        {
            var player = new TestPlayer(7, string.Empty);

            Assert.That(() => player.WithDaysLeft(days), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        private sealed record TestPlayer(int? Days, string Extra) : PlayerModelBase<ConsoleColor>(ConsoleColor.Red, ResourceSet.None, TavernHeroes.Empty, Days);
    }
}
