using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Siniskalc.Data;
using Siniskalc.Runtime;
using Siniskalc.ViewModels;
using Siniskalc.Views;
using Siniskalc.Wpf.Views;

namespace Siniskalc.Tests.ViewModels
{
    /// <summary>
    /// Opens the real main window on a real savegame and visits the hero pages, to catch XAML that does not load
    /// and bindings that point at properties that do not exist.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal sealed class ShellSmokeTests
    {
        private static readonly string SavePath = Path.Combine("TestData", "Hota", "empire_of_the_world_01_m5w4d6.GM1");

        [Test]
        public void HeroPagesLoadWithoutBindingErrors()
        {
            var errors = new BindingErrorListener();
            PresentationTraceSources.Refresh();
            PresentationTraceSources.DataBindingSource.Listeners.Add(errors);
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;

            try
            {
                EnsureApplication();

                var viewModel = TestServices.CreateMainViewModel();
                viewModel.LoadGameFromBytes(TestServices.SavefileCodec.Read(SavePath), SavePath);
                viewModel.IsAdvancedMode = true;
                var window = TestServices.CreateMainWindow(viewModel);
                window.Show();

                try
                {
                    Assert.That(viewModel.NavigationNodes, Is.Not.Empty);

                    // The overview, then a player's page with an edit, before the hero pages.
                    var frame = (Frame)window.FindName("heroFrame");
                    Pump();
                    Assert.That(frame.Content, Is.InstanceOf<OverviewPage>());
                    viewModel.PlayerNodes().Single(n => n.Title == "Purple").IsSelected = true;
                    Pump();
                    Assert.That(frame.Content, Is.InstanceOf<PlayerPage>());
                    (viewModel.CurrentPageOf<PlayerViewModel>()).Resources[0].Value = 12345;
                    viewModel.IsPendingChangesOpen = true;
                    Pump();
                    Assert.That(viewModel.PendingChanges.Single().Path, Is.EqualTo("Purple › Resources"));

                    // The hex tab of the player shows the edited resource.
                    (viewModel.CurrentPageOf<PlayerViewModel>()).SelectedTab = 4;
                    Pump();
                    var playerHex = (HexEditor)((PlayerPage)frame.Content).FindName("hexEditor");
                    Assert.That(playerHex.IsVisible, Is.True);
                    viewModel.ShowOverview();
                    Pump();
                    viewModel.RevertAll();
                    Pump();

                    Assert.That(viewModel.SearchAndAddHero("Christian"), Is.True);
                    Pump();

                    var heroFrame = (Frame)window.FindName("heroFrame");
                    var heroPage = (HeroPage)heroFrame.Content;
                    var tabs = (ListBox)heroPage.FindName("sidebar");

                    foreach (var tab in new[] { "General", "Skills", "Creatures", "Spells", "Artifacts", "Backpack", "Hex" })
                    {
                        tabs.SelectedItem = tabs.Items.OfType<NavButton>().Single(t => t.Text == tab);
                        Pump();
                    }

                    // Edit through the view model and look at the pending changes drawer.
                    var hero = viewModel.SelectedHero();
                    hero.ArmyViewModel.CreatureSlots[0].SelectedCreature = "Angel";
                    hero.GeneralInfoViewModel.Attack = 33;
                    viewModel.IsPendingChangesOpen = true;
                    Pump();

                    Assert.That(viewModel.PendingChanges, Is.Not.Empty);
                    Assert.That(viewModel.NavigationNodes.Any(n => n.HasChanges), Is.True);

                    viewModel.RevertAll();
                    Pump();
                    Assert.That(viewModel.PendingChanges, Is.Empty);
                    Assert.That(viewModel.NavigationNodes.Any(n => n.HasChanges), Is.False);
                }
                finally
                {
                    window.Close();
                }
            }
            finally
            {
                TestServices.CreateMainViewModel().IsAdvancedMode = false;
                PresentationTraceSources.DataBindingSource.Listeners.Remove(errors);
            }

            Assert.That(errors.Messages, Is.Empty, string.Join(Environment.NewLine, errors.Messages));
        }

        [Test]
        public void TheHexTabsAreOnlyThereInAdvancedMode()
        {
            var errors = new BindingErrorListener();
            PresentationTraceSources.Refresh();
            PresentationTraceSources.DataBindingSource.Listeners.Add(errors);
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;

            try
            {
                EnsureApplication();

                var viewModel = TestServices.CreateMainViewModel();
                viewModel.IsAdvancedMode = false;
                viewModel.LoadGameFromBytes(TestServices.SavefileCodec.Read(SavePath), SavePath);
                var window = TestServices.CreateMainWindow(viewModel);
                window.Show();

                try
                {
                    var frame = (Frame)window.FindName("heroFrame");
                    viewModel.PlayerNodes().Single(n => n.Title == "Purple").IsSelected = true;
                    Pump();
                    var tabs = (ListBox)((PlayerPage)frame.Content).FindName("tabs");
                    var hexTab = tabs.Items.OfType<NavButton>().Single(t => t.Text == "Hex");
                    Assert.That(hexTab.Visibility, Is.EqualTo(Visibility.Collapsed));

                    viewModel.IsAdvancedMode = true;
                    Pump();
                    Assert.That(hexTab.Visibility, Is.EqualTo(Visibility.Visible));

                    // Switching it off while the hex tab is the one shown goes back to the first tab.
                    tabs.SelectedItem = hexTab;
                    Pump();
                    viewModel.IsAdvancedMode = false;
                    Pump();
                    Assert.That(tabs.SelectedIndex, Is.EqualTo(0));
                    Assert.That(viewModel.CurrentPageOf<PlayerViewModel>().SelectedTab, Is.EqualTo(0));
                }
                finally
                {
                    window.Close();
                }
            }
            finally
            {
                PresentationTraceSources.DataBindingSource.Listeners.Remove(errors);
            }

            Assert.That(errors.Messages, Is.Empty, string.Join(Environment.NewLine, errors.Messages));
        }

        [Test]
        public void CastlePagesLoadWithoutBindingErrors()
        {
            var errors = new BindingErrorListener();
            PresentationTraceSources.Refresh();
            PresentationTraceSources.DataBindingSource.Listeners.Add(errors);
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;

            try
            {
                EnsureApplication();

                var path = Path.Combine("TestData", "Hota", "castles_01_blacksmith_garrison_tumlac_to_blue.GM1");
                var viewModel = TestServices.CreateMainViewModel();
                viewModel.LoadGameFromBytes(TestServices.SavefileCodec.Read(path), path);
                var window = TestServices.CreateMainWindow(viewModel);
                window.Show();

                try
                {
                    var frame = (Frame)window.FindName("heroFrame");
                    Pump();
                    Assert.That(frame.Content, Is.InstanceOf<OverviewPage>());

                    foreach (var node in viewModel.NavigationNodes.Descendants().Where(c => c.Kind == NavNodeKind.Castle).ToList())
                    {
                        node.IsSelected = true;
                        Pump();
                        Assert.That(frame.Content, Is.InstanceOf<CastlePage>(), node.Title);
                    }

                    viewModel.ShowOverview();
                    Pump();
                }
                finally
                {
                    window.Close();
                }
            }
            finally
            {
                PresentationTraceSources.DataBindingSource.Listeners.Remove(errors);
            }

            Assert.That(errors.Messages, Is.Empty, string.Join(Environment.NewLine, errors.Messages));
        }

        [Test]
        public void SaveHexWindowLoadsWithoutBindingErrors()
        {
            var errors = new BindingErrorListener();
            PresentationTraceSources.Refresh();
            PresentationTraceSources.DataBindingSource.Listeners.Add(errors);
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;

            try
            {
                EnsureApplication();

                var viewModel = TestServices.CreateMainViewModel();
                viewModel.LoadGameFromBytes(TestServices.SavefileCodec.Read(SavePath), SavePath);
                viewModel.SearchAndAddHero("Christian");
                viewModel.SelectedHero().GeneralInfoViewModel.Attack = 33;

                var window = new SaveHexWindow(viewModel.CreateSaveHexDump);
                window.Show();

                try
                {
                    var viewer = (HexViewer)window.FindName("viewer");
                    var grid = (DataGrid)viewer.FindName("bytesGrid");
                    var tree = (TreeView)viewer.FindName("regionTree");
                    Pump();

                    Assert.That(grid.Items.Count, Is.GreaterThan(1000));
                    Assert.That(tree.Items.Count, Is.GreaterThan(10));

                    // Scroll to the middle and to the end, so that rows are made for what is shown there.
                    grid.ScrollIntoView(grid.Items[grid.Items.Count / 2]);
                    Pump();
                    grid.ScrollIntoView(grid.Items[grid.Items.Count - 1]);
                    Pump();

                    // Reading the savefile again keeps the view working.
                    viewer.Reload();
                    Pump();
                    Assert.That(grid.Items.Count, Is.GreaterThan(1000));
                }
                finally
                {
                    window.Close();
                }
            }
            finally
            {
                PresentationTraceSources.DataBindingSource.Listeners.Remove(errors);
            }

            Assert.That(errors.Messages, Is.Empty, string.Join(Environment.NewLine, errors.Messages));
        }

        [Test]
        public void PagesOfAShadowOfDeathGameWithObelisksATavernAndHeroesInACastleLoadWithoutBindingErrors()
        {
            var errors = new BindingErrorListener();
            PresentationTraceSources.Refresh();
            PresentationTraceSources.DataBindingSource.Listeners.Add(errors);
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;

            try
            {
                EnsureApplication();

                var path = Path.Combine("TestData", "Roe", "roe_61_inferno_garrison_and_visiting.GM1");
                var viewModel = TestServices.CreateMainViewModel();
                viewModel.LoadGameFromBytes(TestServices.SavefileCodec.Read(path), path);
                var window = TestServices.CreateMainWindow(viewModel);
                window.Show();

                try
                {
                    var frame = (Frame)window.FindName("heroFrame");
                    Pump();
                    Assert.That(frame.Content, Is.InstanceOf<OverviewPage>());

                    // The tavern, on the heroes tab of the player, and its obelisks on the first tab.
                    viewModel.PlayerNodes().Single(n => n.Title == "Red").IsSelected = true;
                    Pump();
                    var player = viewModel.CurrentPageOf<PlayerViewModel>();
                    Assert.That(player.Obelisks.HasObelisks, Is.True);
                    player.SelectedTab = 3;
                    Pump();
                    player.Obelisks.Obelisks[0].IsVisited = true;
                    Pump();
                    Assert.That(player.Obelisks.Summary, Is.EqualTo("1 of 5 visited"));
                    player.SelectedTab = 1;
                    Pump();
                    player.TavernPlaces[0].Selected = player.TavernPlaces[0].Choices[0];
                    Pump();

                    // The heroes in the Inferno castle, on its title row.
                    var inferno = viewModel.NavigationNodes.Descendants().Single(n => n.Kind == NavNodeKind.Castle && n.Title == "4_Inferno");
                    inferno.IsSelected = true;
                    Pump();
                    Assert.That(frame.Content, Is.InstanceOf<CastlePage>());
                    Assert.That(viewModel.CurrentPageOf<CastleViewModel>().Heroes.Select(h => (h.Role, h.Name)), Is.EqualTo(new[] { ("Garrisoned", "Nymus"), ("Visiting", "Tazar") }));

                    // The asleep box of a hero.
                    Assert.That(viewModel.SearchAndAddHero("Nymus"), Is.True);
                    Pump();
                    Assert.That(viewModel.SelectedHero().CanSleep, Is.True);
                    viewModel.SelectedHero().IsAsleep = true;
                    Pump();

                    Assert.That(viewModel.PendingChanges.Select(c => c.Path), Is.EquivalentTo(new[] { "Obelisks › General", "Red › General", "Nymus › General" }));
                    viewModel.RevertAll();
                    Pump();
                }
                finally
                {
                    window.Close();
                }
            }
            finally
            {
                PresentationTraceSources.DataBindingSource.Listeners.Remove(errors);
            }

            Assert.That(errors.Messages, Is.Empty, string.Join(Environment.NewLine, errors.Messages));
        }

        [Test]
        public void PickingASpellOfTheMageGuildThroughItsComboBoxEditsTheCastle()
        {
            EnsureApplication();

            var path = Path.Combine("TestData", "Hota", "cannot_edit_town_spells.GM1");
            var viewModel = TestServices.CreateMainViewModel();
            viewModel.LoadGameFromBytes(TestServices.SavefileCodec.Read(path), path);
            var window = TestServices.CreateMainWindow(viewModel);
            window.Show();

            try
            {
                var frame = (Frame)window.FindName("heroFrame");
                Pump();
                var castle = viewModel.NavigationNodes.Descendants().First(c => c.Kind == NavNodeKind.Castle);
                castle.IsSelected = true;
                Pump();
                var page = (CastlePage)frame.Content;
                ((ListBox)page.FindName("tabs")).SelectedIndex = 3;
                Pump();

                var combo = Descendants<ComboBox>(page).First(c => c.ItemsSource is IEnumerable<Siniskalc.Entities.Spell> && c.SelectedItem != null);
                var before = (Siniskalc.Entities.Spell)combo.SelectedItem;
                var other = ((IEnumerable<Siniskalc.Entities.Spell>)combo.ItemsSource).First(s => s != before);

                // Picked from the drop-down list, as a user does: the item of the open list is selected.
                combo.IsDropDownOpen = true;
                Pump();
                var item = (ComboBoxItem)combo.ItemContainerGenerator.ContainerFromItem(other);
                item.IsSelected = true;
                Pump();

                Assert.That(viewModel.LoadedGame().History.Current.Castles().GetModels().SelectMany(c => c.MageGuildSpells).SelectMany(l => l), Does.Contain(other));
            }
            finally
            {
                window.Close();
            }
        }

        private static IEnumerable<T> Descendants<T>(DependencyObject root)
            where T : DependencyObject
        {
            for (var i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
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
            if (Application.Current == null)
            {
                var app = new Siniskalc.App { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                app.InitializeComponent();
            }
        }

        private static void Pump()
        {
            Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
        }

        private sealed class BindingErrorListener : TraceListener
        {
            public List<string> Messages { get; } = [];

            public override void Write(string? message)
            {
            }

            public override void WriteLine(string? message)
            {
                this.Messages.Add(message ?? string.Empty);
            }
        }
    }
}
