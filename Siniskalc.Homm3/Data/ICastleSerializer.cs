namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Writes a castle back into its chunk.
    /// </summary>
    public interface ICastleSerializer : ISerializer<CastleChunk, CastleModel>
    {
    }
}
