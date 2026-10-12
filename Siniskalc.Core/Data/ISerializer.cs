namespace Siniskalc.Core.Data
{
    /// <summary>
    /// Writes a model back into the chunk it was read from. It writes the model as it is given: whoever edits builds the final model.
    /// </summary>
    /// <remarks>
    /// The implementations are the <c>Safe...Serializer</c> classes. They are safe because writing the model of a chunk that was
    /// not edited gives back the bytes it was read from, so saving never damages what the editor cannot show. Every method returns a
    /// new chunk and never modifies the one it is given.
    /// </remarks>
    /// <typeparam name="TChunk">The chunk the model is written into, and the chunk that is returned.</typeparam>
    /// <typeparam name="TModel">The model that is written.</typeparam>
    public interface ISerializer<TChunk, in TModel>
        where TChunk : BytesChunk
    {
        /// <summary>
        /// Returns <paramref name="chunk"/> with <paramref name="model"/> written to it.
        /// </summary>
        /// <param name="chunk">
        /// The chunk backing the entity.
        /// </param>
        /// <param name="model">
        /// The entity to serialize.
        /// </param>
        TChunk Serialize(TChunk chunk, TModel model);
    }
}
