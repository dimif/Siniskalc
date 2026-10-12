namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The 54 heroes of a savegame. The index of a hero is its number.
    /// </summary>
    public sealed class Homm2HeroChunkCollection : ChunkCollection<Homm2HeroChunk>
    {
        private readonly Lazy<IReadOnlyList<Homm2HeroModel>> models;
        private readonly IHomm2HeroDiffer differ;

        public Homm2HeroChunkCollection(IEnumerable<Homm2HeroChunk> chunks, IHomm2HeroDeserializer deserializer, IHomm2HeroSerializer serializer, IHomm2HeroDiffer differ)
            : base(chunks)
        {
            this.Deserializer = deserializer;
            this.Serializer = serializer;
            this.differ = differ;
            this.models = new Lazy<IReadOnlyList<Homm2HeroModel>>(() => this.Select(this.Deserializer.Deserialize).ToList());
        }

        public IHomm2HeroDeserializer Deserializer { get; }

        public IHomm2HeroSerializer Serializer { get; }

        public override Homm2HeroChunkCollection WithChunk(int index, Homm2HeroChunk chunk) => new(this.Replace(index, chunk), this.Deserializer, this.Serializer, this.differ);

        public int FindByName(string name) => this.IndexOf(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

        public string GetName(int index) => this[index].Name;

        public Homm2HeroModel GetModel(int index) => this.Deserializer.Deserialize(this[index]);

        /// <summary>
        /// Returns every hero as a model, read once for each version of the collection.
        /// </summary>
        public IReadOnlyList<Homm2HeroModel> GetModels() => this.models.Value;

        /// <summary>
        /// Returns the player each hero serves (<c>null</c> for the ones that serve nobody), by hero number.
        /// </summary>
        public IReadOnlyList<Homm2Color?> GetOwners() => this.Select(this.Deserializer.DeserializeOwner).ToList();

        protected override IEnumerable<PendingChange> DiffChunk(int index, Homm2HeroChunk original, Homm2HeroChunk current)
        {
            return this.differ.Diff(index, this.Deserializer.Deserialize(original), this.Deserializer.Deserialize(current));
        }
    }
}
