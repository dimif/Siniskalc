using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Siniskalc.Heroes.Wpf.ViewModels;
using Siniskalc.Heroes.Wpf.Views;
using Siniskalc.Homm3.Tests.ViewModels;
using Siniskalc.Homm3.ViewModels;
using Siniskalc.Wpf.ViewModels;

namespace Siniskalc.Homm3.Tests.Views
{
    /// <summary>
    /// The pages are made of the blocks of Siniskalc.Heroes.Wpf (header, resources, cards). A binding that finds nothing does not stop a
    /// page from loading, and a block whose template did not apply is simply empty, so these tests look at what is on the page.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal sealed class PageRenderingTests
    {
        private static readonly string SavePath = Path.Combine("TestData", "Hota", "empire_of_the_world_01_m5w4d6.GM1");

        [Test]
        public void ThePagesShowTheirHeaderTheirResourcesAndTheirCardsOnEveryTab()
        {
            EnsureApplication();
            var editor = TestServices.CreateHomm3Editor();
            var shell = TestServices.CreateShell(editor);
            editor.LoadGameFromBytes(TestServices.SavefileCodec.Read(SavePath), SavePath);
            var window = TestServices.CreateShellWindow(shell);
            window.Show();

            try
            {
                var frame = window.HeroFrame();
                Pump();

                editor.PlayerNodes().Single(n => n.Title == "Purple").IsSelected = true;
                Pump();
                var player = editor.CurrentPageOf<PlayerViewModel>();
                foreach (var tab in new[] { 0, 1, 2 })
                {
                    player.SelectedTab = tab;
                    Show(window);
                    AssertRendered((Page)frame.Content, "Purple");
                }

                player.SelectedTab = 0;
                Show(window);
                var texts = Descendants<TextBlock>((Page)frame.Content).Where(t => t.IsVisible).Select(t => t.Text).ToList();
                Assert.That(texts, Is.SupersetOf(new[] { "Wood", "Mercury", "Ore", "Sulfur", "Crystal", "Gems", "Gold", "Quick add" }));

                Assert.That(editor.ShowHero("Christian"), Is.True);
                Pump();
                var hero = editor.SelectedHero();
                foreach (var tab in Enumerable.Range(0, 8))
                {
                    hero.SelectedTab = tab;
                    Show(window);
                    AssertRendered((Page)frame.Content, "Christian");
                }

                var castleNode = editor.NavigationNodes.Descendants().First(n => n.Kind == NavNodeKind.Castle);
                castleNode.IsSelected = true;
                Pump();
                var castle = editor.CurrentPageOf<CastleViewModel>();
                foreach (var tab in Enumerable.Range(0, 5))
                {
                    castle.SelectedTab = tab;
                    Show(window);
                    AssertRendered((Page)frame.Content, castleNode.Title);
                }
            }
            finally
            {
                window.Close();
            }
        }

        [Test]
        public void TheMinesTabOfAPlayerShowsItsMinesAndTheGuardOfThePickedOne()
        {
            EnsureApplication();
            var path = Path.Combine("TestData", "Hota", "Mines", "mines_08_red_abandoned_mine_sulfur.GM2");
            var editor = TestServices.CreateHomm3Editor();
            var shell = TestServices.CreateShell(editor);
            editor.LoadGameFromBytes(TestServices.SavefileCodec.Read(path), path);
            var window = TestServices.CreateShellWindow(shell);
            window.Show();

            try
            {
                var frame = window.HeroFrame();
                Pump();

                var mine = editor.PlayerNodes().Where(n => n.Kind == NavNodeKind.Player).SelectMany(n => n.Descendants()).First(n => n.Kind == NavNodeKind.Mine);
                mine.IsSelected = true;
                Pump();
                var player = editor.CurrentPageOf<PlayerViewModel>();
                Assert.That(player.SelectedTab, Is.EqualTo(OwnerTabs.Mines));
                Assert.That(player.MapObjects.Mines.SelectedMine?.Index, Is.EqualTo(mine.Index));
                Show(window);

                var texts = Descendants<TextBlock>((Page)frame.Content).Where(t => t.IsVisible).Select(t => t.Text).ToList();
                Assert.That(texts, Does.Contain("Mines").And.Contain(player.MapObjects.GuardHeading), "the mines card did not render");
                Assert.That(Descendants<MinesCard>((Page)frame.Content).Count(c => c.IsVisible), Is.EqualTo(1));

                // The finder of creatures of the guard lists every creature of the game: it must only build the rows that are in view.
                Assert.That(Descendants<ListBoxItem>((Page)frame.Content).Count(), Is.LessThan(80), "the finder builds a row for every creature");
            }
            finally
            {
                window.Close();
            }
        }

        [Test]
        public void TheNeutralPageShowsItsHeaderAndItsListsOnEveryTab()
        {
            EnsureApplication();
            var path = Path.Combine("TestData", "Hota", "Mines", "mines_08_red_abandoned_mine_sulfur.GM2");
            var editor = TestServices.CreateHomm3Editor();
            var shell = TestServices.CreateShell(editor);
            editor.LoadGameFromBytes(TestServices.SavefileCodec.Read(path), path);
            var window = TestServices.CreateShellWindow(shell);
            window.Show();

            try
            {
                var frame = window.HeroFrame();
                Pump();

                editor.PlayerNodes().Single(n => n.Title == "Neutral").IsSelected = true;
                Pump();
                var neutral = editor.CurrentPageOf<NeutralViewModel>();
                foreach (var tab in new[] { NeutralTabs.Heroes, NeutralTabs.Towns, NeutralTabs.Mines, NeutralTabs.Dwellings })
                {
                    neutral.SelectedTab = tab;
                    Show(window);
                    AssertRendered((Page)frame.Content, "Neutral");
                }

                neutral.SelectedTab = NeutralTabs.Mines;
                Show(window);
                Assert.That(Descendants<MinesCard>((Page)frame.Content).Count(c => c.IsVisible), Is.EqualTo(1));
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
            Pump();
            window.UpdateLayout();
        }

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

        private static void EnsureApplication()
        {
            TestApplication.Ensure();
        }

        private static void Pump() => Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
    }
}
