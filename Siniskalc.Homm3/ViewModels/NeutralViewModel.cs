namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// The page of what nobody owns: the heroes and towns that belong to no player, and the mines and dwellings of the map nobody has flagged.
    /// </summary>
    public sealed class NeutralViewModel : NeutralPageViewModelBase
    {
        private readonly SaveDocumentHistory history;
        private readonly Action<int> openHero;
        private readonly Action<int> openCastle;

        public NeutralViewModel(SaveDocumentHistory history, IByteEditTargetFactory byteEditTargets, Action<int> openHero, Action<int> openCastle)
            : base(history, PlayerPalette.Of(PlayerColor.Neutral))
        {
            this.history = history;
            this.openHero = openHero;
            this.openCastle = openCastle;
            this.MapObjects = new OwnedMapObjectsViewModel(history, PlayerColor.Neutral, byteEditTargets);
            this.Refresh();
        }

        /// <summary>
        /// Gets the mines and dwellings nobody owns, for the Mines and Dwellings tabs.
        /// </summary>
        public OwnedMapObjectsViewModel MapObjects { get; }

        /// <inheritdoc/>
        protected override string ObjectsSummary =>
            string.Join(" · ", new[] { Count(this.MapObjects.Mines.Rows.Count, "mine", "mines"), Count(this.MapObjects.Dwellings.Count, "dwelling", "dwellings") }.Where(s => s.Length > 0));

        /// <inheritdoc/>
        protected override IReadOnlyList<ILink> LoadHeroes() => OwnedLinks.Heroes(this.history.Current.Heroes(), PlayerColor.Neutral, this.openHero);

        /// <inheritdoc/>
        protected override IReadOnlyList<ILink> LoadCastles() => CastleLinks.OwnedBy(this.history.Current, PlayerColor.Neutral, this.openCastle, this.openHero);

        /// <inheritdoc/>
        protected override void RefreshObjects() => this.MapObjects?.Refresh();
    }
}
