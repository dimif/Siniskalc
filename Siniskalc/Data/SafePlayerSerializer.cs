namespace Siniskalc.Data
{
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Models;

    /// <summary>
    /// Writes a <see cref="PlayerModel"/> back into a <see cref="PlayerChunk"/>, preserving unknown bits.
    /// </summary>
    public sealed class SafePlayerSerializer : IPlayerSerializer
    {
        /// <summary>
        /// Returns <paramref name="chunk"/> with the player's resources and tavern heroes written into it. Everything else is left as it was.
        /// </summary>
        public PlayerChunk Serialize(PlayerChunk chunk, PlayerModel player)
        {
            return SerializeTavern(SerializeResources(chunk, player.Resources), player.Tavern);
        }

        private static PlayerChunk SerializeTavern(PlayerChunk chunk, TavernHeroes tavern)
        {
            var bytes = chunk.ToArray();
            for (var place = 0; place < TavernHeroes.Places; place++)
            {
                // A hero id is a byte, and FF stands for none; RangeGuard keeps an id out of the way of it.
                var id = tavern.Ids[place];
                if (id is { } heroId)
                {
                    RangeGuard.ThrowIfOutOfRange(heroId, Constants.Player.MinHeroId, Constants.Player.MaxHeroId, nameof(tavern));
                }

                bytes[PlayerLayout.TavernHeroes + place] = id is { } value ? (byte)value : Constants.ByteOff;
            }

            return chunk.WithBytes(bytes);
        }

        private static PlayerChunk SerializeResources(PlayerChunk chunk, PlayerResources resources)
        {
            var bytes = chunk.ToArray();
            foreach (var resource in PlayerResources.All)
            {
                ByteBufferHelper.WriteInt32(bytes, PlayerLayout.OffsetOf(resource), resources[resource]);
            }

            return chunk.WithBytes(bytes);
        }
    }
}
