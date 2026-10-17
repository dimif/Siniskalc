namespace Siniskalc.Homm2.Data
{
    /// <summary>
    /// Finds the mines, the shrines and the sphinxes of the map in a savegame.
    /// </summary>
    public interface IMapObjectsReader
    {
        MapObjectChunks Read(byte[] bytes, SaveLayout layout);
    }
}
