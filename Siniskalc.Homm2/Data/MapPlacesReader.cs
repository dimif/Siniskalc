namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the places a hero can visit once from the table of tiles of a savegame (<see cref="MapTileTable"/>). The byte at 9 of the tile
    /// a hero enters a place by tells which it is (it agrees with the pictures of the tiles in the maps of the test saves: Threedom, Broken
    /// Alliance, Arrax the Jerk and the maps of the other sizes): a gazebo is 0x8A, a fort 0xBE, a witch doctor's hut 0xC5, a mercenary camp
    /// 0xC9, the standing stones 0xC2 (on both of their tiles) and Xanadu 0xD6.
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
        public MapPlaces Read(byte[] bytes, SaveLayout layout)
        {
            var tiles = MapTileTable.FindStart(bytes, layout);
            if (tiles < 0)
            {
                Logger.LogWarning("The table of tiles of the map was not found, so the places a hero can visit are not listed");
                return MapPlaces.None;
            }

            var objects = MapTileTable.ObjectBytes(bytes, layout, tiles);
            var size = layout.Width;
            var result = new Dictionary<PlaceKind, IReadOnlyList<MapPosition>>();
            foreach (var place in this.catalogs.For(layout.Version).Places)
            {
                var (kind, value) = (place.Kind, place.Code);

                // The standing stones are two tiles wide: the place is the left one, so a tile with the same byte on its left is the other half.
                result[kind] = MapTileTable.Find(objects, size, value, kind == PlaceKind.StandingStones ? tile => tile % size == 0 || objects[tile - 1] != value : null);
            }

            return new MapPlaces(result);
        }
    }
}
