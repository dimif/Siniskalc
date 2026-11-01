namespace Siniskalc.Core.Data
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// The bytes of a whole savefile at one moment, with its known regions, for showing in a hex view.
    /// </summary>
    /// <param name="Title">What is shown, e.g. the name of the savefile.</param>
    /// <param name="Bytes">The savefile with every edit applied, as it would be saved.</param>
    /// <param name="OriginalBytes">The savefile as it was loaded or last saved, to mark what has been edited.</param>
    /// <param name="Regions">The known regions of <paramref name="Bytes"/>.</param>
    public sealed record SaveHexDump(string Title, byte[] Bytes, byte[] OriginalBytes, IReadOnlyList<ByteRegion> Regions)
    {
        private static readonly ILogger<SaveHexDump> Logger = LogFactory.For<SaveHexDump>();

        /// <summary>
        /// Takes the current state of the savefile in <paramref name="history"/>.
        /// </summary>
        /// <param name="title">What is shown, e.g. the name of the savefile.</param>
        /// <param name="history">The history whose current and original documents are written to bytes.</param>
        /// <param name="regionProvider">Says where each known field lives in the savefile.</param>
        /// <returns>The dump of the current bytes, the original bytes and the regions of the current bytes.</returns>
        /// <exception cref="System.ArgumentException">A chunk of either document lies outside the savefile or overlaps another.</exception>
        public static SaveHexDump Create(string title, SaveDocumentHistory history, ISaveRegionProvider regionProvider)
        {
            var current = history.Current.ToBytes();
            var regions = regionProvider.GetRegions(history.Current, current);
            Logger.LogDebug("Took a hex dump of {Length} bytes with {Count} regions", current.Length, regions.Count);
            return new SaveHexDump(title, current, history.Original.ToBytes(), regions);
        }
    }
}
