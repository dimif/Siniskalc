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
        private sealed class Page
        {
            public Page()
            {
                var rows = new List<MineRowViewModelBase>
                {
                    new Row(MineKind.GoldMine, new MapPosition(12, 7), "Red", "Unguarded"),
                    new Row(MineKind.Sawmill, new MapPosition(3, 40), "Blue", "2 of 7 slots"),
                };
                this.MineList = new SortableListViewModel<MineRowViewModelBase>(
                    rows,
                    new Dictionary<string, Func<MineRowViewModelBase, IComparable>>
                    {
                        ["Resource"] = m => m.ResourceName,
                        ["X"] = m => (m.X, m.Y),
                        ["Y"] = m => (m.Y, m.X),
                        ["Owner"] = m => m.OwnerSort,
                    });
            }

            public SortableListViewModel<MineRowViewModelBase> MineList { get; }

            public IReadOnlyList<MineRowViewModelBase> Mines => this.MineList.Items;

            public MineRowViewModelBase? SelectedMine { get; set; }
        }

        private sealed class Row : MineRowViewModelBase
        {
            private readonly string summary;

            public Row(MineKind kind, MapPosition position, string owner, string summary)
                : base(0, () => kind, () => position, new ChoiceFieldViewModel("Owner", ["Neutral", "Red", "Blue"], () => owner, _ => { }))
            {
                this.summary = summary;
            }

            public override string Summary => this.summary;
        }

        [OneTimeSetUp]
        public void MakeTheApplication() => WpfTestApplication.Ensure();

        [Test]
        public void TheCardShowsTheHeadingsThatSortAndARowForEachMineWithWhatTheGameAddsAfterTheOwner()
        {
            var card = new MinesCard { DataContext = new Page(), Note = "Who owns each mine.", SummaryHeading = "Guard" };

            using var hosted = WpfTestApplication.Show(card);

            var texts = WpfTestApplication.Descendants<TextBlock>(card).Where(t => t.IsVisible).Select(t => t.Text).ToList();
            Assert.That(texts, Is.SupersetOf(new[] { "Mines", "Who owns each mine.", "Guard", "Gold", "Wood", "12", "7", "3", "40", "Unguarded", "2 of 7 slots" }));
            var headings = WpfTestApplication.Descendants<Button>(card).Select(b => b.Content?.ToString()).ToList();
            Assert.That(headings, Is.SupersetOf(new[] { "Resource", "X", "Y", "Owner" }));
            Assert.That(WpfTestApplication.Descendants<ComboBox>(card).Select(c => c.SelectedItem), Is.EqualTo(new[] { "Red", "Blue" }));
        }

        [Test]
        public void ClickingAHeadingSortsTheRowsAndTheOtherWayRoundOnTheSecondClick()
        {
            var page = new Page();
            var card = new MinesCard { DataContext = page };
            using var hosted = WpfTestApplication.Show(card);

            page.MineList.Columns["X"].SortCommand.Execute(null);
            Assert.That(page.Mines.Select(m => m.X), Is.EqualTo(new[] { 3, 12 }));

            page.MineList.Columns["X"].SortCommand.Execute(null);
            Assert.That(page.Mines.Select(m => m.X), Is.EqualTo(new[] { 12, 3 }));
        }
    }
}
