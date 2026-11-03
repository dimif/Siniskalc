namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Writes a player's ordered hero and town lists and selected hero as given. Each list and its count are written together, only when
    /// its places do not hold the model's ids in order; the selected hero only when its byte does not read as the model's.
    /// Town selection is never changed. A list the game wrote stays as it was until it changes.
    /// </summary>
    public sealed class SafePlayerListsSerializer : ISerializer<PlayerChunk, PlayerLists>
    {
        /// <inheritdoc/>
        public PlayerChunk Serialize(PlayerChunk chunk, PlayerLists model)
        {
            var heroIds = model.HeroIds;
            RangeGuard.ThrowIfOutOfRange(heroIds.Count, 0, Constants.Player.HeroListSize, nameof(model));
            var bytes = chunk.ToArray();
            var townIds = model.TownIds;
            RangeGuard.ThrowIfOutOfRange(townIds.Count, 0, Constants.Player.TownListSize, nameof(model));
            var townPlaces = bytes.AsSpan(PlayerLayout.TownIds, Constants.Player.TownListSize);
            if (!townPlaces.ToArray().Where(id => id != Constants.ByteOff).Select(id => (int)id).SequenceEqual(townIds))
            {
                for (var place = 0; place < Constants.Player.TownListSize; place++)
                {
                    if (place < townIds.Count)
                    {
                        RangeGuard.ThrowIfOutOfRange(townIds[place], Constants.Player.MinTownId, Constants.Player.MaxTownId, nameof(model));
                    }

                    townPlaces[place] = place < townIds.Count ? (byte)townIds[place] : Constants.ByteOff;
                }

                bytes[PlayerLayout.TownCount] = (byte)townIds.Count;
            }

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
