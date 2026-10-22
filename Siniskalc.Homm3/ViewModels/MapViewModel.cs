namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The objects of the map that have an owner and a guard the savegame keeps: its mines. One page with the list of the mines, which can be
    /// sorted, and the guard of the mine that is picked.
    /// </summary>
    public sealed class MapViewModel : ResettablePageViewModelBase
    {
        private readonly Siniskalc.Homm3.Runtime.Game game;
        private MineRowViewModel? selectedMine;

        public MapViewModel(Siniskalc.Homm3.Runtime.Game game, EffectLineProvider effectLines)
            : base("Map", game.History)
        {
            this.game = game;
            var document = game.History.Current;
            var rows = Enumerable.Range(0, document.Mines().Count).Select(i => new MineRowViewModel(game.History.CreateMineSession(i), game.GameVersion)).ToList();
            this.MineList = new SortableListViewModel<MineRowViewModel>(
                rows,
                new Dictionary<string, Func<MineRowViewModel, IComparable>>
                {
                    ["Resource"] = m => m.ResourceName,
                    ["Owner"] = m => m.OwnerSort,
                    ["X"] = m => (m.X, m.Y),
                    ["Y"] = m => (m.Y, m.X),
                });
            this.DwellingList = new SortableListViewModel<MapDwellingRowViewModel>(
                Enumerable.Range(0, document.Dwellings().Count).Select(i => new MapDwellingRowViewModel(game.History.CreateDwellingSession(i))).ToList(),
                new Dictionary<string, Func<MapDwellingRowViewModel, IComparable>>
                {
                    ["Creature"] = d => d.CreatureName,
                    ["Owner"] = d => d.OwnerSort,
                    ["X"] = d => (d.X, d.Y),
                    ["Y"] = d => (d.Y, d.X),
                    ["Available"] = d => d.Stock,
                });
            this.Merchants = Enumerable.Range(0, document.Merchants().Count).Select(i => new MerchantViewModel(game.History.CreateMerchantSession(i), game.GameVersion, effectLines)).ToList();
            this.selectedMine = rows.FirstOrDefault();
            this.Refresh();
        }

        /// <summary>
        /// Gets the mines with the headings that sort them by resource, owner or location.
        /// </summary>
        public SortableListViewModel<MineRowViewModel> MineList { get; }

        public IReadOnlyList<MineRowViewModel> Mines => this.MineList.Items;

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

        public bool HasMines => this.Mines.Count > 0;

        /// <summary>
        /// Gets or sets the mine whose guard is shown.
        /// </summary>
        public MineRowViewModel? SelectedMine
        {
            get => this.selectedMine;
            set
            {
                if (value != null && !ReferenceEquals(value, this.selectedMine))
                {
                    this.selectedMine = value;
                    this.RaisePropertyChanged(nameof(this.SelectedMine));
                    this.RaisePropertyChanged(nameof(this.GuardEditor));
                    this.RaisePropertyChanged(nameof(this.GuardHeading));
                }
            }
        }

        public ArmyViewModel? GuardEditor => this.SelectedMine?.GuardEditor;

        public string GuardHeading => this.SelectedMine is { } mine ? $"Guard of the {mine.Kind.ToLowerInvariant()} at {mine.PositionText}" : "Guard";

        /// <summary>
        /// Gets how many mines there are and how many have an owner, such as "18 mines, 4 of them owned".
        /// </summary>
        public string MinesSummary
        {
            get
            {
                var owned = this.game.History.Current.Mines().GetModels().Count(m => !m.Owner.IsNeutral);
                return $"{this.Mines.Count} mines, {owned} of them owned";
            }
        }

        /// <summary>
        /// Gets what the header says: the mines and the dwellings, each with how many have an owner.
        /// </summary>
        public string MapSummary
        {
            get
            {
                var owned = this.game.History.Current.Dwellings().GetModels().Count(d => !d.Owner.IsNeutral);
                return this.Dwellings.Count == 0 ? this.MinesSummary : $"{this.MinesSummary} · {this.Dwellings.Count} dwellings, {owned} of them owned";
            }
        }

        /// <summary>
        /// Shows the mines again, after the history changed.
        /// </summary>
        public void Refresh()
        {
            foreach (var row in this.Mines)
            {
                row.Refresh();
            }

            foreach (var row in this.Dwellings)
            {
                row.Refresh();
            }

            foreach (var merchant in this.Merchants)
            {
                merchant.Refresh();
            }

            this.HasPendingChanges = this.PendingChangesOfPage().Any();
            this.RaiseAllPropertiesChanged();
        }

        /// <inheritdoc/>
        protected override void OnHistoryChanged() => this.Refresh();

        /// <inheritdoc/>
        protected override void Reset()
        {
            foreach (var change in this.PendingChangesOfPage().ToList())
            {
                change.Revert(this.game.History);
            }
        }

        private IEnumerable<Siniskalc.Core.Data.PendingChange> PendingChangesOfPage() =>
            this.game.History.GetPendingChanges().Where(c => c.Kind == Homm3ChunkKinds.Mines || c.Kind == Homm3ChunkKinds.Dwellings || c.Kind == Homm3ChunkKinds.Merchants);
    }
}
