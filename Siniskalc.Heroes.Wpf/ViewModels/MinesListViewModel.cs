namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The mines of a map in any game, as the mines card (<c>MinesCard</c>) shows them: the headings that sort them by resource, owner or place,
    /// the mine that is picked (the first one at first, and never none once there is one) and how many are owned.
    /// </summary>
    /// <typeparam name="TRow">The row of a mine of the game.</typeparam>
    public sealed class MinesListViewModel<TRow> : AbstractViewModelBase
        where TRow : MineRowViewModelBase
    {
        private readonly bool filtered;
        private TRow? selectedMine;

        /// <param name="rows">The mines, in the order of the savegame.</param>
        /// <param name="filter">Says which mines the list shows (those of one owner, say), or null for all.</param>
        public MinesListViewModel(IReadOnlyList<TRow> rows, Func<TRow, bool>? filter = null)
        {
            this.List = new SortableListViewModel<TRow>(
                rows,
                new Dictionary<string, Func<TRow, IComparable>>
                {
                    ["Resource"] = m => m.ResourceName,
                    ["Owner"] = m => m.OwnerSort,
                    ["X"] = m => (m.X, m.Y, m.Level),
                    ["Y"] = m => (m.Y, m.X, m.Level),
                },
                filter: filter);
            this.filtered = filter != null;
            this.selectedMine = this.List.Items.FirstOrDefault();
        }

        /// <summary>
        /// Gets the mines with the headings that sort them by resource, owner or either coordinate (then the level).
        /// </summary>
        public SortableListViewModel<TRow> List { get; }

        /// <summary>
        /// Gets the mines in the order the user chose.
        /// </summary>
        public IReadOnlyList<TRow> Rows => this.List.Items;

        /// <summary>
        /// Gets or sets the mine that is picked in the list. Picking none leaves the mine that was picked.
        /// </summary>
        public TRow? SelectedMine
        {
            get => this.selectedMine;
            set
            {
                if (value != null && !ReferenceEquals(value, this.selectedMine))
                {
                    this.selectedMine = value;
                    this.RaisePropertyChanged(nameof(this.SelectedMine));
                }
            }
        }

        /// <summary>
        /// Picks the mine with the given place among the mines of the savegame, if the list has it.
        /// </summary>
        public void PickMine(int mineIndex) => this.SelectedMine = this.Rows.FirstOrDefault(m => m.Index == mineIndex);

        /// <summary>
        /// Gets a value indicating whether the map has any mine.
        /// </summary>
        public bool HasMines => this.Rows.Count > 0;

        /// <summary>
        /// Gets how many mines there are and how many have an owner, such as "18 mines, 4 of them owned".
        /// </summary>
        public string Summary => $"{this.Rows.Count} mines, {this.Rows.Count(m => m.IsOwned)} of them owned";

        /// <summary>
        /// Shows the mines again, after the history changed. A mine that no longer passes the filter leaves the list, and when it was the
        /// one picked, the first one is picked (none when the list is empty).
        /// </summary>
        public void Refresh()
        {
            foreach (var row in this.Rows)
            {
                row.Refresh();
            }

            // An unfiltered list keeps its order, so a row does not jump away while its owner is being edited.
            if (this.filtered)
            {
                this.List.Sort();
            }

            if (this.selectedMine == null || !this.Rows.Contains(this.selectedMine))
            {
                this.selectedMine = this.Rows.FirstOrDefault();
                this.RaisePropertyChanged(nameof(this.SelectedMine));
            }

            this.RaisePropertyChanged(nameof(this.HasMines));
            this.RaisePropertyChanged(nameof(this.Summary));
        }
    }
}
