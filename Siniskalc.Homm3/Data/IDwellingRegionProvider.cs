namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Describes where each known field lives in the record of a dwelling.
    /// </summary>
    public interface IDwellingRegionProvider
    {
        /// <summary>
        /// Returns the known regions of <paramref name="chunk"/>, ordered by offset. Regions that would run past the end of the record are left out.
        /// </summary>
        IReadOnlyList<ByteRegion> GetRegions(RecordChunk chunk);
    }
}
