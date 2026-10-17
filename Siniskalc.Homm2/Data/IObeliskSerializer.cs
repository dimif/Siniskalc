namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Writes the visited obelisks back into their chunk.
    /// </summary>
    public interface IObeliskSerializer : ISerializer<ObeliskChunk, ObeliskVisits>
    {
    }
}
