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
        /// <param name="original">An earlier version of this collection, of the same kind of chunks and the same number of them.</param>
        /// <returns>The pending changes, in the order of the chunks.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="original"/> holds another kind of chunks or a different number of them.</exception>
        IReadOnlyList<PendingChange> GetPendingChanges(IChunkCollection original);

        /// <summary>
        /// Returns the collection with the chunk at <paramref name="index"/> replaced.
        /// </summary>
        /// <param name="index">The place of the chunk to replace.</param>
        /// <param name="chunk">The chunk to put there, of the kind the collection holds.</param>
        /// <returns>A new collection; this one is not changed.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="index"/> is not a place in the collection.</exception>
        /// <exception cref="System.InvalidCastException"><paramref name="chunk"/> is not of the kind the collection holds.</exception>
        IChunkCollection WithChunk(int index, BytesChunk chunk);
    }
}
