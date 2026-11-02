namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Finds the table of hero owners from the date of the game: it ends two tables and a tail before it (<see cref="HeroOwnersLayout"/>). The
    /// place is not searched for, it is checked: every byte of the table is one the table was seen with, every byte of the second table is FF
    /// and the tail starts with <c>00 08</c>. When that does not hold the savefile has no table, and an owner edit does less.
    /// </summary>
    public sealed class HeroOwnersBytesLoader : IHeroOwnersBytesLoader
    {
        private const string Name = "Hero owners";

        private static readonly ILogger<HeroOwnersBytesLoader> Logger = LogFactory.For<HeroOwnersBytesLoader>();

        private readonly IGameDateLocator dateLocator;

        /// <param name="dateLocator">Finds the date of the game, which the table is in front of.</param>
        public HeroOwnersBytesLoader(IGameDateLocator dateLocator)
        {
            this.dateLocator = dateLocator;
        }

        /// <inheritdoc/>
        public IReadOnlyList<RecordChunk> LoadHeroOwnerChunks(byte[] bytes, GameVersion gameVersion, int searchFrom)
        {
            var length = HeroOwnersLayout.LengthFor(gameVersion);
            if (length == 0)
            {
                return [];
            }

            var date = this.dateLocator.FindOffset(bytes, gameVersion, searchFrom);
            var start = date - HeroOwnersLayout.TailLength - (2 * length);
            if (date < 0 || start < 0)
            {
                Logger.LogDebug("No table of hero owners: no date found");
                return [];
            }

            if (!IsTable(bytes, start, length))
            {
                Logger.LogDebug("No table of hero owners at {Offset:X}: the bytes in front of the date at {DateOffset:X} are not one", start, date);
                return [];
            }

            Logger.LogDebug("Table of {Count} hero owners at {Offset:X}", length, start);
            return [new RecordChunk(Name, bytes.AsSpan(start, length).ToArray(), start, 0)];
        }

        private static bool IsTable(byte[] bytes, int start, int length)
        {
            var tail = start + (2 * length);
            if (bytes[tail] != HeroOwnersLayout.TailFirst || bytes[tail + 1] != HeroOwnersLayout.TailSecond)
            {
                return false;
            }

            for (var i = 0; i < length; i++)
            {
                if (!HeroOwnersLayout.IsSeen(bytes[start + i]) || bytes[start + length + i] != HeroOwnersLayout.Nobody)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
