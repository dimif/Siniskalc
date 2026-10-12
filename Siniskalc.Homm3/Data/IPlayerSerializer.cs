namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Writes a player back into its chunk.
    /// </summary>
    public interface IPlayerSerializer : ISerializer<PlayerChunk, PlayerModel>
    {
    }
}
