namespace Siniskalc.Core.Data
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Looks up which regions cover each byte of a chunk. Where regions overlap, the first one listed wins
    /// for <see cref="FindLeaf"/>; <see cref="FindAll"/> returns every one.
    /// </summary>
    public sealed class ByteRegionMap
    {
        private readonly ByteRegion[] leafAt;
        private readonly int[] shadeAt;

        // The regions after the first one at an offset. Few bytes have any, so they are kept apart from the arrays: a map of a
        // whole savefile is a million bytes long.
        private readonly Dictionary<int, List<ByteRegion>> overlapsAt = [];

        public ByteRegionMap(IReadOnlyList<ByteRegion> regions, int length)
        {
            this.Regions = regions;
            this.leafAt = new ByteRegion[length];
            this.shadeAt = new int[length];

            var leaves = regions.SelectMany(r => r.Leaves()).OrderBy(l => l.Start).ToList();
            for (var i = 0; i < leaves.Count; i++)
            {
                var leaf = leaves[i];
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
        }

        public IReadOnlyList<ByteRegion> Regions { get; }

        /// <summary>
        /// Returns the region (or element) that covers <paramref name="offset"/>, or null for unknown bytes.
        /// </summary>
        public ByteRegion? FindLeaf(int offset) => offset >= 0 && offset < this.leafAt.Length ? this.leafAt[offset] : null;

        /// <summary>
        /// Returns 0 or 1: alternates between neighbouring regions so adjacent ones look different.
        /// </summary>
        public int GetShade(int offset) => offset >= 0 && offset < this.shadeAt.Length ? this.shadeAt[offset] : 0;

        /// <summary>
        /// Returns every region (or element) covering <paramref name="offset"/>.
        /// </summary>
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
