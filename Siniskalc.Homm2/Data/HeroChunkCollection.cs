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
    public sealed class HeroChunkCollection : ChunkCollection<HeroChunk>
    {
        private readonly Lazy<IReadOnlyList<HeroModel>> models;
        private readonly IHeroDiffer differ;

        public HeroChunkCollection(IEnumerable<HeroChunk> chunks, IHeroDeserializer deserializer, IHeroSerializer serializer, IHeroDiffer differ)
            : base(chunks)
        {
            this.Deserializer = deserializer;
            this.Serializer = serializer;
            this.differ = differ;
            this.models = new Lazy<IReadOnlyList<HeroModel>>(() => this.Select(this.Deserializer.Deserialize).ToList());
        }

        public IHeroDeserializer Deserializer { get; }

        public IHeroSerializer Serializer { get; }

        public override HeroChunkCollection WithChunk(int index, HeroChunk chunk) => new(this.Replace(index, chunk), this.Deserializer, this.Serializer, this.differ);

        public int FindByName(string name) => this.IndexOf(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

        public string GetName(int index) => this[index].Name;

        public HeroModel GetModel(int index) => this.Deserializer.Deserialize(this[index]);

        /// <summary>
        /// Returns every hero as a model, read once for each version of the collection.
        /// </summary>
        public IReadOnlyList<HeroModel> GetModels() => this.models.Value;

        /// <summary>
        /// Returns the player each hero serves (<c>null</c> for the ones that serve nobody), by hero number.
        /// </summary>
        public IReadOnlyList<Color?> GetOwners() => this.Select(this.Deserializer.DeserializeOwner).ToList();

        protected override IEnumerable<PendingChange> DiffChunk(int index, HeroChunk original, HeroChunk current)
        {
            return this.differ.Diff(index, this.Deserializer.Deserialize(original), this.Deserializer.Deserialize(current));
        }
    }
}
