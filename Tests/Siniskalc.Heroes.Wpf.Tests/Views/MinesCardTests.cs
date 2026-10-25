namespace Siniskalc.Heroes.Wpf.Tests.Views
{
    using System.Windows.Controls;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Heroes.Wpf.Views;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests what the card of the mines of a map shows: a card whose template did not apply is empty and raises no binding error.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal sealed class MinesCardTests
    {
        private sealed class Row : MineRowViewModelBase
        {
            private readonly string summary;

            public Row(MineKind kind, MapPosition position, string owner, string summary, bool hasLevels)
                : base(0, () => kind, () => position, () => owner != "Neutral", new ChoiceFieldViewModel("Owner", ["Neutral", "Red", "Blue"], () => owner, _ => { }), hasLevels)
            {
                this.summary = summary;
            }

            public override string Summary => this.summary;
        }

        private static MinesListViewModel<MineRowViewModelBase> Mines(bool hasLevels = false) => new(
        [
            new Row(MineKind.GoldMine, new MapPosition(12, 7), "Red", "Unguarded", hasLevels),
            new Row(MineKind.Sawmill, new MapPosition(3, 40, MapPosition.Underground), "Blue", "2 of 7 slots", hasLevels),
        ]);

        [OneTimeSetUp]
        public void MakeTheApplication() => WpfTestApplication.Ensure();

        [Test]
        public void TheCardShowsTheHeadingsThatSortAndARowForEachMineWithWhatTheGameAddsAfterTheOwner()
        {
            var card = new MinesCard { DataContext = Mines(), Note = "Who owns each mine.", SummaryHeading = "Guard" };

            using var hosted = WpfTestApplication.Show(card);

            var texts = WpfTestApplication.Descendants<TextBlock>(card).Where(t => t.IsVisible).Select(t => t.Text).ToList();
            Assert.That(texts, Is.SupersetOf(new[] { "Mines", "Who owns each mine.", "Guard", "Gold", "Wood", "12", "7", "3", "40", "Unguarded", "2 of 7 slots" }));
            var headings = WpfTestApplication.Descendants<Button>(card).Select(b => b.Content?.ToString()).ToList();
            Assert.That(headings, Is.SupersetOf(new[] { "Resource", "X", "Y", "Owner" }));
            Assert.That(WpfTestApplication.Descendants<ComboBox>(card).Select(c => c.SelectedItem), Is.EqualTo(new[] { "Red", "Blue" }));
            Assert.That(texts, Has.None.EqualTo("Level"));
        }

        [Test]
        public void ACardThatShowsTheLevelHasALevelColumnThatTellsTheLevelsApart()
        {
            var card = new MinesCard { DataContext = Mines(hasLevels: true), ShowLevel = true };

            using var hosted = WpfTestApplication.Show(card);

            var texts = WpfTestApplication.Descendants<TextBlock>(card).Where(t => t.IsVisible).Select(t => t.Text).ToList();
            Assert.That(texts, Is.SupersetOf(new[] { "Level", "Surface", "Underground" }));
            double LeftOf(System.Windows.FrameworkElement element) => element.TranslatePoint(new System.Windows.Point(0, 0), card).X;
            var heading = WpfTestApplication.Descendants<TextBlock>(card).Single(t => t.IsVisible && t.Text == "Level");
            var cell = WpfTestApplication.Descendants<TextBlock>(card).First(t => t.IsVisible && t.Text == "Surface");
            var ownerHeading = WpfTestApplication.Descendants<Button>(card).Single(b => b.Content?.ToString() == "Owner");
            var ownerCell = WpfTestApplication.Descendants<ComboBox>(card).First();
            Assert.That(LeftOf(cell), Is.EqualTo(LeftOf(heading)).Within(0.5));
            Assert.That(LeftOf(ownerCell), Is.EqualTo(LeftOf(ownerHeading)).Within(0.5));
        }

        [Test]
        public void ClickingAHeadingSortsTheRowsAndTheOtherWayRoundOnTheSecondClick()
        {
            var mines = Mines();
            var card = new MinesCard { DataContext = mines };
            using var hosted = WpfTestApplication.Show(card);
            IEnumerable<string> Shown() => WpfTestApplication.Descendants<TextBlock>(card).Where(t => t.IsVisible && (t.Text == "Gold" || t.Text == "Wood")).Select(t => t.Text);

            mines.List.Columns["X"].SortCommand.Execute(null);
            card.UpdateLayout();
            Assert.That(mines.Rows.Select(m => m.X), Is.EqualTo(new[] { 3, 12 }));
            Assert.That(Shown(), Is.EqualTo(new[] { "Wood", "Gold" }), "the card shows the rows in the new order");

            mines.List.Columns["X"].SortCommand.Execute(null);
            card.UpdateLayout();
            Assert.That(mines.Rows.Select(m => m.X), Is.EqualTo(new[] { 12, 3 }));
            Assert.That(Shown(), Is.EqualTo(new[] { "Gold", "Wood" }));
        }

        [Test]
        public void TheColumnsOfARowStartWhereTheirHeadingsStart()
        {
            var card = new MinesCard { DataContext = Mines() };
            using var hosted = WpfTestApplication.Show(card);

            double LeftOf(System.Windows.FrameworkElement element) => element.TranslatePoint(new System.Windows.Point(0, 0), card).X;
            var heading = WpfTestApplication.Descendants<Button>(card).Single(b => b.Content?.ToString() == "X");
            var cell = WpfTestApplication.Descendants<TextBlock>(card).First(t => t.IsVisible && t.Text == "12");
            var resourceHeading = WpfTestApplication.Descendants<Button>(card).Single(b => b.Content?.ToString() == "Resource");
            var resourceCell = WpfTestApplication.Descendants<TextBlock>(card).First(t => t.IsVisible && t.Text == "Gold");

            Assert.That(LeftOf(cell), Is.EqualTo(LeftOf(heading)).Within(0.5));
            Assert.That(LeftOf(resourceCell), Is.EqualTo(LeftOf(resourceHeading)).Within(0.5));
        }
    }
}
