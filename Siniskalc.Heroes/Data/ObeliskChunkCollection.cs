namespace Siniskalc.Heroes.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// The visited obelisks of a savefile: one chunk, or none when the savefile has no obelisks or they are not where they are looked for.
    /// </summary>
    /// <typeparam name="TColor">The colour of the game.</typeparam>
    public sealed class ObeliskChunkCollection<TColor> : ChunkCollection<ObeliskChunk>, IModelChunkCollection<ObeliskChunk, ObeliskVisits<TColor>>
        where TColor : AbstractEnumeration, IPlayerColor
    {
        private readonly IDiffer<ObeliskVisits<TColor>> differ;

        public ObeliskChunkCollection(
            IEnumerable<ObeliskChunk> chunks,
            IDeserializer<ObeliskChunk, ObeliskVisits<TColor>> deserializer,
            ISerializer<ObeliskChunk, ObeliskVisits<TColor>> serializer,
            IDiffer<ObeliskVisits<TColor>> differ)
            : base(chunks)
        {
            this.Deserializer = deserializer;
            this.Serializer = serializer;
            this.differ = differ;
        }

        /// <summary>
        /// Gets what reads the visited obelisks from a chunk.
        /// </summary>
        public IDeserializer<ObeliskChunk, ObeliskVisits<TColor>> Deserializer { get; }

        /// <summary>
        /// Gets what writes the visited obelisks to a chunk.
        /// </summary>
        public ISerializer<ObeliskChunk, ObeliskVisits<TColor>> Serializer { get; }

        /// <inheritdoc/>
        IDeserializer<ObeliskChunk, ObeliskVisits<TColor>> IModelChunkCollection<ObeliskChunk, ObeliskVisits<TColor>>.ChunkDeserializer => this.Deserializer;

        /// <inheritdoc/>
        ISerializer<ObeliskChunk, ObeliskVisits<TColor>> IModelChunkCollection<ObeliskChunk, ObeliskVisits<TColor>>.ChunkSerializer => this.Serializer;

        /// <summary>
        /// Gets whether the savefile has obelisks to show.
        /// </summary>
        public bool Any => this.Count > 0;

        /// <inheritdoc/>
        public override ObeliskChunkCollection<TColor> WithChunk(int index, ObeliskChunk chunk) => new(this.Replace(index, chunk), this.Deserializer, this.Serializer, this.differ);

        /// <summary>
        /// Returns the visited obelisks, or null when the savefile has none to show.
        /// </summary>
        public ObeliskVisits<TColor>? GetModel() => this.Any ? this.Deserializer.Deserialize(this[0]) : null;

        /// <inheritdoc/>
        protected override IEnumerable<PendingChange> DiffChunk(int index, ObeliskChunk original, ObeliskChunk current)
        {
            return this.differ.Diff(index, this.Deserializer.Deserialize(original), this.Deserializer.Deserialize(current));
        }
    }
}
