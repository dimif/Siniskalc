namespace Siniskalc.Core.Data
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// Undo/redo history over immutable <see cref="SaveDocument"/> snapshots.
    /// There is one history per savefile, shared by every hero edited in it.
    /// </summary>
    public sealed class SaveDocumentHistory : IPendingChangeHistory
    {
        private static readonly ILogger<SaveDocumentHistory> Logger = LogFactory.For<SaveDocumentHistory>();

        private readonly Stack<SaveDocument> undo = new();
        private readonly Stack<SaveDocument> redo = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveDocumentHistory"/> class.
        /// </summary>
        /// <param name="initial">
        /// The initial history of the document.
        /// </param>
        public SaveDocumentHistory(SaveDocument initial)
        {
            this.Current = initial;
            this.Original = initial;
        }

        /// <summary>
        /// Raised whenever <see cref="Current"/> or <see cref="Original"/> changes, whichever hero the change was made to.
        /// </summary>
        /// <inheritdoc/>
        public event EventHandler? Changed;

        public SaveDocument Current { get; private set; }

        /// <summary>
        /// Gets the document as it was last loaded or saved. Pending changes are the difference between this and <see cref="Current"/>.
        /// </summary>
        public SaveDocument Original { get; private set; }

        public bool CanUndo => this.undo.Count > 0;

        public bool CanRedo => this.redo.Count > 0;

        /// <summary>
        /// Makes the current document the one pending changes are measured against, as after saving it.
        /// Undo and redo still work: undoing past this point shows the undone edits as pending changes.
        /// </summary>
        public void MarkSaved()
        {
            Logger.LogDebug("Marking the document as saved ({Undo} undo and {Redo} redo steps kept)", this.undo.Count, this.redo.Count);

            if (ReferenceEquals(this.Original, this.Current))
            {
                return;
            }

            this.Original = this.Current;
            this.Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Returns what has been edited since the savefile was loaded or last saved.
        /// </summary>
        public IReadOnlyList<PendingChange> GetPendingChanges() => this.Current.GetPendingChanges(this.Original);

        /// <summary>
        /// Puts the value of <paramref name="change"/> back through this history, leaving every other edit alone.
        /// </summary>
        public void Revert(PendingChange change)
        {
            Logger.LogDebug("Reverting the pending change {Change}", change.Path);

            change.Revert(this);
        }

        public void Apply(SaveDocument next)
        {
            Logger.LogDebug("Applying an edit on top of {Undo} undo steps", this.undo.Count);

            this.undo.Push(this.Current);
            this.Current = next;
            this.redo.Clear();
            this.Changed?.Invoke(this, EventArgs.Empty);
        }

        public void Undo()
        {
            Logger.LogDebug("Undoing ({Undo} undo steps available)", this.undo.Count);

            if (!this.CanUndo)
            {
                return;
            }

            this.redo.Push(this.Current);
            this.Current = this.undo.Pop();
            this.Changed?.Invoke(this, EventArgs.Empty);
        }

        public void Redo()
        {
            Logger.LogDebug("Redoing ({Redo} redo steps available)", this.redo.Count);

            if (!this.CanRedo)
            {
                return;
            }

            this.undo.Push(this.Current);
            this.Current = this.redo.Pop();
            this.Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
