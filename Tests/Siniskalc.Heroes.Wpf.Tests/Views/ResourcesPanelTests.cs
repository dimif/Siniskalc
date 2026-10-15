namespace Siniskalc.Heroes.Wpf.Tests.Views
{
    using System.Windows.Controls;
    using System.Windows.Input;
    using Siniskalc.Heroes.Wpf.Views;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests the resources of a player.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal sealed class ResourcesPanelTests
    {
        [OneTimeSetUp]
        public void MakeTheApplication() => WpfTestApplication.Ensure();

        [Test]
        public void ThePanelShowsACardForEachResourceTheQuickAddButtonsAndTheNote()
        {
            var amounts = new[] { 10L, 20L }.Select((value, i) => new AmountRowViewModel($"Resource {i}", "#808080", 0, 999, 1, () => value, () => value, _ => { })).ToList();
            var page = new FakePlayerPage { Resources = amounts, SetAllResourcesCommand = new DelegateCommand(_ => { }), SetRichCommand = new DelegateCommand(_ => { }) };
            var panel = new ResourcesPanel { Note = "Amounts are limited.", DataContext = page };

            using var hosted = WpfTestApplication.Show(panel);

            var texts = WpfTestApplication.Descendants<TextBlock>(panel).Select(t => t.Text).ToList();
            Assert.That(texts, Does.Contain("Resource 0").And.Contain("Resource 1").And.Contain("Amounts are limited.").And.Contain("Quick add"));
            var buttons = WpfTestApplication.Descendants<Button>(panel).ToList();
            Assert.That(buttons.Select(b => b.Content), Is.SupersetOf(new object[] { "All +99", "Gold +100k" }));
            Assert.That(buttons.First(b => Equals(b.Content, "All +99")).Command, Is.SameAs(page.SetAllResourcesCommand));
        }

        private sealed class FakePlayerPage
        {
            public IReadOnlyList<AmountRowViewModel> Resources { get; init; } = [];

            public ICommand SetAllResourcesCommand { get; init; } = null!;

            public ICommand SetRichCommand { get; init; } = null!;
        }
    }
}
