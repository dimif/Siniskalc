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
            Logger.LogTrace("Created a history");
        }

        /// <summary>
        /// Raised whenever <see cref="Current"/> or <see cref="Original"/> changes, whichever hero the change was made to.
        /// </summary>
        /// <inheritdoc/>
        public event EventHandler? Changed;

        /// <summary>
        /// Gets the document with every edit applied, undone ones excluded.
        /// </summary>
        public SaveDocument Current { get; private set; }

        /// <summary>
        /// Gets the document as it was last loaded or saved. Pending changes are the difference between this and <see cref="Current"/>.
        /// </summary>
        public SaveDocument Original { get; private set; }

        /// <summary>
        /// Gets a value indicating whether there is an edit to undo.
        /// </summary>
        public bool CanUndo => this.undo.Count > 0;

        /// <summary>
        /// Gets a value indicating whether there is an undone edit to redo.
        /// </summary>
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
        /// <returns>The pending changes, kind by kind.</returns>
        public IReadOnlyList<PendingChange> GetPendingChanges() => this.Current.GetPendingChanges(this.Original);

        /// <summary>
        /// Puts the value of <paramref name="change"/> back through this history, leaving every other edit alone.
        /// </summary>
        /// <param name="change">A change taken from <see cref="GetPendingChanges"/>.</param>
        /// <exception cref="InvalidOperationException">The reconciler of the savefile refuses the reverted edit.</exception>
        public void Revert(PendingChange change)
        {
            Logger.LogDebug("Reverting the pending change {Change}", change.Path);

            change.Revert(this);
        }

        /// <summary>
        /// Records an edit: <paramref name="next"/>, with the records that follow the edited ones put in step by the reconciler of the savefile
        /// (<see cref="SaveDocument.Reconciled"/>), becomes the current document in one step of the history. When the reconciler refuses the
        /// edit, the history is left as it was.
        /// </summary>
        /// <param name="next">The edited document.</param>
        /// <exception cref="InvalidOperationException">The reconciler of the savefile refuses the edit.</exception>
        public void Apply(SaveDocument next) => this.ApplyAsIs(next.Reconciled(this.Original, this.Current));

        /// <summary>
        /// Records <paramref name="next"/> as it is, in one step of the history, without putting other records in step: an edit of the raw
        /// bytes, or a document that is already whole (such as the original).
        /// </summary>
        /// <param name="next">The document to make current.</param>
        public void ApplyAsIs(SaveDocument next)
        {
            Logger.LogDebug("Applying an edit on top of {Undo} undo steps", this.undo.Count);

            this.undo.Push(this.Current);
            this.Current = next;
            this.redo.Clear();
            this.Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Goes back to the document before the last edit, which can then be redone. Does nothing when there is no edit to undo.
        /// </summary>
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

        /// <summary>
        /// Goes forward again to the edit that was undone last. Does nothing when there is no undone edit.
        /// </summary>
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
