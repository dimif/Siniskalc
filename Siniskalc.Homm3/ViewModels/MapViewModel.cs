namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The objects of the map that have an owner the savegame keeps: its mines (with the guard of the mine that is picked), its dwellings and
    /// what its merchants have in store. One page with a tab for each kind.
    /// </summary>
    public sealed class MapViewModel : RecordsPageViewModelBase
    {
        private readonly Siniskalc.Homm3.Runtime.Game game;

        public MapViewModel(Siniskalc.Homm3.Runtime.Game game, EffectLineProvider effectLines)
            : base("Map", game.History, Homm3ChunkKinds.Mines, Homm3ChunkKinds.Dwellings, Homm3ChunkKinds.Merchants)
        {
            this.game = game;
            var document = game.History.Current;
            this.Mines = new MinesListViewModel<MineRowViewModel>(
                Enumerable.Range(0, document.Mines().Count).Select(i => new MineRowViewModel(game.History.CreateMineSession(i), game.GameVersion)).ToList());
            this.Mines.PropertyChanged += this.OnMinesChanged;
            this.DwellingList = new SortableListViewModel<MapDwellingRowViewModel>(
                Enumerable.Range(0, document.Dwellings().Count).Select(i => new MapDwellingRowViewModel(game.History.CreateDwellingSession(i))).ToList(),
                new Dictionary<string, Func<MapDwellingRowViewModel, IComparable>>
                {
                    ["Creature"] = d => d.CreatureName,
                    ["Owner"] = d => d.OwnerSort,
                    ["X"] = d => (d.X, d.Y, d.Level),
                    ["Y"] = d => (d.Y, d.X, d.Level),
                    ["Available"] = d => d.Stock,
                });
            this.Merchants = Enumerable.Range(0, document.Merchants().Count).Select(i => new MerchantViewModel(game.History.CreateMerchantSession(i), game.GameVersion, effectLines)).ToList();
            this.Refresh();
        }

        /// <summary>
        /// Gets the mines with the headings that sort them by resource, owner or location, the mine whose guard is shown and how many are owned.
        /// </summary>
        public MinesListViewModel<MineRowViewModel> Mines { get; }

        /// <summary>
        /// Gets the dwellings with the headings that sort them by creature, owner, location or the creatures to hire.
        /// </summary>
        public SortableListViewModel<MapDwellingRowViewModel> DwellingList { get; }

        public IReadOnlyList<MapDwellingRowViewModel> Dwellings => this.DwellingList.Items;

        public bool HasDwellings => this.Dwellings.Count > 0;

        /// <summary>
        /// Gets what the merchants have in store: the artifact merchants and the black market.
        /// </summary>
        public IReadOnlyList<MerchantViewModel> Merchants { get; }

        public bool HasMerchants => this.Merchants.Count > 0;

        public ArmyViewModel? GuardEditor => this.Mines.SelectedMine?.GuardEditor;

        public string GuardHeading => this.Mines.SelectedMine is { } mine ? $"Guard of the {mine.Kind.ToLowerInvariant()} at {mine.PositionText}" : "Guard";

        /// <summary>
        /// Gets what the header says: the mines and the dwellings, each with how many have an owner.
        /// </summary>
        public string MapSummary
        {
            get
            {
                var owned = this.game.History.Current.Dwellings().GetModels().Count(d => !d.Owner.IsNeutral);
                return this.Dwellings.Count == 0 ? this.Mines.Summary : $"{this.Mines.Summary} · {this.Dwellings.Count} dwellings, {owned} of them owned";
            }
        }

        /// <inheritdoc/>
        protected override void RefreshRecords()
        {
            this.Mines.Refresh();

            foreach (var row in this.Dwellings)
            {
                row.Refresh();
            }

            foreach (var merchant in this.Merchants)
            {
                merchant.Refresh();
            }
        }

        private void OnMinesChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Mines.SelectedMine))
            {
                this.RaisePropertyChanged(nameof(this.GuardEditor));
                this.RaisePropertyChanged(nameof(this.GuardHeading));
            }
        }
    }
}
