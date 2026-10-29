namespace Siniskalc.Homm2.Tests.Integration.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Threading;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Heroes.Wpf.Views;
    using Siniskalc.Homm2.ViewModels;
    using Siniskalc.Wpf.ViewModels;
    using Siniskalc.Wpf.Views;
    using PlayerColor = Siniskalc.Homm2.Entities.PlayerColor;

    /// <summary>
    /// The pages are made of the blocks of Siniskalc.Heroes.Wpf (header, resources, cards). A binding that finds nothing does not stop a
    /// page from loading, and a block whose template did not apply is simply empty, so these tests look at what is on the page.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    [Apartment(ApartmentState.STA)]
    internal sealed class PageRenderingTests
    {
        private string folder = string.Empty;

        [SetUp]
        public void CreateFolder()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-pages-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
        }

        [TearDown]
        public void DeleteFolder() => Directory.Delete(this.folder, recursive: true);

        [Test]
        public void ThePagesShowTheirHeaderTheirResourcesAndTheirCardsOnEveryTab()
        {
            TestApplication.Ensure();

            var services = new TestApplicationServices(settingsPath: Path.Combine(this.folder, "settings.json"));
            var shell = services.CreateShell();
            shell.LoadGame(Saves.BrokenAllianceSave(24));
            var editor = (Homm2EditorViewModel)shell.Editor;
            var window = new ShellWindow(
                new RecentFilesMenuViewModel(services.RecentFiles),
                new ShellUrlLauncher(_ => { }),
                services.About,
                services.Help,
                NoUserInteraction.Messages(),
                new SavegameFileCommands(services.RecentFiles, NoUserInteraction.FileDialogs(), NoUserInteraction.Messages())) { DataContext = shell };
            window.Show();

            try
            {
                Show(window);
                var hero = editor.CurrentGame!.History.Current.Heroes().GetModels().First(h => h.Owner == PlayerColor.Blue);
                var castle = editor.CurrentGame.History.Current.Castles().GetModels().First(c => c.IsUsed && c.Owner == PlayerColor.Blue);

                editor.ShowPlayer(PlayerColor.Blue);
                foreach (var tab in Enumerable.Range(0, 3))
                {
                    ((PlayerViewModel)editor.CurrentPage!).SelectedTab = tab;
                    Show(window);
                    AssertRendered(Find<Page>(window)!, "Blue");
                }

                ((PlayerViewModel)editor.CurrentPage!).SelectedTab = 0;
                Show(window);
                var texts = Descendants<TextBlock>(Find<Page>(window)!).Where(t => t.IsVisible).Select(t => t.Text).ToList();
                Assert.That(texts, Is.SupersetOf(new[] { "Wood", "Mercury", "Ore", "Sulfur", "Crystal", "Gems", "Gold", "Quick" }));

                editor.ShowHero(hero.Id);
                foreach (var tab in Enumerable.Range(0, 5))
                {
                    ((HeroViewModel)editor.CurrentPage!).SelectedTab = tab;
                    Show(window);
                    AssertRendered(Find<Page>(window)!, hero.Name);
                }

                editor.ShowCastle(castle.Index);
                foreach (var tab in Enumerable.Range(0, 5))
                {
                    ((CastleViewModel)editor.CurrentPage!).SelectedTab = tab;
                    Show(window);
                    AssertRendered(Find<Page>(window)!, castle.Name);
                }
            }
            finally
            {
                window.Close();
            }
        }

        private static void AssertRendered(Page page, string title)
        {
            var header = Descendants<PageHeader>(page).Single();
            Assert.That(VisualTreeHelper.GetChildrenCount(header), Is.GreaterThan(0), "the header has no template");
            Assert.That(Descendants<TextBlock>(header).Select(t => t.Text), Does.Contain(title), "the header does not show the name");

            foreach (var block in Descendants<Control>(page).Where(c => c is ResourcesPanel or ItemsCard && c.IsVisible))
            {
                Assert.That(VisualTreeHelper.GetChildrenCount(block), Is.GreaterThan(0), $"the {block.GetType().Name} has no template");
            }
        }

        private static void Show(Window window)
        {
            Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
            window.UpdateLayout();
        }

        private static T? Find<T>(DependencyObject root)
            where T : DependencyObject => Descendants<T>(root).FirstOrDefault();

        private static IEnumerable<T> Descendants<T>(DependencyObject root)
            where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T match)
                {
                    yield return match;
                }

                foreach (var nested in Descendants<T>(child))
                {
                    yield return nested;
                }
            }
        }
    }
}
