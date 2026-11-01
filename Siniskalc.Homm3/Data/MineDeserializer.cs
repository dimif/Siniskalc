namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Reads the record of a mine: its kind, its owner, the tile it gives and its guard.
    /// </summary>
    public sealed class MineDeserializer(CreatureCodes creatureCodes) : IDeserializer<RecordChunk, MineModel>
    {
        public MineModel Deserialize(RecordChunk chunk) => new(
            chunk.Index,
            (MineKind)chunk.GetByteAt(MineLayout.Kind),
            PlayerColor.FromByte(chunk.GetByteAt(MineLayout.Owner)),
            new MapPosition(chunk.GetByteAt(MineLayout.X), chunk.GetByteAt(MineLayout.Y), chunk.GetByteAt(MineLayout.Level)),
            ArmyReader.Read(chunk, MineLayout.Guard, creatureCodes), creatureCodes.Catalog);
    }
}
