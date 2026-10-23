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
        public OwnedMapObjectsViewModel(SaveDocumentHistory history, PlayerColor owner)
        {
            var ownerName = OwnerNames<PlayerColor>.Of(owner);
            var version = history.Current.GameVersion();
            this.Mines = new MinesListViewModel<MineRowViewModel>(
                Enumerable.Range(0, history.Current.Mines().Count).Select(i => new MineRowViewModel(history.CreateMineSession(i), version)).ToList(),
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
            this.Mines.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(this.Mines.SelectedMine))
                {
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

        public bool HasMines => this.Mines.HasMines;

        public bool HasDwellings => this.Dwellings.Count > 0;

        /// <summary>
        /// Gets the editor of the guard of the mine that is picked, or <c>null</c> when the owner has no mine.
        /// </summary>
        public ArmyViewModel? GuardEditor => this.Mines.SelectedMine?.GuardEditor;

        public string GuardHeading => this.Mines.SelectedMine is { } mine ? $"Guard of the {mine.Kind.ToLowerInvariant()} at {mine.PositionText}" : "Guard";

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
            this.DwellingList.Sort();
            foreach (var row in this.Dwellings)
            {
                row.Refresh();
            }

            this.RaiseAllPropertiesChanged();
        }

        /// <summary>
        /// Picks the mine with the given place among the mines of the savegame, if the owner has it.
        /// </summary>
        public void PickMine(int mineIndex) => this.Mines.PickMine(mineIndex);
    }
}
