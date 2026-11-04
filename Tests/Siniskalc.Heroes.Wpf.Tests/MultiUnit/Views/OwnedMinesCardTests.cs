namespace Siniskalc.Heroes.Wpf.Tests.MultiUnit.Views
{
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Threading;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.Tests.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Heroes.Wpf.Views;

    /// <summary>
    /// Tests what the card of the mines of one owner shows, over a page of a made-up game: a card whose template did not apply is empty and
    /// raises no binding error.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    [Apartment(ApartmentState.STA)]
    internal sealed class OwnedMinesCardTests
    {
        [OneTimeSetUp]
        public void MakeTheApplication() => WpfTestApplication.Ensure();

        [TestCase(true)]
        [TestCase(false)]
        public void ADetailWhoseDataContextIsAPagePropertyRaisesNoBindingErrorInAHiddenTabOnLoadAndOnReload(bool hasGuard)
        {
            var errors = new BindingErrorListener();
            PresentationTraceSources.DataBindingSource.Listeners.Add(errors);
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
            try
            {
                var page = new Page(TestColor.Red) { Guard = hasGuard ? "The guard of the picked mine" : null };
                var detail = new Border();
                detail.SetBinding(FrameworkElement.DataContextProperty, new Binding(nameof(Page.Guard)));
                var text = new TextBlock();
                text.SetBinding(TextBlock.TextProperty, new Binding());
                detail.Child = text;
                var card = new OwnedMinesCard { Detail = detail };
                var tab = new Border { Child = card, Visibility = Visibility.Collapsed };
                var root = new Grid { DataContext = page };
                root.Children.Add(tab);

                using (ShowOutsideAdvancedMode(root))
                {
                    Assert.That(detail.DataContext, Is.EqualTo(page.Guard), "the detail has the page while its tab is hidden");
                    tab.Visibility = Visibility.Visible;
                    Idle();
                    Assert.That(text.IsVisible, Is.True);
                    Assert.That(text.Text, Is.EqualTo(page.Guard ?? string.Empty));
                    root.Children.Remove(tab);
                    Idle();
                    root.Children.Add(tab);
                    Idle();
                    Assert.That(detail.DataContext, Is.EqualTo(page.Guard), "the detail has the page again once it is shown again");
                }

                Idle();
                Assert.That(errors.Messages, Is.Empty);
            }
            finally
            {
                PresentationTraceSources.DataBindingSource.Listeners.Remove(errors);
            }
        }

        [Test]
        public void TheCardShowsTheMinesOfTheOwnerWithItsNoteAndHeadingAndTheDetailOfTheGameBoundToThePage()
        {
            var page = new Page(TestColor.Red);
            var detail = new TextBlock();
            detail.SetBinding(TextBlock.TextProperty, new Binding(nameof(Page.GuardText)));
            var card = new OwnedMinesCard { DataContext = page, Note = "Who owns each mine this player owns.", SummaryHeading = "Guard (read-only)", Detail = detail };

            using var hosted = ShowOutsideAdvancedMode(card);

            var texts = WpfTestApplication.Descendants<TextBlock>(card).Where(t => t.IsVisible).Select(t => t.Text).ToList();
            Assert.That(texts, Is.SupersetOf(new[] { "Mines", "Who owns each mine this player owns.", "Guard (read-only)", "Wood", "Ore", "The guard of the picked mine" }));
            Assert.That(texts, Has.None.EqualTo("Gold").And.None.EqualTo("Crystal"), "the mines of other owners are not listed");
            Assert.That(WpfTestApplication.Descendants<ComboBox>(card).Count(c => c.IsVisible), Is.EqualTo(2), "one owner picker for each mine of the owner");
            Assert.That(texts, Has.None.EqualTo("Level"), "a card that does not show the level has no level column");
            Assert.That(texts, Has.None.EqualTo("Hex: the bytes of the mine that is picked"), "the bytes are hidden outside the advanced mode");
        }

        [Test]
        public void ACardThatShowsTheLevelHasALevelColumn()
        {
            var card = new OwnedMinesCard { DataContext = new Page(TestColor.Red), ShowLevel = true };

            using var hosted = WpfTestApplication.Show(card);

            var texts = WpfTestApplication.Descendants<TextBlock>(card).Where(t => t.IsVisible).Select(t => t.Text).ToList();
            Assert.That(texts, Does.Contain("Level"));
        }

        [Test]
        public void TheCardAsksForTheBytesOfThePickedMineOnlyInTheAdvancedModeAndAgainWhenAnotherMineIsPicked()
        {
            var page = new Page(TestColor.Red);
            var card = new OwnedMinesCard { DataContext = page };
            using var hosted = ShowOutsideAdvancedMode(card);
            var window = Window.GetWindow(card)!;
            Idle();
            Assert.That(page.AskedBytes, Is.Empty, "no bytes are read outside the advanced mode");

            window.DataContext = new { IsAdvancedMode = true };
            Idle();
            Assert.That(page.AskedBytes, Is.EqualTo(new[] { 0 }));
            Assert.That(WpfTestApplication.Descendants<TextBlock>(card).Where(t => t.IsVisible).Select(t => t.Text), Does.Contain("Hex: the bytes of the mine that is picked"));

            page.Mines.PickMine(2);
            Idle();
            Assert.That(page.AskedBytes, Is.EqualTo(new[] { 0, 2 }));
        }

        private static Hosted ShowOutsideAdvancedMode(FrameworkElement card)
        {
            var window = new Window { Content = card, DataContext = new { IsAdvancedMode = false }, ShowInTaskbar = false };
            window.Show();
            Idle();
            window.UpdateLayout();
            return new Hosted(window);
        }

        private static void Idle() => Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);

        private sealed class Hosted(Window window) : IDisposable
        {
            public void Dispose() => window.Close();
        }

        private sealed class BindingErrorListener : TraceListener
        {
            public List<string> Messages { get; } = [];

            public override void Write(string? message)
            {
            }

            public override void WriteLine(string? message) => this.Messages.Add(message ?? string.Empty);
        }

        private sealed class Page(TestColor owner) : IOwnedMineList<MineRowViewModel<IMineModel<TestColor>, TestColor>>
        {
            public MinesListViewModel<MineRowViewModel<IMineModel<TestColor>, TestColor>> Mines { get; } = OwnedMines.List(4, TestMineRows.OfFour, owner);

            public string GuardText => "The guard of the picked mine";

            public string? Guard { get; init; }

            public List<int> AskedBytes { get; } = [];

            public IByteEditTarget? CreateMineByteEditTarget()
            {
                this.AskedBytes.Add(this.Mines.SelectedMine!.Index);
                return null;
            }
        }
    }
}
