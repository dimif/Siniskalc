namespace Siniskalc.Core.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// Lists what an edit changed in one record of a savefile (a hero, a castle, a player), as pending changes that can each be put back.
    /// </summary>
    /// <typeparam name="TModel">
    /// The model of the record.
    /// </typeparam>
    public interface IDiffer<in TModel>
    {
        /// <summary>
        /// Returns the changes from <paramref name="original"/> to <paramref name="current"/>, the record being the one at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">
        /// The index of the record.
        /// </param>
        /// <param name="original">
        /// The original record.
        /// </param>
        /// <param name="current">
        /// The current record.
        /// </param>
        /// <returns>
        /// A read-only list of pending changes.
        /// </returns>
        IReadOnlyList<PendingChange> Diff(int index, TModel original, TModel current);
    }
}
