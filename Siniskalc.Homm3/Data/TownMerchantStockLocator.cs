namespace Siniskalc.Homm3.Data
{
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Resolves shared town stock from one uniquely marked date block. Stock values are not discovery evidence:
    /// all slots may be empty or contain artifact codes the editor does not know.
    /// </summary>
    public sealed class TownMerchantStockLocator(IGameDateLocator dateLocator) : ITownMerchantStockLocator
    {
        private static readonly ILogger<TownMerchantStockLocator> Logger = LogFactory.For<TownMerchantStockLocator>();

        /// <inheritdoc/>
        public int FindOffset(byte[] bytes, GameVersion version, int searchFrom = 0)
        {
            if (version is not (GameVersion.HornOfTheAbyss or GameVersion.ShadowOfDeath))
            {
                Logger.LogDebug("Town merchant stock layout is not established for {Version}", version);
                return -1;
            }

            var date = dateLocator.FindOffset(bytes, version, searchFrom);
            if (!HasRecognizedPrefix(bytes, date))
            {
                Logger.LogDebug("No marked date block for town merchant stock; unmarked dates are not used");
                return -1;
            }

            if (date > bytes.Length - TownMerchantStockLayout.OffsetFromDate - TownMerchantStockLayout.Length)
            {
                Logger.LogWarning("Town merchant stock after date at {Offset:X} is truncated", date);
                return -1;
            }

            var nextDate = dateLocator.FindOffset(bytes, version, date + 1);
            if (HasRecognizedPrefix(bytes, nextDate))
            {
                Logger.LogWarning("Competing marked dates at {First:X} and {Second:X}; town merchant stock is not located", date, nextDate);
                return -1;
            }

            var stock = date + TownMerchantStockLayout.OffsetFromDate;
            Logger.LogDebug("Located shared town merchant stock at {Offset:X}", stock);
            return stock;
        }

        private static bool HasRecognizedPrefix(byte[] bytes, int date) =>
            date >= TownMerchantStockLayout.DatePrefixLength
            && date <= bytes.Length
            && bytes[date - 4] == 0x7F
            && bytes[date - 3] <= 1
            && bytes[date - 2] == 0x02
            && bytes[date - 1] <= 1;
    }
}
