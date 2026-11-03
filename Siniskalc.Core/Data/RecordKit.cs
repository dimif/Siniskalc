namespace Siniskalc.Core.Data
{
    /// <summary>
    /// What reads, writes and compares one kind of record of a savefile.
    /// </summary>
    /// <typeparam name="TChunk">The chunk of a record.</typeparam>
    /// <typeparam name="TModel">The model a record is read as.</typeparam>
    /// <param name="Deserializer">Reads a record from its chunk.</param>
    /// <param name="Serializer">Writes a record back into its chunk.</param>
    /// <param name="Differ">Lists what an edit changed in a record.</param>
    public sealed record RecordKit<TChunk, TModel>(IDeserializer<TChunk, TModel> Deserializer, ISerializer<TChunk, TModel> Serializer, IDiffer<TModel> Differ)
        where TChunk : BytesChunk;
}
