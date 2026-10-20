namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Reads the visited obelisks from their chunk.
    /// </summary>
    public interface IObeliskDeserializer : IDeserializer<ObeliskChunk, ObeliskVisits<PlayerColor>>
    {
    }
}
