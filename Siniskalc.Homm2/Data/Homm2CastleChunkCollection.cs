namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The 72 castle records of a savegame. The index of a castle is its number; the map uses the first few, and the others have no name.
    /// </summary>
    public sealed class Homm2CastleChunkCollection : ChunkCollection<Homm2CastleChunk>
    {
        private readonly Lazy<IReadOnlyList<Homm2CastleModel>> models;
        private readonly IHomm2CastleDiffer differ;

        public Homm2CastleChunkCollection(IEnumerable<Homm2CastleChunk> chunks, IHomm2CastleDeserializer deserializer, IHomm2CastleSerializer serializer, IHomm2CastleDiffer differ)
            : base(chunks)
        {
            this.Deserializer = deserializer;
            this.Serializer = serializer;
            this.differ = differ;
            this.models = new Lazy<IReadOnlyList<Homm2CastleModel>>(() => this.Select(this.Deserializer.Deserialize).ToList());
        }

        public IHomm2CastleDeserializer Deserializer { get; }

        public IHomm2CastleSerializer Serializer { get; }

        public override Homm2CastleChunkCollection WithChunk(int index, Homm2CastleChunk chunk) => new(this.Replace(index, chunk), this.Deserializer, this.Serializer, this.differ);

        public int FindByName(string name) => this.IndexOf(c => c.Name.Length > 0 && string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

        public string GetName(int index) => this[index].Name;

        public Homm2CastleModel GetModel(int index) => this.Deserializer.Deserialize(this[index]);

        /// <summary>
        /// Returns every castle record as a model, read once for each version of the collection.
        /// </summary>
        public IReadOnlyList<Homm2CastleModel> GetModels() => this.models.Value;

        /// <summary>
        /// Returns the numbers of the castles the map uses.
        /// </summary>
        public IReadOnlyList<int> UsedIndexes() => Enumerable.Range(0, this.Count).Where(i => this[i].Name.Length > 0).ToList();

        /// <summary>
        /// Returns the player who owns each castle (<c>null</c> for nobody), by castle number.
        /// </summary>
        public IReadOnlyList<Homm2Color?> GetOwners() => this.Select(this.Deserializer.DeserializeOwner).ToList();

        protected override IEnumerable<PendingChange> DiffChunk(int index, Homm2CastleChunk original, Homm2CastleChunk current)
        {
            return this.differ.Diff(index, this.Deserializer.Deserialize(original), this.Deserializer.Deserialize(current));
        }
    }
}
