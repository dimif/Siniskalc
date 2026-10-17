namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The obelisks of the map and which of them one player has visited, for the page of that player, in any game. A game says how many
    /// obelisks there are, where each is and who has visited it; the tiles, the summary and the buttons that visit all or none are the same.
    /// </summary>
    public abstract class ObeliskVisitsViewModelBase : AbstractViewModelBase
    {
        private readonly ObeliskActionViewModel visitAll;
        private readonly ObeliskActionViewModel clearAll;
        private IReadOnlyList<ObeliskRowViewModel> obelisks = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="ObeliskVisitsViewModelBase"/> class.
        /// </summary>
        /// <param name="puzzleToolTip">What the heading says the obelisks lead to, in the words of the game.</param>
        protected ObeliskVisitsViewModelBase(string puzzleToolTip)
        {
            this.PuzzleToolTip = puzzleToolTip;
            this.VisitAllCommand = new DelegateCommand(_ => this.SetAll(true), _ => this.Count > 0 && this.VisitedCount < this.Count);
            this.ClearAllCommand = new DelegateCommand(_ => this.SetAll(false), _ => this.VisitedCount > 0);
            this.visitAll = new ObeliskActionViewModel("Visit all", "This player has visited every obelisk", this.VisitAllCommand);
            this.clearAll = new ObeliskActionViewModel("None", "This player has visited no obelisk", this.ClearAllCommand);
        }

        /// <summary>
        /// Gets a value indicating whether the savegame has obelisks to show.
        /// </summary>
        public bool HasObelisks => this.Count > 0;

        public IReadOnlyList<ObeliskRowViewModel> Obelisks => this.obelisks;

        /// <summary>
        /// Gets what the page shows as tiles: "Visit all" and "None" first, then the obelisks.
        /// </summary>
        public IReadOnlyList<object> Tiles => [this.visitAll, this.clearAll, .. this.obelisks];

        /// <summary>
        /// Gets how many obelisks the player visited, such as "2 of 7 visited", or nothing when there are none.
        /// </summary>
        public string Summary => this.Count == 0 ? string.Empty : $"{this.VisitedCount} of {this.Count} visited";

        /// <summary>
        /// Gets what the heading says about the obelisks (they show the pieces of a puzzle that leads to the Grail or the Ultimate Artifact).
        /// </summary>
        public string PuzzleToolTip { get; }

        public ICommand VisitAllCommand { get; }

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
                    .Select(i => new ObeliskRowViewModel(i + 1, this.CoordinatesOf(i), () => this.IsVisited(i), visited => this.SetVisited(i, visited)))
                    .ToList();
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
        /// Returns where obelisk <paramref name="index"/> is, such as "X: 34, Y: 58", or nothing when the savegame does not tell.
        /// </summary>
        protected abstract string CoordinatesOf(int index);

        protected abstract bool IsVisited(int index);

        protected abstract void SetVisited(int index, bool visited);

        /// <summary>
        /// Marks every obelisk as visited by the player, or none of them.
        /// </summary>
        protected abstract void SetAll(bool visited);
    }
}
