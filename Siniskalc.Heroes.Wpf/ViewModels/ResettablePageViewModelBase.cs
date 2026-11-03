namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Windows.Input;
    using Siniskalc.Core.Data;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A page of the editor about one hero, player or castle: its tabs, whether the savegame holds edits of it, and Reset. The page
    /// listens to the history of the savegame and so is up to date after every edit, undo and redo.
    /// </summary>
    public abstract class ResettablePageViewModelBase : TabbedPageViewModelBase, IResettablePage, IDisposable
    {
        private readonly IHistoryEvents history;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResettablePageViewModelBase"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="history">The history.</param>
        protected ResettablePageViewModelBase(string title, IHistoryEvents history)
            : base(title)
        {
            this.history = history;
            this.ResetCommand = new DelegateCommand(_ => this.Reset(), _ => this.HasPendingChanges);
            history.Changed += this.OnHistoryChangedCore;
        }

        /// <inheritdoc/>
        public ICommand ResetCommand { get; }

        /// <inheritdoc/>
        public void Dispose() => this.history.Changed -= this.OnHistoryChangedCore;

        /// <summary>
        /// Shows what the page is about again, after the history changed (also after saving, which moves the baseline).
        /// </summary>
        protected abstract void OnHistoryChanged();

        /// <summary>
        /// Puts what the page is about back to what the savegame had.
        /// </summary>
        protected abstract void Reset();

        private void OnHistoryChangedCore(object? sender, EventArgs e) => this.OnHistoryChanged();
    }
}
