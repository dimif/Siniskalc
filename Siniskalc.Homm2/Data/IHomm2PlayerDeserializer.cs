namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the record of a player as a model.
    /// </summary>
    public interface IHomm2PlayerDeserializer : IDeserializer<Homm2PlayerChunk, Homm2PlayerModel>
    {
    }
}
