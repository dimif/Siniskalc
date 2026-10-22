namespace Siniskalc.Homm3.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// The places of each kind a hero visited (see <see cref="VisitedPlaceKind"/>): a number for each kind with a bit for each place.
    /// </summary>
    public sealed class PlaceVisits : IEquatable<PlaceVisits>
    {
        private readonly uint[] masks;

        public PlaceVisits(IEnumerable<uint> masks)
        {
            this.masks = masks.ToArray();
            if (this.masks.Length != VisitedPlaceKind.All.Count)
            {
                throw new ArgumentException("There is a number for each kind of place.", nameof(masks));
            }
        }

        /// <summary>
        /// Gets the visits of a hero that visited nothing.
        /// </summary>
        public static PlaceVisits None { get; } = new(new uint[VisitedPlaceKind.All.Count]);

        /// <summary>
        /// Returns the number of the kind: a bit for each place the hero visited.
        /// </summary>
        public uint Of(VisitedPlaceKind kind) => this.masks[kind.Index];

        /// <summary>
        /// Returns the visits with another number for the kind.
        /// </summary>
        public PlaceVisits With(VisitedPlaceKind kind, uint mask)
        {
            var copy = (uint[])this.masks.Clone();
            copy[kind.Index] = mask;
            return new PlaceVisits(copy);
        }

        public bool Equals(PlaceVisits? other) => other is not null && this.masks.AsSpan().SequenceEqual(other.masks);

        public override bool Equals(object? obj) => this.Equals(obj as PlaceVisits);

        public override int GetHashCode() => this.masks.Aggregate(17, (hash, mask) => (hash * 31) + (int)mask);
    }
}
