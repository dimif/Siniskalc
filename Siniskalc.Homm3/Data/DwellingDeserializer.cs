namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Reads the record of a dwelling: the creature, the owner, the tile and the creatures that can be hired.
    /// </summary>
    public sealed class DwellingDeserializer : IDeserializer<RecordChunk, DwellingModel>
    {
        public DwellingModel Deserialize(RecordChunk chunk)
        {
            var code = chunk.GetByteAt(DwellingLayout.Creature);
            return new DwellingModel(
                chunk.Index,
                code,
                Creature.FromByte(code),
                PlayerColor.FromByte(chunk.GetByteAt(DwellingLayout.Owner)),
                new MapPosition(chunk.GetByteAt(DwellingLayout.X), chunk.GetByteAt(DwellingLayout.Y), chunk.GetByteAt(DwellingLayout.Level)),
                chunk.GetUInt32At(DwellingLayout.Stock));
        }
    }
}
