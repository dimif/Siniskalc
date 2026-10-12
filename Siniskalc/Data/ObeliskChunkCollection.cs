namespace Siniskalc.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Models;

    /// <summary>
    /// The visited obelisks of a savefile: one chunk, or none when the savefile has no obelisks or they are not where they are looked for.
    /// </summary>
    public sealed class ObeliskChunkCollection : ChunkCollection<ObeliskChunk>
    {
        private readonly IObeliskDiffer differ;

        public ObeliskChunkCollection(IEnumerable<ObeliskChunk> chunks, IObeliskDeserializer deserializer, IObeliskSerializer serializer, IObeliskDiffer differ)
            : base(chunks)
        {
            this.Deserializer = deserializer;
            this.Serializer = serializer;
            this.differ = differ;
        }

        /// <summary>
        /// Gets what reads the visited obelisks from a chunk.
        /// </summary>
        public IObeliskDeserializer Deserializer { get; }

        /// <summary>
        /// Gets what writes the visited obelisks to a chunk.
        /// </summary>
        public IObeliskSerializer Serializer { get; }

        /// <summary>
        /// Gets whether the savefile has obelisks to show.
        /// </summary>
        public bool Any => this.Count > 0;

        public override ObeliskChunkCollection WithChunk(int index, ObeliskChunk chunk) => new(this.Replace(index, chunk), this.Deserializer, this.Serializer, this.differ);

        /// <summary>
        /// Returns the visited obelisks, or null when the savefile has none to show.
        /// </summary>
        public ObeliskVisits? GetModel() => this.Any ? this.Deserializer.Deserialize(this[0]) : null;

        protected override IEnumerable<PendingChange> DiffChunk(int index, ObeliskChunk original, ObeliskChunk current)
        {
            return this.differ.Diff(index, this.Deserializer.Deserialize(original), this.Deserializer.Deserialize(current));
        }
    }
}
