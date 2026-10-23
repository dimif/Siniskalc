namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Reads the date and the description of the map from the header of a savegame.
    /// </summary>
    public interface IHeaderReader
    {
        SaveMetadata<PlayerColor> Read(byte[] bytes, SaveLayout layout);
    }
}
