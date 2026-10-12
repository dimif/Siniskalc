namespace Siniskalc.Core.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// An immutable, ordered collection of the chunks of one kind of struct in a savefile (the heroes, or the players).
    /// It knows how to read its chunks, so it can say what differs from another version of itself.
    /// </summary>
    public abstract class ChunkCollection<TChunk> : IReadOnlyList<TChunk>, IChunkCollection
        where TChunk : BytesChunk
    {
        private readonly IReadOnlyList<TChunk> chunks;

        protected ChunkCollection(IEnumerable<TChunk> chunks)
        {
            this.chunks = chunks.ToList().AsReadOnly();
        }

        public int Count => this.chunks.Count;

        IEnumerable<BytesChunk> IChunkCollection.Chunks => this.chunks;

        public TChunk this[int index] => this.chunks[index];

        /// <summary>
        /// Returns what differs between <paramref name="original"/> and this collection, chunk by chunk.
        /// </summary>
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
                    changes.AddRange(this.DiffChunk(i, original[i], this[i]));
                }
            }

            return changes;
        }

        /// <summary>
        /// Returns the collection with the chunk at <paramref name="index"/> replaced.
        /// </summary>
        public abstract ChunkCollection<TChunk> WithChunk(int index, TChunk chunk);

        /// <summary>
        /// Returns the index of the first chunk that <paramref name="predicate"/> accepts, or -1.
        /// </summary>
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

        public IEnumerator<TChunk> GetEnumerator() => this.chunks.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// Returns the changes that turn <paramref name="original"/> into <paramref name="current"/>, both being the chunk at <paramref name="index"/>.
        /// </summary>
        protected abstract IEnumerable<PendingChange> DiffChunk(int index, TChunk original, TChunk current);

        /// <summary>
        /// Returns the chunks with the one at <paramref name="index"/> replaced.
        /// </summary>
        protected IReadOnlyList<TChunk> Replace(int index, TChunk chunk)
        {
            var result = this.chunks.ToList();
            result[index] = chunk;
            return result;
        }
    }
}
