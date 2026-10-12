namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the record of a player. The colour is the name of the chunk, which the loader gives from the place of the record.
    /// </summary>
    public sealed class Homm2PlayerDeserializer : IHomm2PlayerDeserializer
    {
        /// <inheritdoc/>
        public Homm2PlayerModel Deserialize(Homm2PlayerChunk chunk)
        {
            var bytes = chunk.ToArray();
            var resources = Enumerable.Range(0, Constants.Player.ResourceCount).Select(i => (long)chunk.GetUInt32At(Homm2PlayerLayout.Resources + (i * 4))).ToArray();
            var income = Enumerable.Range(0, Constants.Player.ResourceCount - 1).Select(i => (long)chunk.GetUInt32At(Homm2PlayerLayout.Income + (i * 4))).ToArray();

            return new Homm2PlayerModel(
                Enum.Parse<Homm2Color>(chunk.Name),
                new Homm2Resources(resources[0], resources[1], resources[2], resources[3], resources[4], resources[5], resources[6]),
                new Homm2Resources(income[0], income[1], income[2], income[3], income[4], income[5], chunk.GetUInt32At(Homm2PlayerLayout.GoldIncome)),
                Homm2Bytes.IdList(bytes, Homm2PlayerLayout.Heroes, Constants.Player.HeroListSize),
                Homm2Bytes.Id(chunk.GetByteAt(Homm2PlayerLayout.SelectedHero)),
                Homm2Bytes.IdList(bytes, Homm2PlayerLayout.Castles, Constants.Player.CastleListSize),
                Homm2Bytes.Id(chunk.GetByteAt(Homm2PlayerLayout.SelectedCastle)),
                Homm2Bytes.Id(chunk.GetByteAt(Homm2PlayerLayout.DaysLeft)),
                new Homm2TavernHeroes(
                    Homm2Bytes.Id(chunk.GetByteAt(Homm2PlayerLayout.Tavern)),
                    Homm2Bytes.Id(chunk.GetByteAt(Homm2PlayerLayout.Tavern + 1))));
        }
    }
}
