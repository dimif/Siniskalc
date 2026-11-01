namespace Siniskalc.Core.Data
{
    /// <summary>
    /// Keeps the records of a savefile that only follow others in step after an edit: what a game stores twice (a list of what a player owns
    /// beside the owner kept in each thing) is put right here, so that a model and a page edit only the record the value belongs to. It runs
    /// on every edit that goes through <see cref="SaveDocumentHistory.Apply"/>, never on load, undo or redo, and not on an edit of the raw bytes
    /// (<see cref="SaveDocumentHistory.ApplyAsIs"/>).
    /// </summary>
    public interface IDocumentReconciler
    {
        /// <summary>
        /// Returns <paramref name="after"/> with the records that follow the edited ones put in step, or <paramref name="after"/> itself when
        /// nothing has to follow. A record the edit did not touch is returned as it is, so a document that is only loaded is never written to.
        /// </summary>
        /// <param name="original">The document as it was loaded or last saved, which says where a thing that comes back goes.</param>
        /// <param name="before">The document before the edit.</param>
        /// <param name="after">The document with the edit.</param>
        /// <returns><paramref name="after"/>, or a new document made from it with the following records put in step.</returns>
        /// <exception cref="System.InvalidOperationException">The edit breaks a rule of the game; the history is left as it was.</exception>
        SaveDocument Reconcile(SaveDocument original, SaveDocument before, SaveDocument after);
    }
}
