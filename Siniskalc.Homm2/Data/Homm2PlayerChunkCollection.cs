namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The six players of a savegame, one for each colour. The index of a player is its colour.
    /// </summary>
    public sealed class Homm2PlayerChunkCollection : ChunkCollection<Homm2PlayerChunk>
    {
        private readonly Lazy<IReadOnlyList<Homm2PlayerModel>> models;
        private readonly IHomm2PlayerDiffer differ;

        public Homm2PlayerChunkCollection(IEnumerable<Homm2PlayerChunk> chunks, IHomm2PlayerDeserializer deserializer, IHomm2PlayerSerializer serializer, IHomm2PlayerDiffer differ)
            : base(chunks)
        {
            this.Deserializer = deserializer;
            this.Serializer = serializer;
            this.differ = differ;
            this.models = new Lazy<IReadOnlyList<Homm2PlayerModel>>(() => this.Select(this.Deserializer.Deserialize).ToList());
        }

        public IHomm2PlayerDeserializer Deserializer { get; }

        public IHomm2PlayerSerializer Serializer { get; }

        public override Homm2PlayerChunkCollection WithChunk(int index, Homm2PlayerChunk chunk) => new(this.Replace(index, chunk), this.Deserializer, this.Serializer, this.differ);

        public Homm2Color GetColor(int index) => (Homm2Color)index;

        public int FindByColor(Homm2Color color) => (int)color;

        public Homm2PlayerModel GetModel(int index) => this.Deserializer.Deserialize(this[index]);

        /// <summary>
        /// Returns every player as a model, read once for each version of the collection.
        /// </summary>
        public IReadOnlyList<Homm2PlayerModel> GetModels() => this.models.Value;

        protected override IEnumerable<PendingChange> DiffChunk(int index, Homm2PlayerChunk original, Homm2PlayerChunk current)
        {
            return this.differ.Diff(index, this.Deserializer.Deserialize(original), this.Deserializer.Deserialize(current));
        }
    }
}
