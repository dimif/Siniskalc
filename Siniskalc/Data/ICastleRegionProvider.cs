namespace Siniskalc.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Describes where each known field lives in a castle's chunk.
    /// </summary>
    public interface ICastleRegionProvider
    {
        /// <summary>
        /// Returns the known regions of <paramref name="chunk"/>, ordered by offset. The ones after the name depend on the length
        /// of the name. Regions that would run past the end of the chunk are left out.
        /// </summary>
        IReadOnlyList<ByteRegion> GetRegions(CastleChunk chunk);
    }
}
