namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The mines (with the guard of the one that is picked) and the dwellings of the map that one owner has, for the Mines and Dwellings tabs of
    /// that owner's page. A row leaves the lists when its owner is changed to another one, and joins the lists of the new owner.
    /// </summary>
    public sealed class OwnedMapObjectsViewModel : AbstractViewModelBase
    {
        private readonly SaveDocumentHistory history;
        private readonly IByteEditTargetFactory byteEditTargets;
        private readonly GameVersion version;
        private MapDwellingRowViewModel? selectedDwelling;
        private ArmyViewModel? guardEditor;

        public OwnedMapObjectsViewModel(SaveDocumentHistory history, PlayerColor owner, IByteEditTargetFactory byteEditTargets)
        {
            this.history = history;
            this.byteEditTargets = byteEditTargets;
            var ownerName = OwnerNames<PlayerColor>.Of(owner);
            this.version = history.Current.GameVersion();
            this.Mines = new MinesListViewModel<MineRowViewModel>(
                Enumerable.Range(0, history.Current.Mines().Count).Select(i => new MineRowViewModel(history.CreateMineSession(i))).ToList(),
                mine => mine.OwnerSort == ownerName);
            this.DwellingList = new SortableListViewModel<MapDwellingRowViewModel>(
                Enumerable.Range(0, history.Current.Dwellings().Count).Select(i => new MapDwellingRowViewModel(history.CreateDwellingSession(i))).ToList(),
                new Dictionary<string, Func<MapDwellingRowViewModel, IComparable>>
                {
                    ["Creature"] = d => d.CreatureName,
                    ["Owner"] = d => d.OwnerSort,
                    ["X"] = d => (d.X, d.Y, d.Level),
                    ["Y"] = d => (d.Y, d.X, d.Level),
                    ["Available"] = d => d.Stock,
                },
                filter: dwelling => dwelling.OwnerSort == ownerName);
            this.selectedDwelling = this.Dwellings.FirstOrDefault();
            this.Mines.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(this.Mines.SelectedMine))
                {
                    this.guardEditor?.SetTitle(this.GuardHeading);
                    this.guardEditor?.Refresh();
                    this.RaisePropertyChanged(nameof(this.GuardEditor));
                    this.RaisePropertyChanged(nameof(this.GuardHeading));
                }
            };
        }

        /// <summary>
        /// Gets the mines of the owner with the headings that sort them, the mine whose guard is shown and how many there are.
        /// </summary>
        public MinesListViewModel<MineRowViewModel> Mines { get; }

        /// <summary>
        /// Gets the dwellings of the owner with the headings that sort them.
        /// </summary>
        public SortableListViewModel<MapDwellingRowViewModel> DwellingList { get; }

        public IReadOnlyList<MapDwellingRowViewModel> Dwellings => this.DwellingList.Items;

        /// <summary>
        /// Gets or sets the dwelling whose bytes are shown (the first one at first, and never none once there is one). Picking none leaves the dwelling that was picked.
        /// </summary>
        public MapDwellingRowViewModel? SelectedDwelling
        {
            get => this.selectedDwelling;
            set
            {
                if (value != null && !ReferenceEquals(value, this.selectedDwelling))
                {
                    this.selectedDwelling = value;
                    this.RaisePropertyChanged(nameof(this.SelectedDwelling));
                }
            }
        }

        public bool HasMines => this.Mines.HasMines;

        public bool HasDwellings => this.Dwellings.Count > 0;

        /// <summary>
        /// Gets the editor of the guard of the mine that is picked, or <c>null</c> when the owner has no mine.
        /// </summary>
        public ArmyViewModel? GuardEditor => this.Mines.SelectedMine == null ? null : this.guardEditor ??= this.CreateGuardEditor();

        public string GuardHeading => this.Mines.SelectedMine is { } mine ? $"Garrison of the {mine.Kind.ToLowerInvariant()} at {mine.PositionText}" : "Garrison";

        /// <summary>
        /// Gets how many dwellings the owner has, such as "3 dwellings".
        /// </summary>
        public string DwellingsSummary => $"{this.Dwellings.Count} {(this.Dwellings.Count == 1 ? "dwelling" : "dwellings")}";

        /// <summary>
        /// Shows the mines and dwellings again after the history changed, and drops those that have another owner now.
        /// </summary>
        public void Refresh()
        {
            this.Mines.Refresh();
            this.guardEditor?.Refresh();
            this.DwellingList.Sort();
            foreach (var row in this.Dwellings)
            {
                row.Refresh();
            }

            if (this.selectedDwelling == null || !this.Dwellings.Contains(this.selectedDwelling))
            {
                this.selectedDwelling = this.Dwellings.FirstOrDefault();
            }

            this.RaiseAllPropertiesChanged();
        }

        /// <summary>
        /// Creates the raw bytes of the mine that is picked for the hex editor, or <c>null</c> when the owner has none. The caller owns the target and disposes it.
        /// </summary>
        public IByteEditTarget? CreateMineByteEditTarget() =>
            this.Mines.SelectedMine is { } mine ? this.byteEditTargets.ForMine(this.history, mine.Index) : null;

        /// <summary>
        /// Creates the raw bytes of the dwelling that is picked for the hex editor, or <c>null</c> when the owner has none. The caller owns the target and disposes it.
        /// </summary>
        public IByteEditTarget? CreateDwellingByteEditTarget() =>
            this.SelectedDwelling is { } dwelling ? this.byteEditTargets.ForDwelling(this.history, dwelling.Index) : null;

        /// <summary>
        /// Picks the mine with the given place among the mines of the savegame, if the owner has it.
        /// </summary>
        public void PickMine(int mineIndex) => this.Mines.PickMine(mineIndex);

        // One editor serves whichever mine is picked: swapping the editor of the page for another view model rebuilds the whole editor (its
        // seven creature lists and the finder of every creature), which takes most of a second, while refreshing one only updates its values.
        private ArmyViewModel CreateGuardEditor()
        {
            var editor = new ArmyViewModel(
                this.version,
                () => this.Mines.SelectedMine?.Guard ?? new Army<Creature>(Constants.CreatureSlotCount),
                edit => this.Mines.SelectedMine?.UpdateGuard(edit));
            editor.SetTitle(this.GuardHeading);
            return editor;
        }
    }
}
