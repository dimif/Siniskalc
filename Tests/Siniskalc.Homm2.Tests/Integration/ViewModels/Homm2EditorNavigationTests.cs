namespace Siniskalc.Homm2.Tests.Integration.ViewModels
{
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests what the tree of the editor holds and how a node opens its page, on a Broken Alliance savegame loaded into the editor.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class Homm2EditorNavigationTests
    {
        private Homm2EditorHarness harness = null!;
        private Homm2EditorViewModel editor = null!;

        [SetUp]
        public void LoadASavegame()
        {
            this.harness = new Homm2EditorHarness();
            this.editor = this.harness.Editor;
        }

        [TearDown]
        public void DeleteFolder() => this.harness.Dispose();

        [Test]
        public void ALoadedSavegameShowsTheOverviewAndSelectsTheGameNode()
        {
            Assert.That(this.editor.CurrentPage, Is.SameAs(this.editor.Overview));
            Assert.That(this.editor.NavigationNodes, Has.Count.EqualTo(1));
            Assert.That(this.editor.NavigationNodes[0].Title, Is.EqualTo("Game"));
            Assert.That(this.editor.NavigationNodes[0].IsSelected, Is.True);
        }

        [Test]
        public void TheGameHoldsAPlayerForEachColourInTheGameWithItsHeroesAndTowns()
        {
            var document = this.editor.CurrentGame!.History.Current;
            var game = this.editor.NavigationNodes[0];

            var players = game.Children.Where(n => n.Kind == NavNodeKind.Player).ToList();

            Assert.That(players.Select(p => p.Title), Is.EqualTo(this.editor.Overview!.Players.Select(p => p.Name)));
            foreach (var player in players)
            {
                var color = PlayerColor.All[player.Index];
                var heroes = document.Heroes().GetModels().Where(h => h.Owner == color).Select(h => h.Name).Order();
                var towns = document.Castles().GetModels().Where(c => c.IsUsed && c.Owner == color).Select(c => c.Name).Order();
                Assert.That(Titles(player, "Heroes"), Is.EqualTo(heroes), color.ToString());
                Assert.That(Titles(player, "Towns"), Is.EqualTo(towns), color.ToString());
            }
        }

        [Test]
        public void ThePlayerCountdownNoteFollowsEditsUndoAndRedoInBrokenAllianceWithoutACastle()
        {
            this.editor.LoadGame(Saves.Path(Saves.BrokenAlliance, "broken_alliance_21_blue_has_no_castle"));
            var blue = this.editor.NavigationNodes[0].Children.Single(n => n.Kind == NavNodeKind.Player && n.Index == PlayerColor.Blue.Code);
            Assert.That(blue.Subtitle, Is.EqualTo("2 heroes · 8 mines"));
            Assert.That(blue.Notes, Is.EqualTo(new[] { "7 days left" }));
            blue.IsSelected = true;
            var page = (PlayerViewModel)this.editor.CurrentPage!;

            page.DaysLeftCard.DayChoices.Single(c => c.Days == 3).SelectCommand.Execute(null);

            Assert.That(blue.Notes, Is.EqualTo(new[] { "3 days left" }));

            this.editor.CurrentGame!.History.Undo();

            Assert.That(blue.Notes, Is.EqualTo(new[] { "7 days left" }));

            this.editor.CurrentGame.History.Redo();

            Assert.That(blue.Notes, Is.EqualTo(new[] { "3 days left" }));
            Assert.That(this.editor.NavigationNodes[0].Children.Single(n => n.Kind == NavNodeKind.Player && n.Index == PlayerColor.Blue.Code), Is.SameAs(blue));
        }

        [Test]
        public void TheMapNodeOpensTheBytesOfTheEasySmallMapWithoutShrinesOrSphinxes()
        {
            this.editor.LoadGame(Saves.Path(Saves.MapDifficultySize, "map_difficulty_size_06_map_easy_small"));
            var document = this.editor.CurrentGame!.History.Current;
            Assert.That(document.Shrines().Count, Is.Zero);
            Assert.That(document.Sphinxes().Count, Is.Zero);
            var map = this.editor.NavigationNodes[0].Children.Single(n => n.Kind == NavNodeKind.Map);

            map.IsSelected = true;

            var page = (MapViewModel)this.editor.CurrentPage!;
            Assert.That(page.Shrines, Is.Empty);
            Assert.That(page.Sphinxes, Is.Empty);
            Assert.That(map.IsSelected, Is.True);
            page.SelectedTab = 2;
            Assert.That(page.SelectedTab, Is.EqualTo(2));
            Assert.That(page.ByteRecords.Records, Has.Count.EqualTo(document.Mines().Count + 3));
            Assert.That(page.ByteRecords.Records.Select(r => r.Name), Does.Contain("Obelisks").And.Contain(document.Turn().GetName(0)).And.Contain(document.BuiltToday().GetName(0)));

            foreach (var record in page.ByteRecords.Records)
            {
                page.ByteRecords.SelectedRecord = record;
                using var target = page.ByteRecords.CreateTarget();
                Assert.That(target, Is.Not.Null, record.Name);
                Assert.That(target!.Title, Is.EqualTo(record.Name));
            }
        }

        [Test]
        public void PickingAHeroNodeShowsThePageOfThatHero()
        {
            var hero = Descendants(this.editor.NavigationNodes[0]).First(n => n.Kind == NavNodeKind.Hero);

            hero.IsSelected = true;

            var page = (HeroViewModel)this.editor.CurrentPage!;
            Assert.That(page.HeroId, Is.EqualTo(hero.Index));
            Assert.That(page.Name, Is.EqualTo(hero.Title));
        }

        [Test]
        public void PickingATownNodeShowsThePageOfThatTown()
        {
            var town = Descendants(this.editor.NavigationNodes[0]).First(n => n.Kind == NavNodeKind.Castle);

            town.IsSelected = true;

            var page = (CastleViewModel)this.editor.CurrentPage!;
            Assert.That(page.CastleIndex, Is.EqualTo(town.Index));
            Assert.That(page.Name, Is.EqualTo(town.Title));
        }

        [Test]
        public void ATownNodeHasTheCastleBadgeExactlyWhenItsCastleIsBuilt()
        {
            var castles = this.editor.CurrentGame!.History.Current.Castles().GetModels();
            var towns = Descendants(this.editor.NavigationNodes[0]).Where(n => n.Kind == NavNodeKind.Castle).ToList();

            Assert.That(towns.Select(n => castles[n.Index].HasCastle).Distinct().Count(), Is.EqualTo(2), "the save has castles and towns");
            foreach (var town in towns)
            {
                Assert.That(town.Badges.Any(b => b.Key == NavigationBuilder.CastleBadge), Is.EqualTo(castles[town.Index].HasCastle), town.Title);
            }
        }

        [Test]
        public void BuildingTheCastleOfATownGivesItsNodeTheCastleBadge()
        {
            var history = this.editor.CurrentGame!.History;
            var town = Descendants(this.editor.NavigationNodes[0]).First(n => n.Kind == NavNodeKind.Castle && !history.Current.Castles().GetModel(n.Index).HasCastle);

            history.CreateCastleSession(town.Index).Update(c => c with { Town = (c.Town & ~TownBuildings.TownOnly) | TownBuildings.Castle });

            Assert.That(town.Badges.Single(b => b.Key == NavigationBuilder.CastleBadge).Tooltip, Is.EqualTo("Castle"));
        }

        [Test]
        public void ATownNodeWithAHeroInItHasTheVisitorBadgeAndNote()
        {
            var game = this.editor.CurrentGame!;
            var heroes = game.History.Current.Heroes().GetModels();
            var castles = game.History.Current.Castles().GetModels();
            var town = Descendants(this.editor.NavigationNodes[0]).First(n => n.Kind == NavNodeKind.Castle);
            var visitor = castles[town.Index].VisitingHeroId;
            var hero = visitor ?? 0;

            game.History.CreateCastleSession(town.Index).Update(c => c with { VisitingHeroId = hero });

            Assert.That(town.Badges.Single(b => b.Key == NavigationBuilder.VisitorBadge).Tooltip, Is.EqualTo($"Visiting: {heroes[hero].Name}"));
            Assert.That(town.Notes, Is.EqualTo(new[] { heroes[hero].Name }));
        }

        [Test]
        public void PickingAMineOfAPlayerShowsTheMinesTabOfThePlayerWithThatMinePicked()
        {
            var player = this.editor.NavigationNodes[0].Children.FirstOrDefault(n => n.Kind == NavNodeKind.Player && Descendants(n).Any(m => m.Kind == NavNodeKind.Mine));
            Assert.That(player, Is.Not.Null, "a player of the savegame owns a mine");

            var mine = Descendants(player!).First(n => n.Kind == NavNodeKind.Mine);
            Assert.That(Descendants(player!).Where(n => n.Kind == NavNodeKind.OwnerSubGroup).Select(n => n.Tab), Has.All.EqualTo(OwnerTabs.Mines));

            mine.IsSelected = true;

            var page = (PlayerViewModel)this.editor.CurrentPage!;
            Assert.That(page.Color, Is.EqualTo(PlayerColor.All[player!.Index]));
            Assert.That(page.SelectedTab, Is.EqualTo(OwnerTabs.Mines));
            Assert.That(page.Mines.SelectedMine?.Index, Is.EqualTo(mine.Index));
        }

        [Test]
        public void TheNeutralNodeOpensThePageOfWhatNobodyOwnsAndItsMinesOpenTheMinesTab()
        {
            var neutral = this.editor.NavigationNodes[0].Children.Single(n => n.Title == "Neutral");
            Assert.That(neutral.Kind, Is.EqualTo(NavNodeKind.Neutral));

            neutral.IsSelected = true;

            var page = (NeutralViewModel)this.editor.CurrentPage!;
            Assert.That(page.SelectedTab, Is.EqualTo(NeutralTabs.Heroes));

            var mine = Descendants(neutral).FirstOrDefault(n => n.Kind == NavNodeKind.Mine);
            Assert.That(mine, Is.Not.Null, "the savegame has a mine nobody owns");

            mine!.IsSelected = true;

            Assert.That(page.SelectedTab, Is.EqualTo(NeutralTabs.Mines));
            Assert.That(page.Mines.SelectedMine?.Index, Is.EqualTo(mine.Index));
        }

        [Test]
        public void PickingTheHeroesOfAPlayerShowsThePlayerOnTheTabOfHeroes()
        {
            var tab = Descendants(this.editor.NavigationNodes[0]).First(n => n.Kind == NavNodeKind.PlayerTab && n.Tab == 1);

            tab.IsSelected = true;

            var page = (PlayerViewModel)this.editor.CurrentPage!;
            Assert.That(page.Color, Is.EqualTo(PlayerColor.All[tab.Index]));
            Assert.That(page.SelectedTab, Is.EqualTo(1));
        }

        [Test]
        public void OpeningAHeroFromAPlayersPageSelectsItsNodeInTheTree()
        {
            this.editor.ShowPlayer(PlayerColor.Blue, 1);
            var player = (PlayerViewModel)this.editor.CurrentPage!;

            player.Heroes[0].OpenCommand.Execute(null);

            var hero = (HeroViewModel)this.editor.CurrentPage!;
            Assert.That(Descendants(this.editor.NavigationNodes[0]).Single(n => n.Kind == NavNodeKind.Hero && n.Index == hero.HeroId).IsSelected, Is.True);
        }

        [Test]
        public void TheFilterNarrowsTheTreeToWhatMatches()
        {
            var hero = Descendants(this.editor.NavigationNodes[0]).First(n => n.Kind == NavNodeKind.Hero);

            this.editor.NavigationFilter = hero.Title;

            var visible = Descendants(this.editor.NavigationNodes[0]).Where(n => n.IsVisible && n.Kind == NavNodeKind.Hero).ToList();
            Assert.That(visible.Select(n => n.Title), Does.Contain(hero.Title));
            Assert.That(visible.Count, Is.LessThan(Descendants(this.editor.NavigationNodes[0]).Count(n => n.Kind == NavNodeKind.Hero)));

            this.editor.NavigationFilter = string.Empty;
            Assert.That(Descendants(this.editor.NavigationNodes[0]).All(n => n.IsVisible), Is.True);
        }

        [Test]
        public void AnEditedHeroIsMarkedInTheTreeAndOnItsPageAndResetPutsItBack()
        {
            var node = Descendants(this.editor.NavigationNodes[0]).First(n => n.Kind == NavNodeKind.Hero && n.Subtitle.Length > 0);
            this.editor.ShowHero(node.Index);
            var page = (HeroViewModel)this.editor.CurrentPage!;
            var experience = this.editor.CurrentGame!.History.Current.Heroes().GetModel(node.Index).HeroLevel.Experience;

            this.editor.CurrentGame!.History.CreateHeroSession(node.Index).Update(h => h with { HeroLevel = h.HeroLevel with { Experience = experience + 1000 } });

            Assert.That(node.HasChanges, Is.True);
            Assert.That(this.editor.NavigationNodes[0].HasChanges, Is.True, "the nodes above the hero are marked");
            Assert.That(page.HasPendingChanges, Is.True);
            Assert.That(page.LevelAndClass.Experience, Is.EqualTo(experience + 1000));

            page.ResetCommand.Execute(null);

            Assert.That(node.HasChanges, Is.False);
            Assert.That(page.HasPendingChanges, Is.False);
            Assert.That(page.LevelAndClass.Experience, Is.EqualTo(experience));
        }

        [Test]
        public void AHeroThatChangesHandsMovesToTheGroupOfItsNewOwner()
        {
            var document = this.editor.CurrentGame!.History.Current;
            var hero = document.Heroes().GetModels().First(h => h.Owner == PlayerColor.Blue);

            this.editor.CurrentGame.History.CreateHeroSession(hero.PortraitId).Update(h => h with { Owner = PlayerColor.Green });

            var owner = Descendants(this.editor.NavigationNodes[0]).Single(n => n.Kind == NavNodeKind.Player && n.Index == PlayerColor.Green.Code);
            Assert.That(Titles(owner, "Heroes"), Does.Contain(hero.Name));
        }

        [Test]
        public void ALoadedGameWithoutAHeroOrATownOfAPlayerSaysSo()
        {
            var document = this.editor.CurrentGame!.History.Current;
            var color = PlayerColor.All.First(c => document.Heroes().GetModels().All(h => h.Owner != c) && document.Castles().GetModels().All(t => t.Owner != c));
            var player = new PlayerViewModel(this.editor.CurrentGame!, color, TestServices.BanishingText, TestServices.MineGuardianText, _ => { }, _ => { }, TestServices.ByteEditTargets);

            Assert.That(player.HasHeroes, Is.False);
            Assert.That(player.HasCastles, Is.False);
            Assert.That(player.Resources.Select(r => r.Name), Is.EqualTo(new[] { "Gold", "Wood", "Mercury", "Ore", "Sulfur", "Crystal", "Gems" }));
        }

        private static IEnumerable<string> Titles(NavNodeViewModel player, string group) =>
            (player.Children.FirstOrDefault(c => c.Title == group)?.Children.Select(c => c.Title) ?? []).Order();

        private static IEnumerable<NavNodeViewModel> Descendants(NavNodeViewModel node) =>
            node.Children.SelectMany(c => Descendants(c).Prepend(c));
    }
}
