namespace Siniskalc.Homm2.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Fills the navigation tree with the game (what the savefile contains), which holds one node per player (and one for what nobody
    /// owns). A player's node is its resources page, and holds a "Heroes" group and a "Towns" group.
    /// </summary>
    public sealed class NavigationBuilder : OwnerNavigationBuilderBase<Game, PlayerColor>
    {
        private static readonly IReadOnlyDictionary<NavNodeKind, ChunkKind> Kinds = new Dictionary<NavNodeKind, ChunkKind>
        {
            [NavNodeKind.Player] = Homm2ChunkKinds.Player,
            [NavNodeKind.Hero] = Homm2ChunkKinds.Hero,
            [NavNodeKind.Castle] = Homm2ChunkKinds.Castle,
            [NavNodeKind.Mine] = Homm2ChunkKinds.Mines,
        };

        private readonly BanishingTextProvider banishingText;

        /// <param name="banishingText">Gives the words for a player who owns no town.</param>
        public NavigationBuilder(BanishingTextProvider banishingText)
            : base(Kinds)
        {
            this.banishingText = banishingText;
        }

        /// <inheritdoc/>
        public override void RefreshMarks(NavigationTree tree, Game game)
        {
            var document = game.History.Current;
            var castles = document.Castles().GetModels();
            foreach (var (index, node) in tree.CastleNodes)
            {
                node.Notes = NotesOf(VisitorName(document.Heroes().GetModels(), castles[index]));
            }
        }

        /// <inheritdoc/>
        protected override void AddExtraNodes(NavigationTree tree, Game game, NavNodeViewModel overview)
        {
            var document = game.History.Current;
            var count = document.Mines().Count + document.Shrines().Count + document.Sphinxes().Count;
            if (count == 0)
            {
                return;
            }

            var node = new NavNodeViewModel("Map", $"{document.Mines().Count} mines, {document.Shrines().Count} shrines", PlayerPalette.NoPlayer, NavNodeKind.Map, onSelected: tree.OnSelected);
            tree.Register(node);
            overview.Children.Add(node);
        }

        /// <inheritdoc/>
        protected override IReadOnlyList<PlayerColor> CastleOwners(Game game) => game.History.Current.Castles().GetOwners();

        /// <inheritdoc/>
        protected override IReadOnlyList<PlayerColor> CastleOwnersInUse(Game game) => game.History.Current.Castles().GetModels().Where(c => c.IsUsed).Select(c => c.Owner).ToList();

        /// <inheritdoc/>
        protected override IReadOnlyList<PlayerColor> HeroOwners(Game game) => game.History.Current.Heroes().GetOwners();

        /// <inheritdoc/>
        protected override IReadOnlyList<PlayerColor> MineOwners(Game game) => game.History.Current.Mines().GetModels().Select(m => m.Owner).ToList();

        // A player is listed when it has a castle, a hero, or days left to get a castle.
        /// <inheritdoc/>
        protected override IEnumerable<PlayerColor> PlayersInGame(Game game) => game.History.Current.Players().GetModels()
            .Where(p => p.CastleIds.Count > 0 || p.HeroIds.Count > 0 || p.DaysLeft != null)
            .Select(p => p.Color);

        /// <inheritdoc/>
        protected override NavNodeViewModel CreateGroup(NavigationTree tree, Game game, PlayerColor owner)
        {
            // A colour is a page of its own (the resources); what nobody owns only holds its heroes and towns.
            var document = game.History.Current;
            var allHeroes = document.Heroes().GetModels();
            var castles = document.Castles().GetModels().Where(c => c.IsUsed && c.Owner == owner).OrderBy(c => c.Name).ToList();
            var castleModels = castles.ToDictionary(c => c.Index);
            var player = owner.IsNeutral ? null : document.Players().GetModel(owner.Code);
            return OwnerNavGroup.Create(
                tree,
                OwnerNames<PlayerColor>.Of(owner),
                PlayerPalette.Of(owner),
                owner.IsNeutral ? -1 : owner.Code,
                !owner.IsNeutral,
                player == null ? string.Empty : this.banishingText.Nav(player),
                HeroEntriesOf(document.Heroes(), owner),
                castles.Select(c => new NavEntry(c.Index, c.Name, c.Faction.ToString())).ToList(),
                (node, index) => node.Notes = NotesOf(VisitorName(allHeroes, castleModels[index])),
                MineGroupsOf(document.Mines().GetModels(), owner));
        }

        private static IReadOnlyList<string> NotesOf(string visitor) => visitor.Length > 0 ? [visitor] : [];

        private static string VisitorName(IReadOnlyList<HeroModel> heroes, CastleModel castle) =>
            castle.VisitingHeroId is { } id && id >= 0 && id < heroes.Count ? heroes[id].Name : string.Empty;
    }
}
