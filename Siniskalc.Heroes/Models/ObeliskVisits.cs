namespace Siniskalc.Heroes.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// Which of the obelisks of the map each player has visited: a byte for each obelisk with a bit for every colour that visited it (bit
    /// <c>n</c> is the colour with code <c>n</c>), in the order the savefile lists the obelisks.
    /// </summary>
    /// <typeparam name="TColor">The colour of the game.</typeparam>
    public sealed record ObeliskVisits<TColor>
        where TColor : AbstractEnumeration, IPlayerColor
    {
        private readonly byte[] masks;
        private readonly MapPosition?[] positions;

        /// <param name="masks">One byte for each obelisk, with a bit for every colour that has visited it.</param>
        /// <param name="positions">Where each obelisk is on the map, null for one that was not found; may be empty. Read-only.</param>
        public ObeliskVisits(IEnumerable<byte> masks, IEnumerable<MapPosition?>? positions = null)
        {
            this.masks = [.. masks];
            this.positions = [.. positions ?? []];
        }

        /// <summary>
        /// Gets how many obelisks the map has.
        /// </summary>
        public int Count => this.masks.Length;

        /// <summary>
        /// Gets the byte of each obelisk: a bit for every colour that has visited it.
        /// </summary>
        public IReadOnlyList<byte> Masks => this.masks;

        /// <summary>
        /// Returns the visits of a map with <paramref name="count"/> obelisks that nobody has visited.
        /// </summary>
        public static ObeliskVisits<TColor> NoneVisited(int count) => new(new byte[count]);

        /// <summary>
        /// Returns where obelisk <paramref name="index"/> is on the map, or null when that is not known.
        /// </summary>
        public MapPosition? PositionOf(int index) => index >= 0 && index < this.positions.Length ? this.positions[index] : null;

        public bool IsVisited(int index, TColor color)
        {
            RangeGuard.ThrowIfNotIndex(index, this.Count, nameof(index));
            return (this.masks[index] & Bit(color)) != 0;
        }

        /// <summary>
        /// Gets how many obelisks <paramref name="color"/> has visited.
        /// </summary>
        public int VisitedCount(TColor color) => this.masks.Count(m => (m & Bit(color)) != 0);

        public bool AllVisited(TColor color) => this.VisitedCount(color) == this.Count;

        public ObeliskVisits<TColor> WithVisited(int index, TColor color, bool visited)
        {
            RangeGuard.ThrowIfNotIndex(index, this.Count, nameof(index));
            var changed = (byte[])this.masks.Clone();
            changed[index] = visited ? (byte)(changed[index] | Bit(color)) : (byte)(changed[index] & ~Bit(color));
            return new ObeliskVisits<TColor>(changed, this.positions);
        }

        /// <summary>
        /// Returns a copy in which <paramref name="color"/> has visited every obelisk, or none of them. The bits of the other colours stay as they are.
        /// </summary>
        public ObeliskVisits<TColor> WithAll(TColor color, bool visited)
        {
            var result = this;
            for (var i = 0; i < this.Count; i++)
            {
                result = result.WithVisited(i, color, visited);
            }

            return result;
        }

        public bool Equals(ObeliskVisits<TColor>? other) => other != null && this.masks.AsSpan().SequenceEqual(other.masks);

        public override int GetHashCode()
        {
            var hash = default(HashCode);
            foreach (var mask in this.masks)
            {
                hash.Add(mask);
            }

            return hash.ToHashCode();
        }

        private static int Bit(TColor color) => 1 << color.Code;
    }
}
