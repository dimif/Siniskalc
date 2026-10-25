namespace Siniskalc.Heroes.Wpf.Tests.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Heroes.Wpf.Views;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests the resources of a player.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal sealed class ResourcesPanelTests
    {
        private ResourceSet state = ResourceSet.None;

        [OneTimeSetUp]
        public void MakeTheApplication() => WpfTestApplication.Ensure();

        [SetUp]
        public void StartWithNothing() => this.state = ResourceSet.None;

        [Test]
        public void ThePanelShowsOneCardWithALineForEachResourceInTheOrderOfTheGameTheQuickAddButtonsAndTheNote()
        {
            var page = this.Page();
            var panel = new ResourcesPanel { Note = "Amounts are limited.", DataContext = page };

            using var hosted = WpfTestApplication.Show(panel);

            var texts = WpfTestApplication.Descendants<TextBlock>(panel).Select(t => t.Text).ToList();
            Assert.That(texts, Does.Contain("Resources").And.Contain("Quick").And.Contain("Amounts are limited."));
            var names = new[] { "Wood", "Mercury", "Ore", "Sulfur", "Crystal", "Gems", "Gold" };
            Assert.That(texts.Where(names.Contains), Is.EqualTo(names));
            Assert.That(WpfTestApplication.Descendants<Border>(panel).Count(b => b.Width == 672), Is.EqualTo(1), "one card of double width");
            var buttons = WpfTestApplication.Descendants<Button>(panel).ToList();
            Assert.That(buttons.Select(b => b.Content), Is.SupersetOf(new object[] { "Res +100", "Gold +100k" }));
            Assert.That(buttons.First(b => Equals(b.Content, "Res +100")).Command, Is.SameAs(page.SetAllResourcesCommand));
            Assert.That(buttons.Count(b => Equals(b.Content, "+10")), Is.EqualTo(6), "a button on each resource but gold");
            Assert.That(buttons.Count(b => Equals(b.Content, "+10k")), Is.EqualTo(1), "and the thousands of gold");
            Assert.That(WpfTestApplication.Descendants<System.Windows.Shapes.Ellipse>(panel).Select(e => e.Fill), Has.All.InstanceOf<System.Windows.Media.SolidColorBrush>(), "each resource has its colour");
        }

        [Test]
        public void TheButtonOfAResourceAddsToIt()
        {
            var panel = new ResourcesPanel { DataContext = this.Page() };
            using var hosted = WpfTestApplication.Show(panel);

            WpfTestApplication.Descendants<Button>(panel).First(b => Equals(b.Content, "+10")).Command.Execute(null);
            WpfTestApplication.Descendants<Button>(panel).First(b => Equals(b.Content, "+10k")).Command.Execute(null);

            Assert.That((this.state[Resource.Wood], this.state[Resource.Gold]), Is.EqualTo((10L, 10_000L)));
        }

        [Test]
        public void WhatAGameAddsIsShownAboveTheResources()
        {
            var banishing = new TextBlock { Text = "Banishing" };
            var panel = new ResourcesPanel { DataContext = this.Page(), Content = banishing };

            using var hosted = WpfTestApplication.Show(panel);

            var resources = WpfTestApplication.Descendants<TextBlock>(panel).First(t => t.Text == "Resources");
            Assert.That(banishing.TranslatePoint(new Point(0, 0), panel).Y, Is.LessThan(resources.TranslatePoint(new Point(0, 0), panel).Y));
        }

        private FakePlayerPage Page()
        {
            var editor = new ResourcesEditorViewModel(
                new ResourceLimits(0, 999_999, 9_999, 1000, 1, 100, 100_000, 10, 10_000),
                () => this.state,
                () => ResourceSet.None,
                edit => this.state = edit(this.state));
            return new FakePlayerPage { Resources = editor.Rows, SetAllResourcesCommand = editor.SetAllCommand, SetRichCommand = editor.SetRichCommand, SetAllLabel = editor.SetAllLabel, SetRichLabel = editor.SetRichLabel };
        }

        private sealed class FakePlayerPage
        {
            public IReadOnlyList<AmountRowViewModel> Resources { get; init; } = [];

            public ICommand SetAllResourcesCommand { get; init; } = null!;

            public ICommand SetRichCommand { get; init; } = null!;

            public string SetAllLabel { get; init; } = string.Empty;

            public string SetRichLabel { get; init; } = string.Empty;
        }
    }
}
