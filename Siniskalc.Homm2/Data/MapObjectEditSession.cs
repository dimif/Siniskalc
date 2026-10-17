namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// A view onto one object of the map (a mine, a shrine, a sphinx) in a <see cref="SaveDocumentHistory"/>.
    /// </summary>
    public sealed class MapObjectEditSession<TModel> : EditSession<MapObjectChunk, TModel>
        where TModel : class
    {
        private readonly IMapObjectDeserializer<TModel> deserializer;

        public MapObjectEditSession(SaveDocumentHistory history, ChunkKind kind, int index)
            : base(history, kind, index, Collection(history, kind).Deserializer, Collection(history, kind).Serializer)
        {
            this.deserializer = Collection(history, kind).Deserializer;
        }

        /// <summary>
        /// Gets the object as it was when the savegame was loaded or last saved.
        /// </summary>
        public TModel OriginalModel => this.deserializer.Deserialize(this.OriginalChunk);

        private static MapObjectChunkCollection<TModel> Collection(SaveDocumentHistory history, ChunkKind kind) => (MapObjectChunkCollection<TModel>)history.Current.GetCollection(kind);
    }
}
