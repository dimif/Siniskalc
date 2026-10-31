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
        /// <param name="offset">The offset of the byte.</param>
        /// <returns>The region that covers <paramref name="offset"/>, or <see langword="null"/>.</returns>
        ByteRegion? FindLeaf(int offset);

        /// <summary>
        /// Returns 0 or 1 for the byte at <paramref name="offset"/>; neighbouring regions alternate.
        /// </summary>
        /// <param name="offset">The offset of the byte.</param>
        /// <returns>0 or 1.</returns>
        int GetShade(int offset);

        /// <summary>
        /// Returns every region that covers <paramref name="offset"/>, from the outermost to the innermost.
        /// </summary>
        /// <param name="offset">The offset of the byte.</param>
        /// <returns>The regions that cover <paramref name="offset"/>; empty for a byte nothing is known about.</returns>
        IReadOnlyList<ByteRegion> FindAll(int offset);
    }
}
