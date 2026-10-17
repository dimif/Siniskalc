namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests the countdown that puts a player out of the game once it owns no town.
    /// </summary>
    [TestFixture]
    internal sealed class PlayerModelBaseTests
    {
        [Test]
        public void APlayerWhoOwnsATownHasNoCountdown()
        {
            var player = new TestPlayer(null);

            Assert.That((player.HasCountdown, player.IsBanished), Is.EqualTo((false, false)));
        }

        [TestCase(1)]
        [TestCase(7)]
        public void ADayCountRunningMeansACountdown(int days)
        {
            var player = new TestPlayer(days);

            Assert.That((player.HasCountdown, player.IsBanished), Is.EqualTo((true, false)));
        }

        [Test]
        public void ACountdownThatRanOutMeansThePlayerIsOut()
        {
            var player = new TestPlayer(0);

            Assert.That((player.HasCountdown, player.IsBanished), Is.EqualTo((false, true)));
        }

        private sealed record TestPlayer(int? Days) : PlayerModelBase<ConsoleColor>(ConsoleColor.Red, ResourceSet.None, TavernHeroes.Empty, Days);
    }
}
