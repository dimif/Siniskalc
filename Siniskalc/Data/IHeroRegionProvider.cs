namespace Siniskalc.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Describes where each known field lives in a hero's chunk.
    /// </summary>
    public interface IHeroRegionProvider
    {
        /// <summary>Returns the regions of a hero chunk, ordered by offset.</summary>
        IReadOnlyList<ByteRegion> GetRegions(HeroOffsets offsets, int chunkLength = int.MaxValue);

        /// <summary>Returns the regions of what precedes the hero chunk: the header with the biography.</summary>
        IReadOnlyList<ByteRegion> GetHeaderRegions(HeroOffsets offsets);
    }
}
