namespace Siniskalc.Core.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// Immutable snapshot of a savefile: the original bytes plus the current chunks of every <see cref="ChunkKind"/>.
    /// </summary>
    public sealed class SaveDocument
    {
        private static readonly ILogger<SaveDocument> Logger = LogFactory.For<SaveDocument>();

        private readonly byte[] originalBytes;
        private readonly IReadOnlyDictionary<ChunkKind, IChunkCollection> collections;
        private readonly ISavefileBytesPatcher patcher;
        private readonly IDocumentReconciler reconciler;

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveDocument"/> class in which no record follows another.
        /// </summary>
        /// <param name="originalBytes">The original bytes of the savefile.</param>
        /// <param name="collections">The current chunks of every <see cref="ChunkKind"/>.</param>
        /// <param name="patcher">The patcher used to apply changes to the savefile.</param>
        public SaveDocument(byte[] originalBytes, IReadOnlyDictionary<ChunkKind, IChunkCollection> collections, ISavefileBytesPatcher patcher)
            : this(originalBytes, collections, patcher, NoDocumentReconciler.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveDocument"/> class.
        /// </summary>
        /// <param name="originalBytes">The original bytes of the savefile.</param>
        /// <param name="collections">The current chunks of every <see cref="ChunkKind"/>.</param>
        /// <param name="patcher">The patcher used to apply changes to the savefile.</param>
        /// <param name="reconciler">Puts the records that follow others in step after an edit; every document made from this one keeps it.</param>
        public SaveDocument(byte[] originalBytes, IReadOnlyDictionary<ChunkKind, IChunkCollection> collections, ISavefileBytesPatcher patcher, IDocumentReconciler reconciler)
        {
            this.originalBytes = (byte[])originalBytes.Clone();
            this.collections = collections;
            this.patcher = patcher;
            this.reconciler = reconciler;
            Logger.LogDebug("Created a document of {Length} bytes with {Count} kinds of chunks", originalBytes.Length, collections.Count);
        }

        private SaveDocument(SaveDocument source, IReadOnlyDictionary<ChunkKind, IChunkCollection> collections)
        {
            this.originalBytes = source.originalBytes;
            this.collections = collections;
            this.patcher = source.patcher;
            this.reconciler = source.reconciler;
        }

        private IEnumerable<BytesChunk> AllChunks => this.collections.OrderBy(pair => pair.Key).Select(pair => pair.Value).SelectMany(c => c.Chunks).SelectMany(chunk => chunk.PatchParts);

        /// <summary>
        /// Returns the collection of chunks of the given kind.
        /// </summary>
        /// <param name="kind">
        /// The kind of chunks to return.
        /// </param>
        /// <returns>The collection of <paramref name="kind"/>.</returns>
        /// <exception cref="KeyNotFoundException">The document has no chunks of <paramref name="kind"/>.</exception>
        public IChunkCollection GetCollection(ChunkKind kind) => this.collections[kind];

        /// <summary>
        /// Returns the chunks of the given kind.
        /// </summary>
        /// <typeparam name="TChunk">The type of the chunks of <paramref name="kind"/>.</typeparam>
        /// <param name="kind">The kind of chunks to return.</param>
        /// <returns>The chunks of <paramref name="kind"/>.</returns>
        /// <exception cref="KeyNotFoundException">The document has no chunks of <paramref name="kind"/>.</exception>
        /// <exception cref="InvalidCastException">The chunks of <paramref name="kind"/> are not of type <typeparamref name="TChunk"/>.</exception>
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
        /// <param name="kind">The kind of the chunk to replace.</param>
        /// <param name="index">The place of the chunk among those of <paramref name="kind"/>.</param>
        /// <param name="chunk">The chunk to put there.</param>
        /// <returns>A new document; this one is not changed.</returns>
        /// <exception cref="KeyNotFoundException">The document has no chunks of <paramref name="kind"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a place among the chunks of <paramref name="kind"/>.</exception>
        /// <exception cref="InvalidCastException"><paramref name="chunk"/> is not of the type of the chunks of <paramref name="kind"/>.</exception>
        public SaveDocument WithChunk<TChunk>(ChunkKind kind, int index, TChunk chunk)
            where TChunk : BytesChunk
        {
            Logger.LogTrace("Replacing the chunk {Index} of {Kind}", index, kind);
            var replaced = new Dictionary<ChunkKind, IChunkCollection>(this.collections)
            {
                [kind] = this.collections[kind].WithChunk(index, chunk),
            };

            return new SaveDocument(this, replaced);
        }

        /// <summary>
        /// Returns this document, the result of an edit of <paramref name="before"/>, with the records that follow the edited ones put in step
        /// by the reconciler of the savefile (<see cref="IDocumentReconciler"/>).
        /// </summary>
        /// <param name="original">The document as it was loaded or last saved.</param>
        /// <param name="before">The document before the edit.</param>
        /// <returns>This document, or the document the reconciler made from it.</returns>
        /// <exception cref="InvalidOperationException">The edit breaks a rule of the game.</exception>
        public SaveDocument Reconciled(SaveDocument original, SaveDocument before)
        {
            var reconciled = this.reconciler.Reconcile(original, before, this);
            if (ReferenceEquals(reconciled, this))
            {
                Logger.LogDebug("No record had to follow the edit");
            }
            else
            {
                Logger.LogDebug("The reconciler put the records that follow the edit in step");
            }

            return reconciled;
        }

        /// <summary>
        /// Returns what has been edited in this document relative to <paramref name="original"/>,
        /// in kind by kind (heroes first) fashion, and in the order of the savefile inside a kind.
        /// </summary>
        /// <param name="original">The document to measure the edits against, with the same kinds of chunks.</param>
        /// <returns>The pending changes, ordered by <see cref="ChunkKind.Order"/>.</returns>
        /// <exception cref="KeyNotFoundException"><paramref name="original"/> lacks a kind of chunks this document has.</exception>
        /// <exception cref="ArgumentException">A collection of <paramref name="original"/> has another number of chunks.</exception>
        public IReadOnlyList<PendingChange> GetPendingChanges(SaveDocument original)
        {
            var changes = this.collections
                .OrderBy(pair => pair.Key)
                .SelectMany(pair => pair.Value.GetPendingChanges(original.collections[pair.Key]))
                .ToList();
            Logger.LogDebug("The document has {Count} pending changes", changes.Count);
            return changes;
        }

        /// <summary>
        /// Returns the original savefile with the current chunks spliced in.
        /// </summary>
        /// <returns>The bytes of the savefile as it would be saved.</returns>
        /// <exception cref="ArgumentException">A chunk lies outside the savefile or overlaps another.</exception>
        public byte[] ToBytes()
        {
            Logger.LogTrace("Writing the document to bytes");
            return this.patcher.Patch(this.originalBytes, this.AllChunks);
        }

        /// <summary>
        /// Returns a copy of <paramref name="target"/> with only the bytes that this document changed relative to the
        /// original savefile written over it. Edits made to <paramref name="target"/> by other means are kept unless
        /// they touch the same bytes.
        /// </summary>
        /// <param name="target">The bytes to write the changes over, of the same length as the original savefile; not modified.</param>
        /// <returns>The patched copy of <paramref name="target"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="target"/> has another length than the original savefile, or a chunk lies outside it or overlaps another.</exception>
        public byte[] ApplyChangesTo(byte[] target)
        {
            Logger.LogTrace("Applying the changes of the document to {Length} bytes", target.Length);
            return this.patcher.PatchChanges(this.originalBytes, target, this.AllChunks);
        }
    }
}
