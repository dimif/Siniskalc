namespace Siniskalc.Data
{
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// Finds the header of the map (name, size, difficulty) in a savefile.
    /// </summary>
    public interface IMapHeaderLocator
    {
        /// <summary>Reads the header of the map, or returns <c>null</c> when it cannot be found.</summary>
        MapHeader? Find(byte[] bytes, GameVersion gameVersion);

        /// <summary>Returns where the header is, or <c>null</c> when it cannot be found.</summary>
        MapHeaderLocation? Locate(byte[] bytes, GameVersion gameVersion);
    }
}
