namespace Siniskalc.Core.Data
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Core.Data;

    /// <summary>
    /// The bytes of a whole savefile at one moment, with its known regions, for showing in a hex view.
    /// </summary>
    /// <param name="Title">What is shown, e.g. the name of the savefile.</param>
    /// <param name="Bytes">The savefile with every edit applied, as it would be saved.</param>
    /// <param name="OriginalBytes">The savefile as it was loaded or last saved, to mark what has been edited.</param>
    /// <param name="Regions">The known regions of <paramref name="Bytes"/>.</param>
    public sealed record SaveHexDump(string Title, byte[] Bytes, byte[] OriginalBytes, IReadOnlyList<ByteRegion> Regions)
    {
        /// <summary>
        /// Takes the current state of the savefile in <paramref name="history"/>.
        /// </summary>
        public static SaveHexDump Create(string title, SaveDocumentHistory history, ISaveRegionProvider regionProvider)
        {
            var current = history.Current.ToBytes();
            return new SaveHexDump(title, current, history.Original.ToBytes(), regionProvider.GetRegions(history.Current, current));
        }
    }
}
