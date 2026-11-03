namespace Siniskalc.Homm3.Data
{
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Reads a player's ordered hero and town lists and selected hero. FF is an empty place; any other byte is the id it holds.
    /// The counterpart of <see cref="SafePlayerListsSerializer"/>.
    /// </summary>
    public sealed class PlayerListsDeserializer : IDeserializer<PlayerChunk, PlayerLists>
    {
        /// <inheritdoc/>
        public PlayerLists Deserialize(PlayerChunk chunk)
        {
            var ids = Enumerable.Range(PlayerLayout.HeroIds, PlayerLayout.HeroIdPlaces).Select(chunk.GetByteAt).Where(id => id != Constants.ByteOff).Select(id => (int)id);
            var selected = chunk.GetByteAt(PlayerLayout.SelectedHero);
            var towns = Enumerable.Range(PlayerLayout.TownIds, Constants.Player.TownListSize).Select(chunk.GetByteAt)
                .Where(id => id != Constants.ByteOff).Select(id => (int)id);
            return new PlayerLists(new Numbers(ids), selected == Constants.ByteOff ? null : selected) { TownIds = new Numbers(towns) };
        }
    }
}
