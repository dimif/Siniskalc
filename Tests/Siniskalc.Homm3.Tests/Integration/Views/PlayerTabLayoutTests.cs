using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Tests.ViewModels;
using Siniskalc.Homm3.ViewModels;
using Siniskalc.Wpf.ViewModels;

namespace Siniskalc.Homm3.Tests.Integration.Views
{
    /// <summary>
    /// The first card of every tab of a player page starts at the same place: the Resources card, the lists and the other tabs add no margins of their own (<c>TestData/Hota/Dwellings</c>).
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    [Apartment(ApartmentState.STA)]
    internal sealed class PlayerTabLayoutTests
    {
        [Test]
        public void TheFirstCardOfEveryTabOfAPlayerStartsAtTheSamePlace()
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
                editor.NavigationNodes.Descendants().First(n => n.Kind == NavNodeKind.Player).IsSelected = true;
                Pump();
                var color = editor.CurrentPageOf<PlayerViewModel>().Color;
                var index = editor.LoadedGame().History.Current.Players().FindByColor(color);
                foreach (var tab in new[] { 0, 1, 2, 3, 4 })
                {
                    editor.ShowPlayer(index, tab);
                    Pump();
                    window.UpdateLayout();
                    Pump();
                    var page = (Page)window.HeroFrame().Content;
                    var cards = Find<Border>(page).Where(b => b.IsVisible && b.ActualWidth > 0 && b.CornerRadius.TopLeft == 4 && b.Padding.Left == 14).ToList();
                    var first = cards.OrderBy(b => b.TranslatePoint(new Point(0, 0), page).Y).ThenBy(b => b.TranslatePoint(new Point(0, 0), page).X).FirstOrDefault();
                    starts.Add((tab, first?.TranslatePoint(new Point(0, 0), page) ?? new Point(-1, -1)));
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

        /// <summary>
        /// Lets WPF finish the work that is queued on the dispatcher (data binding, template application, layout, rendering) before the test
        /// measures anything. The test thread has no message loop of its own, so nothing queued runs unless it is asked for: invoking an empty
        /// action at <see cref="DispatcherPriority.ContextIdle"/> returns only after every item of a higher priority has been processed.
        /// </summary>
        private static void Pump() => Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
    }
}
