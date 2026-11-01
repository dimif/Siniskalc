namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The obelisks of the map and which of them one player has visited, for the page of that player, in any game. A game says how many
    /// obelisks there are, where each is and who has visited it; the tiles, the summary and the buttons that visit all or none are the same.
    /// </summary>
    public abstract class ObeliskVisitsViewModelBase : AbstractViewModelBase
    {
        private readonly bool hasLevels;
        private IReadOnlyList<ObeliskRowViewModel> obelisks = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="ObeliskVisitsViewModelBase"/> class.
        /// </summary>
        /// <param name="puzzleToolTip">What the heading says the obelisks lead to, in the words of the game.</param>
        /// <param name="hasLevels">Whether the map of the game has two levels, so the list has a column for the level.</param>
        protected ObeliskVisitsViewModelBase(string puzzleToolTip, bool hasLevels = false)
        {
            this.PuzzleToolTip = puzzleToolTip;
            this.hasLevels = hasLevels;
            this.List = NewList([]);
            this.VisitAllCommand = new DelegateCommand(_ => this.SetAll(true), _ => this.Count > 0 && this.VisitedCount < this.Count);
            this.ClearAllCommand = new DelegateCommand(_ => this.SetAll(false), _ => this.VisitedCount > 0);
        }

        /// <summary>
        /// Gets a value indicating whether the savegame has obelisks to show.
        /// </summary>
        public bool HasObelisks => this.Count > 0;

        /// <summary>
        /// Gets a row for each obelisk of the map.
        /// </summary>
        public IReadOnlyList<ObeliskRowViewModel> Obelisks => this.obelisks;

        /// <summary>
        /// Gets the obelisks with the headings that sort them by name, either coordinate (then the level) or whether they were visited.
        /// </summary>
        public SortableListViewModel<ObeliskRowViewModel> List { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the map has two levels, so the list shows the level of each obelisk.
        /// </summary>
        public bool ShowLevel => this.hasLevels;

        /// <summary>
        /// Gets how many obelisks the player visited, such as "2 of 7 visited", or nothing when there are none.
        /// </summary>
        public string Summary => this.Count == 0 ? string.Empty : $"{this.VisitedCount} of {this.Count} visited";

        /// <summary>
        /// Gets what the heading says about the obelisks (they show the pieces of a puzzle that leads to the Grail or the Ultimate Artifact).
        /// </summary>
        public string PuzzleToolTip { get; }

        /// <summary>
        /// Gets the command that marks every obelisk as visited.
        /// </summary>
        public ICommand VisitAllCommand { get; }

        /// <summary>
        /// Gets the command that marks every obelisk as not visited.
        /// </summary>
        public ICommand ClearAllCommand { get; }

        /// <summary>
        /// Gets how many obelisks the map has, 0 when the savegame has none to show.
        /// </summary>
        protected abstract int Count { get; }

        /// <summary>
        /// Gets how many obelisks the player visited.
        /// </summary>
        protected abstract int VisitedCount { get; }

        /// <summary>
        /// Shows the obelisks again. Call after the savegame changed.
        /// </summary>
        public void Refresh()
        {
            this.Prepare();
            if (this.obelisks.Count != this.Count)
            {
                this.obelisks = Enumerable.Range(0, this.Count)
                    .Select(i => new ObeliskRowViewModel(i + 1, this.PositionOf(i), this.hasLevels, () => this.IsVisited(i), visited => this.SetVisited(i, visited)))
                    .ToList();
                this.List = NewList(this.obelisks);
                this.RaisePropertyChanged(nameof(this.List));
            }

            foreach (var row in this.obelisks)
            {
                row.Refresh();
            }

            this.RaiseAllPropertiesChanged();
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Gets ready to read and edit the obelisks of the current document (a game opens its edit session here).
        /// </summary>
        protected abstract void Prepare();

        /// <summary>
        /// Returns where obelisk <paramref name="index"/> is, or null when the savegame does not tell.
        /// </summary>
        protected abstract MapPosition? PositionOf(int index);

        /// <summary>
        /// Returns whether the obelisk at an index has been visited.
        /// </summary>
        /// <param name="index">The obelisk, from 0.</param>
        /// <returns><see langword="true"/> if it has been visited.</returns>
        protected abstract bool IsVisited(int index);

        /// <summary>
        /// Marks the obelisk at an index as visited or not.
        /// </summary>
        /// <param name="index">The obelisk, from 0.</param>
        /// <param name="visited">Whether it has been visited.</param>
        protected abstract void SetVisited(int index, bool visited);

        /// <summary>
        /// Marks every obelisk as visited by the player, or none of them.
        /// </summary>
        protected abstract void SetAll(bool visited);

        private static SortableListViewModel<ObeliskRowViewModel> NewList(IReadOnlyList<ObeliskRowViewModel> rows) => new(
            rows,
            new Dictionary<string, Func<ObeliskRowViewModel, IComparable>>
            {
                ["Obelisk"] = o => o.Number,
                ["X"] = o => (o.X ?? -1, o.Y ?? -1, o.Position?.Level ?? -1),
                ["Y"] = o => (o.Y ?? -1, o.X ?? -1, o.Position?.Level ?? -1),
                ["Level"] = o => (o.Position?.Level ?? -1, o.X ?? -1, o.Y ?? -1),
                ["Visited"] = o => o.IsVisited,
            });
    }
}
