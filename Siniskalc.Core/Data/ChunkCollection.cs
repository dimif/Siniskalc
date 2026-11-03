namespace Siniskalc.Core.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// An immutable, ordered collection of the chunks of one kind of struct in a savefile (the heroes, or the players).
    /// It knows how to read its chunks, so it can say what differs from another version of itself.
    /// </summary>
    /// <typeparam name="TChunk">The chunk of a struct.</typeparam>
    public abstract class ChunkCollection<TChunk> : IReadOnlyList<TChunk>, IChunkCollection
        where TChunk : BytesChunk
    {
        private static readonly ILogger Logger = LogFactory.For(typeof(ChunkCollection<>));

        private readonly IReadOnlyList<TChunk> chunks;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChunkCollection{TChunk}"/> class.
        /// </summary>
        /// <param name="chunks">The chunks, in the order of the savefile. They are copied into the collection.</param>
        protected ChunkCollection(IEnumerable<TChunk> chunks)
        {
            this.chunks = chunks.ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets the number of chunks.
        /// </summary>
        public int Count => this.chunks.Count;

        IEnumerable<BytesChunk> IChunkCollection.Chunks => this.chunks;

        /// <summary>
        /// Gets the chunk at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The place of the chunk in the collection.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative or not less than <see cref="Count"/>.</exception>
        public TChunk this[int index] => this.chunks[index];

        /// <summary>
        /// Returns what differs between <paramref name="original"/> and this collection, chunk by chunk.
        /// </summary>
        /// <param name="original">An earlier version of this collection, with the same number of chunks.</param>
        /// <returns>The pending changes of every chunk whose bytes differ from the same chunk of <paramref name="original"/>, in the order of the chunks.</returns>
        /// <exception cref="ArgumentException"><paramref name="original"/> does not have as many chunks as this collection.</exception>
        /// <exception cref="NullReferenceException"><paramref name="original"/> is <see langword="null"/>.</exception>
        public IReadOnlyList<PendingChange> GetPendingChanges(ChunkCollection<TChunk> original)
        {
            if (original.Count != this.Count)
            {
                throw new ArgumentException("The original must have the same number of chunks.", nameof(original));
            }

            var changes = new List<PendingChange>();

            for (var i = 0; i < this.Count; i++)
            {
                // Chunks that were not edited keep their instance, so this skips them cheaply.
                if (!ReferenceEquals(original[i], this[i]) && !original[i].HasSameBytes(this[i]))
                {
                    Logger.LogTrace("Chunk {Index} ({Name}) differs from the original", i, this[i].Name);
                    changes.AddRange(this.DiffChunk(i, original[i], this[i]));
                }
            }

            Logger.LogDebug("Found {Count} pending changes", changes.Count);
            return changes;
        }

        /// <summary>
        /// Returns the collection with the chunk at <paramref name="index"/> replaced.
        /// </summary>
        /// <param name="index">The place of the chunk to replace.</param>
        /// <param name="chunk">The chunk to put there.</param>
        /// <returns>A new collection; this one is not changed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative or not less than <see cref="Count"/>.</exception>
        public abstract ChunkCollection<TChunk> WithChunk(int index, TChunk chunk);

        /// <summary>
        /// Returns the index of the first chunk with the given name (case does not matter), or -1.
        /// </summary>
        /// <param name="name">The name to look for.</param>
        /// <returns>The index of the first named chunk called <paramref name="name"/>, or -1 if there is none.</returns>
        public int FindByName(string name)
        {
            var index = this.IndexOf(c => this.IsNamed(c) && string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
            Logger.LogTrace("Looked for the chunk {Name} and found it at {Index}", name, index);
            return index;
        }

        /// <summary>
        /// Returns the index of the first chunk that <paramref name="predicate"/> accepts, or -1.
        /// </summary>
        /// <param name="predicate">Says whether a chunk is the one wanted.</param>
        /// <returns>The index of the first chunk <paramref name="predicate"/> accepts, or -1 if it accepts none.</returns>
        public int IndexOf(Func<TChunk, bool> predicate)
        {
            for (var i = 0; i < this.chunks.Count; i++)
            {
                if (predicate(this.chunks[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        IReadOnlyList<PendingChange> IChunkCollection.GetPendingChanges(IChunkCollection original)
        {
            return this.GetPendingChanges(original as ChunkCollection<TChunk> ?? throw new ArgumentException("The original must hold the same kind of chunks.", nameof(original)));
        }

        IChunkCollection IChunkCollection.WithChunk(int index, BytesChunk chunk) => this.WithChunk(index, (TChunk)chunk);

        /// <summary>
        /// Returns an enumerator over the chunks, in order.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<TChunk> GetEnumerator() => this.chunks.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// Returns whether the chunk has a name that can be looked up. A game whose savefile holds records the map does not use (they have no
        /// name) says so here.
        /// </summary>
        /// <param name="chunk">The chunk to ask about.</param>
        /// <returns><see langword="true"/> if <see cref="FindByName"/> may match the name of <paramref name="chunk"/>; the default is always <see langword="true"/>.</returns>
        protected virtual bool IsNamed(TChunk chunk) => true;

        /// <summary>
        /// Returns the changes that turn <paramref name="original"/> into <paramref name="current"/>, both being the chunk at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The place of the chunk in the collection.</param>
        /// <param name="original">The chunk as it was in the earlier version.</param>
        /// <param name="current">The chunk as it is now.</param>
        /// <returns>The changes, which may be none.</returns>
        protected abstract IEnumerable<PendingChange> DiffChunk(int index, TChunk original, TChunk current);

        /// <summary>
        /// Returns the chunks with the one at <paramref name="index"/> replaced.
        /// </summary>
        /// <param name="index">The place of the chunk to replace.</param>
        /// <param name="chunk">The chunk to put there.</param>
        /// <returns>A new list of the chunks; this collection is not changed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative or not less than <see cref="Count"/>.</exception>
        protected IReadOnlyList<TChunk> Replace(int index, TChunk chunk)
        {
            var result = this.chunks.ToList();
            result[index] = chunk;
            return result;
        }
    }
}
