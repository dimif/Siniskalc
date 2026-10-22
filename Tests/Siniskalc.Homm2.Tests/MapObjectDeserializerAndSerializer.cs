namespace Siniskalc.Homm2.Tests
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Data;

    /// <summary>
    /// A reader and a writer of one kind of object of the map, for the tests that read a chunk and write it back.
    /// </summary>
    internal sealed record MapObjectDeserializerAndSerializer<TModel>(IDeserializer<MapObjectChunk, TModel> Reader, ISerializer<MapObjectChunk, TModel> Writer)
    {
        public TModel Deserialize(MapObjectChunk chunk) => this.Reader.Deserialize(chunk);

        public MapObjectChunk Serialize(MapObjectChunk chunk, TModel model) => this.Writer.Serialize(chunk, model);
    }
}
