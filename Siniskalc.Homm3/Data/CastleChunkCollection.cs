namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// The castles of a savefile whose name is stored in it, in the order they are in the savefile.
    /// </summary>
    public sealed class CastleChunkCollection : ModelChunkCollection<CastleChunk, CastleModel, ICastleDeserializer, ICastleSerializer>
    {
        private readonly ICastleDiffer differ;

        public CastleChunkCollection(IEnumerable<CastleChunk> chunks, ICastleDeserializer deserializer, ICastleSerializer serializer, ICastleDiffer differ)
            : base(chunks, deserializer, serializer, differ.Diff)
        {
            this.differ = differ;
        }

        public override CastleChunkCollection WithChunk(int index, CastleChunk chunk) => new(this.Replace(index, chunk), this.Deserializer, this.Serializer, this.differ);

        /// <summary>
        /// Returns who owns each castle, in the order of the collection. Reads the owner byte only.
        /// </summary>
        public IReadOnlyList<PlayerColor?> GetOwners() => this.Select(this.Deserializer.DeserializeOwner).ToList();

        /// <summary>
        /// Returns the index of the castle that holds the hero with the given id (see <see cref="HeroModel.Id"/>) inside it, or -1.
        /// </summary>
        public int FindByGarrisonedHero(int heroId) => this.IndexOf(c => c.GetByteAt(CastleLayout.GarrisonedHero) == heroId);
    }
}
