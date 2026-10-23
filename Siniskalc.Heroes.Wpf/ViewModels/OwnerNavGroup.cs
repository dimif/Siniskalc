namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        /// <param name="mines">The mines of the owner in groups (one per resource), or <c>null</c> for a game that shows none.</param>
        /// <param name="dwellings">The dwellings of the owner in groups (one per faction), or <c>null</c> for a game that shows none.</param>
        public static NavNodeViewModel Create(
            NavigationTree tree,
            string ownerName,
            string colorHex,
            int playerIndex,
            bool owned,
            string status,
            IReadOnlyList<NavEntry> heroes,
            IReadOnlyList<NavEntry> castles,
            Action<NavNodeViewModel, int>? markCastle = null,
            IReadOnlyList<NavGroup>? mines = null,
            IReadOnlyList<NavGroup>? dwellings = null)
        {
            mines = NonEmpty(mines);
            dwellings = NonEmpty(dwellings);
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

            AddCount(parts, mines, "mine", "mines");
            AddCount(parts, dwellings, "dwelling", "dwellings");

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
                node.Children.Add(Group(tree, "Heroes", OwnerTabs.Heroes, colorHex, playerIndex, owned, heroes, NavNodeKind.Hero, null));
            }

            if (castles.Count > 0)
            {
                node.Children.Add(Group(tree, "Towns", OwnerTabs.Towns, colorHex, playerIndex, owned, castles, NavNodeKind.Castle, markCastle));
            }

            if (mines.Count > 0)
            {
                node.Children.Add(GroupedSection(tree, "Mines", OwnerTabs.Mines, colorHex, playerIndex, owned, mines, NavNodeKind.Mine));
            }

            if (dwellings.Count > 0)
            {
                node.Children.Add(GroupedSection(tree, "Dwellings", OwnerTabs.Dwellings, colorHex, playerIndex, owned, dwellings, NavNodeKind.Dwelling));
            }

            node.IsExpanded = node.DefaultExpanded;
            return node;
        }

        private static IReadOnlyList<NavGroup> NonEmpty(IReadOnlyList<NavGroup>? groups) => (groups ?? []).Where(g => g.Entries.Count > 0).ToList();

        private static void AddCount(List<string> parts, IReadOnlyList<NavGroup> groups, string one, string many)
        {
            var count = groups.Sum(g => g.Entries.Count);
            if (count > 0)
            {
                parts.Add($"{count} {(count == 1 ? one : many)}");
            }
        }

        // A section of an owner whose entries are in groups: "Mines" holds "Gold", which holds the gold mines. The section opens the
        // owner's tab; a group opens the same tab and is not registered, as the tab is the section's.
        private static NavNodeViewModel GroupedSection(
            NavigationTree tree,
            string title,
            int tab,
            string colorHex,
            int playerIndex,
            bool owned,
            IReadOnlyList<NavGroup> groups,
            NavNodeKind leafKind)
        {
            var isPlayer = playerIndex >= 0;
            var section = new NavNodeViewModel(title, groups.Sum(g => g.Entries.Count).ToString(), colorHex, isPlayer ? NavNodeKind.PlayerTab : NavNodeKind.Group, playerIndex, tab, isPlayer ? tree.OnSelected : null)
            {
                DefaultExpanded = owned,
            };
            tree.Register(section);

            foreach (var group in groups)
            {
                var node = new NavNodeViewModel(group.Title, group.Entries.Count.ToString(), colorHex, isPlayer ? NavNodeKind.OwnerSubGroup : NavNodeKind.Group, playerIndex, tab, isPlayer ? tree.OnSelected : null);
                foreach (var entry in group.Entries)
                {
                    var leaf = new NavNodeViewModel(entry.Title, entry.Subtitle, colorHex, leafKind, entry.Index, onSelected: tree.OnSelected);
                    node.Children.Add(leaf);
                    tree.Register(leaf);
                }

                section.Children.Add(node);
            }

            section.IsExpanded = section.DefaultExpanded;
            return section;
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
