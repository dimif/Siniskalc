namespace Siniskalc.Data
{
    using System;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// Reads a <see cref="PlayerModel"/> from a <see cref="PlayerChunk"/>. The counterpart of <see cref="SafePlayerSerializer"/>.
    /// </summary>
    public sealed class PlayerDeserializer : IPlayerDeserializer
    {
        public PlayerModel Deserialize(PlayerChunk chunk)
        {
            return new PlayerModel(this.DeserializeColor(chunk), this.DeserializeResources(chunk), this.DeserializeTavern(chunk));
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

        public PlayerResources DeserializeResources(PlayerChunk chunk)
        {
            int Read(Resource resource) => chunk.GetInt32At(PlayerLayout.OffsetOf(resource));

            return new PlayerResources(
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
