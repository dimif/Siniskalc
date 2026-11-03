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

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaceVisits"/> class.
        /// </summary>
        /// <param name="masks">The masks.</param>
        public PlaceVisits(IEnumerable<uint> masks)
        {
            this.masks = masks.ToArray();
            if (this.masks.Length != Constants.Catalog.PlaceCapacity)
            {
                throw new ArgumentException("There is a number for each kind of place.", nameof(masks));
            }
        }

        /// <summary>
        /// Gets the visits of a hero that visited nothing.
        /// </summary>
        public static PlaceVisits None { get; } = new(new uint[Constants.Catalog.PlaceCapacity]);

        /// <summary>
        /// Returns the number of the kind: a bit for each place the hero visited.
        /// </summary>
        /// <param name="kind">The kind of place.</param>
        /// <returns>The set.</returns>
        public uint Of(VisitedPlaceKind kind) => this.masks[kind.Index];

        /// <summary>
        /// Returns the visits with another number for the kind.
        /// </summary>
        /// <param name="kind">The kind of place.</param>
        /// <param name="mask">The new set.</param>
        /// <returns>The new visits; these are not changed.</returns>
        public PlaceVisits With(VisitedPlaceKind kind, uint mask)
        {
            var copy = (uint[])this.masks.Clone();
            copy[kind.Index] = mask;
            return new PlaceVisits(copy);
        }

        /// <inheritdoc/>
        public bool Equals(PlaceVisits? other) => other is not null && this.masks.AsSpan().SequenceEqual(other.masks);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => this.Equals(obj as PlaceVisits);

        /// <inheritdoc/>
        public override int GetHashCode() => this.masks.Aggregate(17, (hash, mask) => (hash * 31) + (int)mask);
    }
}
