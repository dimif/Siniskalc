namespace Siniskalc.Homm3.Tests.ViewModels
{
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// The "Order of play" card of the overview of a Shadow of Death hotseat game (Red and Blue human).
    /// </summary>
    [TestFixture]
    internal sealed class OrderOfPlayTests
    {
        [Test]
        public void TheCardListsThePlayersWithTheTurnLitAndOnlyAHumanWhoHasNotTheTurnCanBePressedButNothingChanges()
        {
            var path = Path.Combine("TestData", "Sod", "Hotseat", "hotseat_00_baseline_200_diff.GM2");
            var game = TestServices.GameService.LoadGameFromFile(path);
            var overview = new OverviewViewModel(game, TestServices.BanishingText, _ => { }, _ => { });

            Assert.That(overview.HasOrderOfPlay, Is.True);
            var current = overview.OrderOfPlay.Where(i => i.IsCurrent).ToList();
            Assert.That(current, Has.Count.EqualTo(1));
            Assert.That(current[0].Command, Is.Null, "the player who has the turn cannot be given it");
            var pressable = overview.OrderOfPlay.Where(i => i.Command != null).Select(i => i.Name).ToList();
            Assert.That(pressable, Is.Not.Empty);

            overview.OrderOfPlay.First(i => i.Command != null).Command!.Execute(null);

            Assert.That(game.History.GetPendingChanges(), Is.Empty);
            Assert.That(overview.OrderOfPlay.Single(i => i.IsCurrent).Name, Is.EqualTo(current[0].Name));
        }
    }
}
