namespace Siniskalc.Core.Data
{
    /// <summary>
    /// Reads an entity from a chunk of a savefile. The counterpart of <see cref="ISerializer{TChunk, TModel}"/>.
    /// </summary>
    /// <typeparam name="TChunk">The chunk the entity is read from.</typeparam>
    /// <typeparam name="TDeserializedEntity">The entity that is read.</typeparam>
    public interface IDeserializer<in TChunk, out TDeserializedEntity>
        where TChunk : BytesChunk
    {
        /// <summary>
        /// Deserializes the entity from the given chunk. The chunk is not modified.
        /// </summary>
        /// <param name="chunk">The chunk to deserialize the entity from.</param>
        /// <returns>The deserialized entity.</returns>
        TDeserializedEntity Deserialize(TChunk chunk);
    }
}
