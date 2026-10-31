namespace Siniskalc.Core.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// Finds the known fields of a savefile that cover a byte, which is all a hex grid needs to colour and describe the bytes it shows.
    /// </summary>
    public interface IByteRegionLookup
    {
        /// <summary>
        /// Returns the most specific region that covers <paramref name="offset"/>, or <c>null</c> for a byte nothing is known about.
        /// </summary>
        ByteRegion? FindLeaf(int offset);

        /// <summary>
        /// Returns 0 or 1 for the byte at <paramref name="offset"/>; neighbouring regions alternate.
        /// </summary>
        int GetShade(int offset);

        /// <summary>
        /// Returns every region that covers <paramref name="offset"/>, from the outermost to the innermost.
        /// </summary>
        IReadOnlyList<ByteRegion> FindAll(int offset);
    }
}
