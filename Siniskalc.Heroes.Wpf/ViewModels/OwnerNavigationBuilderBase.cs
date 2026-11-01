namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Entities;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Fills the navigation tree of a game of the series: an overview node, and below it one node for each player in the game and one for
    /// what nobody owns. A game supplies how it names the owners and what it shows on their nodes.
    /// </summary>
    /// <typeparam name="TGame">The game the tree is built for.</typeparam>
    /// <typeparam name="TColor">The colour of the game, which is an enumeration of its own.</typeparam>
    public abstract class OwnerNavigationBuilderBase<TGame, TColor> : INavigationBuilder<TGame>
        where TGame : AbstractSavegame
        where TColor : AbstractEnumeration, IPlayerColor
    {
        private readonly IReadOnlyDictionary<NavNodeKind, ChunkKind> kinds;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwnerNavigationBuilderBase"/> class.
        /// </summary>
        /// <param name="kinds">The kinds.</param>
        protected OwnerNavigationBuilderBase(IReadOnlyDictionary<NavNodeKind, ChunkKind> kinds)
        {
            this.kinds = kinds;
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<NavNodeKind, ChunkKind> ChunkKinds => this.kinds;

        /// <summary>
        /// Returns who owns each castle, hero, mine and dwelling, as one text that changes when any of them changes hands. Reads the owner bytes only.
        /// </summary>
        public string StructureKey(TGame game) =>
            string.Join(",", this.CastleOwners(game).Select(o => o.ToString())) + ";" + string.Join(",", this.HeroOwners(game).Select(o => o.ToString()))
            + ";" + string.Join(",", this.MineOwners(game).Select(o => o.ToString())) + ";" + string.Join(",", this.DwellingOwners(game).Select(o => o.ToString()));

        /// <inheritdoc/>
        public void Build(NavigationTree tree, TGame game)
        {
            var overview = new NavNodeViewModel("Game", string.IsNullOrEmpty(game.Name) ? string.Empty : System.IO.Path.GetFileName(game.Name), PlayerPalette.NoPlayer, NavNodeKind.Overview, onSelected: tree.OnSelected)
            {
                DefaultExpanded = true,
                IsExpanded = true,
            };
            tree.SetOverview(overview);

            foreach (var owner in this.Owners(game))
            {
                overview.Children.Add(this.CreateGroup(tree, game, owner));
            }

            this.AddExtraNodes(tree, game, overview);
        }

        /// <inheritdoc/>
        public abstract void RefreshMarks(NavigationTree tree, TGame game);

        /// <summary>
        /// Returns the entry of each hero <paramref name="owner"/> owns, ordered by name.
        /// </summary>
        /// <param name="heroes">The heroes of the savefile.</param>
        /// <param name="owner">The colour whose heroes are wanted.</param>
        protected static IReadOnlyList<NavEntry> HeroEntriesOf(IHeroSummaries<TColor> heroes, TColor owner) =>
            heroes.GetSummaries()
                .Where(s => s.Owner == owner)
                .OrderBy(s => s.Name)
                .Select(s => new NavEntry(s.Index, s.Name, s.Description))
                .ToList();

        /// <summary>
        /// Returns the mines <paramref name="owner"/> owns in groups, one for each resource they give, in the order of the resources.
        /// </summary>
        /// <param name="mines">The mines of the map.</param>
        /// <param name="owner">The colour whose mines are wanted.</param>
        /// <param name="hasLevels">Whether the map of the game has two levels, so a place says which level it is on.</param>
        protected static IReadOnlyList<NavGroup> MineGroupsOf(IEnumerable<IMineModel<TColor>> mines, TColor owner, bool hasLevels) =>
            mines
                .Where(m => m.Owner == owner)
                .GroupBy(m => MineKinds.ResourceOf(m.Kind))
                .OrderBy(g => g.Key)
                .Select(g => new NavGroup(g.Key.ToString(), g.OrderBy(m => m.Position.Level).ThenBy(m => m.Position.X).ThenBy(m => m.Position.Y).Select(m => new NavEntry(m.Index, MapLevelText.At(m.Position, hasLevels), string.Empty)).ToList()))
                .ToList();

        /// <summary>
        /// Adds the nodes a game has besides its players, under the node of the game. None by default.
        /// </summary>
        protected virtual void AddExtraNodes(NavigationTree tree, TGame game, NavNodeViewModel overview)
        {
        }

        /// <summary>
        /// Gets the owner of each castle of the savefile, in the order of the castles.
        /// </summary>
        protected abstract IReadOnlyList<TColor> CastleOwners(TGame game);

        /// <summary>
        /// Gets the owners of the castles that are in the game, which can be fewer than the savefile has records for.
        /// </summary>
        protected virtual IReadOnlyList<TColor> CastleOwnersInUse(TGame game) => this.CastleOwners(game);

        /// <summary>
        /// Gets the owner of each hero of the savefile, in the order of the heroes.
        /// </summary>
        protected abstract IReadOnlyList<TColor> HeroOwners(TGame game);

        /// <summary>
        /// Gets the owner of each mine of the map, in the order of the mines. None by default.
        /// </summary>
        protected virtual IReadOnlyList<TColor> MineOwners(TGame game) => [];

        /// <summary>
        /// Gets the owner of each dwelling of the map, in the order of the dwellings. None by default, for a game whose map has none.
        /// </summary>
        protected virtual IReadOnlyList<TColor> DwellingOwners(TGame game) => [];

        /// <summary>
        /// Gets the colours of the players the savefile has data for. A player listed here has a node, whether it owns anything or not.
        /// </summary>
        protected abstract IEnumerable<TColor> PlayersInGame(TGame game);

        /// <summary>
        /// Creates the node of an owner, with its heroes and towns, or of what nobody owns when <paramref name="owner"/> is neutral.
        /// </summary>
        protected abstract NavNodeViewModel CreateGroup(NavigationTree tree, TGame game, TColor owner);

        // A player is listed when the savefile has data for it or it owns a hero or a castle; what nobody owns comes last, as neutral has the highest code.
        private IReadOnlyList<TColor> Owners(TGame game)
        {
            return Siniskalc.Heroes.Models.PlayersInGame.Of(this.PlayersInGame(game), this.HeroOwners(game), this.CastleOwnersInUse(game));
        }
    }
}
