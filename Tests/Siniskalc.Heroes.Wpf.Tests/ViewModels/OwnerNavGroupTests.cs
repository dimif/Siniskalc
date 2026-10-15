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
        public void TheGameMarksEachTownNode()
        {
            var marked = new List<int>();

            OwnerNavGroup.Create(NewTree(), "Red", "#FF0000", 0, true, string.Empty, [], Castles, (node, index) => marked.Add(index));

            Assert.That(marked, Is.EqualTo(new[] { 5, 6 }));
        }
    }
}
