namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the row of one obelisk: its number and place and the check box that says whether the player visited it.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ObeliskRowViewModelTests
    {
        private bool visited;
        private List<bool> written = null!;

        [SetUp]
        public void StartNotVisited()
        {
            this.visited = false;
            this.written = [];
        }

        private ObeliskRowViewModel Row(string coordinates = "X: 5, Y: 6") =>
            new(3, coordinates, () => this.visited, v =>
            {
                this.written.Add(v);
                this.visited = v;
            });

        [Test]
        public void ARowIsLabelledWithItsNumberAndShowsItsPlace()
        {
            var row = this.Row();

            Assert.That((row.Number, row.Label, row.Coordinates, row.HasCoordinates), Is.EqualTo((3, "Obelisk 3", "X: 5, Y: 6", true)));
        }

        [Test]
        public void AnObeliskWhosePlaceIsNotKnownHasNoCoordinates()
        {
            Assert.That(this.Row(string.Empty).HasCoordinates, Is.False);
            Assert.That(this.Row("5").HasCoordinates, Is.True, "any text is a place");
        }

        [Test]
        public void TickingTheBoxWritesTheVisitAndTheSameStateWritesNothing()
        {
            var row = this.Row();

            row.IsVisited = false;
            row.IsVisited = true;
            row.IsVisited = true;
            row.IsVisited = false;

            Assert.That(this.written, Is.EqualTo(new[] { true, false }));
        }

        [Test]
        public void TheBoxShowsWhatTheGameHoldsAndRefreshTellsEveryBinding()
        {
            var row = this.Row();
            var raised = new List<string?>();
            row.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            this.visited = true;
            row.Refresh();

            Assert.That(row.IsVisited, Is.True);
            Assert.That(raised, Is.EqualTo(new[] { string.Empty }));
        }
    }
}
