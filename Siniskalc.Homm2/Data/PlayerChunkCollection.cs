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
    public sealed class PlayerChunkCollection : ChunkCollection<PlayerChunk>
    {
        private readonly Lazy<IReadOnlyList<PlayerModel>> models;
        private readonly IPlayerDiffer differ;

        public PlayerChunkCollection(IEnumerable<PlayerChunk> chunks, IPlayerDeserializer deserializer, IPlayerSerializer serializer, IPlayerDiffer differ)
            : base(chunks)
        {
            this.Deserializer = deserializer;
            this.Serializer = serializer;
            this.differ = differ;
            this.models = new Lazy<IReadOnlyList<PlayerModel>>(() => this.Select(this.Deserializer.Deserialize).ToList());
        }

        public IPlayerDeserializer Deserializer { get; }

        public IPlayerSerializer Serializer { get; }

        public override PlayerChunkCollection WithChunk(int index, PlayerChunk chunk) => new(this.Replace(index, chunk), this.Deserializer, this.Serializer, this.differ);

        public Color GetColor(int index) => (Color)index;

        public int FindByColor(Color color) => (int)color;

        public PlayerModel GetModel(int index) => this.Deserializer.Deserialize(this[index]);

        /// <summary>
        /// Returns every player as a model, read once for each version of the collection.
        /// </summary>
        public IReadOnlyList<PlayerModel> GetModels() => this.models.Value;

        protected override IEnumerable<PendingChange> DiffChunk(int index, PlayerChunk original, PlayerChunk current)
        {
            return this.differ.Diff(index, this.Deserializer.Deserialize(original), this.Deserializer.Deserialize(current));
        }
    }
}
