namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the "send to" list of the position card of a hero, which both games use.
    /// </summary>
    [TestFixture]
    internal sealed class SendToViewModelTests
    {
        private static readonly MapDestination[] Places =
        [
            new("Whiteshield", "Castle", new MapPosition(30, 17), "30, 17", false, "Knight"),
            new("Colar Blau", "Castle", new MapPosition(12, 9), "12, 9", true, "Sorceress"),
            new("Xabran", "Castle", new MapPosition(3, 4), "3, 4", false, "Necromancer"),
            new("Gold mine", "Mine", new MapPosition(44, 8), "44, 8", false, IsApproximate: true),
            new("Pikeman dwelling", "Dwelling", new MapPosition(5, 6), "5, 6", false),
        ];

        [Test]
        public void ThePlacesAreListedByNameAndTheSearchKeepsThoseWhoseNameContainsIt()
        {
            var list = new SendToViewModel(() => Places, _ => { });

            Assert.That(list.Destinations.Select(c => c.Name), Is.EqualTo(new[] { "Colar Blau", "Gold mine", "Pikeman dwelling", "Whiteshield", "Xabran" }));

            list.SearchText = "  AR ";
            Assert.That(list.Destinations.Select(c => c.Name), Is.EqualTo(new[] { "Colar Blau" }));
        }

        [Test]
        public void TheSearchAlsoFindsAPlaceByItsKindItsPositionOrItsFaction()
        {
            var list = new SendToViewModel(() => Places, _ => { });

            list.SearchText = "30, 1";
            Assert.That(list.Destinations.Select(c => c.Name), Is.EqualTo(new[] { "Whiteshield" }));

            list.SearchText = "necro";
            Assert.That(list.Destinations.Select(c => c.Name), Is.EqualTo(new[] { "Xabran" }));

            list.SearchText = "mine";
            Assert.That(list.Destinations.Select(c => c.Name), Is.EqualTo(new[] { "Gold mine" }), "the kind is searched too");

            list.SearchText = "nothing like it";
            Assert.That(list.Destinations, Is.Empty);
        }

        [Test]
        public void TheLineOfAPlaceNamesItsKindItsPlaceAndItsFactionAndSkipsWhatItHasNot()
        {
            Assert.That(Places[0].Label, Is.EqualTo("Whiteshield  ·  Castle  ·  30, 17  ·  Knight"));
            Assert.That(Places[3].Label, Is.EqualTo("Gold mine  ·  Mine  ·  44, 8"));
            Assert.That(Places[3].ToolTip, Does.Contain("a few steps off"));
        }

        [Test]
        public void PickingACastleMovesTheHeroToItsEntranceAndClosesTheList()
        {
            MapPosition? sent = null;
            var list = new SendToViewModel(() => Places, at => sent = at) { IsOpen = true, SearchText = "xab" };

            list.SendCommand.Execute(list.Destinations.Single());

            Assert.That(sent, Is.EqualTo(new MapPosition(3, 4)));
            Assert.That((list.IsOpen, list.SearchText), Is.EqualTo((false, string.Empty)));
        }

        [Test]
        public void ACastleWhoseEntranceIsTakenIsListedButCannotBePicked()
        {
            MapPosition? sent = null;
            var list = new SendToViewModel(() => Places, at => sent = at);
            var taken = list.Destinations.Single(c => c.IsOccupied);

            Assert.That(list.SendCommand.CanExecute(taken), Is.False);
            list.SendCommand.Execute(taken);

            Assert.That(sent, Is.Null);
            Assert.That(taken.ToolTip, Does.Contain("Another hero"));
        }

        [Test]
        public void ThereIsNothingToOpenWhenThePlayerOwnsNothing()
        {
            var list = new SendToViewModel(() => [], _ => { });

            Assert.That((list.HasDestinations, list.OpenCommand.CanExecute(null)), Is.EqualTo((false, false)));
        }
    }
}
