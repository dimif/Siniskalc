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
    public sealed class Homm2NavigationTree
    {
        private readonly Action<Homm2NavNodeViewModel> onSelected;
        private readonly Dictionary<int, Homm2NavNodeViewModel> heroNodes = [];
        private readonly Dictionary<int, Homm2NavNodeViewModel> playerNodes = [];
        private readonly Dictionary<(int Player, int Tab), Homm2NavNodeViewModel> playerTabNodes = [];
        private readonly Dictionary<int, Homm2NavNodeViewModel> castleNodes = [];
        private string filter = string.Empty;

        /// <param name="onSelected">Called with the node when the user picks one.</param>
        public Homm2NavigationTree(Action<Homm2NavNodeViewModel> onSelected)
        {
            this.onSelected = onSelected;
        }

        /// <summary>
        /// Gets the top level nodes: the game, which holds the players.
        /// </summary>
        public ObservableCollection<Homm2NavNodeViewModel> Nodes { get; } = [];

        public Homm2NavNodeViewModel? Overview { get; private set; }

        public Homm2NavNodeViewModel? HeroNode(int heroId) => this.heroNodes.GetValueOrDefault(heroId);

        public Homm2NavNodeViewModel? CastleNode(int castleIndex) => this.castleNodes.GetValueOrDefault(castleIndex);

        /// <summary>
        /// Gets the node of the player's resources (tab 0), heroes (1) or towns (2), or the player's node when it has no such group.
        /// </summary>
        public Homm2NavNodeViewModel? PlayerTabNode(int playerIndex, int tab) => this.playerTabNodes.GetValueOrDefault((playerIndex, tab)) ?? this.playerNodes.GetValueOrDefault(playerIndex);

        /// <summary>
        /// Rebuilds the tree for <paramref name="game"/>, keeping the current filter.
        /// </summary>
        public void Build(Homm2Game game)
        {
            this.Nodes.Clear();
            this.heroNodes.Clear();
            this.playerNodes.Clear();
            this.playerTabNodes.Clear();
            this.castleNodes.Clear();

            this.Overview = new Homm2NavNodeViewModel("Game", string.IsNullOrEmpty(game.Name) ? string.Empty : System.IO.Path.GetFileName(game.Name), Homm2Palette.NoPlayer, Homm2NavNodeKind.Overview, onSelected: this.onSelected)
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
                .Concat(heroes.Select(h => h.Owner).OfType<Homm2Color>())
                .Concat(castles.Select(c => c.Owner).OfType<Homm2Color>())
                .Distinct()
                .OrderBy(c => c)
                .Select(c => (Homm2Color?)c)
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
        public void Reveal(Homm2NavNodeViewModel? node)
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
        public void RefreshVisitors(Homm2Game game)
        {
            var document = game.History.Current;
            var castles = document.Castles().GetModels();
            foreach (var (index, node) in this.castleNodes)
            {
                node.VisitingHeroName = VisitorName(document.Heroes().GetModels(), castles[index]);
            }
        }

        private static IEnumerable<Homm2NavNodeViewModel> Ancestors(Homm2NavNodeViewModel from, Homm2NavNodeViewModel target)
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

        private static bool Mark(Homm2NavNodeViewModel node, HashSet<(ChunkKind Kind, int Index)> changed)
        {
            var below = false;
            foreach (var child in node.Children)
            {
                below |= Mark(child, changed);
            }

            var own = node.Kind switch
            {
                Homm2NavNodeKind.Player => changed.Contains((Homm2ChunkKinds.Player, node.Index)),
                Homm2NavNodeKind.Hero => changed.Contains((Homm2ChunkKinds.Hero, node.Index)),
                Homm2NavNodeKind.Castle => changed.Contains((Homm2ChunkKinds.Castle, node.Index)),
                _ => false,
            };

            node.HasChanges = own || below;
            return node.HasChanges;
        }

        private static string VisitorName(IReadOnlyList<Homm2HeroModel> heroes, Homm2CastleModel castle) =>
            castle.VisitingHero is { } id && id >= 0 && id < heroes.Count ? heroes[id].Name : string.Empty;

        private Homm2NavNodeViewModel CreateGroup(Homm2Color? owner, string status, IReadOnlyList<Homm2HeroModel> heroes, IReadOnlyList<Homm2CastleModel> castles, IReadOnlyList<Homm2HeroModel> allHeroes)
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
            var colorHex = Homm2Palette.Of(owner);
            var node = new Homm2NavNodeViewModel(
                owner?.ToString() ?? "Neutral",
                string.Join(" · ", parts),
                colorHex,
                isPlayer ? Homm2NavNodeKind.Player : Homm2NavNodeKind.Group,
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
                var group = new Homm2NavNodeViewModel("Heroes", heroes.Count.ToString(), colorHex, isPlayer ? Homm2NavNodeKind.PlayerTab : Homm2NavNodeKind.Group, playerIndex, 1, isPlayer ? this.onSelected : null)
                {
                    DefaultExpanded = owner != null,
                };
                if (isPlayer)
                {
                    this.playerTabNodes[(playerIndex, 1)] = group;
                }

                foreach (var hero in heroes)
                {
                    var heroNode = new Homm2NavNodeViewModel(hero.Name, $"{hero.Race}, level {hero.Level}", colorHex, Homm2NavNodeKind.Hero, hero.HeroId, onSelected: this.onSelected);
                    group.Children.Add(heroNode);
                    this.heroNodes[hero.HeroId] = heroNode;
                }

                group.IsExpanded = group.DefaultExpanded;
                node.Children.Add(group);
            }

            if (castles.Count > 0)
            {
                var group = new Homm2NavNodeViewModel("Towns", castles.Count.ToString(), colorHex, isPlayer ? Homm2NavNodeKind.PlayerTab : Homm2NavNodeKind.Group, playerIndex, 2, isPlayer ? this.onSelected : null)
                {
                    DefaultExpanded = owner != null,
                };
                if (isPlayer)
                {
                    this.playerTabNodes[(playerIndex, 2)] = group;
                }

                foreach (var castle in castles)
                {
                    var castleNode = new Homm2NavNodeViewModel(castle.Name, castle.Race?.ToString() ?? string.Empty, colorHex, Homm2NavNodeKind.Castle, castle.Index, onSelected: this.onSelected)
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
