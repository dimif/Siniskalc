namespace Siniskalc.Homm3.Data
{
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Reads the obelisk flags of a savefile: one byte for each obelisk with a bit for every colour that visited it.
    /// </summary>
    public sealed class ObeliskDeserializer : IDeserializer<ObeliskChunk, ObeliskVisits<PlayerColor>>
    {
        public ObeliskVisits<PlayerColor> Deserialize(ObeliskChunk chunk)
        {
            var count = chunk.GetByteAt(ObeliskLayout.CountOffset);
            return new ObeliskVisits<PlayerColor>(Enumerable.Range(0, count).Select(i => chunk.GetByteAt(ObeliskLayout.FlagsOffset + i)), chunk.Positions);
        }
    }
}
