namespace Siniskalc.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// Reads a player from its chunk.
    /// </summary>
    public interface IPlayerDeserializer : IDeserializer<PlayerChunk, PlayerModel>
    {
        PlayerColor DeserializeColor(PlayerChunk chunk);

        PlayerResources DeserializeResources(PlayerChunk chunk);

        TavernHeroes DeserializeTavern(PlayerChunk chunk);
    }
}
