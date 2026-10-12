namespace Siniskalc.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Describes where each known field lives in a player's chunk.
    /// </summary>
    public interface IPlayerRegionProvider
    {
        IReadOnlyList<ByteRegion> GetRegions(PlayerLayout layout, int chunkLength = int.MaxValue);
    }
}
