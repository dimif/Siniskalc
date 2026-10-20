namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using System.Linq;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the obelisk flags of a savegame: one byte for each obelisk of the map, with a bit for every colour that visited it.
    /// </summary>
    public sealed class ObeliskDeserializer : IObeliskDeserializer
    {
        public ObeliskVisits<PlayerColor> Deserialize(ObeliskChunk chunk)
        {
            return new ObeliskVisits<PlayerColor>(Enumerable.Range(0, chunk.Positions.Count).Select(i => chunk.GetByteAt(i)), chunk.Positions);
        }
    }
}
