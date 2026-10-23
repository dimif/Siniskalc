namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Describes where each known field lives in the record of a mine, using the offsets of <see cref="MineLayout"/>.
    /// </summary>
    public sealed class MineRegionProvider : IMineRegionProvider
    {
        /// <inheritdoc/>
        public IReadOnlyList<ByteRegion> GetRegions(RecordChunk chunk)
        {
            var guard = MineLayout.Guard;
            var regions = new List<ByteRegion>
            {
                new("Pos X", MineLayout.X, 1, ByteRegionKind.Position),
                new("Pos Y", MineLayout.Y, 1, ByteRegionKind.Position),
                new("Pos Lvl", MineLayout.Level, 1, ByteRegionKind.Position),
                new("Owner", MineLayout.Owner, 1, ByteRegionKind.Identity, null, "PlayerColor colour; FF means none"),
                new("Kind", MineLayout.Kind, 1, ByteRegionKind.Identity, null, "0 sawmill, 1 alchemist's lab, 2 ore, 3 sulfur, 4 crystal, 5 gems, 6 gold"),
                new("Abandoned", MineLayout.Abandoned, 1, ByteRegionKind.Other, null, "1 for a mine that was abandoned at the start"),
                Slots("Guard creatures", guard.TypeOffset, guard.SlotCount, guard.TypeSize),
                Slots("Guard count", guard.CountOffset, guard.SlotCount, guard.CountSize),
            };

            return regions.Where(r => r.End <= chunk.Length).OrderBy(r => r.Start).ToList();
        }

        private static ByteRegion Slots(string name, int start, int count, int size)
        {
            var parts = Enumerable.Range(0, count).Select(i => new ByteRegion($"Slot {i + 1}", start + (i * size), size, ByteRegionKind.Creatures, name)).ToList();
            return new ByteRegion(name, start, count * size, ByteRegionKind.Creatures, null, null, parts);
        }
    }
}
