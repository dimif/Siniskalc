namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// The mines of a savegame, in the order of the table of their records.
    /// </summary>
    public sealed class MineChunkCollection : ModelChunkCollection<MineChunk, MineModel, IMineDeserializer, IMineSerializer>
    {
        private readonly IMineDiffer differ;

        public MineChunkCollection(IEnumerable<MineChunk> chunks, IMineDeserializer deserializer, IMineSerializer serializer, IMineDiffer differ)
            : base(chunks, deserializer, serializer, differ.Diff)
        {
            this.differ = differ;
        }

        public override MineChunkCollection WithChunk(int index, MineChunk chunk) => new(this.Replace(index, chunk), this.Deserializer, this.Serializer, this.differ);
    }
}
