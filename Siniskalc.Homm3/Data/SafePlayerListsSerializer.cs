namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Writes the list of heroes of a player, and the selected hero, into the record of the player, as they are given. The list and its
    /// count are written together, and only when the places of the list in the chunk do not hold the ids of the model in that order; the
    /// selected hero only when its byte does not read as the model's. So a list the game wrote stays as it was until it changes.
    /// </summary>
    public sealed class SafePlayerListsSerializer : ISerializer<PlayerChunk, PlayerLists>
    {
        /// <inheritdoc/>
        public PlayerChunk Serialize(PlayerChunk chunk, PlayerLists model)
        {
            var heroIds = model.HeroIds;
            RangeGuard.ThrowIfOutOfRange(heroIds.Count, 0, Constants.Player.HeroListSize, nameof(model));
            var bytes = chunk.ToArray();
            var places = bytes.AsSpan(PlayerLayout.HeroIds, PlayerLayout.HeroIdPlaces).ToArray();
            if (!places.Where(id => id != Constants.ByteOff).Select(id => (int)id).SequenceEqual(heroIds))
            {
                for (var place = 0; place < PlayerLayout.HeroIdPlaces; place++)
                {
                    if (place < heroIds.Count)
                    {
                        RangeGuard.ThrowIfOutOfRange(heroIds[place], Constants.Player.MinHeroId, Constants.Player.MaxHeroId, nameof(model));
                    }

                    bytes[PlayerLayout.HeroIds + place] = place < heroIds.Count ? (byte)heroIds[place] : Constants.ByteOff;
                }

                bytes[PlayerLayout.HeroCount] = (byte)heroIds.Count;
            }

            var selected = bytes[PlayerLayout.SelectedHero];
            if ((selected == Constants.ByteOff ? null : (int?)selected) != model.SelectedHero)
            {
                if (model.SelectedHero is { } id)
                {
                    RangeGuard.ThrowIfOutOfRange(id, Constants.Player.MinHeroId, Constants.Player.MaxHeroId, nameof(model));
                }

                bytes[PlayerLayout.SelectedHero] = model.SelectedHero is { } value ? (byte)value : Constants.ByteOff;
            }

            return chunk.WithBytes(bytes);
        }
    }
}
