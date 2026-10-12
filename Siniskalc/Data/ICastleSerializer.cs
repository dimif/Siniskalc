namespace Siniskalc.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Models;

    /// <summary>
    /// Writes a castle back into its chunk.
    /// </summary>
    public interface ICastleSerializer : ISerializer<CastleChunk, CastleModel>
    {
    }
}
