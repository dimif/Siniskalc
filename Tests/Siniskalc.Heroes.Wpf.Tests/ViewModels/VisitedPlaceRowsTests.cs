namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// The row of a kind of place a hero visited: the bits of its places, read and changed through the game's own number.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class VisitedPlaceRowsTests
    {
        private uint mask;

        [SetUp]
        public void Start() => this.mask = 0b1000;

        private VisitedPlaceRowViewModel Row() => VisitedPlaceRows.Create(
            "Forts",
            "A point of defense",
            [(1, new MapPosition(3, 4)), (2, new MapPosition(5, 6))],
            () => this.mask,
            edit => this.mask = edit(this.mask));

        [Test]
        public void EachPlaceOfTheMapHasABoxTickedByItsBit()
        {
            var row = this.Row();

            Assert.That(row.Places.Select(p => p.IsChecked), Is.EqualTo(new[] { false, false }));

            row.Places[1].IsChecked = true;

            Assert.That(this.mask, Is.EqualTo(0b1100u), "bit 2 is the second place, and a bit of a place not on the map is kept");
            Assert.That(row.Places[1].IsChecked, Is.True);
        }

        [Test]
        public void RememberingTicksEveryPlaceOfTheMapAndForgettingClearsTheNumber()
        {
            var row = this.Row();

            row.RememberCommand.Execute(null);
            Assert.That(this.mask, Is.EqualTo(0b1110u));

            row.ForgetCommand.Execute(null);
            Assert.That(this.mask, Is.EqualTo(0u));
        }
    }
}
