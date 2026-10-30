namespace Siniskalc.Homm2.ViewModels
{
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;

    /// <summary>
    /// A page of the editor about one hero, player or castle: its tab, whether the savegame holds edits of it, and Reset. The page reads
    /// the models of the current document and so is up to date after every edit, undo and redo.
    /// </summary>
    public abstract class PageViewModel : ResettablePageViewModelBase
    {
        private readonly ChunkKind kind;
        private readonly int index;

        private readonly IPendingChangeHistory history;
        private readonly IByteEditTargetFactory? byteEditTargets;

        protected PageViewModel(IPendingChangeHistory history, ChunkKind kind, int index, IByteEditTargetFactory? byteEditTargets = null)
            : base(kind.ToString(), history)
        {
            this.history = history;
            this.byteEditTargets = byteEditTargets;
            this.kind = kind;
            this.index = index;
        }

        /// <summary>
        /// Reads what the page shows from the current document. Called by the page once it is built, and after every change.
        /// </summary>
        public void Refresh()
        {
            this.Read();
            this.HasPendingChanges = this.PendingChangesOfPage().Any();
            this.RaiseAllPropertiesChanged();
        }

        /// <summary>
        /// Creates the target the hex tab of the page works on: the raw bytes of what the page is about. The caller disposes it. <c>null</c>
        /// when the editor was built without the means to make one, and the page has no hex tab.
        /// </summary>
        public IByteEditTarget? CreateByteEditTarget() => this.byteEditTargets == null ? null : this.CreateByteEditTarget(this.byteEditTargets);

        protected abstract void Read();

        protected abstract IByteEditTarget CreateByteEditTarget(IByteEditTargetFactory factory);

        /// <inheritdoc/>
        protected override void OnHistoryChanged() => this.Refresh();

        /// <inheritdoc/>
        protected override void Reset()
        {
            foreach (var change in this.PendingChangesOfPage().ToList())
            {
                this.history.Revert(change);
            }
        }

        private System.Collections.Generic.IEnumerable<PendingChange> PendingChangesOfPage() =>
            this.history.GetPendingChanges().Where(c => c.Kind == this.kind && c.Index == this.index);
    }
}
