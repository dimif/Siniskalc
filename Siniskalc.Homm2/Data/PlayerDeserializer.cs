namespace Siniskalc.Homm2.Data
{
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the record of a player. The colour is the name of the chunk, which the loader gives from the place of the record. The lists of
    /// heroes and castles are read on their own (<see cref="PlayerListsDeserializer"/>).
    /// </summary>
    public sealed class PlayerDeserializer : IDeserializer<PlayerChunk, PlayerModel>
    {
        /// <inheritdoc/>
        public PlayerModel Deserialize(PlayerChunk chunk)
        {
            var resources = Enumerable.Range(0, Constants.Player.ResourceCount).Select(i => (long)chunk.GetUInt32At(PlayerLayout.Resources + (i * 4))).ToArray();
            var income = Enumerable.Range(0, Constants.Player.ResourceCount - 1).Select(i => (long)chunk.GetUInt32At(PlayerLayout.Income + (i * 4))).ToArray();

            return new PlayerModel(
                PlayerColor.All.First(c => c.Name == chunk.Name),
                new ResourceSet(resources[0], resources[1], resources[2], resources[3], resources[4], resources[5], resources[6]),
                new ResourceSet(income[0], income[1], income[2], income[3], income[4], income[5], chunk.GetUInt32At(PlayerLayout.GoldIncome)),
                Bytes.Id(chunk.GetByteAt(PlayerLayout.DaysLeft)),
                new TavernHeroes(
                    Bytes.Id(chunk.GetByteAt(PlayerLayout.Tavern)),
                    Bytes.Id(chunk.GetByteAt(PlayerLayout.Tavern + 1))));
        }
    }
}
