namespace Siniskalc.Homm2.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Fills the navigation tree with the game (what the savefile contains), which holds one node per player (and one for what nobody
    /// owns). A player's node is its resources page, and holds a "Heroes" group and a "Towns" group.
    /// </summary>
    public sealed class NavigationBuilder : INavigationBuilder<Game>
    {
        private static readonly IReadOnlyDictionary<NavNodeKind, ChunkKind> Kinds = new Dictionary<NavNodeKind, ChunkKind>
        {
            [NavNodeKind.Player] = Homm2ChunkKinds.Player,
            [NavNodeKind.Hero] = Homm2ChunkKinds.Hero,
            [NavNodeKind.Castle] = Homm2ChunkKinds.Castle,
        };

        /// <inheritdoc/>
        public IReadOnlyDictionary<NavNodeKind, ChunkKind> ChunkKinds => Kinds;

        /// <inheritdoc/>
        public string StructureKey(Game game)
        {
            var document = game.History.Current;
            return string.Join(",", document.Castles().GetOwners().Select(o => o?.ToString() ?? "-"))
                + ";" + string.Join(",", document.Heroes().GetOwners().Select(o => o?.ToString() ?? "-"));
        }

        /// <inheritdoc/>
        public void Build(NavigationTree tree, Game game)
        {
            var overview = new NavNodeViewModel("Game", string.IsNullOrEmpty(game.Name) ? string.Empty : System.IO.Path.GetFileName(game.Name), Palette.NoPlayer, NavNodeKind.Overview, onSelected: tree.OnSelected)
            {
                DefaultExpanded = true,
                IsExpanded = true,
            };
            tree.SetOverview(overview);

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
                overview.Children.Add(CreateGroup(
                    tree,
                    owner,
                    player == null ? string.Empty : player.IsOut ? "out of the game" : player.DaysLeft is { } days ? $"{days} days left" : string.Empty,
                    heroes.Where(h => h.Owner == owner).OrderBy(h => h.Name).ToList(),
                    castles.Where(c => c.Owner == owner).OrderBy(c => c.Name).ToList(),
                    heroes));
            }
        }

        /// <inheritdoc/>
        public void RefreshMarks(NavigationTree tree, Game game)
        {
            var document = game.History.Current;
            var castles = document.Castles().GetModels();
            foreach (var (index, node) in tree.CastleNodes)
            {
                node.Notes = NotesOf(VisitorName(document.Heroes().GetModels(), castles[index]));
            }
        }

        private static IReadOnlyList<string> NotesOf(string visitor) => visitor.Length > 0 ? [visitor] : [];

        private static string VisitorName(IReadOnlyList<HeroModel> heroes, CastleModel castle) =>
            castle.VisitingHero is { } id && id >= 0 && id < heroes.Count ? heroes[id].Name : string.Empty;

        private static NavNodeViewModel CreateGroup(NavigationTree tree, Color? owner, string status, IReadOnlyList<HeroModel> heroes, IReadOnlyList<CastleModel> castles, IReadOnlyList<HeroModel> allHeroes)
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
                onSelected: isPlayer ? tree.OnSelected : null)
            {
                // Only what players own is of interest at first; the many heroes of the tavern pool are one click away.
                DefaultExpanded = owner != null,
            };
            tree.Register(node);

            if (heroes.Count > 0)
            {
                var group = new NavNodeViewModel("Heroes", heroes.Count.ToString(), colorHex, isPlayer ? NavNodeKind.PlayerTab : NavNodeKind.Group, playerIndex, 1, isPlayer ? tree.OnSelected : null)
                {
                    DefaultExpanded = owner != null,
                };
                tree.Register(group);

                foreach (var hero in heroes)
                {
                    var heroNode = new NavNodeViewModel(hero.Name, $"{hero.Race}, level {hero.Level}", colorHex, NavNodeKind.Hero, hero.HeroId, onSelected: tree.OnSelected);
                    group.Children.Add(heroNode);
                    tree.Register(heroNode);
                }

                group.IsExpanded = group.DefaultExpanded;
                node.Children.Add(group);
            }

            if (castles.Count > 0)
            {
                var group = new NavNodeViewModel("Towns", castles.Count.ToString(), colorHex, isPlayer ? NavNodeKind.PlayerTab : NavNodeKind.Group, playerIndex, 2, isPlayer ? tree.OnSelected : null)
                {
                    DefaultExpanded = owner != null,
                };
                tree.Register(group);

                foreach (var castle in castles)
                {
                    var castleNode = new NavNodeViewModel(castle.Name, castle.Race?.ToString() ?? string.Empty, colorHex, NavNodeKind.Castle, castle.Index, onSelected: tree.OnSelected)
                    {
                        Notes = NotesOf(VisitorName(allHeroes, castle)),
                    };
                    group.Children.Add(castleNode);
                    tree.Register(castleNode);
                }

                group.IsExpanded = group.DefaultExpanded;
                node.Children.Add(group);
            }

            node.IsExpanded = node.DefaultExpanded;
            return node;
        }
    }
}
