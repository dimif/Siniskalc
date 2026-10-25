namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the "send to castle" list of the position card of a hero, which both games use.
    /// </summary>
    [TestFixture]
    internal sealed class SendToCastleViewModelTests
    {
        private static readonly CastleDestination[] Castles =
        [
            new("Whiteshield", new MapPosition(30, 17), "30, 17", false),
            new("Colar Blau", new MapPosition(12, 9), "12, 9", true),
            new("Xabran", new MapPosition(3, 4), "3, 4", false),
        ];

        [Test]
        public void TheCastlesAreListedByNameAndTheSearchKeepsThoseWhoseNameContainsIt()
        {
            var list = new SendToCastleViewModel(() => Castles, _ => { });

            Assert.That(list.Castles.Select(c => c.Name), Is.EqualTo(new[] { "Colar Blau", "Whiteshield", "Xabran" }));

            list.SearchText = "  AR ";
            Assert.That(list.Castles.Select(c => c.Name), Is.EqualTo(new[] { "Colar Blau" }));
        }

        [Test]
        public void PickingACastleMovesTheHeroToItsEntranceAndClosesTheList()
        {
            MapPosition? sent = null;
            var list = new SendToCastleViewModel(() => Castles, at => sent = at) { IsOpen = true, SearchText = "xab" };

            list.SendCommand.Execute(list.Castles.Single());

            Assert.That(sent, Is.EqualTo(new MapPosition(3, 4)));
            Assert.That((list.IsOpen, list.SearchText), Is.EqualTo((false, string.Empty)));
        }

        [Test]
        public void ACastleWhoseEntranceIsTakenIsListedButCannotBePicked()
        {
            MapPosition? sent = null;
            var list = new SendToCastleViewModel(() => Castles, at => sent = at);
            var taken = list.Castles.Single(c => c.IsOccupied);

            Assert.That(list.SendCommand.CanExecute(taken), Is.False);
            list.SendCommand.Execute(taken);

            Assert.That(sent, Is.Null);
            Assert.That(taken.ToolTip, Does.Contain("Another hero"));
        }

        [Test]
        public void ThereIsNothingToOpenWhenThePlayerHasNoCastle()
        {
            var list = new SendToCastleViewModel(() => [], _ => { });

            Assert.That((list.HasCastles, list.OpenCommand.CanExecute(null)), Is.EqualTo((false, false)));
        }
    }
}
