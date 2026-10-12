namespace Siniskalc.Core.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// What <see cref="SaveDocument"/> needs of a collection of chunks without knowing their kind: to patch them into the
    /// savefile, to say what differs from an earlier version, and to replace one.
    /// </summary>
    public interface IChunkCollection
    {
        /// <summary>
        /// Gets the chunks, in the order they are in the savefile.
        /// </summary>
        IEnumerable<BytesChunk> Chunks { get; }

        /// <summary>
        /// Returns what differs between <paramref name="original"/>, an earlier version of this collection, and this one.
        /// </summary>
        IReadOnlyList<PendingChange> GetPendingChanges(IChunkCollection original);

        /// <summary>
        /// Returns the collection with the chunk at <paramref name="index"/> replaced.
        /// </summary>
        IChunkCollection WithChunk(int index, BytesChunk chunk);
    }
}
