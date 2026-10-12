namespace Siniskalc.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// The castles of a savefile whose name is stored in it, in the order they are in the savefile.
    /// </summary>
    public sealed class CastleChunkCollection : ChunkCollection<CastleChunk>
    {
        private readonly Lazy<IReadOnlyList<CastleModel>> models;
        private readonly ICastleDiffer differ;

        public CastleChunkCollection(IEnumerable<CastleChunk> chunks, ICastleDeserializer deserializer, ICastleSerializer serializer, ICastleDiffer differ)
            : base(chunks)
        {
            this.Deserializer = deserializer;
            this.Serializer = serializer;
            this.differ = differ;
            this.models = new Lazy<IReadOnlyList<CastleModel>>(() => this.Select(this.Deserializer.Deserialize).ToList());
        }

        /// <summary>
        /// Gets what reads a castle from a chunk.
        /// </summary>
        public ICastleDeserializer Deserializer { get; }

        /// <summary>
        /// Gets what writes a castle to a chunk.
        /// </summary>
        public ICastleSerializer Serializer { get; }

        public override CastleChunkCollection WithChunk(int index, CastleChunk chunk) => new(this.Replace(index, chunk), this.Deserializer, this.Serializer, this.differ);

        /// <summary>
        /// Returns the index of the first castle with the given name, or -1.
        /// </summary>
        public int FindByName(string name) => this.IndexOf(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Returns the name of the castle at <paramref name="index"/>.
        /// </summary>
        public string GetName(int index) => this[index].Name;

        /// <summary>
        /// Returns the castle at <paramref name="index"/>.
        /// </summary>
        public CastleModel GetModel(int index) => this.Deserializer.Deserialize(this[index]);

        /// <summary>
        /// Returns who owns each castle, in the order of the collection. Reads the owner byte only.
        /// </summary>
        public IReadOnlyList<PlayerColor?> GetOwners() => this.Select(this.Deserializer.DeserializeOwner).ToList();

        /// <summary>
        /// Returns every castle, in the order of the collection, so that the castle at <c>i</c> is the one with index <c>i</c>. They are
        /// read once for each version of the collection.
        /// </summary>
        public IReadOnlyList<CastleModel> GetModels() => this.models.Value;

        /// <summary>
        /// Returns the index of the castle that holds the hero with the given id (see <see cref="HeroModel.HeroId"/>) inside it, or -1.
        /// </summary>
        public int FindByGarrisonedHero(int heroId) => this.IndexOf(c => c.GetByteAt(CastleLayout.GarrisonedHero) == heroId);

        protected override IEnumerable<PendingChange> DiffChunk(int index, CastleChunk original, CastleChunk current)
        {
            return this.differ.Diff(index, this.Deserializer.Deserialize(original), this.Deserializer.Deserialize(current));
        }
    }
}
