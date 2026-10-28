using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Siniskalc.Homm3.Tests.ViewModels;
using Siniskalc.Homm3.ViewModels;
using Siniskalc.Wpf.ViewModels;

namespace Siniskalc.Homm3.Tests.Integration.Views
{
    /// <summary>
    /// The hex cards below the picked mine and dwelling are as tall as their record needs: every row of bytes and every known field is in view (<c>TestData/Hota/Dwellings</c>).
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    [Apartment(ApartmentState.STA)]
    internal sealed class HexCardsTests
    {
        [Test]
        public void TheHexCardOfAMineAndOfADwellingShowsEveryRowOfTheRecordWithoutScrolling()
        {
            TestApplication.Ensure();
            var path = Path.Combine("TestData", "Hota", "Dwellings", "dwellings_05_tazar_flags_portal_of_glory.GM2");
            var editor = TestServices.CreateHomm3Editor();
            var shell = TestServices.CreateShell(editor);
            editor.LoadGameFromBytes(TestServices.SavefileCodec.Read(path), path);
            var window = TestServices.CreateShellWindow(shell);
            window.Show();
            try
            {
                shell.IsAdvancedMode = true;
                Pump();
                var frame = window.HeroFrame();
                foreach (var kind in new[] { NavNodeKind.Mine, NavNodeKind.Dwelling })
                {
                    var node = editor.NavigationNodes.Descendants().First(n => n.Kind == kind);
                    node.IsSelected = true;
                    Pump();
                    window.UpdateLayout();
                    Pump();
                    var grid = Find<DataGrid>((Page)frame.Content).First(g => g.IsVisible);
                    var scroll = Find<ScrollViewer>(grid).First();
                    var editorControl = Find<Siniskalc.Wpf.Views.HexEditor>((Page)frame.Content).First(h => h.IsVisible);
                    var fields = Find<ScrollViewer>(editorControl).First(v => v.Content is ItemsControl);
                    Assert.That(fields.ViewportHeight, Is.GreaterThanOrEqualTo(fields.ExtentHeight), $"the known fields of the {kind} need scrolling");
                    Assert.That(grid.Items.Count, Is.GreaterThan(0));
                    Assert.That(scroll.ViewportHeight, Is.GreaterThanOrEqualTo(scroll.ExtentHeight), $"the grid of the {kind} cuts off rows");
                }
            }
            finally
            {
                window.Close();
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
