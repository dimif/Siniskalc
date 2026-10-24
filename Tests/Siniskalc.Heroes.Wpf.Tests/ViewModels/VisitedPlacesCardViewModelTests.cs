namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The two commands at the top of the card of visited places, which make a hero forget or remember everything at once.
    /// </summary>
    [TestFixture]
    internal sealed class VisitedPlacesCardViewModelTests
    {
        private static VisitedPlaceRowViewModel Kind(string name, bool[] visited) =>
            new(
                name,
                () => visited.Count(v => v),
                () => Array.Fill(visited, false),
                () => Array.Fill(visited, true),
                string.Empty,
                visited.Select((_, i) => new ToggleRowViewModel($"Place {i + 1}", () => visited[i], on => visited[i] = on)).ToList());

        [Test]
        public void AllNoneMakesTheHeroForgetEveryKindAndUntickEveryBox()
        {
            var forts = new[] { true, true };
            var gazebos = new[] { false, true };
            var arena = true;
            var card = new VisitedPlacesCardViewModel("Visited", [Kind("Forts", forts), Kind("Gazebos", gazebos)], [new ToggleRowViewModel("Arena", () => arena, on => arena = on)]);

            card.ForgetAllCommand.Execute(null);

            Assert.That(forts.Concat(gazebos), Has.All.False);
            Assert.That(arena, Is.False);
        }

        [Test]
        public void AllVisitedMakesTheHeroRememberEveryPlaceOfEveryKindAndTickEveryBox()
        {
            var forts = new[] { false, true };
            var gazebos = new[] { false, false };
            var arena = false;
            var card = new VisitedPlacesCardViewModel("Visited", [Kind("Forts", forts), Kind("Gazebos", gazebos)], [new ToggleRowViewModel("Arena", () => arena, on => arena = on)]);

            card.RememberAllCommand.Execute(null);

            Assert.That(forts.Concat(gazebos), Has.All.True);
            Assert.That(arena, Is.True);
        }
    }
}
