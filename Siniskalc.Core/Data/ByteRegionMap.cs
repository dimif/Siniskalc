namespace Siniskalc.Core.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// Looks up which regions cover each byte of a chunk. Where regions overlap, the first one listed wins
    /// for <see cref="FindLeaf"/>; <see cref="FindAll"/> returns every one.
    /// </summary>
    public sealed class ByteRegionMap : IByteRegionLookup
    {
        private static readonly ILogger<ByteRegionMap> Logger = LogFactory.For<ByteRegionMap>();

        private readonly ByteRegion[] leafAt;
        private readonly int[] shadeAt;

        // The regions after the first one at an offset. Few bytes have any, so they are kept apart from the arrays: a map of a
        // whole savefile is a million bytes long.
        private readonly Dictionary<int, List<ByteRegion>> overlapsAt = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteRegionMap"/> class.
        /// </summary>
        /// <param name="regions">The regions to map, in the order that decides which one wins where they overlap.</param>
        /// <param name="length">The number of bytes of the chunk. Parts of a region outside 0 to <paramref name="length"/> are left out.</param>
        /// <exception cref="System.OverflowException"><paramref name="length"/> is negative.</exception>
        public ByteRegionMap(IReadOnlyList<ByteRegion> regions, int length)
        {
            Logger.LogTrace("Mapping {Count} regions over {Length} bytes", regions.Count, length);
            this.Regions = regions;
            this.leafAt = new ByteRegion[length];
            this.shadeAt = new int[length];

            var leaves = regions.SelectMany(r => r.Leaves()).OrderBy(l => l.Start).ToList();
            Logger.LogTrace("Found {Count} leaves in the regions", leaves.Count);
            for (var i = 0; i < leaves.Count; i++)
            {
                var leaf = leaves[i];
                if (leaf.Start < 0 || leaf.End > length)
                {
                    Logger.LogTrace("Leaf {Name} at {Offset:X} for {Length:X} bytes is partly outside the chunk of {ChunkLength:X} bytes", leaf.FullName, leaf.Start, leaf.Length, length);
                }

                for (var offset = leaf.Start; offset < leaf.End && offset < length; offset++)
                {
                    if (offset < 0)
                    {
                        continue;
                    }

                    if (this.leafAt[offset] == null)
                    {
                        this.leafAt[offset] = leaf;

                        // Neighbouring regions of the same kind alternate, so they can be told apart.
                        this.shadeAt[offset] = i % 2;
                    }
                    else
                    {
                        if (!this.overlapsAt.TryGetValue(offset, out var overlaps))
                        {
                            this.overlapsAt[offset] = overlaps = [];
                        }

                        overlaps.Add(leaf);
                    }
                }
            }

            Logger.LogTrace("Mapped {Count} leaves, with {Overlaps} offsets covered by more than one", leaves.Count, this.overlapsAt.Count);
        }

        /// <summary>
        /// Gets the regions the map was built from.
        /// </summary>
        public IReadOnlyList<ByteRegion> Regions { get; }

        /// <summary>
        /// Returns the region (or element) that covers <paramref name="offset"/>, or null for unknown bytes.
        /// </summary>
        /// <param name="offset">The offset of the byte, relative to the start of the chunk.</param>
        /// <returns>The first listed leaf that covers <paramref name="offset"/>, or <see langword="null"/> if none does or <paramref name="offset"/> is outside the chunk.</returns>
        public ByteRegion? FindLeaf(int offset) => offset >= 0 && offset < this.leafAt.Length ? this.leafAt[offset] : null;

        /// <summary>
        /// Returns 0 or 1: alternates between neighbouring regions so adjacent ones look different.
        /// </summary>
        /// <param name="offset">The offset of the byte, relative to the start of the chunk.</param>
        /// <returns>0 or 1, the shade of the leaf at <paramref name="offset"/>; 0 for an unknown byte or an offset outside the chunk.</returns>
        public int GetShade(int offset) => offset >= 0 && offset < this.shadeAt.Length ? this.shadeAt[offset] : 0;

        /// <summary>
        /// Returns every region (or element) covering <paramref name="offset"/>.
        /// </summary>
        /// <param name="offset">The offset of the byte, relative to the start of the chunk.</param>
        /// <returns>The leaves that cover <paramref name="offset"/>, the one <see cref="FindLeaf"/> returns first; empty for an unknown byte or an offset outside the chunk.</returns>
        public IReadOnlyList<ByteRegion> FindAll(int offset)
        {
            var leaf = this.FindLeaf(offset);
            if (leaf == null)
            {
                return [];
            }

            return this.overlapsAt.TryGetValue(offset, out var overlaps) ? [leaf, .. overlaps] : [leaf];
        }
    }
}
