namespace Siniskalc.Core.Data
{
    using System;
    using Siniskalc.Core.Models;

    /// <summary>
    /// Makes the pending change of a record that is a chunk with a model: a change and the way to put it back.
    /// </summary>
    public static class ChunkPendingChange
    {
        /// <summary>
        /// Makes the pending change of the record at <paramref name="index"/> of the chunks of <paramref name="kind"/>; putting it back
        /// edits the model of that record with <paramref name="revert"/>.
        /// </summary>
        /// <typeparam name="TChunk">The chunk of the record.</typeparam>
        /// <typeparam name="TModel">The model of the record.</typeparam>
        /// <param name="kind">The kind of the chunk.</param>
        /// <param name="name">The name shown for the record, such as the name of a hero.</param>
        /// <param name="index">The place of the record among the chunks of its kind.</param>
        /// <param name="change">What changed.</param>
        /// <param name="revert">Puts the model back to what it was.</param>
        /// <returns>The pending change. Its <see cref="PendingChange.Revert"/> opens an <see cref="EditSession{TChunk, TModel}"/> on the record and applies <paramref name="revert"/>; it throws what that session throws, such as <see cref="ArgumentOutOfRangeException"/> when <paramref name="index"/> is no longer a record of <paramref name="kind"/>.</returns>
        public static PendingChange For<TChunk, TModel>(ChunkKind kind, string name, int index, Change change, Func<TModel, TModel> revert)
            where TChunk : BytesChunk
            where TModel : class
        {
            return new PendingChange(name, change, kind, index, history => new EditSession<TChunk, TModel>(history, kind, index).Update(revert));
        }
    }
}
