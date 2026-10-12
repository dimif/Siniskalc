using Siniskalc;
using Siniskalc.Runtime;
using Siniskalc.ViewModels;

namespace Siniskalc.Tests.ViewModels
{
    /// <summary>
    /// Tests the overview, the player pages and how they sit in the navigation tree, on a real HotA savegame.
    /// </summary>
    [TestFixture]
    internal sealed class PlayerAndOverviewViewModelTests
    {
        private static readonly string SavePath = Path.Combine("TestData", "Hota", "empire_of_the_world_01_m5w4d6.GM1");

        private MainViewModel main = null!;

        [SetUp]
        public void LoadGame()
        {
            this.main = TestServices.CreateMainViewModel();
            this.main.LoadGameFromBytes(TestServices.SavefileCodec.Read(SavePath), SavePath);
        }

        private NavNodeViewModel PlayerNode(string color) =>
            this.main.PlayerNodes().Single(n => n.Title == color && n.Kind == NavNodeKind.Player);

        [Test]
        public void ALoadedSavegameStartsOnTheOverview()
        {
            Assert.That(this.main.CurrentPage, Is.SameAs(this.main.Overview));
            Assert.That(this.main.NavigationNodes[0].Kind, Is.EqualTo(NavNodeKind.Overview));
            Assert.That(this.main.NavigationNodes[0].Title, Is.EqualTo("Game"));
            Assert.That(this.main.NavigationNodes[0].Subtitle, Is.EqualTo("empire_of_the_world_01_m5w4d6.GM1"));
            Assert.That(this.main.NavigationNodes[0].IsSelected, Is.True);
        }

        [Test]
        public void EveryPlayerIsANodeHoldingItsHeroesAndTowns()
        {
            var players = this.main.PlayerNodes().ToList();

            // Neutral heroes and towns belong to no player, so that node holds them but has no page.
            Assert.That(players.Select(g => g.Title), Is.EqualTo(new[] { "Red", "Blue", "Tan", "Green", "Orange", "Purple", "Teal", "Pink", "Neutral" }));
            Assert.That(players.Take(8).Select(g => g.Kind), Has.All.EqualTo(NavNodeKind.Player));
            Assert.That(players.Last().Kind, Is.EqualTo(NavNodeKind.Group));
            Assert.That(players.SelectMany(p => p.Children).Select(c => c.Title).Distinct(), Is.EquivalentTo(new[] { "Heroes", "Towns" }));
            Assert.That(players.Take(8).SelectMany(p => p.Children), Has.All.Matches<NavNodeViewModel>(c => c.Kind == NavNodeKind.PlayerTab));
            Assert.That(players.Last().Children, Has.All.Matches<NavNodeViewModel>(c => c.Kind == NavNodeKind.Group));

            var purpleHeroes = players.Single(g => g.Title == "Purple").Children.Single(c => c.Title == "Heroes").Children;
            Assert.That(purpleHeroes, Is.Not.Empty);
            Assert.That(purpleHeroes, Has.All.Matches<NavNodeViewModel>(n => n.IsHero));
            Assert.That(players.Last().Children.Single(c => c.Title == "Heroes").Children, Has.All.Matches<NavNodeViewModel>(n => n.IsHero));
        }

        [Test]
        public void TheGameIsTheOnlyRootAndHoldsThePlayers()
        {
            var root = this.main.NavigationNodes.Single();

            Assert.That(root.Title, Is.EqualTo("Game"));
            Assert.That(root.Kind, Is.EqualTo(NavNodeKind.Overview));
            Assert.That(root.IsExpanded, Is.True);
            Assert.That(root.Children.Select(c => c.Title).Take(3), Is.EqualTo(new[] { "Red", "Blue", "Tan" }));
            Assert.That(root.Children, Has.None.Matches<NavNodeViewModel>(c => c.Kind == NavNodeKind.Overview));
        }

        [Test]
        public void OwnedNodesStartExpandedAndNeutralOnesCollapsed()
        {
            var purple = this.PlayerNode("Purple");

            Assert.That(purple.IsExpanded, Is.True);
            Assert.That(purple.Children, Has.All.Matches<NavNodeViewModel>(c => c.IsExpanded));
            Assert.That(this.main.PlayerNodes().Single(n => n.Title == "Neutral").IsExpanded, Is.False);
        }

        [Test]
        public void SelectingAHeroOpensTheNodesAboveIt()
        {
            var neutral = this.main.PlayerNodes().Single(n => n.Title == "Neutral");
            var hero = neutral.Descendants().First(n => n.IsHero);

            hero.IsSelected = true;

            Assert.That(neutral.IsExpanded, Is.True);
            Assert.That(neutral.Children.Single(c => c.Title == "Heroes").IsExpanded, Is.True);
            Assert.That((this.main.CurrentPageOf<HeroViewModel>()).Name, Is.EqualTo(hero.Title));
        }

        [Test]
        public void EditsMarkTheNodesAboveTheEditedHeroAndPlayer()
        {
            Assert.That(this.main.SearchAndAddHero("Christian"), Is.True);
            this.main.SelectedHero().GeneralInfoViewModel.Attack = 77;

            var purple = this.PlayerNode("Purple");
            Assert.That(purple.Descendants().Single(n => n.Title == "Christian").HasChanges, Is.True);
            Assert.That(purple.Children.Single(c => c.Title == "Heroes").HasChanges, Is.True);
            Assert.That(purple.HasChanges, Is.True);
            Assert.That(purple.Children.Single(c => c.Title == "Heroes").Children.Count(c => c.HasChanges), Is.EqualTo(1));
            Assert.That(this.PlayerNode("Red").HasChanges, Is.False);
        }

        [Test]
        public void SelectingAResourcesNodeShowsThePlayer()
        {
            this.PlayerNode("Purple").IsSelected = true;

            var player = this.main.CurrentPageOf<PlayerViewModel>();
            Assert.That(player.Name, Is.EqualTo("Purple"));
            Assert.That(player.Resources.Select(r => r.Name), Is.EqualTo(new[] { "Gold", "Wood", "Mercury", "Ore", "Sulfur", "Crystal", "Gems" }));
            Assert.That(player.Resources.Single(r => r.IsGold).Value, Is.EqualTo(220976));
            Assert.That(player.Resources.Single(r => r.Name == "Mercury").Value, Is.EqualTo(153));
        }

        [Test]
        public void PickingAHeroAfterAPlayerShowsTheHeroEvenWhenItWasSelectedBefore()
        {
            Assert.That(this.main.SearchAndAddHero("Christian"), Is.True);
            var hero = this.main.SelectedHeroViewModel;

            this.PlayerNode("Purple").IsSelected = true;
            Assert.That(this.main.CurrentPage, Is.InstanceOf<PlayerViewModel>());

            this.main.SelectedHeroViewModel = hero;
            Assert.That(this.main.CurrentPage, Is.SameAs(hero));
        }

        [Test]
        public void EditingAResourceIsUndoableAndMarksThePlayerAndItsNode()
        {
            this.PlayerNode("Purple").IsSelected = true;
            var player = this.main.CurrentPageOf<PlayerViewModel>();
            var gold = player.Resources.Single(r => r.IsGold);

            gold.Value = 5_000_000;

            Assert.That(gold.Value, Is.EqualTo(5_000_000));
            Assert.That(gold.IsChanged, Is.True);
            Assert.That(gold.DeltaText, Is.EqualTo("+4,779,024 from save").Or.EqualTo(string.Format("+{0:N0} from save", 5_000_000 - 220976)));
            Assert.That(player.HasPendingChanges, Is.True);
            Assert.That(this.PlayerNode("Purple").HasChanges, Is.True);
            Assert.That(this.main.PlayerNodes().Single(n => n.Title == "Purple").HasChanges, Is.True);
            Assert.That(this.PlayerNode("Red").HasChanges, Is.False);
            Assert.That(this.main.PendingChanges.Single().Path, Is.EqualTo("Purple › Resources"));

            this.main.Undo();

            Assert.That(gold.Value, Is.EqualTo(220976));
            Assert.That(gold.IsChanged, Is.False);
            Assert.That(gold.DeltaText, Is.Empty);
            Assert.That(player.HasPendingChanges, Is.False);
            Assert.That(this.main.PendingChanges, Is.Empty);
        }

        [Test]
        public void ResourceAmountsStayInsideTheirLimits()
        {
            this.PlayerNode("Purple").IsSelected = true;
            var player = this.main.CurrentPageOf<PlayerViewModel>();

            player.Resources.Single(r => r.IsGold).Value = long.MaxValue;
            player.Resources.Single(r => r.Name == "Wood").Value = -4;
            player.Resources.Single(r => r.Name == "Gems").Value = 5_000_000;

            Assert.That(player.Resources.Single(r => r.IsGold).Value, Is.EqualTo(Constants.Resources.MaxGold));
            Assert.That(player.Resources.Single(r => r.Name == "Wood").Value, Is.EqualTo(0));
            Assert.That(player.Resources.Single(r => r.Name == "Gems").Value, Is.EqualTo(Constants.Resources.MaxOther));
        }

        [Test]
        public void QuickSetCommandsAndResetPlayerWork()
        {
            this.PlayerNode("Teal").IsSelected = true;
            var player = this.main.CurrentPageOf<PlayerViewModel>();
            var gold = player.Resources.Single(r => r.IsGold).Value;

            Assert.That(player.ResetCommand.CanExecute(null), Is.False);

            var others = player.Resources.Where(r => !r.IsGold).Select(r => r.Value).ToList();

            player.SetAllResourcesCommand.Execute(null);
            Assert.That(player.Resources.Where(r => !r.IsGold).Select(r => r.Value), Is.EqualTo(others.Select(v => Math.Min(v + 99, Constants.Resources.MaxOther))));
            Assert.That(player.Resources.Single(r => r.IsGold).Value, Is.EqualTo(gold));

            player.SetRichCommand.Execute(null);
            Assert.That(player.Resources.Single(r => r.IsGold).Value, Is.EqualTo(Math.Min(gold + Constants.Resources.GoldIncrease, Constants.Resources.MaxGold)));
            Assert.That(player.ResetCommand.CanExecute(null), Is.True);

            player.ResetCommand.Execute(null);
            Assert.That(player.HasPendingChanges, Is.False);
            Assert.That(player.Resources.Single(r => r.IsGold).Value, Is.EqualTo(gold));
            Assert.That(this.main.PendingChanges, Is.Empty);
        }

        [Test]
        public void PendingPlayerChangesCanBeRevertedOneByOne()
        {
            this.PlayerNode("Red").IsSelected = true;
            var player = this.main.CurrentPageOf<PlayerViewModel>();
            var original = player.Resources.ToDictionary(r => r.Name, r => r.Value);
            player.Resources.Single(r => r.Name == "Ore").Value = 1;
            player.Resources.Single(r => r.Name == "Sulfur").Value = 2;

            this.main.RevertChange(this.main.PendingChanges.Single(c => c.Change.Description == "Ore"));

            Assert.That(player.Resources.Single(r => r.Name == "Ore").Value, Is.EqualTo(original["Ore"]));
            Assert.That(player.Resources.Single(r => r.Name == "Sulfur").Value, Is.EqualTo(2));
            Assert.That(this.main.PendingChanges.Select(c => c.Change.Description), Is.EqualTo(new[] { "Sulfur" }));
        }

        [Test]
        public void TheOverviewListsEveryPlayerWithTheirGoldAndHeroes()
        {
            var overview = this.main.LoadedOverview();
            var purple = overview.Players.Single(p => p.Name == "Purple");

            Assert.That(overview.Players.Select(p => p.Name), Is.EqualTo(new[] { "Red", "Blue", "Tan", "Green", "Orange", "Purple", "Teal", "Pink" }));
            Assert.That(overview.Players, Has.All.Matches<PlayerCardViewModel>(p => p.HasResources));
            Assert.That(overview.Players.Select(p => p.OpenCommand.CanExecute(null)), Has.All.True);
            Assert.That(overview.VersionName, Is.EqualTo("Horn of the Abyss"));
            Assert.That(overview.FileName, Is.EqualTo("empire_of_the_world_01_m5w4d6.GM1"));
            Assert.That(overview.DateText, Is.EqualTo("Month 5, Week 4, Day 6"));
            Assert.That(overview.MapName, Is.EqualTo("The empire of the world"));
            Assert.That(overview.MapSizeText, Is.EqualTo("Extra large (144 x 144) · 1 level").Or.EqualTo("Extra large (144 x 144) · 2 levels"));
            Assert.That(overview.MapDifficultyText, Does.StartWith("Rating: "));
            Assert.That(overview.DifficultyText, Is.EqualTo("Difficulty: Queen (160%)"));
            Assert.That(overview.Summary, Does.EndWith(" · Difficulty: Queen (160%)"));
            Assert.That(overview.Summary, Does.StartWith("8 players · "));
            Assert.That(purple.Gold, Is.EqualTo(220976.ToString("N0")));
            Assert.That(purple.OtherResources, Does.Contain("153 mercury").And.Contain("28 wood"));
            Assert.That(purple.Heroes.Select(h => h.Name), Does.Contain("Christian").And.Contain("Maximus"));
            Assert.That(purple.Heroes.Single(h => h.Name == "Maximus").Detail, Is.EqualTo("Knight, level 32"));
            Assert.That(overview.UnownedNote, Is.Not.Empty);
        }

        [Test]
        public void TheOverviewOpensPlayersAndHeroes()
        {
            var purple = this.main.LoadedOverview().Players.Single(p => p.Name == "Purple");

            purple.Heroes.Single(h => h.Name == "Christian").OpenCommand.Execute(null);
            Assert.That((this.main.CurrentPageOf<HeroViewModel>()).Name, Is.EqualTo("Christian"));

            this.main.ShowOverview();
            Assert.That(this.main.CurrentPage, Is.SameAs(this.main.Overview));

            purple.OpenCommand.Execute(null);
            Assert.That((this.main.CurrentPageOf<PlayerViewModel>()).Name, Is.EqualTo("Purple"));
            Assert.That(this.PlayerNode("Purple").IsSelected, Is.True);
        }

        [Test]
        public void TheOverviewShowsGoldEditedOnAPlayerPageWhenItIsShownAgain()
        {
            this.PlayerNode("Purple").IsSelected = true;
            (this.main.CurrentPageOf<PlayerViewModel>()).Resources.Single(r => r.IsGold).Value = 7;

            this.main.ShowOverview();

            Assert.That(this.main.LoadedOverview().Players.Single(p => p.Name == "Purple").Gold, Is.EqualTo("7"));
        }

        [Test]
        public void LoadingAnotherSavegameStartsOnItsOverview()
        {
            this.PlayerNode("Purple").IsSelected = true;

            this.main.LoadGameFromBytes(TestServices.SavefileCodec.Read(SavePath), SavePath);

            Assert.That(this.main.CurrentPage, Is.SameAs(this.main.Overview));
            Assert.That(this.main.PendingChanges, Is.Empty);
        }

        [Test]
        public void TheNavigationFilterFindsPlayersAndShowsTheirHeroesAndTowns()
        {
            this.main.HeroFilter = "purple";

            var visible = this.main.PlayerNodes().Where(n => n.IsVisible).ToList();
            Assert.That(visible.Select(n => n.Title), Is.EqualTo(new[] { "Purple" }));
            Assert.That(visible[0].Descendants().All(c => c.IsVisible), Is.True);
        }
    }
}
