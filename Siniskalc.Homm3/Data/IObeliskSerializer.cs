namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Writes the visited obelisks back into their chunk.
    /// </summary>
    public interface IObeliskSerializer : ISerializer<ObeliskChunk, ObeliskVisits>
    {
    }
}
