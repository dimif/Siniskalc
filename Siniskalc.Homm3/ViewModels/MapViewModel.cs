namespace Siniskalc.Homm3.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Data;

    /// <summary>
    /// What the map keeps that no player owns: what its merchants have in store. The mines and dwellings of the map are on the pages of their
    /// owners and on the page of Neutral.
    /// </summary>
    public sealed class MapViewModel : RecordsPageViewModelBase
    {
        public MapViewModel(Siniskalc.Homm3.Runtime.Game game, EffectLineProvider effectLines)
            : base("Map", game.History, Homm3ChunkKinds.Merchants)
        {
            var document = game.History.Current;
            this.Merchants = Enumerable.Range(0, document.Merchants().Count).Select(i => new MerchantViewModel(game.History.CreateMerchantSession(i), game.GameVersion, effectLines, LocationOf(game, i))).ToList();
            this.Refresh();
        }

        /// <summary>
        /// Gets what the merchants have in store: each of the black markets of the map.
        /// </summary>
        public IReadOnlyList<MerchantViewModel> Merchants { get; }

        public bool HasMerchants => this.Merchants.Count > 0;

        /// <summary>
        /// Gets what the header says: how many merchants the map has.
        /// </summary>
        public string MapSummary => $"{this.Merchants.Count} {(this.Merchants.Count == 1 ? "merchant" : "merchants")}";

        /// <inheritdoc/>
        protected override void RefreshRecords()
        {
            foreach (var merchant in this.Merchants)
            {
                merchant.Refresh();
            }
        }

        private static string LocationOf(Siniskalc.Homm3.Runtime.Game game, int merchant) =>
            game.Places.Merchants.Where(m => m.Number == merchant).Select(m => $"At {MapLevelText.Coordinates(m.Position.X, m.Position.Y)}").FirstOrDefault() ?? string.Empty;
    }
}
