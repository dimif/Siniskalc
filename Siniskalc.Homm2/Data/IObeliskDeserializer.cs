namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the visited obelisks from their chunk.
    /// </summary>
    public interface IObeliskDeserializer : IDeserializer<ObeliskChunk, ObeliskVisits<PlayerColor>>
    {
    }
}
