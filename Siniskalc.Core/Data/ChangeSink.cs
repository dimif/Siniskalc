namespace Siniskalc.Core.Data
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Core.Models;

    /// <summary>
    /// Collects the pending changes a differ finds in one record. The game supplies how a change of its record is put back.
    /// </summary>
    /// <typeparam name="TModel">The model of the record.</typeparam>
    public sealed class ChangeSink<TModel>
    {
        private readonly Func<Change, Func<TModel, TModel>, PendingChange> make;
        private readonly List<PendingChange> changes = [];

        /// <param name="make">Makes the pending change of the record from a change and the way to put it back.</param>
        public ChangeSink(Func<Change, Func<TModel, TModel>, PendingChange> make)
        {
            this.make = make;
        }

        /// <summary>
        /// Gets the changes added so far.
        /// </summary>
        public IReadOnlyList<PendingChange> All => this.changes;

        /// <summary>
        /// Adds a change: what changed in <paramref name="area"/>, its text before and after, and how to put it back.
        /// </summary>
        public void Add(ChangeArea area, string what, string before, string after, Func<TModel, TModel> revert)
        {
            this.changes.Add(this.make(new Change(area, what, before, after), revert));
        }
    }
}
