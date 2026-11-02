namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// The castles of a savefile whose name is stored in it, in the order they are in the savefile.
    /// </summary>
    public sealed class CastleChunkCollection : ModelChunkCollection<CastleChunk, CastleModel, ICastleDeserializer, ISerializer<CastleChunk, CastleModel>>
    {
        private readonly ICastleDiffer differ;

        /// <summary>
        /// Initializes a new instance of the <see cref="CastleChunkCollection"/> class.
        /// </summary>
        /// <param name="chunks">The chunks.</param>
        /// <param name="deserializer">The deserializer.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="differ">The differ.</param>
        public CastleChunkCollection(IEnumerable<CastleChunk> chunks, ICastleDeserializer deserializer, ISerializer<CastleChunk, CastleModel> serializer, ICastleDiffer differ)
            : base(chunks, deserializer, serializer, differ.Diff)
        {
            this.differ = differ;
        }

        /// <inheritdoc/>
        public override CastleChunkCollection WithChunk(int index, CastleChunk chunk) => new(this.Replace(index, chunk), this.Deserializer, this.Serializer, this.differ);

        /// <summary>
        /// Returns who owns each castle, in the order of the collection. Reads the owner byte only.
        /// </summary>
        /// <returns>The owners, in the order of the records.</returns>
        public IReadOnlyList<PlayerColor> GetOwners() => this.Select(this.Deserializer.DeserializeOwner).ToList();

        /// <summary>
        /// Returns the index of the castle that holds the hero with the given id (see <see cref="HeroModel.Id"/>) inside it, or -1.
        /// </summary>
        /// <param name="heroId">The number of the hero.</param>
        /// <returns>The number of the castle, or -1 when no castle has the hero.</returns>
        public int FindByGarrisonedHero(int heroId) => this.IndexOf(c => c.GetByteAt(CastleLayout.GarrisonedHero) == heroId);
    }
}
