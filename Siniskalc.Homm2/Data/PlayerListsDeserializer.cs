namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the lists of heroes and castles of the record of a player, and what is selected. A place that holds no number (FF) is no hero or
    /// castle, and any other byte is read as the number it holds.
    /// </summary>
    public sealed class PlayerListsDeserializer : IDeserializer<PlayerChunk, PlayerLists>
    {
        /// <inheritdoc/>
        public PlayerLists Deserialize(PlayerChunk chunk)
        {
            var bytes = chunk.ToArray();
            return new PlayerLists(
                Bytes.IdList(bytes, PlayerLayout.Heroes, Constants.Player.HeroListSize),
                Bytes.Id(chunk.GetByteAt(PlayerLayout.SelectedHero)),
                Bytes.IdList(bytes, PlayerLayout.Castles, Constants.Player.CastleListSize),
                Bytes.Id(chunk.GetByteAt(PlayerLayout.SelectedCastle)));
        }
    }
}
