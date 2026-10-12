namespace Siniskalc.Core.Data
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Undo/redo history over immutable <see cref="SaveDocument"/> snapshots.
    /// There is one history per savefile, shared by every hero edited in it.
    /// </summary>
    public sealed class SaveDocumentHistory
    {
        private readonly Stack<SaveDocument> undo = new();
        private readonly Stack<SaveDocument> redo = new();

        public SaveDocumentHistory(SaveDocument initial)
        {
            this.Current = initial;
            this.Original = initial;
        }

        /// <summary>
        /// Raised whenever <see cref="Current"/> or <see cref="Original"/> changes, whichever hero the change was made to.
        /// </summary>
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

        public void Apply(SaveDocument next)
        {
            this.undo.Push(this.Current);
            this.Current = next;
            this.redo.Clear();
            this.Changed?.Invoke(this, EventArgs.Empty);
        }

        public void Undo()
        {
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
