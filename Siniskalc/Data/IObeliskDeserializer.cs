namespace Siniskalc.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Models;

    /// <summary>
    /// Reads the visited obelisks from their chunk.
    /// </summary>
    public interface IObeliskDeserializer : IDeserializer<ObeliskChunk, ObeliskVisits>
    {
    }
}
