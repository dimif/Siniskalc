namespace Siniskalc.Homm3.Data
{
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Reads the list of heroes of the record of a player, and the selected hero. A place that holds FF is no hero, and any other byte is
    /// read as the id it holds. The counterpart of <see cref="SafePlayerListsSerializer"/>.
    /// </summary>
    public sealed class PlayerListsDeserializer : IDeserializer<PlayerChunk, PlayerLists>
    {
        /// <inheritdoc/>
        public PlayerLists Deserialize(PlayerChunk chunk)
        {
            var ids = Enumerable.Range(PlayerLayout.HeroIds, PlayerLayout.HeroIdPlaces).Select(chunk.GetByteAt).Where(id => id != Constants.ByteOff).Select(id => (int)id);
            var selected = chunk.GetByteAt(PlayerLayout.SelectedHero);
            return new PlayerLists(new Numbers(ids), selected == Constants.ByteOff ? null : selected);
        }
    }
}
