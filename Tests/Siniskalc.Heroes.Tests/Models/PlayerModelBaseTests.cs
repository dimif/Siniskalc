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

        [TestCase(null)]
        [TestCase(7)]
        [TestCase(3)]
        public void APlayerWhoGainsATownStopsCounting(int? days)
        {
            Assert.That(new TestPlayer(days).DaysLeftOnGainingATown(), Is.Null);
        }

        [Test]
        public void APlayerWhoLosesItsLastTownHasSevenDaysLeft()
        {
            Assert.That(new TestPlayer(null).DaysLeftOnLosingATown(townsLeft: 0), Is.EqualTo(Constants.Player.MaxDaysLeft));
        }

        [Test]
        public void APlayerWhoLosesATownAndKeepsAnotherHasNoCountdown()
        {
            Assert.That(new TestPlayer(null).DaysLeftOnLosingATown(townsLeft: 2), Is.Null);
        }

        [Test]
        public void ABanishedPlayerStaysBanishedWhateverTownItGainsOrLoses()
        {
            var banished = new TestPlayer(0);

            Assert.That((banished.DaysLeftOnGainingATown(), banished.DaysLeftOnLosingATown(0)), Is.EqualTo(((int?)0, (int?)0)));
        }

        private sealed record TestPlayer(int? Days) : PlayerModelBase<ConsoleColor>(ConsoleColor.Red, ResourceSet.None, TavernHeroes.Empty, Days);
    }
}
