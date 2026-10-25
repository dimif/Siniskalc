namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    [TestFixture]
    internal sealed class OwnerNavGroupTests
    {
        private static readonly NavEntry[] Heroes = [new(3, "Orrin", "Knight, level 2")];
        private static readonly NavEntry[] Castles = [new(5, "Gryphon", "Castle"), new(6, "Citadel", "Stronghold")];

        private static NavigationTree NewTree() => new(_ => { }, new Dictionary<NavNodeKind, ChunkKind>());

        [Test]
        public void APlayerHasItsPageAndAGroupOfHeroesAndOneOfTowns()
        {
            var tree = NewTree();

            var node = OwnerNavGroup.Create(tree, "Red", "#FF0000", 0, true, "3 days left", Heroes, Castles);

            Assert.That(node.Kind, Is.EqualTo(NavNodeKind.Player));
            Assert.That(node.Subtitle, Is.EqualTo("3 days left · 1 hero · 2 towns"));
            Assert.That(node.DefaultExpanded, Is.True);
            Assert.That(node.Children.Select(c => (c.Title, c.Kind, c.Tab)), Is.EqualTo(new[] { ("Heroes", NavNodeKind.PlayerTab, 1), ("Towns", NavNodeKind.PlayerTab, 2) }));
            Assert.That(tree.HeroNode(3)?.Title, Is.EqualTo("Orrin"));
            Assert.That(tree.CastleNode(6)?.Subtitle, Is.EqualTo("Stronghold"));
        }

        [Test]
        public void WhatNobodyOwnsIsAPlainGroupThatStartsClosed()
        {
            var node = OwnerNavGroup.Create(NewTree(), "Neutral", "#888888", -1, false, string.Empty, Heroes, []);

            Assert.That(node.Kind, Is.EqualTo(NavNodeKind.Group));
            Assert.That(node.DefaultExpanded, Is.False);
            Assert.That(node.Children.Single().Kind, Is.EqualTo(NavNodeKind.Group));
        }

        [Test]
        public void TheMinesOfAPlayerAreInOneGroupForEachResourceBelowItsMinesGroup()
        {
            var tree = NewTree();
            NavGroup[] mines = [new("Wood", [new(0, "Sawmill at 1, 2", string.Empty)]), new("Gold", [new(4, "Gold mine at 3, 4", string.Empty), new(7, "Gold mine at 5, 6", string.Empty)]), new("Ore", [])];

            var node = OwnerNavGroup.Create(tree, "Red", "#FF0000", 0, true, string.Empty, [], [], mines: mines);

            var section = node.Children.Single();
            Assert.That((section.Title, section.Subtitle, section.Kind, section.Tab), Is.EqualTo(("Mines", "3", NavNodeKind.PlayerTab, OwnerTabs.Mines)));
            Assert.That(section.Children.Select(g => (g.Title, g.Subtitle, g.Kind, g.Tab)), Is.EqualTo(new[] { ("Wood", "1", NavNodeKind.OwnerSubGroup, OwnerTabs.Mines), ("Gold", "2", NavNodeKind.OwnerSubGroup, OwnerTabs.Mines) }));
            Assert.That(section.Children[1].Children.Select(m => (m.Kind, m.Index)), Is.EqualTo(new[] { (NavNodeKind.Mine, 4), (NavNodeKind.Mine, 7) }));
            Assert.That(node.Subtitle, Is.EqualTo("3 mines"));
            Assert.That(tree.MineNode(7)?.Title, Is.EqualTo("Gold mine at 5, 6"));
            Assert.That(tree.PlayerTabNode(0, OwnerTabs.Mines), Is.SameAs(section));
            Assert.That(section.Children.Select(g => g.IsExpanded), Has.All.False);
        }

        [Test]
        public void TheDwellingsOfWhatNobodyOwnsAreGroupedToo()
        {
            var tree = NewTree();

            var node = OwnerNavGroup.Create(tree, "Neutral", "#888888", -1, false, string.Empty, [], [], dwellings: [new("Castle", [new(2, "Pikeman dwelling at 1, 1", string.Empty)])]);

            var section = node.Children.Single();
            Assert.That((section.Title, section.Kind, section.Tab), Is.EqualTo(("Dwellings", NavNodeKind.Group, OwnerTabs.Dwellings)));
            Assert.That(section.Children.Single().Kind, Is.EqualTo(NavNodeKind.Group));
            Assert.That(tree.DwellingNode(2)?.Kind, Is.EqualTo(NavNodeKind.Dwelling));
            Assert.That(node.Subtitle, Is.EqualTo("1 dwelling"));
        }

        [Test]
        public void AnOwnerWithoutMinesOrDwellingsHasNoSectionForThem()
        {
            var node = OwnerNavGroup.Create(NewTree(), "Blue", "#0000FF", 1, true, string.Empty, Heroes, [], mines: [new("Gold", [])], dwellings: []);

            Assert.That(node.Children.Select(c => c.Title), Is.EqualTo(new[] { "Heroes" }));
        }

        [Test]
        public void WhatNobodyOwnsCanHaveAPageOfItsOwnWhoseTabsStartAtTheHeroes()
        {
            var tree = NewTree();

            var node = OwnerNavGroup.Create(tree, "Neutral", "#888888", -1, false, string.Empty, Heroes, Castles, mines: [new("Gold", [new(4, "Gold mine at 3, 4", string.Empty)])], neutralPage: true);

            Assert.That(node.Kind, Is.EqualTo(NavNodeKind.Neutral));
            Assert.That(node.Children.Select(c => (c.Title, c.Kind, c.Tab)), Is.EqualTo(new[]
            {
                ("Heroes", NavNodeKind.NeutralTab, NeutralTabs.Heroes),
                ("Towns", NavNodeKind.NeutralTab, NeutralTabs.Towns),
                ("Mines", NavNodeKind.NeutralTab, NeutralTabs.Mines),
            }));
            Assert.That(node.Children[2].Children.Single().Kind, Is.EqualTo(NavNodeKind.OwnerSubGroup));
            Assert.That(tree.NeutralNode, Is.SameAs(node));
            Assert.That(tree.NeutralTabNode(NeutralTabs.Mines), Is.SameAs(node.Children[2]));
            Assert.That(tree.NeutralTabNode(NeutralTabs.Dwellings), Is.SameAs(node), "a tab with no group falls back to the node");
        }

        [Test]
        public void TheGameMarksEachTownNode()
        {
            var marked = new List<int>();

            OwnerNavGroup.Create(NewTree(), "Red", "#FF0000", 0, true, string.Empty, [], Castles, (node, index) => marked.Add(index));

            Assert.That(marked, Is.EqualTo(new[] { 5, 6 }));
        }
    }
}
