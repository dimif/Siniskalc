namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Writes a model into the record of a player.
    /// </summary>
    public interface IHomm2PlayerSerializer : ISerializer<Homm2PlayerChunk, Homm2PlayerModel>
    {
    }
}
