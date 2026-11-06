using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Siniskalc.Homm3.Tests.ViewModels;
using Siniskalc.Wpf.ViewModels;
using Siniskalc.Homm3.ViewModels;

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
        [TestCase(MapSections.DragonUtopias)]
        [TestCase(MapSections.CyclopsStockpiles)]
        [TestCase(MapSections.DwarvenTreasuries)]
        [TestCase(MapSections.ImpCaches)]
        [TestCase(MapSections.MedusaStores)]
        [TestCase(MapSections.NagaBanks)]
        public void EachBankHexViewFollowsSelectionAndReleasesItsBytesWhenHidden(int section)
        {
            TestApplication.Ensure();
            var path = Path.Combine("TestData", "Hota", "CreatureBanks", "creature_banks_00_baseline.GM2");
            var editor = TestServices.CreateHomm3Editor();
            var shell = TestServices.CreateShell(editor);
            editor.LoadGameFromBytes(TestServices.SavefileCodec.Read(path), path);
            var window = TestServices.CreateShellWindow(shell);
            window.Show();
            try
            {
                shell.IsAdvancedMode = true;
                var location = editor.NavigationNodes.Descendants().First(n => n.Kind == NavNodeKind.MapObject && n.Tab == section);
                location.IsSelected = true;
                editor.CurrentPageOf<MapViewModel>().SelectedTab = MapSections.Hex;
                Pump();
                window.UpdateLayout();
                var page = (Page)window.HeroFrame().Content;
                var hex = Find<Siniskalc.Wpf.Views.HexEditor>(page).Single(h => h.Name == "recordHexEditor");
                var grid = Find<DataGrid>(hex).Single();
                Assert.That(hex.IsVisible, Is.True);
                Assert.That(grid.Items.Count, Is.GreaterThan(0));
                var rows = grid.ItemsSource;
                var model = editor.CurrentPageOf<MapViewModel>();
                model.SelectedBank!.SelectedStrength = 0;
                Pump();
                Assert.That(grid.ItemsSource, Is.SameAs(rows), "an edit refreshes the same target rather than rebinding");
                model.SelectedBank = model.VisibleBanks[1];
                Pump();
                Assert.That(grid.ItemsSource, Is.Not.Null.And.Not.SameAs(rows));
                shell.IsAdvancedMode = false;
                Pump();
                Assert.That(grid.ItemsSource, Is.Null);
                shell.IsAdvancedMode = true;
                model.SelectedTab = MapSections.Hex;
                Pump();
                Assert.That(grid.Items.Count, Is.GreaterThan(0));
                model.SelectedBank = model.VisibleBanks[0];
                Pump();
                Assert.That(grid.Items.Count, Is.GreaterThan(0));
                model.ShowSection(MapSections.Merchants);
                Pump();
                Assert.That(grid.ItemsSource, Is.Null);
            }
            finally
            {
                shell.IsAdvancedMode = false;
                window.Close();
            }
        }

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
                    // The dwellings of Neutral are many, so the list of dwellings must scroll inside its card.
                    var node = editor.PlayerNodes().Single(p => p.Title == "Neutral").Descendants().First(n => n.Kind == kind);
                    node.IsSelected = true;
                    Pump();
                    window.UpdateLayout();
                    Pump();
                    var grid = Find<DataGrid>((Page)frame.Content).First(g => g.IsVisible);
                    var scroll = Find<ScrollViewer>(grid).First();
                    var editorControl = Find<Siniskalc.Wpf.Views.HexEditor>((Page)frame.Content).First(h => h.IsVisible);
                    // Windows keeps a window inside the screen, so on a small display (the 1024 pixels of a CI runner) the shell is narrower than its 1330 and the fields
                    // flow into fewer columns. The card is checked at the width it has in the default shell.
                    editorControl.Width = DefaultHexEditorWidth;
                    window.UpdateLayout();
                    var fields = Find<ScrollViewer>(editorControl).First(v => v.Content is ItemsControl);
                    Assert.That(fields.ViewportHeight, Is.GreaterThanOrEqualTo(fields.ExtentHeight), $"the known fields of the {kind} need scrolling");
                    if (kind == NavNodeKind.Dwelling)
                    {
                        var rows = Find<ListBox>((Page)frame.Content).First(l => l.Name == "PART_List" && l.IsVisible);
                        Assert.That(rows.Items.Count, Is.GreaterThan(10));
                        Assert.That(rows.ActualHeight, Is.LessThanOrEqualTo(340), "the list of dwellings must scroll inside its card, so the bytes below stay in reach");
                    }

                    Assert.That(grid.Items.Count, Is.GreaterThan(0));
                    Assert.That(scroll.ViewportHeight, Is.GreaterThanOrEqualTo(scroll.ExtentHeight), $"the grid of the {kind} cuts off rows");

                    shell.IsAdvancedMode = false;
                    Pump();
                    Assert.That(grid.ItemsSource, Is.Null, $"the hidden {kind} releases its target");
                    shell.IsAdvancedMode = true;
                    Pump();
                    Assert.That(grid.Items.Count, Is.GreaterThan(0), $"the {kind} rebinds when advanced mode returns");
                }
            }
            finally
            {
                window.Close();
            }
        }

        // Mirrors the width the hex editor card gets in the default shell (ShellWindow.xaml, Width="1330"). If the shell's default width or layout changes, re-measure it
        // there: this test checks the card at this width, not at whatever the shell gives it.
        private const double DefaultHexEditorWidth = 900;

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
