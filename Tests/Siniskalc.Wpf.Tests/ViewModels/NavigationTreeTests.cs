namespace Siniskalc.Wpf.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests the tree of the pages of a savefile: which node is registered for which record, the filter, revealing a node and the
    /// marks of the nodes that have pending changes.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class NavigationTreeTests
    {
        private static readonly ChunkKind HeroKind = new("Hero", 0);
        private static readonly ChunkKind CastleKind = new("Castle", 1);
        private static readonly ChunkKind PlayerKind = new("Player", 2);

        private readonly List<NavNodeViewModel> selected = [];
        private NavigationTree tree = null!;

        [SetUp]
        public void StartWithAnEmptyTree()
        {
            this.selected.Clear();
            this.tree = new NavigationTree(this.selected.Add, new Dictionary<NavNodeKind, ChunkKind>
            {
                [NavNodeKind.Hero] = HeroKind,
                [NavNodeKind.Castle] = CastleKind,
                [NavNodeKind.Player] = PlayerKind,
            });
        }

        private NavNodeViewModel Node(string title, NavNodeKind kind, int index = -1, int tab = 0) =>
            new(title, string.Empty, "#000000", kind, index, tab, this.tree.OnSelected);

        private static PendingChange Change(ChunkKind kind, int index) =>
            new("owner", new Change(new ChangeArea("General"), "Value", "0", "1"), kind, index, _ => { });

        [Test]
        public void TheOverviewIsTheFirstNodeOfTheTree()
        {
            var overview = this.Node("Overview", NavNodeKind.Overview);

            this.tree.SetOverview(overview);

            Assert.That(this.tree.Overview, Is.SameAs(overview));
            Assert.That(this.tree.Nodes, Is.EqualTo(new[] { overview }));
        }

        [Test]
        public void HeroesAndCastlesAreFoundByTheIndexOfTheirRecord()
        {
            var hero = this.Node("Orrin", NavNodeKind.Hero, 3);
            var castle = this.Node("Haret", NavNodeKind.Castle, 5);
            this.tree.Register(hero);
            this.tree.Register(castle);

            Assert.That(this.tree.HeroNode(3), Is.SameAs(hero));
            Assert.That(this.tree.CastleNode(5), Is.SameAs(castle));
            Assert.That(this.tree.HeroNode(4), Is.Null);
            Assert.That(this.tree.CastleNode(3), Is.Null);
            Assert.That(this.tree.CastleNodes.Select(c => c.Key), Is.EqualTo(new[] { 5 }));
        }

        [Test]
        public void TheMapMinesAndDwellingsAreFoundToo()
        {
            var map = this.Node("Map", NavNodeKind.Map);
            var mine = this.Node("Mine", NavNodeKind.Mine, 2);
            var dwelling = this.Node("Dwelling", NavNodeKind.Dwelling, 7);
            this.tree.Register(map);
            this.tree.Register(mine);
            this.tree.Register(dwelling);

            Assert.That((this.tree.MapNode, this.tree.MineNode(2), this.tree.DwellingNode(7)), Is.EqualTo((map, mine, dwelling)));
            Assert.That((this.tree.MineNode(7), this.tree.DwellingNode(2)), Is.EqualTo(((NavNodeViewModel?)null, (NavNodeViewModel?)null)));
        }

        [Test]
        public void APlayerIsItsOwnFirstTabAndOtherTabsAreFoundByNumber()
        {
            var player = this.Node("Red", NavNodeKind.Player, 1);
            var heroes = this.Node("Heroes", NavNodeKind.PlayerTab, 1, 2);
            this.tree.Register(player);
            this.tree.Register(heroes);

            Assert.That(this.tree.PlayerNode(1), Is.SameAs(player));
            Assert.That(this.tree.PlayerTabNode(1, 0), Is.SameAs(player));
            Assert.That(this.tree.PlayerTabNode(1, 2), Is.SameAs(heroes));
            Assert.That(this.tree.PlayerNodes.Select(p => p.Key), Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void ATabThatHasNoNodeFallsBackToThePlayerAndAPlayerNotInTheTreeHasNone()
        {
            var player = this.Node("Red", NavNodeKind.Player, 1);
            this.tree.Register(player);

            Assert.That(this.tree.PlayerTabNode(1, 9), Is.SameAs(player));
            Assert.That(this.tree.PlayerTabNode(4, 0), Is.Null);
            Assert.That(this.tree.PlayerNode(4), Is.Null);
        }

        [Test]
        public void TheNeutralNodeIsTheOneWithIndexMinusOneAndItsTabsFallBackToIt()
        {
            var neutral = this.Node("Neutral", NavNodeKind.Neutral);
            var mines = this.Node("Mines", NavNodeKind.NeutralTab, -1, 1);
            var ownedMines = this.Node("Red mines", NavNodeKind.NeutralTab, 0, 1);
            this.tree.Register(neutral);
            this.tree.Register(mines);
            this.tree.Register(ownedMines);

            Assert.That(this.tree.NeutralNode, Is.SameAs(neutral));
            Assert.That(this.tree.NeutralTabNode(1), Is.SameAs(mines));
            Assert.That(this.tree.NeutralTabNode(1, 0), Is.SameAs(ownedMines));
            Assert.That(this.tree.NeutralTabNode(5), Is.SameAs(neutral));
        }

        [Test]
        public void ANodeOfAKindTheTreeDoesNotIndexIsNotRegistered()
        {
            this.tree.Register(this.Node("Group", NavNodeKind.Group));

            Assert.That(this.tree.Nodes, Is.Empty);
            Assert.That((this.tree.MapNode, this.tree.NeutralNode), Is.EqualTo(((NavNodeViewModel?)null, (NavNodeViewModel?)null)));
        }

        [Test]
        public void ClearingForgetsEveryNode()
        {
            this.tree.SetOverview(this.Node("Overview", NavNodeKind.Overview));
            this.tree.Register(this.Node("Orrin", NavNodeKind.Hero, 3));
            this.tree.Register(this.Node("Haret", NavNodeKind.Castle, 5));
            this.tree.Register(this.Node("Red", NavNodeKind.Player, 1));
            this.tree.Register(this.Node("Map", NavNodeKind.Map));
            this.tree.Register(this.Node("Mine", NavNodeKind.Mine, 2));
            this.tree.Register(this.Node("Dwelling", NavNodeKind.Dwelling, 7));
            this.tree.Register(this.Node("Neutral", NavNodeKind.Neutral));
            this.tree.Register(this.Node("Mines", NavNodeKind.NeutralTab, -1, 1));

            this.tree.Clear();

            Assert.That(this.tree.Nodes, Is.Empty);
            NavNodeViewModel?[] all = [this.tree.Overview, this.tree.MapNode, this.tree.HeroNode(3), this.tree.CastleNode(5), this.tree.PlayerNode(1), this.tree.MineNode(2), this.tree.DwellingNode(7), this.tree.NeutralNode];
            Assert.That(all, Is.All.Null);
            Assert.That(this.tree.NeutralTabNode(1), Is.Null);
        }

        [Test]
        public void TheFilterIsAppliedToEveryNodeOfTheTree()
        {
            var group = this.Node("Heroes", NavNodeKind.Group);
            group.Children.Add(this.Node("Orrin", NavNodeKind.Hero, 0));
            group.Children.Add(this.Node("Ufretin", NavNodeKind.Hero, 1));
            this.tree.Nodes.Add(group);

            this.tree.ApplyFilter("ufre");

            Assert.That(group.Children.Select(c => c.IsVisible), Is.EqualTo(new[] { false, true }));
        }

        [Test]
        public void ReapplyingTheFilterRunsItAgainOnNodesAddedSinceAndDoesNothingWhenThereIsNone()
        {
            var group = this.Node("Heroes", NavNodeKind.Group);
            this.tree.Nodes.Add(group);
            this.tree.ApplyFilter("ufre");
            var late = this.Node("Orrin", NavNodeKind.Hero, 0);
            group.Children.Add(late);

            this.tree.ReapplyFilter();

            Assert.That(late.IsVisible, Is.False);

            this.tree.ApplyFilter(string.Empty);
            late.IsVisible = false;
            this.tree.ReapplyFilter();

            Assert.That(late.IsVisible, Is.False, "an empty filter is not applied again");
        }

        [Test]
        public void ANullFilterIsTheSameAsNone()
        {
            var node = this.Node("Orrin", NavNodeKind.Hero, 0);
            this.tree.Nodes.Add(node);

            this.tree.ApplyFilter(null!);

            Assert.That(node.IsVisible, Is.True);
            Assert.That(() => this.tree.ReapplyFilter(), Throws.Nothing);
        }

        [Test]
        public void RevealingANodeExpandsItsAncestorsAndSelectsItAndTellsTheTree()
        {
            var group = this.Node("Players", NavNodeKind.Group);
            var player = this.Node("Red", NavNodeKind.Player, 0);
            var hero = this.Node("Orrin", NavNodeKind.Hero, 0);
            group.Children.Add(player);
            player.Children.Add(hero);
            this.tree.Nodes.Add(group);

            this.tree.Reveal(hero);

            Assert.That((group.IsExpanded, player.IsExpanded, hero.IsSelected), Is.EqualTo((true, true, true)));
            Assert.That(this.selected, Is.EqualTo(new[] { hero }));
        }

        [Test]
        public void RevealingATopNodeSelectsItAndExpandsNothingElse()
        {
            var top = this.Node("Overview", NavNodeKind.Overview);
            var other = this.Node("Other", NavNodeKind.Group);
            other.Children.Add(this.Node("Child", NavNodeKind.Hero, 0));
            this.tree.Nodes.Add(top);
            this.tree.Nodes.Add(other);

            this.tree.Reveal(top);

            Assert.That((top.IsSelected, other.IsExpanded), Is.EqualTo((true, false)));
        }

        [Test]
        public void RevealingNothingDoesNothing()
        {
            Assert.That(() => this.tree.Reveal(null), Throws.Nothing);
            Assert.That(this.selected, Is.Empty);
        }

        [Test]
        public void ANodeWhoseRecordHasAChangeIsMarkedAndSoAreTheNodesAboveIt()
        {
            var group = this.Node("Heroes", NavNodeKind.Group);
            var changed = this.Node("Orrin", NavNodeKind.Hero, 3);
            var untouched = this.Node("Ufretin", NavNodeKind.Hero, 4);
            group.Children.Add(changed);
            group.Children.Add(untouched);
            this.tree.Nodes.Add(group);

            this.tree.MarkChanges([Change(HeroKind, 3)]);

            Assert.That((changed.HasChanges, untouched.HasChanges, group.HasChanges), Is.EqualTo((true, false, true)));
        }

        [Test]
        public void AChangeOfAnotherKindWithTheSameIndexMarksNothing()
        {
            var hero = this.Node("Orrin", NavNodeKind.Hero, 3);
            this.tree.Nodes.Add(hero);

            this.tree.MarkChanges([Change(CastleKind, 3)]);

            Assert.That(hero.HasChanges, Is.False);
        }

        [Test]
        public void MarksAreTakenOffWhenTheChangesAreGone()
        {
            var hero = this.Node("Orrin", NavNodeKind.Hero, 3);
            this.tree.Nodes.Add(hero);
            this.tree.MarkChanges([Change(HeroKind, 3)]);

            this.tree.MarkChanges([]);

            Assert.That(hero.HasChanges, Is.False);
        }

        [Test]
        public void ANodeOfAKindWithNoChunkKindHasNoChangesOfItsOwn()
        {
            var group = this.Node("Heroes", NavNodeKind.Group, 3);
            this.tree.Nodes.Add(group);

            this.tree.MarkChanges([Change(HeroKind, 3)]);

            Assert.That(group.HasChanges, Is.False);
        }
    }
}
