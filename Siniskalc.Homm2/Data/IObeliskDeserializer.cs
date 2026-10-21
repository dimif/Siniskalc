namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the visited obelisks from their chunk.
    /// </summary>
    public interface IObeliskDeserializer : IDeserializer<ObeliskChunk, ObeliskVisits<PlayerColor>>
    {
    }
}
