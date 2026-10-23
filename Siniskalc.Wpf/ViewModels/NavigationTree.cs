namespace Siniskalc.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Siniskalc.Core.Data;

    /// <summary>
    /// The tree the user picks pages from: the game (what the savefile contains), which holds what the game has to show, in general one
    /// node per player. A game fills it through an <see cref="INavigationBuilder{TGame}"/>; the tree keeps the nodes by what they
    /// stand for, applies the filter, reveals a node and marks the nodes that have unsaved edits.
    /// </summary>
    public sealed class NavigationTree
    {
        private readonly IReadOnlyDictionary<NavNodeKind, ChunkKind> chunkKinds;
        private readonly Dictionary<int, NavNodeViewModel> heroNodes = [];
        private readonly Dictionary<int, NavNodeViewModel> playerNodes = [];
        private readonly Dictionary<(int Player, int Tab), NavNodeViewModel> playerTabNodes = [];
        private readonly Dictionary<int, NavNodeViewModel> castleNodes = [];
        private readonly Dictionary<int, NavNodeViewModel> neutralTabNodes = [];
        private readonly Dictionary<int, NavNodeViewModel> mineNodes = [];
        private readonly Dictionary<int, NavNodeViewModel> dwellingNodes = [];
        private string filter = string.Empty;

        /// <param name="onSelected">Called with the node when the user picks one.</param>
        /// <param name="chunkKinds">The kind of chunk a node of each kind stands for, to mark the nodes whose chunk has edits.</param>
        public NavigationTree(Action<NavNodeViewModel> onSelected, IReadOnlyDictionary<NavNodeKind, ChunkKind> chunkKinds)
        {
            this.OnSelected = onSelected;
            this.chunkKinds = chunkKinds;
        }

        /// <summary>
        /// Gets what is called with a node the user picks, for the builder to give to the nodes that are pages.
        /// </summary>
        public Action<NavNodeViewModel> OnSelected { get; }

        /// <summary>
        /// Gets the top level nodes: the game, which holds the rest.
        /// </summary>
        public ObservableCollection<NavNodeViewModel> Nodes { get; } = [];

        public NavNodeViewModel? Overview { get; private set; }

        /// <summary>
        /// Gets the node of the page of the map's objects, or <c>null</c> when the game has none.
        /// </summary>
        public NavNodeViewModel? MapNode { get; private set; }

        /// <summary>
        /// Gets the nodes of the players, by the player's number.
        /// </summary>
        public IEnumerable<KeyValuePair<int, NavNodeViewModel>> PlayerNodes => this.playerNodes;

        /// <summary>
        /// Gets the nodes of the castles, by the castle's number.
        /// </summary>
        public IEnumerable<KeyValuePair<int, NavNodeViewModel>> CastleNodes => this.castleNodes;

        public NavNodeViewModel? HeroNode(int heroIndex) => this.heroNodes.GetValueOrDefault(heroIndex);

        public NavNodeViewModel? PlayerNode(int playerIndex) => this.playerNodes.GetValueOrDefault(playerIndex);

        /// <summary>
        /// Gets the node of the player's resources (tab 0), heroes (1) or towns (2), or the player's node when it has no such group.
        /// </summary>
        public NavNodeViewModel? PlayerTabNode(int playerIndex, int tab) => this.playerTabNodes.GetValueOrDefault((playerIndex, tab)) ?? this.PlayerNode(playerIndex);

        public NavNodeViewModel? CastleNode(int castleIndex) => this.castleNodes.GetValueOrDefault(castleIndex);

        /// <summary>
        /// Gets the node of what nobody owns, or <c>null</c> when the game has none.
        /// </summary>
        public NavNodeViewModel? NeutralNode { get; private set; }

        /// <summary>
        /// Gets the node of the tab <paramref name="tab"/> of the neutral page, or the neutral node when it has no such group.
        /// </summary>
        public NavNodeViewModel? NeutralTabNode(int tab) => this.neutralTabNodes.GetValueOrDefault(tab) ?? this.NeutralNode;

        public NavNodeViewModel? MineNode(int mineIndex) => this.mineNodes.GetValueOrDefault(mineIndex);

        public NavNodeViewModel? DwellingNode(int dwellingIndex) => this.dwellingNodes.GetValueOrDefault(dwellingIndex);

        /// <summary>
        /// Empties the tree, for the builder to fill it again.
        /// </summary>
        public void Clear()
        {
            this.Nodes.Clear();
            this.Overview = null;
            this.MapNode = null;
            this.heroNodes.Clear();
            this.playerNodes.Clear();
            this.playerTabNodes.Clear();
            this.castleNodes.Clear();
            this.mineNodes.Clear();
            this.dwellingNodes.Clear();
            this.neutralTabNodes.Clear();
            this.NeutralNode = null;
        }

        /// <summary>
        /// Makes <paramref name="node"/> the root of the tree.
        /// </summary>
        public void SetOverview(NavNodeViewModel node)
        {
            this.Overview = node;
            this.Nodes.Add(node);
        }

        /// <summary>
        /// Remembers <paramref name="node"/> by what it stands for, so the editor can find it again: a hero, a castle, a player or a
        /// tab of a player's page, a mine or a dwelling.
        /// </summary>
        public void Register(NavNodeViewModel node)
        {
            switch (node.Kind)
            {
                case NavNodeKind.Map:
                    this.MapNode = node;
                    break;
                case NavNodeKind.Hero:
                    this.heroNodes[node.Index] = node;
                    break;
                case NavNodeKind.Castle:
                    this.castleNodes[node.Index] = node;
                    break;
                case NavNodeKind.Neutral:
                    this.NeutralNode = node;
                    break;
                case NavNodeKind.NeutralTab:
                    this.neutralTabNodes[node.Tab] = node;
                    break;
                case NavNodeKind.Mine:
                    this.mineNodes[node.Index] = node;
                    break;
                case NavNodeKind.Dwelling:
                    this.dwellingNodes[node.Index] = node;
                    break;
                case NavNodeKind.Player:
                    this.playerNodes[node.Index] = node;
                    this.playerTabNodes[(node.Index, 0)] = node;
                    break;
                case NavNodeKind.PlayerTab:
                    this.playerTabNodes[(node.Index, node.Tab)] = node;
                    break;
            }
        }

        /// <summary>
        /// Narrows the tree to the nodes matching <paramref name="text"/>.
        /// </summary>
        public void ApplyFilter(string text)
        {
            this.filter = text ?? string.Empty;
            foreach (var node in this.Nodes)
            {
                node.ApplyFilter(this.filter);
            }
        }

        /// <summary>
        /// Applies the filter that was set to the nodes that were built since.
        /// </summary>
        public void ReapplyFilter()
        {
            if (this.filter.Length > 0)
            {
                this.ApplyFilter(this.filter);
            }
        }

        /// <summary>
        /// Selects <paramref name="node"/> and makes sure it is on screen: the nodes above it may be collapsed.
        /// </summary>
        public void Reveal(NavNodeViewModel? node)
        {
            if (node == null)
            {
                return;
            }

            foreach (var ancestor in this.Nodes.SelectMany(n => Ancestors(n, node)))
            {
                ancestor.IsExpanded = true;
            }

            node.IsSelected = true;
        }

        /// <summary>
        /// Marks the nodes of the heroes, players and castles that have <paramref name="changes"/>, and the nodes above them.
        /// </summary>
        public void MarkChanges(IEnumerable<PendingChange> changes)
        {
            var changed = changes.Select(c => (c.Kind, c.Index)).ToHashSet();

            foreach (var node in this.Nodes)
            {
                this.Mark(node, changed);
            }
        }

        private static IEnumerable<NavNodeViewModel> Ancestors(NavNodeViewModel from, NavNodeViewModel target)
        {
            foreach (var child in from.Children)
            {
                if (child == target)
                {
                    return [from];
                }

                var path = Ancestors(child, target).ToList();
                if (path.Count > 0)
                {
                    return path.Append(from);
                }
            }

            return [];
        }

        private bool Mark(NavNodeViewModel node, HashSet<(ChunkKind Kind, int Index)> changed)
        {
            var below = false;
            foreach (var child in node.Children)
            {
                below |= this.Mark(child, changed);
            }

            var own = this.chunkKinds.TryGetValue(node.Kind, out var kind) && changed.Contains((kind, node.Index));

            node.HasChanges = own || below;
            return node.HasChanges;
        }
    }
}
