namespace Siniskalc.Core.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// Describes where each known field lives in the whole savefile.
    /// </summary>
    public interface ISaveRegionProvider
    {
        /// <summary>
        /// Returns the top level regions of the savefile, ordered by offset.
        /// </summary>
        /// <param name="document">Where the heroes, players and castles are.</param>
        /// <param name="bytes">The bytes of the savefile that go with <paramref name="document"/>.</param>
        /// <returns>The regions, ordered by offset.</returns>
        IReadOnlyList<ByteRegion> GetRegions(SaveDocument document, byte[] bytes);
    }
}
