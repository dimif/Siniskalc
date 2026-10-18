namespace Siniskalc.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A list of rows the user can sort by clicking the heading of a column; clicking the same heading again reverses the order. The rows are
    /// put in order when a heading is clicked, not each time a row changes, so a row does not jump away while it is being edited. It knows no
    /// game; <see cref="Sort"/> puts the rows in order again.
    /// </summary>
    /// <typeparam name="TRow">The row.</typeparam>
    public sealed class SortableListViewModel<TRow> : AbstractViewModelBase
    {
        private readonly IReadOnlyList<TRow> rows;
        private readonly IReadOnlyDictionary<string, Func<TRow, IComparable>> keys;
        private IReadOnlyList<TRow> items;
        private string? column;
        private bool descending;

        /// <param name="rows">The rows, in the order they have when not sorted.</param>
        /// <param name="keys">For each column, what a row is sorted by.</param>
        /// <param name="initialColumn">The column the list is sorted by first, or null for the order of the rows.</param>
        public SortableListViewModel(IReadOnlyList<TRow> rows, IReadOnlyDictionary<string, Func<TRow, IComparable>> keys, string? initialColumn = null)
        {
            this.rows = rows;
            this.keys = keys;
            this.items = rows;
            this.Columns = keys.Keys.ToDictionary(name => name, name => new SortColumnViewModel(name, new DelegateCommand(_ => this.SortBy(name))));
            if (initialColumn != null)
            {
                this.SortBy(initialColumn);
            }
        }

        /// <summary>
        /// Gets the headings of the columns, by name.
        /// </summary>
        public IReadOnlyDictionary<string, SortColumnViewModel> Columns { get; }

        /// <summary>
        /// Gets the rows in the order the user chose.
        /// </summary>
        public IReadOnlyList<TRow> Items => this.items;

        /// <summary>
        /// Sorts by a column; the second time in a row, in the other direction.
        /// </summary>
        public void SortBy(string name)
        {
            this.descending = this.column == name && !this.descending;
            this.column = name;
            this.Sort();
        }

        /// <summary>
        /// Puts the rows in the order of the column that is chosen again, for when the rows changed.
        /// </summary>
        public void Sort()
        {
            if (this.column != null)
            {
                var key = this.keys[this.column];
                this.items = (this.descending ? this.rows.OrderByDescending(key) : this.rows.OrderBy(key)).ToList();
            }

            foreach (var heading in this.Columns.Values)
            {
                heading.ShowDirection(heading.Name == this.column ? (this.descending ? "▼" : "▲") : string.Empty);
            }

            this.RaisePropertyChanged(nameof(this.Items));
        }
    }
}
