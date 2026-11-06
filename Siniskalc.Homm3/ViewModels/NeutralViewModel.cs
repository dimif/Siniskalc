namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="NeutralViewModel"/> class.
        /// </summary>
        /// <param name="history">The history of the savegame.</param>
        /// <param name="byteEditTargets">Makes the targets of the hex editor.</param>
        /// <param name="openHero">Opens the page of a hero, by its number.</param>
        /// <param name="openCastle">Opens the page of a castle, by its number.</param>
        /// <param name="owner">The colour whose page it is; <see langword="null"/> for what nobody owns.</param>
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
        /// Gets the mines and dwellings nobody owns, for the Mines and Dwellings tabs.
        /// </summary>
        public OwnedMapObjectsViewModel MapObjects { get; }

        /// <inheritdoc/>
        protected override string ObjectsSummary =>
            string.Join(" · ", new[] { Count(this.MapObjects.Mines.Rows.Count, "mine", "mines"), Count(this.MapObjects.Dwellings.Count, "dwelling", "dwellings") }.Where(s => s.Length > 0));

        /// <summary>
        /// Gets the number a navigation node of <paramref name="owner"/> that has no player page carries (-1 for neutral).
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The index.</returns>
        public static int IndexOf(PlayerColor owner) => owner.IsNeutral ? -1 : -2 - owner.Code;

        /// <summary>
        /// Gets the colour that <see cref="IndexOf"/> numbered <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The owner.</returns>
        public static PlayerColor OwnerOf(int index) => index == -1 ? PlayerColor.Neutral : PlayerColor.FromByte((byte)(-2 - index));

        /// <inheritdoc/>
        protected override IReadOnlyList<ILink> LoadHeroes()
        {
            var disabled = this.DisabledIds();
            return OwnedLinks.Heroes(this.history.Current.Heroes(), this.owner, this.openHero, h => !disabled.Contains(h.HeroId));
        }

        /// <inheritdoc/>
        protected override IReadOnlyList<ILink> LoadDisabledHeroes()
        {
            var disabled = this.DisabledIds();
            return OwnedLinks.Heroes(this.history.Current.Heroes(), this.owner, this.openHero, h => disabled.Contains(h.HeroId));
        }

        // Only what nobody owns can hold a hero the game keeps out of play.
        private IReadOnlySet<int> DisabledIds() => this.owner.IsNeutral ? Data.DisabledHeroes.Of(this.history.Current) : new HashSet<int>();

        /// <inheritdoc/>
        protected override IReadOnlyList<ILink> LoadCastles() => CastleLinks.OwnedBy(this.history.Current, this.owner, this.openCastle, this.openHero);

        /// <inheritdoc/>
        protected override void RefreshObjects() => this.MapObjects?.Refresh();
    }
}
