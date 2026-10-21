namespace Siniskalc.Homm2.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Where the places of each <see cref="PlaceKind"/> are on the map, in the order of the map (row by row, from the top left), which is the
    /// order of the bits a hero keeps: the first place of a kind is bit 1 (bit 0 is not used). The positions are the entrance tile of the place.
    /// Only the first 31 places of a kind have a bit.
    /// </summary>
    public sealed class MapPlaces
    {
        /// <summary>
        /// The most places of a kind a hero has a bit for (a bit is the 32 bit number of the kind, and bit 0 is not used).
        /// </summary>
        public const int MaxPerKind = 31;

        private readonly IReadOnlyDictionary<PlaceKind, IReadOnlyList<MapPosition>> places;

        public MapPlaces(IReadOnlyDictionary<PlaceKind, IReadOnlyList<MapPosition>> places)
        {
            this.places = places;
        }

        /// <summary>
        /// Gets the places of a map in which none were found.
        /// </summary>
        public static MapPlaces None { get; } = new(new Dictionary<PlaceKind, IReadOnlyList<MapPosition>>());

        /// <summary>
        /// Returns the places of a kind, the ones that have a bit, in the order of the bits (the first is bit 1).
        /// </summary>
        public IReadOnlyList<MapPosition> Of(PlaceKind kind) => this.places.TryGetValue(kind, out var list) ? list.Take(MaxPerKind).ToList() : [];
    }
}
