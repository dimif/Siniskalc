using Siniskalc.Homm3.ViewModels;
using Siniskalc.Wpf.ViewModels;
using Siniskalc.Homm3.Tests.ViewModels;

namespace Siniskalc.Homm3.Tests.Integration.ViewModels
{
    /// <summary>
    /// Tests the tree of the editor: its players, which nodes are open or marked, and which page a node shows.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class PlayerNavigationTests
    {
        private static readonly string SavePath = Path.Combine("TestData", "Hota", "empire_of_the_world_01_m5w4d6.GM1");

        private Homm3EditorViewModel main = null!;

        [SetUp]
        public void LoadGame()
        {
            this.main = TestServices.CreateHomm3Editor();
            this.main.LoadGameFromBytes(TestServices.SavefileCodec.Read(SavePath), SavePath);
        }

        private NavNodeViewModel PlayerNode(string color) =>
            this.main.PlayerNodes().Single(n => n.Title == color && n.Kind == NavNodeKind.Player);

        [Test]
        public void EveryPlayerIsANodeHoldingItsHeroesAndTowns()
        {
            var players = this.main.PlayerNodes().ToList();

            // Neutral heroes and towns belong to no player, so that node holds them but has no page.
            Assert.That(players.Select(g => g.Title), Is.EqualTo(new[] { "Red", "Blue", "Tan", "Green", "Orange", "Purple", "Teal", "Pink", "Neutral" }));
            Assert.That(players.Take(8).Select(g => g.Kind), Has.All.EqualTo(NavNodeKind.Player));
            Assert.That(players.Last().Kind, Is.EqualTo(NavNodeKind.Neutral));
            Assert.That(players.SelectMany(p => p.Children).Select(c => c.Title).Distinct(), Is.SubsetOf(new[] { "Heroes", "Towns", "Mines", "Dwellings" }).And.Contain("Heroes").And.Contain("Towns"));
            Assert.That(players.Take(8).SelectMany(p => p.Children), Has.All.Matches<NavNodeViewModel>(c => c.Kind == NavNodeKind.PlayerTab));
            Assert.That(players.Last().Children, Has.All.Matches<NavNodeViewModel>(c => c.Kind == NavNodeKind.NeutralTab));

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
            Assert.That(this.main.ShowHero("Christian"), Is.True);
            this.main.SelectedHero().GeneralInfoViewModel.PrimarySkills.Attack.Base = 77;

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
        public void ShowingAHeroAfterAPlayerBringsBackTheSamePageOfTheHero()
        {
            Assert.That(this.main.ShowHero("Christian"), Is.True);
            var hero = this.main.CurrentPage;

            this.PlayerNode("Purple").IsSelected = true;
            Assert.That(this.main.CurrentPage, Is.InstanceOf<PlayerViewModel>());

            Assert.That(this.main.ShowHero("Christian"), Is.True);
            Assert.That(this.main.CurrentPage, Is.SameAs(hero));
        }

        [Test]
        public void AHeroThatTheSavegameDoesNotHaveIsNotShown()
        {
            var page = this.main.CurrentPage;

            Assert.That(this.main.ShowHero("Nobody Of That Name"), Is.False);
            Assert.That(this.main.ShowHero(" "), Is.False);
            Assert.That(this.main.CurrentPage, Is.SameAs(page));
        }

        [Test]
        public void TheNavigationFilterFindsPlayersAndShowsTheirHeroesAndTowns()
        {
            this.main.NavigationFilter = "purple";

            var visible = this.main.PlayerNodes().Where(n => n.IsVisible).ToList();
            Assert.That(visible.Select(n => n.Title), Is.EqualTo(new[] { "Purple" }));
            Assert.That(visible[0].Descendants().All(c => c.IsVisible), Is.True);
        }
    }
}
