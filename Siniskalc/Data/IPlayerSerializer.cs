namespace Siniskalc.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Models;

    /// <summary>
    /// Writes a player back into its chunk.
    /// </summary>
    public interface IPlayerSerializer : ISerializer<PlayerChunk, PlayerModel>
    {
    }
}
