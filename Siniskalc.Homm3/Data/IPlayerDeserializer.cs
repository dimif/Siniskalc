namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

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
