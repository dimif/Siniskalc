namespace Siniskalc.Core.Data
{
    /// <summary>
    /// A collection of chunks that can be read into models and written back from them.
    /// </summary>
    /// <typeparam name="TChunk">The chunk.</typeparam>
    /// <typeparam name="TModel">The model of a chunk.</typeparam>
    public interface IModelChunkCollection<TChunk, TModel>
        where TChunk : BytesChunk
    {
        /// <summary>
        /// Gets what reads a chunk into its model.
        /// </summary>
        IDeserializer<TChunk, TModel> ChunkDeserializer { get; }

        /// <summary>
        /// Gets what writes a model into a chunk.
        /// </summary>
        ISerializer<TChunk, TModel> ChunkSerializer { get; }
    }
}
