namespace Siniskalc.Core.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// What a page about one record of a savefile needs of the history of the savefile: to know it changed, which edits are pending, and
    /// to put one of them back.
    /// </summary>
    public interface IPendingChangeHistory : IHistoryEvents
    {
        /// <summary>
        /// Returns what has been edited since the savefile was loaded or last saved.
        /// </summary>
        /// <returns>The pending changes, kind by kind.</returns>
        IReadOnlyList<PendingChange> GetPendingChanges();

        /// <summary>
        /// Puts the value of <paramref name="change"/> back, leaving every other edit alone.
        /// </summary>
        /// <param name="change">A change taken from <see cref="GetPendingChanges"/>.</param>
        void Revert(PendingChange change);
    }
}
