namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Writes the visited obelisks back into their chunk.
    /// </summary>
    public interface IObeliskSerializer : ISerializer<ObeliskChunk, ObeliskVisits<PlayerColor>>
    {
    }
}
