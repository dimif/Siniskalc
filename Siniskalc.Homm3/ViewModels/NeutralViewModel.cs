namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// The page of what nobody owns: the heroes and towns that belong to no player, and the mines and dwellings of the map nobody has flagged.
    /// A colour that owns things but has no data in the savegame (no player page) gets the same page, for what it owns.
    /// </summary>
    public sealed class NeutralViewModel : NeutralPageViewModelBase
    {
        private readonly SaveDocumentHistory history;
        private readonly PlayerColor owner;
        private readonly Action<int> openHero;
        private readonly Action<int> openCastle;

        public NeutralViewModel(SaveDocumentHistory history, IByteEditTargetFactory byteEditTargets, Action<int> openHero, Action<int> openCastle, PlayerColor? owner = null)
            : base(history, PlayerPalette.Of(owner ?? PlayerColor.Neutral), OwnerNames<PlayerColor>.Of(owner ?? PlayerColor.Neutral))
        {
            this.owner = owner ?? PlayerColor.Neutral;
            this.history = history;
            this.openHero = openHero;
            this.openCastle = openCastle;
            this.MapObjects = new OwnedMapObjectsViewModel(history, this.owner, byteEditTargets);
            this.Refresh();
        }

        /// <summary>
        /// Gets the number the navigation tree knows the page by: -1 for what nobody owns, below that for a colour with no player page.
        /// </summary>
        public int OwnerIndex => IndexOf(this.owner);

        /// <summary>
        /// Gets the number a navigation node of <paramref name="owner"/> that has no player page carries (-1 for neutral).
        /// </summary>
        public static int IndexOf(PlayerColor owner) => owner.IsNeutral ? -1 : -2 - owner.Code;

        /// <summary>
        /// Gets the colour that <see cref="IndexOf"/> numbered <paramref name="index"/>.
        /// </summary>
        public static PlayerColor OwnerOf(int index) => index == -1 ? PlayerColor.Neutral : PlayerColor.FromByte((byte)(-2 - index));

        /// <summary>
        /// Gets the mines and dwellings nobody owns, for the Mines and Dwellings tabs.
        /// </summary>
        public OwnedMapObjectsViewModel MapObjects { get; }

        /// <inheritdoc/>
        protected override string ObjectsSummary =>
            string.Join(" · ", new[] { Count(this.MapObjects.Mines.Rows.Count, "mine", "mines"), Count(this.MapObjects.Dwellings.Count, "dwelling", "dwellings") }.Where(s => s.Length > 0));

        /// <inheritdoc/>
        protected override IReadOnlyList<ILink> LoadHeroes() => OwnedLinks.Heroes(this.history.Current.Heroes(), this.owner, this.openHero);

        /// <inheritdoc/>
        protected override IReadOnlyList<ILink> LoadCastles() => CastleLinks.OwnedBy(this.history.Current, this.owner, this.openCastle, this.openHero);

        /// <inheritdoc/>
        protected override void RefreshObjects() => this.MapObjects?.Refresh();
    }
}
