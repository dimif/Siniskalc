namespace Siniskalc.Core.Data
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A known range of bytes inside an entity's byte chunk, e.g. the creature types of a hero.
    /// A region made of equal-sized elements (creature slots, backpack slots, ...) lists them in <see cref="Parts"/>.
    /// </summary>
    /// <param name="Name">What the bytes are, e.g. "Attack".</param>
    /// <param name="Start">Offset of the first byte, relative to the start of the chunk.</param>
    /// <param name="Length">Number of bytes.</param>
    /// <param name="Kind">The category the region belongs to.</param>
    /// <param name="Group">For a part, the name of the region it belongs to; otherwise null.</param>
    /// <param name="Description">An optional note on how the bytes are read.</param>
    /// <param name="Parts">The elements of an array-like region, in order. Empty when the region is not split up.</param>
    public sealed record ByteRegion(
        string Name,
        int Start,
        int Length,
        ByteRegionKind Kind,
        string? Group = null,
        string? Description = null,
        IReadOnlyList<ByteRegion>? Parts = null)
    {
        /// <summary>
        /// Gets the offset just past the last byte.
        /// </summary>
        public int End => this.Start + this.Length;

        /// <summary>
        /// Gets the name including the group, e.g. "Creatures: Slot 2 count".
        /// </summary>
        public string FullName => this.Group == null ? this.Name : $"{this.Group}: {this.Name}";

        /// <summary>
        /// Gets the elements of the region, or an empty list.
        /// </summary>
        public IReadOnlyList<ByteRegion> Elements => this.Parts ?? [];

        /// <summary>
        /// Returns whether the byte at <paramref name="offset"/> is inside this region.
        /// </summary>
        public bool Contains(int offset) => offset >= this.Start && offset < this.End;

        /// <summary>
        /// Returns the region itself, or the leaves of its elements when it has some.
        /// </summary>
        public IEnumerable<ByteRegion> Leaves() => this.Elements.Count == 0 ? [this] : this.Elements.SelectMany(e => e.Leaves());

        /// <summary>
        /// Returns a copy of the region, and of its elements, moved <paramref name="offset"/> bytes along. Used to place the
        /// regions of a chunk at their position in the savefile. <paramref name="groupPrefix"/> is put in front of the group of
        /// every leaf (the leaf gets it as its group if it has none), so that the leaves of different chunks can be told apart.
        /// </summary>
        public ByteRegion Shifted(int offset, string? groupPrefix = null)
        {
            var group = groupPrefix == null ? this.Group : this.Group == null ? groupPrefix : $"{groupPrefix} / {this.Group}";
            return this with
            {
                Start = this.Start + offset,
                Group = group,
                Parts = this.Parts?.Select(p => p.Shifted(offset, groupPrefix)).ToList(),
            };
        }
    }
}
