namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// Writes an object of the map back into its chunk.
    /// </summary>
    public interface IMapObjectSerializer<TModel> : ISerializer<MapObjectChunk, TModel>
    {
    }
}
