namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// The page of what nobody owns: the heroes and towns that belong to no player, and the mines of the map nobody has flagged.
    /// </summary>
    public sealed class NeutralViewModel : NeutralPageViewModelBase, IOwnedMines
    {
        private readonly Game game;
        private readonly Action<int> openHero;
        private readonly Action<int> openCastle;
        private readonly IByteEditTargetFactory? byteEditTargets;

        public NeutralViewModel(Game game, Action<int> openHero, Action<int> openCastle, IByteEditTargetFactory? byteEditTargets = null)
            : base(game.History, PlayerPalette.Of(PlayerColor.Neutral))
        {
            this.game = game;
            this.openHero = openHero;
            this.openCastle = openCastle;
            this.byteEditTargets = byteEditTargets;
            this.Mines = MinesLists.Create(game, PlayerColor.Neutral);
            this.Refresh();
        }

        /// <summary>
        /// Gets the mines nobody owns, for the Mines tab.
        /// </summary>
        public MinesListViewModel<MineRowViewModel> Mines { get; }

        /// <inheritdoc/>
        protected override string ObjectsSummary => Count(this.Mines.Rows.Count, "mine", "mines");

        /// <inheritdoc/>
        public Siniskalc.Core.Data.IByteEditTarget? CreateMineByteEditTarget() =>
            this.Mines.SelectedMine is { } mine ? this.byteEditTargets?.ForMine(this.game.History, mine.Index) : null;

        /// <inheritdoc/>
        protected override IReadOnlyList<ILink> LoadHeroes() => OwnedLinks.Heroes(this.game.History.Current.Heroes(), PlayerColor.Neutral, this.openHero);

        /// <inheritdoc/>
        protected override IReadOnlyList<ILink> LoadCastles() => CastleLinks.OwnedBy(this.game.History.Current, PlayerColor.Neutral, this.openCastle, this.openHero);

        /// <inheritdoc/>
        protected override void RefreshObjects() => this.Mines?.Refresh();
    }
}
