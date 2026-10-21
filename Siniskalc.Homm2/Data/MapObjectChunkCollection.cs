namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;

    /// <summary>
    /// The objects of one kind of the map (the mines, the shrines or the sphinxes), as many as the map has.
    /// </summary>
    public sealed class MapObjectChunkCollection<TModel> : ModelChunkCollection<MapObjectChunk, TModel, IMapObjectDeserializer<TModel>, IMapObjectSerializer<TModel>>
    {
        private readonly IMapObjectDiffer<TModel> differ;

        public MapObjectChunkCollection(IEnumerable<MapObjectChunk> chunks, MapObjectKit<TModel> kit)
            : base(chunks, kit.Deserializer, kit.Serializer, kit.Differ.Diff)
        {
            this.differ = kit.Differ;
        }

        /// <inheritdoc/>
        public override MapObjectChunkCollection<TModel> WithChunk(int index, MapObjectChunk chunk) =>
            new(this.Replace(index, chunk), new MapObjectKit<TModel>(this.Deserializer, this.Serializer, this.differ));
    }
}
