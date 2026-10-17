namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// Reads an object of the map (a mine, a shrine, a sphinx) from its chunk.
    /// </summary>
    public interface IMapObjectDeserializer<TModel> : IDeserializer<MapObjectChunk, TModel>
    {
    }
}
