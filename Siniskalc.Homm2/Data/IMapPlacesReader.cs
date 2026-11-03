namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Finds the places of the map that a hero remembers having visited (forts, gazebos...).
    /// </summary>
    public interface IMapPlacesReader
    {
        /// <summary>
        /// Reads the places from the table of tiles of a savegame; the places are the same in every save of a game. A savegame in which the
        /// table of tiles is not found has none.
        /// </summary>
        MapPlaces Read(byte[] bytes, SaveLayout layout);
    }
}
