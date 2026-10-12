namespace Siniskalc.Data
{
    using System;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// Finds the date of the game in a savefile.
    /// </summary>
    /// <remarks>
    /// The date is three 16-bit integers in a row (day, week, month), followed by a long run of zero bytes. Its position
    /// varies with the map, so it is found by that shape: the first such run after the players. The shape was found by
    /// comparing four Horn of the Abyss saves of one game made on consecutive days (month 5 week 4 day 6 to month 6 week 1
    /// day 2) and three Shadow of Death saves (days 1 to 3). Restoration of Erathia and Armageddon's Blade are assumed to
    /// store it the same way.
    /// That shape alone also matches other data that comes before the date in many savefiles (a Shadow of Death save with a Conflux
    /// castle read "month 2 week 4 day 5" for week 4 day 3). In the savefiles where it was looked at the date is preceded by the marker
    /// 7F, a flag, 02 and a flag (<see cref="MarkerLength"/> bytes), so a match with the marker wins; the first match of the shape alone is
    /// only used when no match has it, as in the Horn of the Abyss saves of "the empire of the world", which have another marker.
    /// </remarks>
    public sealed class GameDateLocator : IGameDateLocator
    {
        private const int ZeroRunAfter = 32;
        private const int DaysInWeek = 7;
        private const int WeeksInMonth = 4;
        private const int MaxMonth = 255;
        private const int MarkerLength = 4;
        private const byte MarkerStart = 0x7F;
        private const byte MarkerThird = 0x02;

        /// <summary>
        /// Returns the date in the savefile, or <c>null</c> when it cannot be found.
        /// </summary>
        /// <param name="bytes">The savefile bytes.</param>
        /// <param name="gameVersion">The version of the savefile.</param>
        /// <param name="searchFrom">Where to start looking; the date comes after the players.</param>
        public GameDate? Find(byte[] bytes, GameVersion gameVersion, int searchFrom = 0)
        {
            var at = this.FindOffset(bytes, gameVersion, searchFrom);
            return at < 0
                ? null
                : new GameDate(BitConverter.ToUInt16(bytes, at + 4), BitConverter.ToUInt16(bytes, at + 2), BitConverter.ToUInt16(bytes, at));
        }

        /// <summary>
        /// Returns where the date is (the 16-bit day, week and month, in that order), or -1 when it cannot be found.
        /// </summary>
        public int FindOffset(byte[] bytes, GameVersion gameVersion, int searchFrom = 0)
        {
            if (!gameVersion.HasGameSetup())
            {
                return -1;
            }

            var firstMatch = -1;
            for (var i = Math.Max(0, searchFrom); i + 6 + ZeroRunAfter <= bytes.Length; i++)
            {
                var day = BitConverter.ToUInt16(bytes, i);
                var week = BitConverter.ToUInt16(bytes, i + 2);
                var month = BitConverter.ToUInt16(bytes, i + 4);

                if (day is >= 1 and <= DaysInWeek && week is >= 1 and <= WeeksInMonth && month is >= 1 and <= MaxMonth && IsZero(bytes, i + 6, ZeroRunAfter))
                {
                    if (HasMarker(bytes, i))
                    {
                        return i;
                    }

                    if (firstMatch < 0)
                    {
                        firstMatch = i;
                    }
                }
            }

            return firstMatch;
        }

        /// <summary>
        /// Returns whether the four bytes in front of <paramref name="at"/> are 7F, a flag, 02 and a flag.
        /// </summary>
        private static bool HasMarker(byte[] bytes, int at)
        {
            return at >= MarkerLength && bytes[at - 4] == MarkerStart && bytes[at - 3] <= 1 && bytes[at - 2] == MarkerThird && bytes[at - 1] <= 1;
        }

        private static bool IsZero(byte[] bytes, int start, int length)
        {
            return bytes.AsSpan(start, length).IndexOfAnyExcept((byte)0) < 0;
        }
    }
}
