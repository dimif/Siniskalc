namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Reads the places a hero can visit once from the table of tiles of a savegame (<see cref="MapTileTable"/>). The byte at 9 of the tile
    /// a hero enters a place by tells which it is (it agrees with the pictures of the tiles in the maps of the test saves: Threedom, Broken
    /// Alliance, Arrax the Jerk and the maps of the other sizes): a gazebo is 0x8A, a fort 0xBE, a witch doctor's hut 0xC5, a mercenary camp
    /// 0xC9, the standing stones 0xC2 (on both of their tiles) and Xanadu 0xD6. The places of a kind are numbered from 1 in the order of the
    /// map (row by row, from the top left), which is the bit of the hero's set that each has (bit 0 is not used); a place after the
    /// <see cref="Constants.Hero.MaxVisitedPlacesPerKind"/>th of a kind has no bit and is dropped.
    /// </summary>
    public sealed class MapPlacesReader : IMapPlacesReader
    {
        private static readonly ILogger<MapPlacesReader> Logger = LogFactory.For<MapPlacesReader>();

        private readonly GameCatalogs catalogs;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapPlacesReader"/> class.
        /// </summary>
        /// <param name="catalogs">The catalogs.</param>
        public MapPlacesReader(GameCatalogs catalogs)
        {
            this.catalogs = catalogs;
        }

        /// <inheritdoc/>
        public MapPlaces<PlaceKind> Read(byte[] bytes, SaveLayout layout)
        {
            var tiles = MapTileTable.FindStart(bytes, layout);
            if (tiles < 0)
            {
                Logger.LogWarning("The table of tiles of the map was not found, so the places a hero can visit are not listed");
                return MapPlaces<PlaceKind>.None;
            }

            var objects = MapTileTable.ObjectBytes(bytes, layout, tiles);
            var size = layout.Width;
            var result = new Dictionary<PlaceKind, IReadOnlyList<(int Number, MapPosition Position)>>();
            foreach (var place in this.catalogs.For(layout.Version).Places)
            {
                var (kind, value) = (place.Kind, place.Code);

                // The standing stones are two tiles wide: the place is the left one, so a tile with the same byte on its left is the other half.
                var found = MapTileTable.Find(objects, size, value, kind == PlaceKind.StandingStones ? tile => tile % size == 0 || objects[tile - 1] != value : null);
                result[kind] = Numbered(kind, found);
            }

            return new MapPlaces<PlaceKind>(result);
        }

        private static IReadOnlyList<(int Number, MapPosition Position)> Numbered(PlaceKind kind, IReadOnlyList<MapPosition> found)
        {
            if (found.Count > Constants.Hero.MaxVisitedPlacesPerKind)
            {
                Logger.LogWarning(
                    "The map has {Count} places of kind {Kind}, but a hero has a bit for only the first {Max}; the others are not listed",
                    found.Count,
                    kind,
                    Constants.Hero.MaxVisitedPlacesPerKind);
            }

            // The first place of a kind is bit 1 of the hero's set (bit 0 is not used).
            return found.Take(Constants.Hero.MaxVisitedPlacesPerKind).Select((position, index) => (index + 1, position)).ToList();
        }
    }
}
