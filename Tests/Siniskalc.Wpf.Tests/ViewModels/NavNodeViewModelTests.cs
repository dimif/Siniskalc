namespace Siniskalc.Wpf.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests a node of the navigation tree: what it shows, its selection, expansion and marks, and how it filters itself and its children.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class NavNodeViewModelTests
    {
        private static NavNodeViewModel Node(string title, NavNodeKind kind = NavNodeKind.Group, Action<NavNodeViewModel>? onSelected = null, params NavNodeViewModel[] children)
        {
            var node = new NavNodeViewModel(title, "sub", "#FF0000", kind, onSelected: onSelected);
            foreach (var child in children)
            {
                node.Children.Add(child);
            }

            return node;
        }

        private static List<string> Raised(NavNodeViewModel node)
        {
            var raised = new List<string>();
            node.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);
            return raised;
        }

        [Test]
        public void ANodeShowsWhatItWasGivenAndStartsVisibleAndNotSelectedOrExpanded()
        {
            var node = new NavNodeViewModel("Orrin", "Knight", "#0000FF", NavNodeKind.Hero, 4, 2);

            Assert.That((node.Title, node.Subtitle, node.ColorHex, node.Kind, node.Index, node.Tab), Is.EqualTo(("Orrin", "Knight", "#0000FF", NavNodeKind.Hero, 4, 2)));
            Assert.That((node.IsVisible, node.IsSelected, node.IsExpanded, node.HasChanges), Is.EqualTo((true, false, false, false)));
            Assert.That(node.Children, Is.Empty);
        }

        [Test]
        public void ANodeIsAHeroOrACastleByItsKind()
        {
            Assert.That(Node("a", NavNodeKind.Hero).IsHero, Is.True);
            Assert.That(Node("a", NavNodeKind.Hero).IsCastle, Is.False);
            Assert.That(Node("a", NavNodeKind.Castle).IsCastle, Is.True);
            Assert.That(Node("a", NavNodeKind.Player).IsHero, Is.False);
        }

        [Test]
        public void SelectingANodeTellsTheTreeOnceAndDeselectingTellsNothing()
        {
            var selected = new List<NavNodeViewModel>();
            var node = Node("a", onSelected: selected.Add);

            node.IsSelected = true;
            node.IsSelected = true;
            node.IsSelected = false;

            Assert.That(selected, Is.EqualTo(new[] { node }));
        }

        [Test]
        public void ChangingAPropertyRaisesItsNotificationAndSettingTheSameValueRaisesNothing()
        {
            var node = Node("a");
            var raised = Raised(node);

            node.IsExpanded = true;
            node.IsExpanded = true;
            node.IsVisible = false;
            node.IsVisible = false;
            node.IsSelected = true;
            node.IsSelected = true;

            Assert.That(raised, Is.EqualTo(new[] { nameof(NavNodeViewModel.IsExpanded), nameof(NavNodeViewModel.IsVisible), nameof(NavNodeViewModel.IsSelected) }));
        }

        [Test]
        public void TheMarkOfChangesIsADotWhileTheNodeHasChanges()
        {
            var node = Node("a");
            var raised = Raised(node);
            Assert.That(node.ChangesMark, Is.Empty);

            node.HasChanges = true;

            Assert.That(node.ChangesMark, Is.EqualTo("  ●"));
            Assert.That(raised, Is.EqualTo(new[] { nameof(NavNodeViewModel.HasChanges), nameof(NavNodeViewModel.ChangesMark) }));

            node.HasChanges = false;
            Assert.That(node.ChangesMark, Is.Empty);
        }

        [Test]
        public void NotesAreJoinedAfterTheTitleAndOnlyANewListRaisesNotifications()
        {
            var node = Node("a");
            var raised = Raised(node);

            node.Notes = ["garrisoned", "capital"];
            node.Notes = ["garrisoned", "capital"];

            Assert.That(node.NotesText, Is.EqualTo(" · garrisoned · capital"));
            Assert.That(raised, Is.EqualTo(new[] { nameof(NavNodeViewModel.Notes), nameof(NavNodeViewModel.NotesText) }));
        }

        [Test]
        public void NoNotesGiveNoText()
        {
            var node = Node("a");

            node.Notes = ["x"];
            node.Notes = null!;

            Assert.That(node.Notes, Is.Empty);
            Assert.That(node.NotesText, Is.Empty);
        }

        [Test]
        public void BadgesAreKeptAndOnlyADifferentListRaisesANotification()
        {
            var node = Node("a");
            var raised = Raised(node);
            var badge = new NavBadge("capital", "♛", "Capital");

            node.Badges = [badge];
            node.Badges = [new NavBadge("capital", "♛", "Capital")];
            node.Badges = null!;

            Assert.That(node.Badges, Is.Empty);
            Assert.That(raised, Is.EqualTo(new[] { nameof(NavNodeViewModel.Badges), nameof(NavNodeViewModel.Badges) }));
            Assert.That(badge.Tone, Is.EqualTo(NavBadgeTone.Muted));
        }

        [Test]
        public void AnEmptyFilterShowsEveryNode()
        {
            var tree = Node("Players", children: [Node("Red"), Node("Blue")]);

            tree.ApplyFilter("  ");

            Assert.That(tree.Children.Select(c => c.IsVisible), Is.All.True);
            Assert.That(tree.IsVisible, Is.True);
        }

        [Test]
        public void AFilterMatchesTheTitleWhateverTheCaseAndTheSpacesAround()
        {
            var tree = Node("Players", children: [Node("Red"), Node("Blue")]);

            tree.ApplyFilter(" RED ");

            Assert.That(tree.Children.Select(c => (c.Title, c.IsVisible)), Is.EqualTo(new[] { ("Red", true), ("Blue", false) }));
        }

        [Test]
        public void ANodeStaysVisibleWhenAChildMatchesAndIsExpandedSoThatTheMatchShows()
        {
            var tree = Node("Players", children: [Node("Red"), Node("Blue")]);

            tree.ApplyFilter("blue");

            Assert.That((tree.IsVisible, tree.IsExpanded), Is.EqualTo((true, true)));
        }

        [Test]
        public void ANodeWithNoMatchAmongItselfOrItsChildrenIsHiddenAndNotExpanded()
        {
            var tree = Node("Players", children: [Node("Red")]);

            tree.ApplyFilter("zzz");

            Assert.That((tree.IsVisible, tree.IsExpanded, tree.Children[0].IsVisible), Is.EqualTo((false, false, false)));
        }

        [Test]
        public void WhenTheNodeItselfMatchesAllItsChildrenAreShown()
        {
            var tree = Node("Players", children: [Node("Red"), Node("Blue")]);

            tree.ApplyFilter("players");

            Assert.That(tree.Children.Select(c => c.IsVisible), Is.All.True);
        }

        [Test]
        public void ANodeWithNoChildrenThatMatchesDoesNotPassItToItsSiblings()
        {
            var tree = Node("Players", children: [Node("Red"), Node("Redwood")]);

            tree.ApplyFilter("redw");

            Assert.That(tree.Children.Select(c => (c.Title, c.IsVisible)), Is.EqualTo(new[] { ("Red", false), ("Redwood", true) }));
        }

        [Test]
        public void ClearingTheFilterGoesBackToWhatWasExpandedByDefault()
        {
            var open = Node("Players", children: [Node("Red")]);
            open.DefaultExpanded = true;
            var shut = Node("Heroes", children: [Node("Orrin")]);

            open.ApplyFilter("zzz");
            shut.ApplyFilter("orrin");
            open.ApplyFilter(string.Empty);
            shut.ApplyFilter(string.Empty);

            Assert.That((open.IsExpanded, shut.IsExpanded), Is.EqualTo((true, false)));
        }
    }
}
