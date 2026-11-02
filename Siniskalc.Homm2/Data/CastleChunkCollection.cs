namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The 72 castle records of a savegame. The index of a castle is its number; the map uses the first few, and the others have no name.
    /// </summary>
    public sealed class CastleChunkCollection : ModelChunkCollection<CastleChunk, CastleModel, ICastleDeserializer, ISerializer<CastleChunk, CastleModel>>
    {
        private readonly IDiffer<CastleModel> differ;

        /// <summary>
        /// Initializes a new instance of the <see cref="CastleChunkCollection"/> class.
        /// </summary>
        /// <param name="chunks">The chunks.</param>
        /// <param name="deserializer">The deserializer.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="differ">The differ.</param>
        public CastleChunkCollection(IEnumerable<CastleChunk> chunks, ICastleDeserializer deserializer, ISerializer<CastleChunk, CastleModel> serializer, IDiffer<CastleModel> differ)
            : base(chunks, deserializer, serializer, differ.Diff)
        {
            this.differ = differ;
        }

        /// <inheritdoc/>
        public override CastleChunkCollection WithChunk(int index, CastleChunk chunk) => new(this.Replace(index, chunk), this.Deserializer, this.Serializer, this.differ);

        /// <summary>
        /// Returns the numbers of the castles the map uses.
        /// </summary>
        /// <returns>The numbers of the castles that have a name.</returns>
        public IReadOnlyList<int> UsedIndexes() => Enumerable.Range(0, this.Count).Where(i => this[i].Name.Length > 0).ToList();

        /// <summary>
        /// Returns the player who owns each castle (<c>null</c> for nobody), by castle number.
        /// </summary>
        /// <returns>The owners, in the order of the records.</returns>
        public IReadOnlyList<PlayerColor> GetOwners() => this.Select(this.Deserializer.DeserializeOwner).ToList();

        /// <inheritdoc/>
        protected override bool IsNamed(CastleChunk chunk) => chunk.Name.Length > 0;
    }
}
