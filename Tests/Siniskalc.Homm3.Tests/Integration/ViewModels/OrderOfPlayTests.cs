using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.ViewModels
{
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// The "Order of play" card of the overview of a Shadow of Death hotseat game (Red and Blue human).
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class OrderOfPlayTests
    {
        [Test]
        public void TheCardListsThePlayersWithTheTurnLitAndTheButtonOfAHumanWhoHasNotTheTurnIsDisabledWithTheReason()
        {
            var path = Path.Combine("TestData", "Sod", "Hotseat", "hotseat_00_baseline_200_diff.GM2");
            var game = SavedGames.Get(path);
            var overview = new OverviewViewModel(game, TestServices.BanishingText, _ => { }, _ => { });

            Assert.That(overview.HasOrderOfPlay, Is.True);
            var current = overview.OrderOfPlay.Where(i => i.IsCurrent).ToList();
            Assert.That(current.Select(i => i.Name), Is.EqualTo(new[] { "Red" }));
            Assert.That(current[0].Command, Is.Null, "the player who has the turn cannot be given it");
            var others = overview.OrderOfPlay.Where(i => i.Command != null).ToList();
            Assert.That(others, Is.Not.Empty, "a human who has not the turn has a button");
            Assert.That(others.Select(i => i.Command!.CanExecute(null)), Has.All.False, "writing the turn is not built for HoMM3");
            Assert.That(others.Select(i => i.ToolTip), Has.All.EqualTo(OverviewViewModel.GiveTheTurnNotBuilt));
            Assert.That(game.History.GetPendingChanges(), Is.Empty);
        }
    }
}
