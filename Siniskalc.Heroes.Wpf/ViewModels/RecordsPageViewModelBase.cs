namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;

    /// <summary>
    /// A page that edits the records of one or more kinds of a savegame (the mines, the shrines, the dwellings of a map): it has pending changes
    /// when a record of those kinds was edited, a reset puts every record of those kinds back, and it shows its records again after every edit,
    /// undo and redo.
    /// </summary>
    public abstract class RecordsPageViewModelBase : ResettablePageViewModelBase
    {
        private readonly SaveDocumentHistory history;
        private readonly IReadOnlyList<ChunkKind> kinds;

        /// <param name="title">The title of the page.</param>
        /// <param name="history">The history of the savegame.</param>
        /// <param name="kinds">The kinds of the records the page edits.</param>
        protected RecordsPageViewModelBase(string title, SaveDocumentHistory history, params ChunkKind[] kinds)
            : base(title, history)
        {
            this.history = history;
            this.kinds = kinds;
        }

        /// <summary>
        /// Shows the records again and says whether the page has pending changes, after the history changed.
        /// </summary>
        public void Refresh()
        {
            this.RefreshRecords();
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
                change.Revert(this.history);
            }
        }

        /// <summary>
        /// Shows the rows of the records again.
        /// </summary>
        protected abstract void RefreshRecords();

        private IEnumerable<PendingChange> PendingChangesOfPage() => this.history.GetPendingChanges().Where(c => this.kinds.Contains(c.Kind));
    }
}
