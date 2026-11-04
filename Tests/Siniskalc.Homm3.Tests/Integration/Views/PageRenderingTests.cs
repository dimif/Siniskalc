using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Siniskalc.Heroes.Wpf.ViewModels;
using Siniskalc.Heroes.Wpf.Views;
using Siniskalc.Homm3.Tests.ViewModels;
using Siniskalc.Homm3.Tests.Data;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.ViewModels;
using Siniskalc.Wpf.ViewModels;

namespace Siniskalc.Homm3.Tests.Integration.Views
{
    /// <summary>
    /// The pages are made of the blocks of Siniskalc.Heroes.Wpf (header, resources, cards). A binding that finds nothing does not stop a
    /// page from loading, and a block whose template did not apply is simply empty, so these tests look at what is on the page.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    [Apartment(ApartmentState.STA)]
    internal sealed class PageRenderingTests
    {
        private static readonly string SavePath = Path.Combine("TestData", "Hota", "empire_of_the_world_01_m5w4d6.GM1");

        [Test]
        public void EveryBankTypeUsesTheSameOverviewAndCardsWithItsOwnGuardsRewardsAndControls()
        {
            EnsureApplication();
            var path = Path.Combine("TestData", "Hota", "CreatureBanks", "creature_banks_00_baseline.GM2");
            var game = SavedGames.Get(path);
            using var model = new MapViewModel(game, TestServices.EffectLines, TestServices.BankText, TestServices.ByteEditTargets);
            var page = new Siniskalc.Homm3.Views.MapPage(model);
            var window = TestServices.CreateShellWindow(TestServices.CreateShell());
            window.Show();
            try
            {
                window.HeroFrame().Content = page;
                foreach (var kind in Enum.GetValues<CreatureBankKind>())
                {
                    model.ShowSection(MapSections.ForBank(kind));
                    Show(window);
                    var texts = Descendants<TextBlock>(page).Where(t => t.IsVisible).Select(t => t.Text).ToList();
                    var overview = Descendants<ListCard>(page).Single();
                    Assert.That(overview.ItemsSource, Is.SameAs(model.VisibleBanks));
                    Assert.That(model.VisibleBanks.Select(b => b.Kind), Is.All.EqualTo(kind));
                    Assert.That(model.VisibleBanks.Count, Is.EqualTo(kind == CreatureBankKind.DragonUtopia ? 8 : 4));
                    Assert.That(texts, Does.Contain(model.BankGroupName));
                    Assert.That(texts, Is.SupersetOf(model.VisibleBanks.Select(b => b.RewardSummary)));
                    Assert.That(texts, Is.SupersetOf(model.VisibleBanks.SelectMany(b => b.Garrison)));
                    var cards = Descendants<Border>(page).Where(b => Equals(b.Tag, "CreatureBankCard")).ToList();
                    Assert.That(cards.Count, Is.EqualTo(model.VisibleBanks.Count));
                    Assert.That(cards.Select(c => c.ActualWidth), Is.All.EqualTo(overview.CardWidth));
                    var upgradeChoices = Descendants<CheckBox>(page).Where(c => c.IsVisible && Equals(c.Content, "Upgraded centre stack")).ToList();
                    Assert.That(upgradeChoices.Count, Is.EqualTo(kind == CreatureBankKind.DragonUtopia ? 0 : 4));
                    Assert.That(texts.Count(t => t == "Artifact 1"), Is.EqualTo(kind == CreatureBankKind.DragonUtopia ? 8 : 0));
                    var bank = model.VisibleBanks[0];
                    bank.SelectedDefeat = 1;
                    Show(window);
                    var reset = Descendants<Button>(cards[0]).Single(b => Equals(b.Content, "Reset"));
                    Assert.That(reset.IsEnabled, Is.True);
                    reset.Command.Execute(null);
                    Show(window);
                    Assert.That(bank.IsCleared, Is.False);
                    Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes), "clearing and resetting restores the original guards including any centre upgrade");
                }
            }
            finally
            {
                window.Close();
            }
        }

        [Test]
        public void TheDragonUtopiasListRendersEightBanksTheirRewardsGarrisonsAndTheResetAction()
        {
            EnsureApplication();
            var path = Path.Combine("TestData", "Hota", "DragonUtopias", "dragon_utopias_01_first_cleared.GM2");
            var game = SavedGames.Get(path);
            using var model = new MapViewModel(game, TestServices.EffectLines, TestServices.BankText, TestServices.ByteEditTargets);
            var page = new Siniskalc.Homm3.Views.MapPage(model);
            var window = TestServices.CreateShellWindow(TestServices.CreateShell());
            window.Show();
            try
            {
                window.HeroFrame().Content = page;
                model.SelectedTab = 1;
                Show(window);
                var texts = Descendants<TextBlock>(page).Where(t => t.IsVisible).Select(t => t.Text).ToList();
                Assert.That(texts, Is.SupersetOf(model.VisibleBanks.Select(u => u.Heading)));
                Assert.That(texts, Does.Contain("Utopia 1 at 4, 15, Surface"));
                Assert.That(texts, Does.Not.Contain("These guards defend the Utopia."));
                Assert.That(texts, Does.Contain("Admiral's Hat").And.Contain("Angelic Alliance").And.Contain("Armageddon's Blade"));
                Assert.That(texts, Does.Contain("Defeated").And.Contain("Uncleared"));
                Assert.That(texts, Does.Contain("Garrison"));
                var overview = Descendants<ListCard>(page).Single();
                Assert.That(overview.ItemsSource, Is.SameAs(model.VisibleBanks));
                Assert.That(texts, Does.Contain("Tier").And.Contain("Location").And.Contain("State"));
                Assert.That(texts, Does.Contain(model.VisibleBanks[0].RewardSummary));
                var stateSelectors = Descendants<ComboBox>(overview).ToList();
                Assert.That(stateSelectors, Has.Count.EqualTo(8));
                Assert.That(stateSelectors[0].SelectedItem, Is.EqualTo("Defeated"));
                stateSelectors[0].SelectedIndex = 0;
                Show(window);
                Assert.That(model.VisibleBanks[0].IsCleared, Is.False);
                game.History.Undo();
                Show(window);
                Assert.That(stateSelectors[0].SelectedItem, Is.EqualTo("Defeated"));
                Assert.That(texts, Does.Contain("8 Green Dragons").And.Contain("7 Red Dragons").And.Contain("6 Gold Dragons").And.Contain("5 Black Dragons"));
                Assert.That(texts, Does.Contain(TestServices.BankText.Resources(CreatureBankKind.DragonUtopia, 4)));
                Assert.That(texts.Any(t => t.StartsWith("Inspect each", StringComparison.Ordinal)), Is.False);
                Assert.That(Descendants<Expander>(page), Is.Empty);
                var presets = Descendants<ComboBox>(page).Where(c => c.IsVisible && c.Items.Count == 4).ToList();
                Assert.That(presets.Count, Is.EqualTo(8));
                Assert.That(presets.All(c => c.Items.Count == 4), Is.True);
                var restore = Descendants<Button>(page).First(b => b.Command == model.VisibleBanks[0].RestoreCommand);
                Assert.That(restore.Content, Is.EqualTo("Reset"));
                Assert.That(restore.Style, Is.SameAs(page.FindResource("PillButtonStyle")));
                Assert.That(Descendants<Button>(page).Any(b => Equals(b.Content, "Undo changes")), Is.False);
                Assert.That(restore.IsVisible && restore.IsEnabled, Is.True);
                restore.Command.Execute(null);
                Show(window);
                Assert.That(model.VisibleBanks[0].IsCleared, Is.False);
                Assert.That(restore.IsEnabled, Is.False);
                var firstCard = Descendants<Border>(page).Single(b => b.DataContext == model.VisibleBanks[0] && Equals(b.Tag, "CreatureBankCard"));
                Assert.That(firstCard.ActualWidth, Is.EqualTo(overview.CardWidth));
                Assert.That(firstCard.ActualHeight, Is.LessThan(290));
                var status = Descendants<TextBlock>(firstCard).Single(t => t.Text == "Uncleared");
                Assert.That(Math.Abs(status.TranslatePoint(new Point(), firstCard).Y - restore.TranslatePoint(new Point(), firstCard).Y), Is.LessThan(10));
                var strength = Descendants<ComboBox>(firstCard).Single();
                var garrison = Descendants<TextBlock>(firstCard).Single(t => t.Text == "Garrison");
                var rewards = Descendants<TextBlock>(firstCard).Single(t => t.Text == "Rewards");
                var strengthHeading = Descendants<TextBlock>(firstCard).Single(t => t.Text == "Change guard strength and rewards");
                Assert.That(strengthHeading.TranslatePoint(new Point(), firstCard).Y, Is.GreaterThan(garrison.TranslatePoint(new Point(), firstCard).Y), "the strength sits under the garrison");
                Assert.That(strength.TranslatePoint(new Point(), firstCard).X, Is.EqualTo(garrison.TranslatePoint(new Point(), firstCard).X).Within(1));
                Assert.That(strength.ActualWidth, Is.GreaterThan(250), "the strength fills the column that holds it");
                Assert.That(garrison.TranslatePoint(new Point(), firstCard).Y, Is.EqualTo(rewards.TranslatePoint(new Point(), firstCard).Y).Within(1));
                Assert.That(strength.TranslatePoint(new Point(), firstCard).X, Is.LessThan(rewards.TranslatePoint(new Point(), firstCard).X));
                Assert.That(strength.IsVisible, Is.True);
                var gold = Descendants<TextBlock>(firstCard).Single(t => t.Text == model.VisibleBanks[0].ResourceReward);
                Assert.That(gold.FontSize, Is.EqualTo(garrison.FontSize));
                Assert.That(gold.FontWeight, Is.EqualTo(FontWeights.Normal));
                Assert.That(rewards.TranslatePoint(new Point(), firstCard).X, Is.GreaterThan(garrison.TranslatePoint(new Point(), firstCard).X + 200));
                var resetTexts = Descendants<TextBlock>(firstCard).Where(t => t.IsVisible).Select(t => t.Text).ToList();
                Assert.That(resetTexts, Does.Contain("Uncleared").And.Contain("Garrison"));
                Assert.That(resetTexts, Does.Not.Contain("Garrison after reset"));
                game.History.Undo();
                Show(window);
                Assert.That(restore.IsEnabled, Is.True);
                Assert.That(Descendants<TextBlock>(page).Where(t => t.IsVisible).Count(t => t.Text == "Artifact 1"), Is.EqualTo(8));
                model.SelectedTab = 0;
                Show(window);
                Assert.That(Descendants<TextBlock>(page).Where(t => t.IsVisible).Select(t => t.Text), Does.Contain("Black market 1"));
            }
            finally
            {
                window.Close();
            }
        }

        [TestCase(MapSections.DragonUtopias)]
        [TestCase(MapSections.CyclopsStockpiles)]
        [TestCase(MapSections.DwarvenTreasuries)]
        [TestCase(MapSections.ImpCaches)]
        [TestCase(MapSections.MedusaStores)]
        [TestCase(MapSections.NagaBanks)]
        public void PickingAnyBankTypeInTheNavigationHighlightsItsCardAndScrollsItIntoView(int section)
        {
            EnsureApplication();
            var path = Path.Combine("TestData", "Hota", "CreatureBanks", "creature_banks_00_baseline.GM2");
            var editor = TestServices.CreateHomm3Editor();
            editor.LoadGameFromBytes(TestServices.SavefileCodec.Read(path), path);
            var node = editor.NavigationNodes.Descendants().Last(n => n.Kind == NavNodeKind.MapObject && n.Tab == section);
            node.IsSelected = true;
            var model = editor.CurrentPageOf<MapViewModel>();
            using (model)
            {
                var page = new Siniskalc.Homm3.Views.MapPage(model);
                var window = TestServices.CreateShellWindow(TestServices.CreateShell());
                window.Show();
                try
                {
                    window.HeroFrame().Content = page;
                    Show(window);
                    var list = Descendants<ListBox>(page).Single(l => l.Name == "banks");
                    Assert.That(list.SelectedItem, Is.SameAs(model.VisibleBanks.Single(b => b.Index == node.Index)));
                    var selected = (ListBoxItem)list.ItemContainerGenerator.ContainerFromItem(list.SelectedItem);
                    Assert.That(selected.IsSelected, Is.True);
                    var card = Descendants<Border>(selected).Single(b => Equals(b.Tag, "CreatureBankCard"));
                    Assert.That(card.BorderBrush, Is.EqualTo(page.FindResource("HighlightBrush")));
                    var viewport = Descendants<ScrollViewer>(page).First(s => s.Name == "cardsViewer");
                    var origin = card.TranslatePoint(new Point(), viewport);
                    Assert.That(origin.Y, Is.GreaterThanOrEqualTo(-1).And.LessThan(viewport.ActualHeight));
                    Assert.That(origin.Y + card.ActualHeight, Is.LessThanOrEqualTo(viewport.ActualHeight + 1));
                    Assert.That(editor.NavigationNodes.Descendants().Where(n => n.IsSelected), Is.EqualTo(new[] { node }));
                }
                finally
                {
                    window.Close();
                }
            }
        }

        [Test]
        public void TheTownMerchantTabRendersSevenSharedSlotsAndDisappearsWhenItsBuildingIsRemoved()
        {
            EnsureApplication();
            var path = Path.Combine("TestData", "Hota", "MerchantStock", "merchant_stock_00_baseline.GM2");
            var game = SavedGames.Get(path);
            var index = game.History.Current.Castles().GetModels().ToList().FindIndex(c => c.Buildings.Any(b => b.IsArtifactMerchant));
            using var model = TestServices.CreateCastleViewModel(game.History.CreateCastleSession(index));
            var page = new Siniskalc.Homm3.Views.CastlePage(model);
            var window = TestServices.CreateShellWindow(TestServices.CreateShell());
            window.Show();
            try
            {
                window.HeroFrame().Content = page;
                model.SelectedTab = 4;
                Show(window);
                var tab = Descendants<Siniskalc.Wpf.Views.NavButton>(page).Single(b => b.Text == "Artifact merchant");
                Assert.That(tab.IsVisible, Is.True);
                var texts = Descendants<TextBlock>(page).Where(t => t.IsVisible).Select(t => t.Text).ToList();
                Assert.That(texts, Does.Contain("Shared town stock").And.Contain("Stoic Watchman").And.Contain("Head of Legion"));
                Assert.That(texts, Is.SupersetOf(Enumerable.Range(1, 7).Select(i => $"Slot {i}")));
                var reset = Descendants<Button>(page).Single(b => Equals(b.Content, "Reset shared stock"));
                Assert.That(reset.IsVisible, Is.True);
                model.Merchant!.Slots[6].ClearCommand.Execute(null);
                Show(window);
                Assert.That(Descendants<TextBlock>(page).Where(t => t.IsVisible).Select(t => t.Text), Does.Contain("Shared stock has unsaved changes"));
                Assert.That(reset.Command.CanExecute(null), Is.True);

                var building = game.History.Current.Castles().GetModel(index).Buildings.Single(b => b.IsArtifactMerchant);
                game.History.CreateCastleSession(index).Update(c => CastleBuilder.Unbuild(c, building));
                Show(window);
                Assert.That(tab.IsVisible, Is.False);
                Assert.That(model.SelectedTab, Is.EqualTo(0));
                game.History.Undo();
                Show(window);
                Assert.That(tab.IsVisible, Is.True);
                model.SelectedTab = 4;
                Show(window);
                Assert.That(model.Merchant.Slots[6].HasArtifact, Is.False);
            }
            finally
            {
                window.Close();
            }
        }

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
                Assert.That(texts, Is.SupersetOf(new[] { "Wood", "Mercury", "Ore", "Sulfur", "Crystal", "Gems", "Gold", "Quick" }));

                Assert.That(editor.ShowHero("Christian"), Is.True);
                Pump();
                var hero = editor.SelectedHero();
                foreach (var tab in Enumerable.Range(0, 8))
                {
                    hero.SelectedTab = tab;
                    Show(window);
                    AssertRendered((Page)frame.Content, "Christian");
                    if (tab == 2)
                    {
                        Assert.That(Descendants<TextBlock>((Page)frame.Content).Any(t => t.IsVisible && t.Text == "Find creatures"), Is.True);
                    }
                }

                var castleNode = editor.NavigationNodes.Descendants().First(n => n.Kind == NavNodeKind.Castle);
                castleNode.IsSelected = true;
                Pump();
                var castle = editor.CurrentPageOf<CastleViewModel>();
                foreach (var tab in new[] { 0, 1, 2, 3, 5 })
                {
                    castle.SelectedTab = tab;
                    Show(window);
                    AssertRendered((Page)frame.Content, castleNode.Title);
                    if (tab == 1)
                    {
                        Assert.That(Descendants<TextBlock>((Page)frame.Content).Any(t => t.IsVisible && t.Text == "Find creatures"), Is.True);
                    }
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
                Assert.That(texts, Does.Not.Contain("Find creatures"));
                Assert.That(Descendants<Siniskalc.Heroes.Wpf.Views.CreatureSlotPicker>((Page)frame.Content).Any(p => p.IsVisible), Is.True);

                // The finder of creatures of the guard lists every creature of the game: it must only build the rows that are in view.
                Assert.That(Descendants<ListBoxItem>((Page)frame.Content).Count(), Is.LessThan(80), "the finder builds a row for every creature");

                var guard = player.MapObjects.GuardEditor!;
                var heading = Descendants<TextBlock>((Page)frame.Content).Single(t => t.Text == player.MapObjects.GuardHeading);
                var headingRow = (StackPanel)heading.Parent;
                var badge = Descendants<TextBlock>(headingRow).Single(t => t.Text == "edited");
                Assert.That(badge.IsVisible, Is.False);
                guard.CreatureSlots[0].SelectedCreature = guard.CreatureSlots[0].SelectedCreature == "Pikeman" ? "Archer" : "Pikeman";
                Show(window);
                Assert.That(guard.HasPendingChanges, Is.True);
                Assert.That(badge.IsVisible, Is.True);
                editor.Undo();
                Show(window);
                Assert.That(badge.IsVisible, Is.False);
                editor.Redo();
                Show(window);
                Assert.That(badge.IsVisible, Is.True);
                var other = editor.PlayerNodes().Single(n => n.Kind == NavNodeKind.Neutral).Descendants().First(n => n.Kind == NavNodeKind.Mine);
                other.IsSelected = true;
                Show(window);
                Assert.That(editor.CurrentPageOf<NeutralViewModel>().MapObjects.GuardEditor!.HasPendingChanges, Is.False);
                mine.IsSelected = false;
                mine.IsSelected = true;
                Show(window);
                Assert.That(badge.IsVisible, Is.True);
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
