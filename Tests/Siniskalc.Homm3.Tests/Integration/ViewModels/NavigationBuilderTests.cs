namespace Siniskalc.Homm3.Tests.Integration.ViewModels
{
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.ViewModels;
    using Siniskalc.Wpf.ViewModels;
    using Siniskalc.Homm3.Tests.ViewModels;

    /// <summary>
    /// The mines and dwellings in the navigation tree of a HoMM3 game (<c>TestData/Hota/Mines</c>).
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class NavigationBuilderTests
    {
        private static readonly string SavePath = Path.Combine("TestData", "Hota", "Mines", "mines_08_red_abandoned_mine_sulfur.GM2");

        private Homm3EditorViewModel main = null!;

        [SetUp]
        public void LoadGame()
        {
            this.main = TestServices.CreateHomm3Editor();
            this.main.LoadGameFromBytes(TestServices.SavefileCodec.Read(SavePath), SavePath);
        }

        [Test]
        public void TheMinesOfAnOwnerAreGroupedByResourceBelowItsMinesNode()
        {
            var mines = this.main.PlayerNodes().Single(n => n.Title == "Red").Children.Single(c => c.Title == "Mines");

            Assert.That(mines.Kind, Is.EqualTo(NavNodeKind.PlayerTab));
            Assert.That(mines.Tab, Is.EqualTo(OwnerTabs.Mines));
            Assert.That(mines.Children, Is.Not.Empty.And.All.Matches<NavNodeViewModel>(g => g.Kind == NavNodeKind.OwnerSubGroup));
            Assert.That(mines.Children.Select(g => g.Title), Is.Ordered.Using<string>((a, b) => ResourceOrder(a).CompareTo(ResourceOrder(b))));
            Assert.That(mines.Children.SelectMany(g => g.Children), Has.All.Matches<NavNodeViewModel>(m => m.Kind == NavNodeKind.Mine && m.Title.StartsWith("At ", StringComparison.Ordinal)));
            Assert.That(mines.Subtitle, Is.EqualTo(mines.Children.SelectMany(g => g.Children).Count().ToString()));
        }

        [Test]
        public void EveryMineIsOnceInTheTreeUnderItsOwner()
        {
            var game = this.main.PlayerNodes();
            var leaves = game.Descendants().Where(n => n.Kind == NavNodeKind.Mine).Select(n => n.Index).ToList();

            Assert.That(leaves, Has.Count.EqualTo(18));
            Assert.That(leaves.Distinct().Count(), Is.EqualTo(18));
            Assert.That(game.Single(n => n.Title == "Neutral").Descendants().Count(n => n.Kind == NavNodeKind.Mine), Is.EqualTo(10));
        }

        [Test]
        public void TheDwellingsOfAnOwnerAreGroupedByFactionWithNeutralLastAndUnknownAtTheEnd()
        {
            var neutral = this.main.PlayerNodes().Single(n => n.Title == "Neutral");
            var dwellings = neutral.Children.SingleOrDefault(c => c.Title == "Dwellings");
            if (dwellings == null)
            {
                Assert.Ignore("The save has no dwellings.");
            }

            var titles = dwellings!.Children.Select(g => g.Title).ToList();
            Assert.That(titles, Is.Unique);
            Assert.That(dwellings.Children.SelectMany(g => g.Children), Has.All.Matches<NavNodeViewModel>(d => d.Kind == NavNodeKind.Dwelling));
            Assert.That(titles.IndexOf("Neutral"), Is.EqualTo(titles.Contains("Unknown") ? titles.Count - 2 : titles.Count - 1).Or.EqualTo(-1));
        }

        [Test]
        public void PickingAMineSelectsOnlyThatMineAndNotTheMinesNodeOfItsOwner()
        {
            var red = this.main.PlayerNodes().Single(n => n.Title == "Red");
            var mine = red.Descendants().First(n => n.Kind == NavNodeKind.Mine);

            mine.IsSelected = true;

            Assert.That(this.main.NavigationNodes.Descendants().Where(n => n.IsSelected), Is.EqualTo(new[] { mine }));
            Assert.That(this.main.CurrentPageOf<PlayerViewModel>().SelectedTab, Is.EqualTo(OwnerTabs.Mines));
        }

        [Test]
        public void PickingAMineOfBlueInTheSaveWhoseBlueStructHasAZeroFlagShowsItOnBluesPlayerPage()
        {
            var main = LoadFlaggedByBlue();
            var mine = main.PlayerNodes().Single(n => n.Title == "Blue").Descendants().First(n => n.Kind == NavNodeKind.Mine);

            mine.IsSelected = true;

            var page = main.CurrentPageOf<PlayerViewModel>();
            Assert.That(page.Name, Is.EqualTo("Blue"));
            Assert.That(page.SelectedTab, Is.EqualTo(OwnerTabs.Mines));
            Assert.That(page.MapObjects.Mines.SelectedMine?.Index, Is.EqualTo(mine.Index));
            Assert.That(main.NavigationNodes.Descendants().Where(n => n.IsSelected), Is.EqualTo(new[] { mine }));
        }

        [Test]
        public void TheNodesOfRedAndBlueOfTheSaveWithAZeroFlagOpenTheirPlayerPagesAndNotTheNeutralOne()
        {
            var main = LoadFlaggedByBlue();

            foreach (var name in new[] { "Red", "Blue" })
            {
                main.PlayerNodes().Single(n => n.Title == name).IsSelected = true;

                Assert.That(main.CurrentPageOf<PlayerViewModel>().Name, Is.EqualTo(name));
            }

            Assert.That(main.CurrentPageOf<PlayerViewModel>().MapObjects.Mines.Rows.Select(m => m.OwnerSort), Is.All.EqualTo("Blue"));
        }

        private static Homm3EditorViewModel LoadFlaggedByBlue()
        {
            var path = Path.Combine("TestData", "Hota", "Mines", "mines_20_flagged_by_colours_with_no_player.GM2");
            var main = TestServices.CreateHomm3Editor();
            main.LoadGameFromBytes(TestServices.SavefileCodec.Read(path), path);
            return main;
        }

        [Test]
        public void TheNeutralNodeOpensThePageOfWhatNobodyOwns()
        {
            var neutral = this.main.PlayerNodes().Single(n => n.Title == "Neutral");
            Assert.That(neutral.Kind, Is.EqualTo(NavNodeKind.Neutral));

            neutral.IsSelected = true;

            var page = this.main.CurrentPageOf<NeutralViewModel>();
            Assert.That(page.SelectedTab, Is.EqualTo(NeutralTabs.Heroes));
            Assert.That(page.MapObjects.Mines.Rows, Has.Count.EqualTo(10));
            Assert.That(page.Summary, Does.Contain("10 mines"));
        }

        [Test]
        public void PickingANeutralMineShowsTheMinesTabOfThePageWithThatMinePicked()
        {
            var mine = this.main.PlayerNodes().Single(n => n.Title == "Neutral").Descendants().First(n => n.Kind == NavNodeKind.Mine);

            mine.IsSelected = true;

            var page = this.main.CurrentPageOf<NeutralViewModel>();
            Assert.That(page.SelectedTab, Is.EqualTo(NeutralTabs.Mines));
            Assert.That(page.MapObjects.Mines.SelectedMine?.Index, Is.EqualTo(mine.Index));
        }

        [Test]
        public void AMineGivenToAPlayerLeavesTheNeutralListAndJoinsThePlayers()
        {
            this.main.PlayerNodes().Single(n => n.Title == "Neutral").IsSelected = true;
            var page = this.main.CurrentPageOf<NeutralViewModel>();
            var row = page.MapObjects.Mines.Rows[0];

            row.OwnerField.Selected = "Blue";

            Assert.That(page.MapObjects.Mines.Rows, Has.Count.EqualTo(9));
            Assert.That(page.MapObjects.Mines.Rows.Select(m => m.Index), Does.Not.Contain(row.Index));
        }

        [Test]
        public void ChangingTheOwnerOfAMineChangesTheStructureKey()
        {
            var builder = new NavigationBuilder(new BanishingTextProvider());
            var game = TestServices.GameService.LoadGameFromFile(SavePath);
            var before = builder.StructureKey(game);

            game.History.CreateMineSession(0).Update(m => m with { Owner = m.Owner == Siniskalc.Homm3.Entities.PlayerColor.Blue ? Siniskalc.Homm3.Entities.PlayerColor.Red : Siniskalc.Homm3.Entities.PlayerColor.Blue });

            Assert.That(builder.StructureKey(game), Is.Not.EqualTo(before));
        }

        private static int ResourceOrder(string resource) => (int)Enum.Parse<Siniskalc.Heroes.Entities.Resource>(resource);
    }
}
