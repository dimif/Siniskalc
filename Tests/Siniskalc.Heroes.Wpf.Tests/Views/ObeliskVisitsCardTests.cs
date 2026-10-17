namespace Siniskalc.Heroes.Wpf.Tests.Views
{
    using System.Windows.Controls;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Heroes.Wpf.Views;

    /// <summary>
    /// Tests what the card of the obelisks a player visited shows, and what the base of its view model does for any game.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal sealed class ObeliskVisitsCardTests
    {
        [OneTimeSetUp]
        public void MakeTheApplication() => WpfTestApplication.Ensure();

        [Test]
        public void TheCardShowsTheSummaryTheTwoButtonsAndATileForEachObelisk()
        {
            var obelisks = new FakeObelisks(3) { Visited = { [1] = true } };
            var card = new ObeliskVisitsCard { DataContext = obelisks };

            using var hosted = WpfTestApplication.Show(card);

            var texts = WpfTestApplication.Descendants<TextBlock>(card).Select(t => t.Text).ToList();
            Assert.That(texts, Is.SupersetOf(new[] { "Visited obelisks", "1 of 3 visited", "Visit all", "None", "Obelisk 1", "Obelisk 2", "Obelisk 3", "X: 2, Y: 2" }));
            Assert.That(WpfTestApplication.Descendants<CheckBox>(card).Select(c => c.IsChecked), Is.EqualTo(new bool?[] { false, true, false }));
        }

        [Test]
        public void VisitingAllAndClearingAllAreOfferedOnlyWhenTheyWouldChangeSomething()
        {
            var obelisks = new FakeObelisks(2);

            Assert.That(obelisks.ClearAllCommand.CanExecute(null), Is.False);
            obelisks.VisitAllCommand.Execute(null);
            Assert.That(obelisks.Visited, Is.EqualTo(new[] { true, true }));
            Assert.That(obelisks.VisitAllCommand.CanExecute(null), Is.False);
            Assert.That(obelisks.Summary, Is.EqualTo("2 of 2 visited"));

            obelisks.ClearAllCommand.Execute(null);
            Assert.That(obelisks.Visited, Is.EqualTo(new[] { false, false }));
        }

        [Test]
        public void ASavegameWithoutObelisksHasNoSummaryAndNoTab()
        {
            var obelisks = new FakeObelisks(0);

            Assert.That(obelisks.HasObelisks, Is.False);
            Assert.That(obelisks.Summary, Is.Empty);
        }

        private sealed class FakeObelisks : ObeliskVisitsViewModelBase
        {
            public FakeObelisks(int count)
                : base("They lead to the prize")
            {
                this.Visited = new bool[count];
                this.Refresh();
            }

            public bool[] Visited { get; }

            protected override int Count => this.Visited.Length;

            protected override int VisitedCount => this.Visited.Count(v => v);

            protected override void Prepare()
            {
            }

            protected override string CoordinatesOf(int index) => $"X: {index + 1}, Y: {index + 1}";

            protected override bool IsVisited(int index) => this.Visited[index];

            protected override void SetVisited(int index, bool visited) => this.Visited[index] = visited;

            protected override void SetAll(bool visited)
            {
                Array.Fill(this.Visited, visited);
                this.Refresh();
            }
        }
    }
}
