namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the record of a player. The colour is the name of the chunk, which the loader gives from the place of the record.
    /// </summary>
    public sealed class PlayerDeserializer : IPlayerDeserializer
    {
        /// <inheritdoc/>
        public PlayerModel Deserialize(PlayerChunk chunk)
        {
            var bytes = chunk.ToArray();
            var resources = Enumerable.Range(0, Constants.Player.ResourceCount).Select(i => (long)chunk.GetUInt32At(PlayerLayout.Resources + (i * 4))).ToArray();
            var income = Enumerable.Range(0, Constants.Player.ResourceCount - 1).Select(i => (long)chunk.GetUInt32At(PlayerLayout.Income + (i * 4))).ToArray();

            return new PlayerModel(
                PlayerColor.All.First(c => c.Name == chunk.Name),
                new ResourceSet(resources[0], resources[1], resources[2], resources[3], resources[4], resources[5], resources[6]),
                new ResourceSet(income[0], income[1], income[2], income[3], income[4], income[5], chunk.GetUInt32At(PlayerLayout.GoldIncome)),
                Bytes.IdList(bytes, PlayerLayout.Heroes, Constants.Player.HeroListSize),
                Bytes.Id(chunk.GetByteAt(PlayerLayout.SelectedHero)),
                Bytes.IdList(bytes, PlayerLayout.Castles, Constants.Player.CastleListSize),
                Bytes.Id(chunk.GetByteAt(PlayerLayout.SelectedCastle)),
                Bytes.Id(chunk.GetByteAt(PlayerLayout.DaysLeft)),
                new TavernHeroes(
                    Bytes.Id(chunk.GetByteAt(PlayerLayout.Tavern)),
                    Bytes.Id(chunk.GetByteAt(PlayerLayout.Tavern + 1))));
        }
    }
}
