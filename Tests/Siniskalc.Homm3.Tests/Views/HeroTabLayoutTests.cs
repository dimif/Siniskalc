using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Siniskalc.Homm3.Tests.ViewModels;
using Siniskalc.Homm3.ViewModels;
using Siniskalc.Wpf.ViewModels;

namespace Siniskalc.Homm3.Tests.Views
{
    /// <summary>
    /// The first card of every tab of a hero page starts at the same place: the tabs do not add margins of their own (<c>TestData/Hota/Dwellings</c>).
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal sealed class HeroTabLayoutTests
    {
        [Test]
        public void TheFirstCardOfEveryTabOfAHeroStartsAtTheSamePlace()
        {
            TestApplication.Ensure();
            var path = Path.Combine("TestData", "Hota", "Dwellings", "dwellings_05_tazar_flags_portal_of_glory.GM2");
            var editor = TestServices.CreateHomm3Editor();
            var shell = TestServices.CreateShell(editor);
            editor.LoadGameFromBytes(TestServices.SavefileCodec.Read(path), path);
            var window = TestServices.CreateShellWindow(shell);
            window.Show();
            var starts = new List<(int Tab, Point At)>();
            try
            {
                Pump();
                var node = editor.NavigationNodes.Descendants().First(n => n.Kind == NavNodeKind.Hero);
                node.IsSelected = true;
                Pump();
                var hero = editor.CurrentPageOf<HeroViewModel>();
                foreach (var tab in new[] { 0, 1, 2, 3, 4, 5 })
                {
                    hero.SelectedTab = tab;
                    Pump();
                    window.UpdateLayout();
                    Pump();
                    var frame = window.HeroFrame();
                    var page = (Page)frame.Content;
                    var cards = Find<Border>(page).Where(b => b.IsVisible && b.CornerRadius.TopLeft == 4 && b.Padding.Left == 14).ToList();
                    var first = cards.OrderBy(b => b.TranslatePoint(new Point(0, 0), page).Y).ThenBy(b => b.TranslatePoint(new Point(0, 0), page).X).FirstOrDefault();
                    Assert.That(first, Is.Not.Null, $"tab {tab} shows no card");
                    starts.Add((tab, first!.TranslatePoint(new Point(0, 0), page)));
                }
            }
            finally
            {
                window.Close();
            }

            var expected = starts[1].At;
            foreach (var (tab, at) in starts)
            {
                Assert.That((at.X, at.Y), Is.EqualTo((expected.X, expected.Y)).Within(0.5), $"the first card of tab {tab}");
            }
        }

        private static IEnumerable<T> Find<T>(DependencyObject root)
            where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T m)
                {
                    yield return m;
                }

                foreach (var n in Find<T>(child))
                {
                    yield return n;
                }
            }
        }

        private static void Pump() => Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
    }
}
