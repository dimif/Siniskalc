namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Writes a <see cref="PlayerModel"/> back into a <see cref="PlayerChunk"/>, preserving unknown bits.
    /// </summary>
    public sealed class SafePlayerSerializer : ISerializer<PlayerChunk, PlayerModel>
    {
        private static readonly ILogger<SafePlayerSerializer> Logger = LogFactory.For<SafePlayerSerializer>();

        /// <summary>
        /// Returns <paramref name="chunk"/> with the player's resources, tavern heroes, list of heroes, selected hero and days left before banishing written
        /// into it. Everything else is left as it was.
        /// </summary>
        public PlayerChunk Serialize(PlayerChunk chunk, PlayerModel player)
        {
            // The number of towns (PlayerLayout.TownCount) and the list of their indices follow the owners of the castles in the saves, but
            // the order of the list is not established, so they are left as they were, also after a castle changed owner.
            using var subject = Logger.BeginSubject("Player", chunk.Name);
            Logger.LogTrace("Town count and town list not written, they may be stale after a castle owner edit");
            return SerializeHeroes(SerializeDaysLeft(SerializeTavern(SerializeResources(chunk, player.Resources), player.Tavern), player.DaysLeft), player.HeroIds, player.SelectedHero);
        }

        // The list and its count are written together, and only when the places of the list in the chunk do not hold the ids of the model in
        // that order; the selected hero only when its byte does not read as the model's. So a list the game wrote stays as it was until it is edited.
        private static PlayerChunk SerializeHeroes(PlayerChunk chunk, Numbers heroIds, int? selectedHero)
        {
            RangeGuard.ThrowIfOutOfRange(heroIds.Count, 0, Constants.Player.HeroListSize, nameof(heroIds));
            var bytes = chunk.ToArray();
            var places = bytes.AsSpan(PlayerLayout.HeroIds, PlayerLayout.HeroIdPlaces).ToArray();
            if (!places.Where(id => id != Constants.ByteOff).Select(id => (int)id).SequenceEqual(heroIds))
            {
                for (var place = 0; place < PlayerLayout.HeroIdPlaces; place++)
                {
                    if (place < heroIds.Count)
                    {
                        RangeGuard.ThrowIfOutOfRange(heroIds[place], Constants.Player.MinHeroId, Constants.Player.MaxHeroId, nameof(heroIds));
                    }

                    bytes[PlayerLayout.HeroIds + place] = place < heroIds.Count ? (byte)heroIds[place] : Constants.ByteOff;
                }

                bytes[PlayerLayout.HeroCount] = (byte)heroIds.Count;
            }

            var selected = bytes[PlayerLayout.SelectedHero];
            if ((selected == Constants.ByteOff ? null : (int?)selected) != selectedHero)
            {
                if (selectedHero is { } id)
                {
                    RangeGuard.ThrowIfOutOfRange(id, Constants.Player.MinHeroId, Constants.Player.MaxHeroId, nameof(selectedHero));
                }

                bytes[PlayerLayout.SelectedHero] = selectedHero is { } value ? (byte)value : Constants.ByteOff;
            }

            return chunk.WithBytes(bytes);
        }

        private static PlayerChunk SerializeDaysLeft(PlayerChunk chunk, int? daysLeft)
        {
            var current = chunk.GetByteAt(PlayerLayout.DaysLeft);
            if (daysLeft is not { } days)
            {
                // A byte that is no count of days reads as none, so it is left alone while the model has none.
                return current > Constants.Player.MaxDaysLeft ? chunk : chunk.WithBytes(WithByte(chunk, PlayerLayout.DaysLeft, Constants.ByteOff));
            }

            RangeGuard.ThrowIfOutOfRange(days, Constants.Player.MinDaysLeft, Constants.Player.MaxDaysLeft, nameof(daysLeft));
            return current == days ? chunk : chunk.WithBytes(WithByte(chunk, PlayerLayout.DaysLeft, (byte)days));
        }

        private static byte[] WithByte(PlayerChunk chunk, int index, byte value)
        {
            var bytes = chunk.ToArray();
            bytes[index] = value;
            return bytes;
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

        private static PlayerChunk SerializeResources(PlayerChunk chunk, ResourceSet resources)
        {
            var bytes = chunk.ToArray();
            foreach (var resource in ResourceSet.All)
            {
                // The savefile holds an amount in 32 bits; a larger one is refused and not cut.
                RangeGuard.ThrowIfOutOfRange<long>(resources[resource], int.MinValue, int.MaxValue, nameof(resources));
                ByteBufferHelper.WriteInt32(bytes, PlayerLayout.OffsetOf(resource), (int)resources[resource]);
            }

            return chunk.WithBytes(bytes);
        }
    }
}
