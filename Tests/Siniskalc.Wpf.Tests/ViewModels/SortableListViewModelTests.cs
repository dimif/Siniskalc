namespace Siniskalc.Wpf.Tests.ViewModels
{
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A list of rows sorted by the heading of a column.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SortableListViewModelTests
    {
        private static SortableListViewModel<(string Name, int Size)> Create(string? initial = null) => new(
            [("pear", 3), ("apple", 9), ("fig", 3)],
            new Dictionary<string, Func<(string Name, int Size), IComparable>>
            {
                ["Name"] = r => r.Name,
                ["Size"] = r => r.Size,
            },
            initial);

        [Test]
        public void TheRowsKeepTheirOrderUntilAHeadingIsClicked()
        {
            Assert.That(Create().Items.Select(r => r.Name), Is.EqualTo(new[] { "pear", "apple", "fig" }));
        }

        [Test]
        public void ClickingAHeadingSortsByItAndClickingItAgainReversesTheOrder()
        {
            var list = Create();

            list.Columns["Name"].SortCommand.Execute(null);
            Assert.That(list.Items.Select(r => r.Name), Is.EqualTo(new[] { "apple", "fig", "pear" }));
            Assert.That(list.Columns["Name"].Label, Is.EqualTo("Name ▲"));
            Assert.That(list.Columns["Size"].Label, Is.EqualTo("Size"));

            list.Columns["Name"].SortCommand.Execute(null);
            Assert.That(list.Items.Select(r => r.Name), Is.EqualTo(new[] { "pear", "fig", "apple" }));
            Assert.That(list.Columns["Name"].Label, Is.EqualTo("Name ▼"));
        }

        [Test]
        public void RowsThatAreEqualInTheColumnKeepTheirOrder()
        {
            var list = Create("Size");

            Assert.That(list.Items.Select(r => r.Name), Is.EqualTo(new[] { "pear", "fig", "apple" }));
        }
    }
}
