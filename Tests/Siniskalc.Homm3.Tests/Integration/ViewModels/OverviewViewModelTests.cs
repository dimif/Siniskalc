using Siniskalc.Homm3.Entities;
using Siniskalc.Heroes.Wpf.ViewModels;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.ViewModels;
using Siniskalc.Wpf.ViewModels;
using Siniskalc.Homm3.Tests.ViewModels;

namespace Siniskalc.Homm3.Tests.Integration.ViewModels
{
    /// <summary>
    /// Tests the overview page: what a loaded savegame starts on and what the cards of the players show and open.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class OverviewViewModelTests
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
        public void ALoadedSavegameStartsOnTheOverview()
        {
            Assert.That(this.main.CurrentPage, Is.SameAs(this.main.Overview));
            Assert.That(this.main.NavigationNodes[0].Kind, Is.EqualTo(NavNodeKind.Overview));
            Assert.That(this.main.NavigationNodes[0].Title, Is.EqualTo("Game"));
            Assert.That(this.main.NavigationNodes[0].Subtitle, Is.EqualTo("empire_of_the_world_01_m5w4d6.GM1"));
            Assert.That(this.main.NavigationNodes[0].IsSelected, Is.True);
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
            Assert.That(purple.Heroes.Single(h => h.Name == "Maximus").Detail, Is.EqualTo("Knight, Level 32"));
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
        public void TheOverviewRefreshesWhileShownAndAfterUndoAndRedoAwayFromIt()
        {
            var overview = this.main.LoadedOverview();
            var purple = overview.Players.Single(p => p.Name == "Purple");
            var original = purple.Gold;
            var game = this.main.CurrentGame!;
            var index = game.History.Current.Players().FindByColor(Siniskalc.Homm3.Entities.PlayerColor.Purple);
            var session = game.History.CreatePlayerSession(index);
            session.Update(player => player with { Resources = player.Resources.With(Siniskalc.Heroes.Entities.Resource.Gold, 7) });

            Assert.That(overview.Players.Single(p => p.Name == "Purple").Gold, Is.EqualTo("7"));
            this.main.Undo();
            Assert.That(overview.Players.Single(p => p.Name == "Purple").Gold, Is.EqualTo(original));
            this.main.Redo();
            Assert.That(overview.Players.Single(p => p.Name == "Purple").Gold, Is.EqualTo("7"));

            overview.Players.Single(p => p.Name == "Purple").OpenCommand.Execute(null);
            this.main.Undo();
            this.main.ShowOverview();
            Assert.That(overview.Players.Single(p => p.Name == "Purple").Gold, Is.EqualTo(original));

            overview.Players.Single(p => p.Name == "Purple").OpenCommand.Execute(null);
            this.main.Redo();
            this.main.ShowOverview();
            Assert.That(overview.Players.Single(p => p.Name == "Purple").Gold, Is.EqualTo("7"));
        }

        [Test]
        public void LoadingAnotherSavegameStartsOnItsOverview()
        {
            this.PlayerNode("Purple").IsSelected = true;

            this.main.LoadGameFromBytes(TestServices.SavefileCodec.Read(SavePath), SavePath);

            Assert.That(this.main.CurrentPage, Is.SameAs(this.main.Overview));
            Assert.That(this.main.PendingChanges, Is.Empty);
        }
    }
}
