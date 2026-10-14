namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;

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

        public NavNodeViewModel? HeroNode(int heroId) => this.heroNodes.GetValueOrDefault(heroId);

        public NavNodeViewModel? CastleNode(int castleIndex) => this.castleNodes.GetValueOrDefault(castleIndex);

        /// <summary>
        /// Gets the node of the player's resources (tab 0), heroes (1) or towns (2), or the player's node when it has no such group.
        /// </summary>
        public NavNodeViewModel? PlayerTabNode(int playerIndex, int tab) => this.playerTabNodes.GetValueOrDefault((playerIndex, tab)) ?? this.playerNodes.GetValueOrDefault(playerIndex);

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

            this.Overview = new NavNodeViewModel("Game", string.IsNullOrEmpty(game.Name) ? string.Empty : System.IO.Path.GetFileName(game.Name), Palette.NoPlayer, NavNodeKind.Overview, onSelected: this.onSelected)
            {
                DefaultExpanded = true,
                IsExpanded = true,
            };
            this.Nodes.Add(this.Overview);

            var document = game.History.Current;
            var heroes = document.Heroes().GetModels();
            var castles = document.Castles().GetModels().Where(c => c.IsUsed).ToList();
            var players = document.Players().GetModels();

            // A player is listed when it has a castle, a hero, or days left to get a castle.
            var owners = players
                .Where(p => p.CastleIds.Count > 0 || p.HeroIds.Count > 0 || p.DaysLeft != null)
                .Select(p => p.Color)
                .Concat(heroes.Select(h => h.Owner).OfType<Color>())
                .Concat(castles.Select(c => c.Owner).OfType<Color>())
                .Distinct()
                .OrderBy(c => c)
                .Select(c => (Color?)c)
                .ToList();
            if (heroes.Any(h => h.Owner == null) || castles.Any(c => c.Owner == null))
            {
                owners.Add(null);
            }

            foreach (var owner in owners)
            {
                var player = owner is { } color ? players[(int)color] : null;
                this.Overview.Children.Add(this.CreateGroup(
                    owner,
                    player == null ? string.Empty : player.IsOut ? "out of the game" : player.DaysLeft is { } days ? $"{days} days left" : string.Empty,
                    heroes.Where(h => h.Owner == owner).OrderBy(h => h.Name).ToList(),
                    castles.Where(c => c.Owner == owner).OrderBy(c => c.Name).ToList(),
                    heroes));
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
        public void MarkChanges(IEnumerable<PendingChange> changes)
        {
            var changed = changes.Select(c => (c.Kind, c.Index)).ToHashSet();

            foreach (var node in this.Nodes)
            {
                Mark(node, changed);
            }
        }

        /// <summary>
        /// Shows the hero that visits each castle as it is in <paramref name="game"/> now, after an edit or an undo.
        /// </summary>
        public void RefreshVisitors(Game game)
        {
            var document = game.History.Current;
            var castles = document.Castles().GetModels();
            foreach (var (index, node) in this.castleNodes)
            {
                node.VisitingHeroName = VisitorName(document.Heroes().GetModels(), castles[index]);
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
                NavNodeKind.Player => changed.Contains((Homm2ChunkKinds.Player, node.Index)),
                NavNodeKind.Hero => changed.Contains((Homm2ChunkKinds.Hero, node.Index)),
                NavNodeKind.Castle => changed.Contains((Homm2ChunkKinds.Castle, node.Index)),
                _ => false,
            };

            node.HasChanges = own || below;
            return node.HasChanges;
        }

        private static string VisitorName(IReadOnlyList<HeroModel> heroes, CastleModel castle) =>
            castle.VisitingHero is { } id && id >= 0 && id < heroes.Count ? heroes[id].Name : string.Empty;

        private NavNodeViewModel CreateGroup(Color? owner, string status, IReadOnlyList<HeroModel> heroes, IReadOnlyList<CastleModel> castles, IReadOnlyList<HeroModel> allHeroes)
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

            // A colour is a page of its own (the resources); what nobody owns only holds its heroes and towns.
            var playerIndex = owner == null ? -1 : (int)owner.Value;
            var isPlayer = playerIndex >= 0;
            var colorHex = Palette.Of(owner);
            var node = new NavNodeViewModel(
                owner?.ToString() ?? "Neutral",
                string.Join(" · ", parts),
                colorHex,
                isPlayer ? NavNodeKind.Player : NavNodeKind.Group,
                playerIndex,
                onSelected: isPlayer ? this.onSelected : null)
            {
                // Only what players own is of interest at first; the many heroes of the tavern pool are one click away.
                DefaultExpanded = owner != null,
            };
            if (isPlayer)
            {
                this.playerNodes[playerIndex] = node;
                this.playerTabNodes[(playerIndex, 0)] = node;
            }

            if (heroes.Count > 0)
            {
                var group = new NavNodeViewModel("Heroes", heroes.Count.ToString(), colorHex, isPlayer ? NavNodeKind.PlayerTab : NavNodeKind.Group, playerIndex, 1, isPlayer ? this.onSelected : null)
                {
                    DefaultExpanded = owner != null,
                };
                if (isPlayer)
                {
                    this.playerTabNodes[(playerIndex, 1)] = group;
                }

                foreach (var hero in heroes)
                {
                    var heroNode = new NavNodeViewModel(hero.Name, $"{hero.Race}, level {hero.Level}", colorHex, NavNodeKind.Hero, hero.HeroId, onSelected: this.onSelected);
                    group.Children.Add(heroNode);
                    this.heroNodes[hero.HeroId] = heroNode;
                }

                group.IsExpanded = group.DefaultExpanded;
                node.Children.Add(group);
            }

            if (castles.Count > 0)
            {
                var group = new NavNodeViewModel("Towns", castles.Count.ToString(), colorHex, isPlayer ? NavNodeKind.PlayerTab : NavNodeKind.Group, playerIndex, 2, isPlayer ? this.onSelected : null)
                {
                    DefaultExpanded = owner != null,
                };
                if (isPlayer)
                {
                    this.playerTabNodes[(playerIndex, 2)] = group;
                }

                foreach (var castle in castles)
                {
                    var castleNode = new NavNodeViewModel(castle.Name, castle.Race?.ToString() ?? string.Empty, colorHex, NavNodeKind.Castle, castle.Index, onSelected: this.onSelected)
                    {
                        VisitingHeroName = VisitorName(allHeroes, castle),
                    };
                    group.Children.Add(castleNode);
                    this.castleNodes[castle.Index] = castleNode;
                }

                group.IsExpanded = group.DefaultExpanded;
                node.Children.Add(group);
            }

            node.IsExpanded = node.DefaultExpanded;
            return node;
        }
    }
}
