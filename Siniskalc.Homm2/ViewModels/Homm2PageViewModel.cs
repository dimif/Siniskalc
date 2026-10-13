namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A page of the editor about one hero, player or castle: its tab, whether the savegame holds edits of it, and Reset. The page reads
    /// the models of the current document and so is up to date after every edit, undo and redo.
    /// </summary>
    public abstract class Homm2PageViewModel : AbstractViewModelBase, IDisposable
    {
        private readonly ChunkKind kind;
        private readonly int index;
        private int selectedTab;
        private bool hasPendingChanges;

        private readonly IHomm2ByteEditTargetFactory? byteEditTargets;

        protected Homm2PageViewModel(Homm2Game game, ChunkKind kind, int index, IHomm2ByteEditTargetFactory? byteEditTargets = null)
        {
            this.byteEditTargets = byteEditTargets;
            this.Game = game;
            this.kind = kind;
            this.index = index;
            this.ResetCommand = new DelegateCommand(_ => this.Reset(), _ => this.HasPendingChanges);
            game.History.Changed += this.OnHistoryChanged;
        }

        /// <summary>
        /// Gets the position of the tab that is shown, from the first one.
        /// </summary>
        public int SelectedTab
        {
            get => this.selectedTab;
            set
            {
                if (this.selectedTab != value)
                {
                    this.selectedTab = value;
                    this.RaisePropertyChanged(nameof(this.SelectedTab));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the savegame holds edits of what the page is about that are not saved.
        /// </summary>
        public bool HasPendingChanges
        {
            get => this.hasPendingChanges;
            private set
            {
                if (this.hasPendingChanges != value)
                {
                    this.hasPendingChanges = value;
                    this.RaisePropertyChanged(nameof(this.HasPendingChanges));
                }
            }
        }

        /// <summary>
        /// Gets the command that puts everything of the page back to what the savegame had.
        /// </summary>
        public ICommand ResetCommand { get; }

        protected Homm2Game Game { get; }

        public void Dispose() => this.Game.History.Changed -= this.OnHistoryChanged;

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

        protected abstract IByteEditTarget CreateByteEditTarget(IHomm2ByteEditTargetFactory factory);

        private void OnHistoryChanged(object? sender, EventArgs e) => this.Refresh();

        private void Reset()
        {
            foreach (var change in this.PendingChangesOfPage().ToList())
            {
                change.Revert(this.Game.History);
            }
        }

        private System.Collections.Generic.IEnumerable<PendingChange> PendingChangesOfPage() =>
            this.Game.History.GetPendingChanges().Where(c => c.Kind == this.kind && c.Index == this.index);
    }
}
