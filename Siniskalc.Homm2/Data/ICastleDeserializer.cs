namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the record of a castle as a model.
    /// </summary>
    public interface ICastleDeserializer : IDeserializer<CastleChunk, CastleModel>
    {
        /// <summary>
        /// Reads only the player who owns the castle, which is cheaper than the whole model.
        /// </summary>
        Color? DeserializeOwner(CastleChunk chunk);
    }
}
