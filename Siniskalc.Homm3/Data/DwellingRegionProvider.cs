namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Describes where each known field lives in the record of a dwelling, using the offsets of <see cref="DwellingLayout"/>.
    /// </summary>
    public sealed class DwellingRegionProvider : IDwellingRegionProvider
    {
        /// <inheritdoc/>
        public IReadOnlyList<ByteRegion> GetRegions(RecordChunk chunk)
        {
            var regions = new List<ByteRegion>
            {
                new("Owner", DwellingLayout.Owner, 1, ByteRegionKind.Identity, null, "PlayerColor colour; FF means none"),
                new("Object type", DwellingLayout.ObjectType, 1, ByteRegionKind.Identity, null, "17 for a creature generator"),
                new("Creature", DwellingLayout.Creature, 4, ByteRegionKind.Creatures, null, "The number of the creature the dwelling gives"),
                new("Creatures to hire", DwellingLayout.Stock, 4, ByteRegionKind.Creatures, null, "32 bits; the creatures that can be hired now"),
                new("Pos X", DwellingLayout.X, 1, ByteRegionKind.Position),
                new("Pos Y", DwellingLayout.Y, 1, ByteRegionKind.Position),
                new("Pos Lvl", DwellingLayout.Level, 1, ByteRegionKind.Position),
            };

            return regions.Where(r => r.End <= chunk.Length).OrderBy(r => r.Start).ToList();
        }
    }
}
