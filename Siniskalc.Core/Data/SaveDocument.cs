namespace Siniskalc.Core.Data
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Immutable snapshot of a savefile: the original bytes plus the current chunks of every <see cref="ChunkKind"/>.
    /// </summary>
    public sealed class SaveDocument
    {
        private readonly byte[] originalBytes;
        private readonly IReadOnlyDictionary<ChunkKind, IChunkCollection> collections;
        private readonly ISavefileBytesPatcher patcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveDocument"/> class.
        /// </summary>
        /// <param name="originalBytes">The original bytes of the savefile.</param>
        /// <param name="collections">The current chunks of every <see cref="ChunkKind"/>.</param>
        /// <param name="patcher">The patcher used to apply changes to the savefile.</param>
        public SaveDocument(byte[] originalBytes, IReadOnlyDictionary<ChunkKind, IChunkCollection> collections, ISavefileBytesPatcher patcher)
        {
            this.originalBytes = (byte[])originalBytes.Clone();
            this.collections = collections;
            this.patcher = patcher;
        }

        private SaveDocument(SaveDocument source, IReadOnlyDictionary<ChunkKind, IChunkCollection> collections)
        {
            this.originalBytes = source.originalBytes;
            this.collections = collections;
            this.patcher = source.patcher;
        }

        private IEnumerable<BytesChunk> AllChunks => this.collections.OrderBy(pair => pair.Key).Select(pair => pair.Value).SelectMany(c => c.Chunks).SelectMany(chunk => chunk.PatchParts);

        /// <summary>
        /// Returns the collection of chunks of the given kind.
        /// </summary>
        /// <param name="kind">
        /// The kind of chunks to return.
        /// </param>
        public IChunkCollection GetCollection(ChunkKind kind) => this.collections[kind];

        /// <summary>
        /// Returns the chunks of the given kind.
        /// </summary>
        public ChunkCollection<TChunk> GetChunks<TChunk>(ChunkKind kind)
            where TChunk : BytesChunk
        {
            return (ChunkCollection<TChunk>)this.collections[kind];
        }

        /// <summary>
        /// Returns the document with the chunk of the given kind at <paramref name="index"/> replaced.
        /// </summary>
        /// <typeparam name="TChunk">
        /// The type of chunk to replace. Must be the same as the type of the chunk at <paramref name="index"/>.
        /// </typeparam>
        public SaveDocument WithChunk<TChunk>(ChunkKind kind, int index, TChunk chunk)
            where TChunk : BytesChunk
        {
            var replaced = new Dictionary<ChunkKind, IChunkCollection>(this.collections)
            {
                [kind] = this.collections[kind].WithChunk(index, chunk),
            };

            return new SaveDocument(this, replaced);
        }

        /// <summary>
        /// Returns what has been edited in this document relative to <paramref name="original"/>,
        /// in kind by kind (heroes first) fashion, and in the order of the savefile inside a kind.
        /// </summary>
        public IReadOnlyList<PendingChange> GetPendingChanges(SaveDocument original)
        {
            return this.collections
                .OrderBy(pair => pair.Key)
                .SelectMany(pair => pair.Value.GetPendingChanges(original.collections[pair.Key]))
                .ToList();
        }

        /// <summary>
        /// Returns the original savefile with the current chunks spliced in.
        /// </summary>
        public byte[] ToBytes() => this.patcher.Patch(this.originalBytes, this.AllChunks);

        /// <summary>
        /// Returns a copy of <paramref name="target"/> with only the bytes that this document changed relative to the
        /// original savefile written over it. Edits made to <paramref name="target"/> by other means are kept unless
        /// they touch the same bytes.
        /// </summary>
        public byte[] ApplyChangesTo(byte[] target) => this.patcher.PatchChanges(this.originalBytes, target, this.AllChunks);
    }
}
