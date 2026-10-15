namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Builds the node of one owner in the navigation tree of any game: the player's page (or a plain group for what nobody owns, or
    /// a colour with no data of its own), with its "Heroes" group and its "Towns" group.
    /// </summary>
    public static class OwnerNavGroup
    {
        /// <summary>
        /// Adds the node of an owner and the nodes under it to <paramref name="tree"/>, and returns it.
        /// </summary>
        /// <param name="tree">The tree the nodes are registered in.</param>
        /// <param name="ownerName">What the owner is called, "Neutral" for nobody.</param>
        /// <param name="colorHex">The colour of the owner's markers.</param>
        /// <param name="playerIndex">The number of the owner's page, or -1 when it has none (the group only holds heroes and towns).</param>
        /// <param name="owned">Whether a player owns it, which opens the group at first (the many neutral heroes and towns are one click away).</param>
        /// <param name="status">The first part of the summary, such as the days a player has left, or empty.</param>
        /// <param name="heroes">The heroes of the owner.</param>
        /// <param name="castles">The towns of the owner.</param>
        /// <param name="markCastle">Sets what a game shows on the node of a town (badges, notes), or <c>null</c>.</param>
        public static NavNodeViewModel Create(
            NavigationTree tree,
            string ownerName,
            string colorHex,
            int playerIndex,
            bool owned,
            string status,
            IReadOnlyList<NavEntry> heroes,
            IReadOnlyList<NavEntry> castles,
            Action<NavNodeViewModel, int>? markCastle = null)
        {
            var parts = new List<string>();
            if (status.Length > 0)
            {
                parts.Add(status);
            }

            if (heroes.Count > 0)
            {
                parts.Add($"{heroes.Count} {(heroes.Count == 1 ? "hero" : "heroes")}");
            }

            if (castles.Count > 0)
            {
                parts.Add($"{castles.Count} {(castles.Count == 1 ? "town" : "towns")}");
            }

            var isPlayer = playerIndex >= 0;
            var node = new NavNodeViewModel(
                ownerName,
                string.Join(" · ", parts),
                colorHex,
                isPlayer ? NavNodeKind.Player : NavNodeKind.Group,
                playerIndex,
                onSelected: isPlayer ? tree.OnSelected : null)
            {
                DefaultExpanded = owned,
            };
            tree.Register(node);

            if (heroes.Count > 0)
            {
                node.Children.Add(Group(tree, "Heroes", 1, colorHex, playerIndex, owned, heroes, NavNodeKind.Hero, null));
            }

            if (castles.Count > 0)
            {
                node.Children.Add(Group(tree, "Towns", 2, colorHex, playerIndex, owned, castles, NavNodeKind.Castle, markCastle));
            }

            node.IsExpanded = node.DefaultExpanded;
            return node;
        }

        private static NavNodeViewModel Group(
            NavigationTree tree,
            string title,
            int tab,
            string colorHex,
            int playerIndex,
            bool owned,
            IReadOnlyList<NavEntry> entries,
            NavNodeKind kind,
            Action<NavNodeViewModel, int>? mark)
        {
            var isPlayer = playerIndex >= 0;
            var group = new NavNodeViewModel(title, entries.Count.ToString(), colorHex, isPlayer ? NavNodeKind.PlayerTab : NavNodeKind.Group, playerIndex, tab, isPlayer ? tree.OnSelected : null)
            {
                DefaultExpanded = owned,
            };
            tree.Register(group);

            foreach (var entry in entries)
            {
                var child = new NavNodeViewModel(entry.Title, entry.Subtitle, colorHex, kind, entry.Index, onSelected: tree.OnSelected);
                mark?.Invoke(child, entry.Index);
                group.Children.Add(child);
                tree.Register(child);
            }

            group.IsExpanded = group.DefaultExpanded;
            return group;
        }
    }
}
