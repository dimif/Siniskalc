namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the words the pages and the navigation tree use for a player who owns no town.
    /// </summary>
    [TestFixture]
    internal sealed class BanishingTextProviderTests
    {
        private readonly BanishingTextProvider provider = new();

        [TestCase(7, "No towns: 7 days left", "7 days left")]
        [TestCase(1, "No towns: 1 day left", "1 day left")]
        [TestCase(0, "Banished", "banished")]
        public void APlayerWhoOwnsNoTownReadsAsTheDaysLeftOrBanished(int days, string pill, string nav)
        {
            var player = new TestPlayer(days);

            Assert.That((this.provider.Pill(player), this.provider.Nav(player)), Is.EqualTo((pill, nav)));
        }

        [Test]
        public void APlayerWhoOwnsATownHasNoText()
        {
            var player = new TestPlayer(null);

            Assert.That((this.provider.Pill(player), this.provider.Nav(player)), Is.EqualTo((string.Empty, string.Empty)));
        }

        private sealed record TestPlayer(int? Days) : PlayerModelBase<ConsoleColor>(ConsoleColor.Red, ResourceSet.None, TavernHeroes.Empty, Days);
    }
}
