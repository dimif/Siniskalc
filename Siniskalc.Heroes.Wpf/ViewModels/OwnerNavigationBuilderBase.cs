namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
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
        where TColor : struct, Enum
    {
        private readonly IReadOnlyDictionary<NavNodeKind, ChunkKind> kinds;

        protected OwnerNavigationBuilderBase(IReadOnlyDictionary<NavNodeKind, ChunkKind> kinds)
        {
            this.kinds = kinds;
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<NavNodeKind, ChunkKind> ChunkKinds => this.kinds;

        /// <summary>
        /// Returns who owns each castle and hero, as one text that changes when any of them changes hands. Reads the owner bytes only.
        /// </summary>
        public string StructureKey(TGame game) =>
            string.Join(",", this.CastleOwners(game).Select(o => o?.ToString() ?? "-")) + ";" + string.Join(",", this.HeroOwners(game).Select(o => o?.ToString() ?? "-"));

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
        }

        /// <inheritdoc/>
        public abstract void RefreshMarks(NavigationTree tree, TGame game);

        /// <summary>
        /// Gets the owner of each castle of the savefile, in the order of the castles.
        /// </summary>
        protected abstract IReadOnlyList<TColor?> CastleOwners(TGame game);

        /// <summary>
        /// Gets the owners of the castles that are in the game, which can be fewer than the savefile has records for.
        /// </summary>
        protected virtual IReadOnlyList<TColor?> CastleOwnersInUse(TGame game) => this.CastleOwners(game);

        /// <summary>
        /// Gets the owner of each hero of the savefile, in the order of the heroes.
        /// </summary>
        protected abstract IReadOnlyList<TColor?> HeroOwners(TGame game);

        /// <summary>
        /// Gets the colours of the players the savefile has data for. A player listed here has a node, whether it owns anything or not.
        /// </summary>
        protected abstract IEnumerable<TColor> PlayersInGame(TGame game);

        /// <summary>
        /// Creates the node of an owner, with its heroes and towns, or of what nobody owns when <paramref name="owner"/> is <c>null</c>.
        /// </summary>
        protected abstract NavNodeViewModel CreateGroup(NavigationTree tree, TGame game, TColor? owner);

        // A player is listed when the savefile has data for it or it owns a hero or a castle; what nobody owns comes last.
        private IReadOnlyList<TColor?> Owners(TGame game)
        {
            var heroOwners = this.HeroOwners(game);
            var castleOwners = this.CastleOwnersInUse(game);
            var owners = this.PlayersInGame(game)
                .Concat(heroOwners.OfType<TColor>())
                .Concat(castleOwners.OfType<TColor>())
                .Distinct()
                .OrderBy(c => c)
                .Select(c => (TColor?)c)
                .ToList();
            if (heroOwners.Any(o => o == null) || castleOwners.Any(o => o == null))
            {
                owners.Add(null);
            }

            return owners;
        }
    }
}
