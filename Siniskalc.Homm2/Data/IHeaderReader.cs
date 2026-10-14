namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// Reads the date and the description of the map from the header of a savegame.
    /// </summary>
    public interface IHeaderReader
    {
        SaveMetadata Read(byte[] bytes, SaveLayout layout);
    }
}
