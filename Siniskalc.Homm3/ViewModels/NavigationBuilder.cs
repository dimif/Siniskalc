namespace Siniskalc.Homm3.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Entities;
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
    public sealed class NavigationBuilder : OwnerNavigationBuilderBase<Game, PlayerColor>
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
            : base(Kinds)
        {
            this.banishingText = banishingText;
        }

        /// <inheritdoc/>
        protected override IReadOnlyList<PlayerColor> CastleOwners(Game game) => game.History.Current.Castles().GetOwners();

        /// <inheritdoc/>
        protected override IReadOnlyList<PlayerColor> HeroOwners(Game game) => game.History.Current.Heroes().GetOwners();

        // A player is listed when the savefile has the player's data or one of the player's heroes.
        /// <inheritdoc/>
        protected override IEnumerable<PlayerColor> PlayersInGame(Game game) => game.History.Current.Players().GetColors();

        /// <summary>
        /// Shows the crown and the Grail mark of the castles and the days left of the players with no town as they are in
        /// <paramref name="game"/> now, after an edit or an undo.
        /// </summary>
        public override void RefreshMarks(NavigationTree tree, Game game)
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

        /// <inheritdoc/>
        protected override NavNodeViewModel CreateGroup(NavigationTree tree, Game game, PlayerColor owner)
        {
            var summaries = game.History.Current.Heroes().GetSummaries();
            var heroes = summaries.Where(s => s.Owner == owner).OrderBy(s => s.Name).ToList();
            var castles = game.History.Current.Castles().GetModels()
                .Select((model, index) => (Index: index, Model: model))
                .Where(c => c.Model.Owner == owner)
                .OrderBy(c => c.Model.Name)
                .ToList();
            // A player with data in the savefile is a page of its own (the resources); the others only hold their heroes and towns.
            var playerIndex = owner.IsNeutral ? -1 : game.History.Current.Players().FindByColor(owner);
            var castleModels = castles.ToDictionary(c => c.Index, c => c.Model);
            var node = OwnerNavGroup.Create(
                tree,
                OwnerNames<PlayerColor>.Of(owner),
                PlayerPalette.Of(owner),
                playerIndex,
                !owner.IsNeutral,
                string.Empty,
                heroes.Select(h => new NavEntry(h.Index, h.Name, h.Description)).ToList(),
                castles.Select(c => new NavEntry(c.Index, c.Model.Name, c.Model.Faction.Name)).ToList(),
                (castleNode, index) => this.MarkCastle(castleNode, game, castleModels[index]));
            if (playerIndex >= 0)
            {
                node.Notes = NotesOf(this.banishingText.Nav(game.History.Current.Players().GetModel(playerIndex)));
            }

            return node;
        }
    }
}
