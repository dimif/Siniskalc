namespace Siniskalc.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Entities;

    /// <summary>
    /// Which of the obelisks of the map each player has visited. The obelisks are counted in the order the savefile lists them (which is
    /// not the order of the visits). The game keeps a byte for each obelisk with one bit for each colour (bit 0 is red, 1 blue, and so on in
    /// the order of <see cref="PlayerColor"/>), and sets the bits of the whole team of the player that visits; what other teams visit does
    /// not count.
    /// </summary>
    public sealed record ObeliskVisits
    {
        private readonly byte[] masks;
        private readonly Position?[] positions;

        /// <param name="masks">One byte for each obelisk, with a bit for every colour that has visited it.</param>
        /// <param name="positions">Where each obelisk is on the map, null for one that was not found; may be empty. Read-only.</param>
        public ObeliskVisits(IEnumerable<byte> masks, IEnumerable<Position?>? positions = null)
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
        /// Returns where obelisk <paramref name="index"/> is on the map, or null when that is not known.
        /// </summary>
        public Position? PositionOf(int index) => index >= 0 && index < this.positions.Length ? this.positions[index] : null;

        /// <summary>
        /// Returns the visits of a map with <paramref name="count"/> obelisks that nobody has visited.
        /// </summary>
        public static ObeliskVisits NoneVisited(int count) => new(new byte[count]);

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
        /// Returns a copy in which <paramref name="color"/> has visited every obelisk, or none of them. The bits of the other colours stay as they are.
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

        private static int Bit(PlayerColor color) => 1 << (int)color;
    }
}
