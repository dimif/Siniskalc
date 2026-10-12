namespace Siniskalc.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Models;

    /// <summary>
    /// Writes the visited obelisks back into their chunk.
    /// </summary>
    public interface IObeliskSerializer : ISerializer<ObeliskChunk, ObeliskVisits>
    {
    }
}
