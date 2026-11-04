namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Finds the places of the map that a hero remembers having visited (forts, gazebos...).
    /// </summary>
    public interface IMapPlacesReader
    {
        /// <summary>
        /// Reads the places from the table of tiles of a savegame; the places are the same in every save of a game. A savegame in which the
        /// table of tiles is not found has none. The places of a kind are numbered from 1 in the order of the map, the bit of the hero's set
        /// that each has, and only the first <see cref="Constants.Hero.MaxVisitedPlacesPerKind"/> of a kind are kept.
        /// </summary>
        /// <param name="bytes">The bytes of the savegame.</param>
        /// <param name="layout">The layout of the savegame.</param>
        /// <returns>The places of each kind, by their number.</returns>
        MapPlaces<PlaceKind> Read(byte[] bytes, SaveLayout layout);
    }
}
