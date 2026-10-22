namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the record of a mine: its owner, its kind and (from the chunk) where it is.
    /// </summary>
    public sealed class MineDeserializer : IMapObjectDeserializer<MineModel>
    {
        public MineModel Deserialize(MapObjectChunk chunk) => new(chunk.Index, (MineKind)chunk.GetByteAt(1), chunk.Position!, Bytes.ReadColor(chunk.GetByteAt(0)));
    }
}
