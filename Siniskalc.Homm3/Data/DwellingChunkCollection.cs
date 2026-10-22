namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// The dwellings of a savegame, in the order of the table of their records.
    /// </summary>
    public sealed class DwellingChunkCollection : ModelChunkCollection<DwellingChunk, DwellingModel, IDwellingDeserializer, IDwellingSerializer>
    {
        private readonly IDwellingDiffer differ;

        public DwellingChunkCollection(IEnumerable<DwellingChunk> chunks, IDwellingDeserializer deserializer, IDwellingSerializer serializer, IDwellingDiffer differ)
            : base(chunks, deserializer, serializer, differ.Diff)
        {
            this.differ = differ;
        }

        public override DwellingChunkCollection WithChunk(int index, DwellingChunk chunk) => new(this.Replace(index, chunk), this.Deserializer, this.Serializer, this.differ);
    }
}
