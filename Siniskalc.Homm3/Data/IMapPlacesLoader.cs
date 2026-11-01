namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Finds where the places a hero can visit are on the map.
    /// </summary>
    public interface IMapPlacesLoader
    {
        MapPlaces Load(byte[] bytes, Entities.EntityCatalog catalog);
    }
}
