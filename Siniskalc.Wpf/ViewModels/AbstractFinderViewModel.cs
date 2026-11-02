namespace Siniskalc.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows.Input;

    /// <summary>
    /// What every finder has: a list of rows grouped under headings, a search text that narrows it,
    /// a summary line, and optionally an action for a row (such as "Equip" or "Add"). Show one with the <c>FinderView</c> control.
    /// Derive from <see cref="FinderViewModel{TRow}"/> to say what the rows are.
    /// </summary>
    public abstract class AbstractFinderViewModel : AbstractViewModelBase
    {
        private string filter = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractFinderViewModel"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="noun">The noun.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="items">The items.</param>
        protected AbstractFinderViewModel(string title, string noun, IReadOnlyList<FinderColumn> columns, ListCollectionView items)
        {
            this.Title = title;
            this.Noun = noun;
            this.Columns = columns;
            this.Items = items;
            this.ChooseCommand = new DelegateCommand(p => this.ChooseRow(p), p => this.HasAction && p != null && this.CanChooseRow(p));
            this.ChooseSecondaryCommand = new DelegateCommand(p => this.ChooseRowSecondary(p), p => this.HasSecondaryAction && p != null && this.CanChooseRowSecondary(p));
        }

        /// <summary>
        /// Gets the heading above the finder; empty for none.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets what the rows are, in the plural, for the summary: "creatures".
        /// </summary>
        public string Noun { get; }

        /// <summary>
        /// Gets the columns of the list, which the header of the list shows and sorts by.
        /// </summary>
        public IReadOnlyList<FinderColumn> Columns { get; }

        /// <summary>
        /// Gets the rows, grouped and narrowed by the search text.
        /// </summary>
        public ListCollectionView Items { get; }

        /// <summary>
        /// Gets or sets the text the rows are narrowed by.
        /// </summary>
        public string Filter
        {
            get => this.filter;
            set
            {
                value ??= string.Empty;
                if (this.filter != value)
                {
                    this.filter = value;
                    this.RaisePropertyChanged(nameof(this.Filter));
                    this.RefreshList();
                }
            }
        }

        /// <summary>
        /// Gets whether a row has an action button.
        /// </summary>
        public bool HasAction { get; private set; }

        /// <summary>
        /// Gets the text of the action button, such as "Equip".
        /// </summary>
        public string ActionLabel { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the command that runs the action on a row. Its parameter is the row.
        /// </summary>
        public ICommand ChooseCommand { get; }

        /// <summary>
        /// Gets whether a row has a second action button, shown after the first.
        /// </summary>
        public bool HasSecondaryAction { get; private set; }

        /// <summary>
        /// Gets the text of the second action button, such as "Backpack".
        /// </summary>
        public string SecondaryActionLabel { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the command that runs the second action on a row. Its parameter is the row.
        /// </summary>
        public ICommand ChooseSecondaryCommand { get; }

        /// <summary>
        /// Gets the property of a row that explains it when the pointer rests on the row, or null for none.
        /// </summary>
        public virtual string? RowToolTipPath => null;

        /// <summary>
        /// Gets what the search box understands, shown when the pointer rests on it.
        /// </summary>
        public virtual string SearchHint => "Type part of a name";

        /// <summary>
        /// Gets the hint shown inside the empty search box, naming what can be searched by.
        /// </summary>
        public virtual string SearchPlaceholder => "Search by name...";

        /// <summary>
        /// Gets whether the rows can be selected with Shift and Ctrl. The finder decides what a selection is for.
        /// </summary>
        public virtual bool AllowsMultiSelect => false;

        /// <summary>
        /// Gets a hint shown under the list.
        /// </summary>
        public virtual string Footer => this.HasAction ? "Double-click a row to use it too." : string.Empty;

        /// <summary>
        /// Gets a line telling what the list shows, such as "93 creatures" or "showing 12 of 93 creatures".
        /// </summary>
        public virtual string Summary
        {
            get
            {
                var shown = this.Items.Count;
                var total = this.TotalCount;
                return shown == total ? $"{shown} {this.Noun}" : $"showing {shown} of {total} {this.Noun}";
            }
        }

        /// <summary>
        /// Gets how many rows there are before anything is narrowed.
        /// </summary>
        protected abstract int TotalCount { get; }

        /// <summary>
        /// Gets the entries of the right-click menu of a row; none by default.
        /// </summary>
        /// <param name="row">The row, as the list holds it, or <see langword="null"/> for no row.</param>
        /// <returns>The menu entries; empty for none.</returns>
        public virtual IReadOnlyList<FinderMenuItem> GetRowMenu(object? row) => [];

        /// <summary>
        /// Runs the action on a row, when it can be run. Does nothing for anything but a row of this finder.
        /// </summary>
        /// <param name="row">The row, as the list holds it, or <see langword="null"/> for no row.</param>
        public abstract void ChooseRow(object? row);

        /// <summary>
        /// Shows the summary and the action buttons again. Call after something they depend on changed.
        /// </summary>
        public void Refresh()
        {
            this.RaisePropertyChanged(nameof(this.Summary));
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Runs the second action on a row, when it can be run.
        /// </summary>
        /// <param name="row">The row, as the list holds it, or <see langword="null"/> for no row.</param>
        public abstract void ChooseRowSecondary(object? row);

        /// <summary>
        /// Returns whether the action can be run on a row.
        /// </summary>
        /// <param name="row">The row, as the list holds it.</param>
        /// <returns><see langword="true"/> if the action can be run on <paramref name="row"/>.</returns>
        protected abstract bool CanChooseRow(object row);

        /// <summary>
        /// Returns whether the second action can be run on a row.
        /// </summary>
        /// <param name="row">The row, as the list holds it.</param>
        /// <returns><see langword="true"/> if the second action can be run on <paramref name="row"/>.</returns>
        protected abstract bool CanChooseRowSecondary(object row);

        /// <summary>
        /// Returns whether a row passes the search text and the filters, so the list shows it.
        /// </summary>
        /// <param name="row">The row, as the list holds it.</param>
        /// <returns><see langword="true"/> if the list shows <paramref name="row"/>.</returns>
        protected abstract bool MatchesRow(object row);

        /// <summary>
        /// Starts narrowing the list. Call at the end of the constructor of the derived class, when what it matches on is set up.
        /// </summary>
        protected void StartFiltering()
        {
            this.Items.Filter = this.MatchesRow;
        }

        /// <summary>
        /// Sets up the second action button.
        /// </summary>
        /// <param name="label">The text of the button.</param>
        protected void EnableSecondaryAction(string label)
        {
            this.HasSecondaryAction = true;
            this.SecondaryActionLabel = label;
        }

        /// <summary>
        /// Sets up the action button.
        /// </summary>
        /// <param name="label">The text of the button.</param>
        protected void EnableAction(string label)
        {
            this.HasAction = true;
            this.ActionLabel = label;
        }

        /// <summary>
        /// Tells whether every word of the search text is part of at least one of <paramref name="fields"/>, or there is no search text.
        /// So "relic helm" finds the helms of relic class, whichever fields hold those words.
        /// </summary>
        /// <returns><see langword="true"/> if every word of the search text is in one of the fields, or there is no search text.</returns>
        /// <param name="fields">The texts of a row to look in; a <see langword="null"/> one is skipped.</param>
        protected bool TextMatches(params string?[] fields)
        {
            var words = this.filter.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return words.All(word => fields.Any(f => f != null && f.Contains(word, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Applies the search text and the filters to the list again and updates the summary.
        /// </summary>
        protected void RefreshList()
        {
            this.Items.Refresh();
            this.RaisePropertyChanged(nameof(this.Summary));
        }
    }
}
