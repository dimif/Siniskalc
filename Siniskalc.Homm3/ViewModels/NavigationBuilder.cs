namespace Siniskalc.Homm3.ViewModels
{
    using Siniskalc.Heroes.Entities;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Homm3.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Fills the navigation tree with the game (what the savefile contains), which holds one node per player (and one for what
    /// nobody owns). A player's node is its resources page, and holds a "Heroes" group and a "Towns" group.
    /// </summary>
    public sealed class NavigationBuilder : INavigationBuilder<Game>
    {
        public const string CapitalBadge = "capital";
        public const string GrailBadge = "grail";
        public const string GarrisonBadge = "garrison";

        private static readonly IReadOnlyDictionary<NavNodeKind, ChunkKind> Kinds = new Dictionary<NavNodeKind, ChunkKind>
        {
            [NavNodeKind.Player] = Homm3ChunkKinds.Player,
            [NavNodeKind.Hero] = Homm3ChunkKinds.Hero,
            [NavNodeKind.Castle] = Homm3ChunkKinds.Castle,
        };

        private readonly BanishingTextProvider banishingText;

        /// <param name="banishingText">Words the countdown of a player with no town is shown with.</param>
        public NavigationBuilder(BanishingTextProvider banishingText)
        {
            this.banishingText = banishingText;
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<NavNodeKind, ChunkKind> ChunkKinds => Kinds;

        /// <summary>
        /// Returns who owns each castle and hero, as one text that changes when any of them changes hands. Reads the owner bytes only.
        /// </summary>
        public string StructureKey(Game game)
        {
            var document = game.History.Current;
            return string.Join(",", document.Castles().GetOwners().Select(o => o?.ToString() ?? "-"))
                + ";" + string.Join(",", document.Heroes().GetOwners().Select(o => o?.ToString() ?? "-"));
        }

        /// <inheritdoc/>
        public void Build(NavigationTree tree, Game game)
        {
            var overview = new NavNodeViewModel("Game", string.IsNullOrEmpty(game.Name) ? string.Empty : System.IO.Path.GetFileName(game.Name), PlayerPalette.NoPlayer, NavNodeKind.Overview, onSelected: tree.OnSelected)
            {
                DefaultExpanded = true,
                IsExpanded = true,
            };
            tree.SetOverview(overview);

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
                overview.Children.Add(this.CreateGroup(
                    tree,
                    game,
                    owner,
                    summaries.Where(s => s.Owner == owner).OrderBy(s => s.Name).ToList(),
                    castles.Where(c => c.Model.Owner == owner).OrderBy(c => c.Model.Name).ToList()));
            }
        }

        /// <summary>
        /// Shows the crown and the Grail mark of the castles and the days left of the players with no town as they are in
        /// <paramref name="game"/> now, after an edit or an undo.
        /// </summary>
        public void RefreshMarks(NavigationTree tree, Game game)
        {
            var castles = game.History.Current.Castles().GetModels();
            foreach (var (index, node) in tree.CastleNodes)
            {
                if (index < castles.Count)
                {
                    this.MarkCastle(node, game, castles[index]);
                }
            }

            foreach (var (index, node) in tree.PlayerNodes)
            {
                node.Notes = NotesOf(this.banishingText.Nav(game.History.Current.Players().GetModel(index)));
            }
        }

        private static IReadOnlyList<string> NotesOf(string text) => text.Length > 0 ? [text] : [];

        private static string GarrisonedHeroName(Game game, CastleModel castle) =>
            castle.GarrisonedHeroId is { } id ? game.History.Current.Heroes().FindById(id)?.Name ?? $"Unknown hero {id}" : string.Empty;

        private void MarkCastle(NavNodeViewModel node, Game game, CastleModel castle)
        {
            var badges = new List<NavBadge>();
            if (castle.HasBuilt(Building.Capitol))
            {
                badges.Add(new NavBadge(CapitalBadge, "♛", "Capital", NavBadgeTone.Accent));
            }

            if (castle.Buildings.FirstOrDefault(b => b.IsGrail)?.Name is { } grail)
            {
                badges.Add(new NavBadge(GrailBadge, "✦", $"Grail: {grail}", NavBadgeTone.Highlight));
            }

            var garrisoned = GarrisonedHeroName(game, castle);
            if (garrisoned.Length > 0)
            {
                badges.Add(new NavBadge(GarrisonBadge, "⚔", $"Garrisoned: {garrisoned}", NavBadgeTone.Muted));
            }

            node.Badges = badges;
            node.Notes = NotesOf(garrisoned);
        }

        private NavNodeViewModel CreateGroup(NavigationTree tree, Game game, PlayerColor? owner, IReadOnlyList<HeroSummary<PlayerColor>> heroes, IReadOnlyList<(int Index, CastleModel Model)> castles)
        {
            // A player with data in the savefile is a page of its own (the resources); the others only hold their heroes and towns.
            var playerIndex = owner == null ? -1 : game.History.Current.Players().FindByColor(owner.Value);
            var castleModels = castles.ToDictionary(c => c.Index, c => c.Model);
            var node = OwnerNavGroup.Create(
                tree,
                OwnerNames<PlayerColor>.Of(owner),
                PlayerPalette.Of(PlayerColors<PlayerColor>.Of(owner)),
                playerIndex,
                owner != null,
                string.Empty,
                heroes.Select(h => new NavEntry(h.Index, h.Name, h.Description)).ToList(),
                castles.Select(c => new NavEntry(c.Index, c.Model.Name, c.Model.Faction?.Name ?? string.Empty)).ToList(),
                (castleNode, index) => this.MarkCastle(castleNode, game, castleModels[index]));
            if (playerIndex >= 0)
            {
                node.Notes = NotesOf(this.banishingText.Nav(game.History.Current.Players().GetModel(playerIndex)));
            }

            return node;
        }
    }
}
