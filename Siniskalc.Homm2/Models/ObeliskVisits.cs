namespace Siniskalc.Homm2.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Which of the obelisks of the map each player has visited: a byte for each obelisk with a bit for every colour that visited it (bit 0 is
    /// Blue, in the order of <see cref="PlayerColor"/>), in the order of the obelisks on the map.
    /// </summary>
    public sealed record ObeliskVisits
    {
        private readonly byte[] masks;
        private readonly MapPosition[] positions;

        /// <param name="masks">One byte for each obelisk.</param>
        /// <param name="positions">Where each obelisk is on the map; may be empty. Read-only.</param>
        public ObeliskVisits(IEnumerable<byte> masks, IEnumerable<MapPosition>? positions = null)
        {
            this.masks = [.. masks];
            this.positions = [.. positions ?? []];
        }

        /// <summary>
        /// Gets how many obelisks the map has.
        /// </summary>
        public int Count => this.masks.Length;

        public IReadOnlyList<byte> Masks => this.masks;

        /// <summary>
        /// Returns where obelisk <paramref name="index"/> is on the map, or null when that is not known.
        /// </summary>
        public MapPosition? PositionOf(int index) => index >= 0 && index < this.positions.Length ? this.positions[index] : null;

        public bool IsVisited(int index, PlayerColor color)
        {
            RangeGuard.ThrowIfNotIndex(index, this.Count, nameof(index));
            return (this.masks[index] & Bit(color)) != 0;
        }

        /// <summary>
        /// Gets how many obelisks <paramref name="color"/> has visited.
        /// </summary>
        public int VisitedCount(PlayerColor color) => this.masks.Count(m => (m & Bit(color)) != 0);

        public bool AllVisited(PlayerColor color) => this.VisitedCount(color) == this.Count;

        public ObeliskVisits WithVisited(int index, PlayerColor color, bool visited)
        {
            RangeGuard.ThrowIfNotIndex(index, this.Count, nameof(index));
            var changed = (byte[])this.masks.Clone();
            changed[index] = visited ? (byte)(changed[index] | Bit(color)) : (byte)(changed[index] & ~Bit(color));
            return new ObeliskVisits(changed, this.positions);
        }

        /// <summary>
        /// Returns a copy in which <paramref name="color"/> has visited every obelisk, or none of them. The bits of the other colours stay.
        /// </summary>
        public ObeliskVisits WithAll(PlayerColor color, bool visited)
        {
            var result = this;
            for (var i = 0; i < this.Count; i++)
            {
                result = result.WithVisited(i, color, visited);
            }

            return result;
        }

        public bool Equals(ObeliskVisits? other) => other != null && this.masks.AsSpan().SequenceEqual(other.masks);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var mask in this.masks)
            {
                hash.Add(mask);
            }

            return hash.ToHashCode();
        }

        private static int Bit(PlayerColor color) => 1 << color.Code;
    }
}
