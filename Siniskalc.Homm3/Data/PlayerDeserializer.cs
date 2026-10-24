namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Linq;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Reads a <see cref="PlayerModel"/> from a <see cref="PlayerChunk"/>. The counterpart of <see cref="SafePlayerSerializer"/>.
    /// </summary>
    public sealed class PlayerDeserializer : IPlayerDeserializer
    {
        public PlayerModel Deserialize(PlayerChunk chunk)
        {
            return new PlayerModel(
                this.DeserializeColor(chunk),
                this.DeserializeResources(chunk),
                this.DeserializeTavern(chunk),
                this.DeserializeHeroIds(chunk),
                this.DeserializeSelectedHero(chunk),
                this.DeserializeDaysLeft(chunk));
        }

        /// <summary>
        /// Reads the ids of the heroes of the player: the places of its list that are not FF, in their order.
        /// </summary>
        public Numbers DeserializeHeroIds(PlayerChunk chunk) => new(
            Enumerable.Range(PlayerLayout.HeroIds, PlayerLayout.HeroIdPlaces).Select(chunk.GetByteAt).Where(id => id != Constants.ByteOff).Select(id => (int)id));

        /// <summary>
        /// Reads the id of the selected hero; FF reads as none.
        /// </summary>
        public int? DeserializeSelectedHero(PlayerChunk chunk)
        {
            var id = chunk.GetByteAt(PlayerLayout.SelectedHero);
            return id == Constants.ByteOff ? null : id;
        }

        /// <summary>
        /// Reads the days left before banishing. A player who owns a town has FF, which reads as none, and so does a byte that holds more than the game counts.
        /// </summary>
        public int? DeserializeDaysLeft(PlayerChunk chunk)
        {
            var days = chunk.GetByteAt(PlayerLayout.DaysLeft);
            return days <= Constants.Player.MaxDaysLeft ? days : null;
        }

        public TavernHeroes DeserializeTavern(PlayerChunk chunk)
        {
            int? Read(int place)
            {
                var id = chunk.GetByteAt(PlayerLayout.TavernHeroes + place);
                return id == Constants.ByteOff ? null : (int?)id;
            }

            return new TavernHeroes(Read(0), Read(1));
        }

        public PlayerColor DeserializeColor(PlayerChunk chunk)
        {
            var colorByte = chunk.GetByteAt(chunk.Layout.ColorOffset);
            if (colorByte >= 1 && colorByte <= Constants.Player.ColorCount)
            {
                return PlayerLayout.ColorFromByte(colorByte);
            }

            // A slot that is not in play has no colour byte; the loader gave the chunk the colour it follows.
            return chunk.Color;
        }

        public ResourceSet DeserializeResources(PlayerChunk chunk)
        {
            int Read(Resource resource) => chunk.GetInt32At(PlayerLayout.OffsetOf(resource));

            return new ResourceSet(
                Read(Resource.Wood),
                Read(Resource.Mercury),
                Read(Resource.Ore),
                Read(Resource.Sulfur),
                Read(Resource.Crystal),
                Read(Resource.Gems),
                Read(Resource.Gold));
        }
    }
}
