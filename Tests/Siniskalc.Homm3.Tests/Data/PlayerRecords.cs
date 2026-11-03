using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Records of a player as the game writes a fresh one, for either layout: no hero, no selected hero, no tavern hero, no days left, a
    /// colour byte, and zero for the rest. A test sets the bytes it is about, so none of them needs a savefile.
    /// </summary>
    internal static class PlayerRecords
    {
        public static IEnumerable<PlayerLayout> Layouts => [PlayerLayout.ShadowOfDeath, PlayerLayout.HornOfTheAbyss];

        public static PlayerChunk Blank(PlayerLayout layout, PlayerColor chunkColor, byte colorByte, Action<byte[]>? set = null)
        {
            var bytes = new byte[layout.Stride];
            Array.Fill(bytes, Constants.ByteOff, PlayerLayout.HeroIds, PlayerLayout.HeroIdPlaces);
            Array.Fill(bytes, Constants.ByteOff, PlayerLayout.TownIds, Constants.Player.TownListSize);
            bytes[PlayerLayout.SelectedHero] = Constants.ByteOff;
            bytes[PlayerLayout.TavernHeroes] = Constants.ByteOff;
            bytes[PlayerLayout.TavernHeroes + 1] = Constants.ByteOff;
            bytes[PlayerLayout.DaysLeft] = Constants.ByteOff;
            bytes[layout.ColorOffset] = colorByte;
            set?.Invoke(bytes);
            return new PlayerChunk(chunkColor, bytes, 1000, layout);
        }
    }
}
