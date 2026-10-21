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
    public sealed class CastleChunkCollection : ModelChunkCollection<CastleChunk, CastleModel, ICastleDeserializer, ICastleSerializer>
    {
        private readonly ICastleDiffer differ;

        public CastleChunkCollection(IEnumerable<CastleChunk> chunks, ICastleDeserializer deserializer, ICastleSerializer serializer, ICastleDiffer differ)
            : base(chunks, deserializer, serializer, differ.Diff)
        {
            this.differ = differ;
        }

        /// <inheritdoc/>
        public override CastleChunkCollection WithChunk(int index, CastleChunk chunk) => new(this.Replace(index, chunk), this.Deserializer, this.Serializer, this.differ);

        /// <summary>
        /// Returns the numbers of the castles the map uses.
        /// </summary>
        public IReadOnlyList<int> UsedIndexes() => Enumerable.Range(0, this.Count).Where(i => this[i].Name.Length > 0).ToList();

        /// <summary>
        /// Returns the player who owns each castle (<c>null</c> for nobody), by castle number.
        /// </summary>
        public IReadOnlyList<PlayerColor> GetOwners() => this.Select(this.Deserializer.DeserializeOwner).ToList();

        /// <inheritdoc/>
        protected override bool IsNamed(CastleChunk chunk) => chunk.Name.Length > 0;
    }
}
