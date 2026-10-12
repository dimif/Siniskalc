namespace Siniskalc.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Data;
    using Siniskalc.Entities;
    using Siniskalc.Models;
    using Siniskalc.Runtime;

    /// <summary>
    /// The tree the user picks pages from: the game (what the savefile contains), which holds one node per player (and one for what
    /// nobody owns). A player's node is its resources page, and holds a "Heroes" group and a "Towns" group.
    /// </summary>
    public sealed class NavigationTree
    {
        private readonly Action<NavNodeViewModel> onSelected;
        private readonly Dictionary<int, NavNodeViewModel> heroNodes = [];
        private readonly Dictionary<int, NavNodeViewModel> playerNodes = [];
        private readonly Dictionary<(int Player, int Tab), NavNodeViewModel> playerTabNodes = [];
        private readonly Dictionary<int, NavNodeViewModel> castleNodes = [];
        private string filter = string.Empty;

        /// <param name="onSelected">Called with the node when the user picks one.</param>
        public NavigationTree(Action<NavNodeViewModel> onSelected)
        {
            this.onSelected = onSelected;
        }

        /// <summary>
        /// Gets the top level nodes: the game, which holds the players.
        /// </summary>
        public ObservableCollection<NavNodeViewModel> Nodes { get; } = [];

        public NavNodeViewModel? Overview { get; private set; }

        public NavNodeViewModel? HeroNode(int heroIndex) => this.heroNodes.GetValueOrDefault(heroIndex);

        public NavNodeViewModel? PlayerNode(int playerIndex) => this.playerNodes.GetValueOrDefault(playerIndex);

        /// <summary>
        /// Gets the node of the player's resources (tab 0), heroes (1) or towns (2), or the player's node when it has no such group.
        /// </summary>
        public NavNodeViewModel? PlayerTabNode(int playerIndex, int tab) => this.playerTabNodes.GetValueOrDefault((playerIndex, tab)) ?? this.PlayerNode(playerIndex);

        public NavNodeViewModel? CastleNode(int castleIndex) => this.castleNodes.GetValueOrDefault(castleIndex);

        /// <summary>
        /// Rebuilds the tree for <paramref name="game"/>, keeping the current filter.
        /// </summary>
        public void Build(Game game)
        {
            this.Nodes.Clear();
            this.heroNodes.Clear();
            this.playerNodes.Clear();
            this.playerTabNodes.Clear();
            this.castleNodes.Clear();

            this.Overview = new NavNodeViewModel("Game", string.IsNullOrEmpty(game.Name) ? string.Empty : System.IO.Path.GetFileName(game.Name), PlayerPalette.NoPlayer, onSelected: this.onSelected, kind: NavNodeKind.Overview)
            {
                DefaultExpanded = true,
                IsExpanded = true,
            };
            this.Nodes.Add(this.Overview);

            var summaries = game.History.Current.Heroes().GetSummaries();
            var playerColors = game.History.Current.Players().GetColors();
            var castles = game.History.Current.Castles().GetModels()
                .Select((model, index) => (Index: index, Model: model))
                .ToList();

            // A player is listed when the savefile has the player's data or one of the player's heroes.
            var owners = playerColors
                .Concat(summaries.Select(s => s.Owner).OfType<PlayerColor>())
                .Concat(castles.Select(c => c.Model.Owner).OfType<PlayerColor>())
                .Distinct()
                .OrderBy(c => c)
                .Select(c => (PlayerColor?)c)
                .ToList();
            if (summaries.Any(s => s.Owner == null) || castles.Any(c => c.Model.Owner == null))
            {
                owners.Add(null);
            }

            foreach (var owner in owners)
            {
                this.Overview.Children.Add(this.CreateGroup(
                    game,
                    owner,
                    summaries.Where(s => s.Owner == owner).OrderBy(s => s.Name).ToList(),
                    castles.Where(c => c.Model.Owner == owner).OrderBy(c => c.Model.Name).ToList()));
            }

            if (this.filter.Length > 0)
            {
                this.ApplyFilter(this.filter);
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
        /// <summary>
        /// Shows the crown and the Grail mark of the castles as they are in <paramref name="game"/> now, after an edit or an undo.
        /// </summary>
        public void RefreshCastleMarks(Game game)
        {
            var castles = game.History.Current.Castles().GetModels();
            foreach (var (index, node) in this.castleNodes)
            {
                if (index < castles.Count)
                {
                    node.IsCapital = castles[index].HasBuilt(Building.Capitol);
                    node.GrailText = castles[index].Buildings.FirstOrDefault(b => b.IsGrail)?.Name ?? string.Empty;
                    node.GarrisonedHeroName = GarrisonedHeroName(game, castles[index]);
                }
            }
        }

        public void MarkChanges(IEnumerable<PendingChange> changes)
        {
            var changed = changes.Select(c => (c.Kind, c.Index)).ToHashSet();

            foreach (var node in this.Nodes)
            {
                Mark(node, changed);
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

        private static bool Mark(NavNodeViewModel node, HashSet<(ChunkKind Kind, int Index)> changed)
        {
            var below = false;
            foreach (var child in node.Children)
            {
                below |= Mark(child, changed);
            }

            var own = node.Kind switch
            {
                NavNodeKind.Player => changed.Contains((Homm3ChunkKinds.Player, node.PlayerIndex)),
                NavNodeKind.Hero => changed.Contains((Homm3ChunkKinds.Hero, node.HeroIndex)),
                NavNodeKind.Castle => changed.Contains((Homm3ChunkKinds.Castle, node.CastleIndex)),
                _ => false,
            };

            node.HasChanges = own || below;
            return node.HasChanges;
        }

        private static string GarrisonedHeroName(Game game, CastleModel castle) =>
            castle.GarrisonedHeroId is { } id ? game.History.Current.Heroes().FindById(id)?.Name ?? $"Unknown hero {id}" : string.Empty;

        private NavNodeViewModel CreateGroup(Game game, PlayerColor? owner, IReadOnlyList<HeroSummary> heroes, IReadOnlyList<(int Index, CastleModel Model)> castles)
        {
            var parts = new List<string>();
            if (heroes.Count > 0)
            {
                parts.Add($"{heroes.Count} {(heroes.Count == 1 ? "hero" : "heroes")}");
            }

            if (castles.Count > 0)
            {
                parts.Add($"{castles.Count} {(castles.Count == 1 ? "town" : "towns")}");
            }

            // A player with data in the savefile is a page of its own (the resources); the others only hold their heroes and towns.
            var playerIndex = owner == null ? -1 : game.History.Current.Players().FindByColor(owner.Value);
            var colorHex = PlayerPalette.Of(owner);
            var node = new NavNodeViewModel(
                owner?.ToString() ?? "Neutral",
                string.Join(" · ", parts),
                colorHex,
                onSelected: playerIndex >= 0 ? this.onSelected : null,
                kind: playerIndex >= 0 ? NavNodeKind.Player : NavNodeKind.Group,
                playerIndex: playerIndex)
            {
                // Only what players own is of interest at first; the many neutral heroes and towns are one click away.
                DefaultExpanded = owner != null,
            };
            if (playerIndex >= 0)
            {
                this.playerNodes[playerIndex] = node;
                this.playerTabNodes[(playerIndex, 0)] = node;
            }

            if (heroes.Count > 0)
            {
                var group = new NavNodeViewModel("Heroes", heroes.Count.ToString(), colorHex, onSelected: playerIndex >= 0 ? this.onSelected : null, kind: playerIndex >= 0 ? NavNodeKind.PlayerTab : NavNodeKind.Group, playerIndex: playerIndex, tab: 1) { DefaultExpanded = owner != null };
                if (playerIndex >= 0)
                {
                    this.playerTabNodes[(playerIndex, 1)] = group;
                }

                foreach (var summary in heroes)
                {
                    var subtitle = summary.Description;
                    var heroNode = new NavNodeViewModel(summary.Name, subtitle, colorHex, summary.Index, this.onSelected);
                    group.Children.Add(heroNode);
                    this.heroNodes[summary.Index] = heroNode;
                }

                group.IsExpanded = group.DefaultExpanded;
                node.Children.Add(group);
            }

            if (castles.Count > 0)
            {
                var group = new NavNodeViewModel("Towns", castles.Count.ToString(), colorHex, onSelected: playerIndex >= 0 ? this.onSelected : null, kind: playerIndex >= 0 ? NavNodeKind.PlayerTab : NavNodeKind.Group, playerIndex: playerIndex, tab: 2) { DefaultExpanded = owner != null };
                if (playerIndex >= 0)
                {
                    this.playerTabNodes[(playerIndex, 2)] = group;
                }

                foreach (var (index, castle) in castles)
                {
                    var castleNode = new NavNodeViewModel(castle.Name, castle.Faction?.Name ?? string.Empty, colorHex, onSelected: this.onSelected, kind: NavNodeKind.Castle, castleIndex: index);
                    castleNode.IsCapital = castle.HasBuilt(Building.Capitol);
                    castleNode.GrailText = castle.Buildings.FirstOrDefault(b => b.IsGrail)?.Name ?? string.Empty;
                    castleNode.GarrisonedHeroName = GarrisonedHeroName(game, castle);
                    group.Children.Add(castleNode);
                    this.castleNodes[index] = castleNode;
                }

                group.IsExpanded = group.DefaultExpanded;
                node.Children.Add(group);
            }

            node.IsExpanded = node.DefaultExpanded;
            return node;
        }
    }
}
