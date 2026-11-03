namespace Siniskalc.Core.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// The records of one kind of a savefile (the mines, the dwellings, the shrines), as many as it has, read, written and compared with
    /// the <see cref="RecordKit{TChunk, TModel}"/> of their kind. An <see cref="EditSession{TChunk, TModel}"/> built from the kind alone takes
    /// its reader and writer from here.
    /// </summary>
    /// <typeparam name="TChunk">The chunk of a record.</typeparam>
    /// <typeparam name="TModel">The model a record is read as.</typeparam>
    public sealed class RecordChunkCollection<TChunk, TModel> : ModelChunkCollection<TChunk, TModel, IDeserializer<TChunk, TModel>, ISerializer<TChunk, TModel>>
        where TChunk : BytesChunk
    {
        /// <param name="chunks">The records, in the order of the savefile.</param>
        /// <param name="kit">What reads, writes and compares a record.</param>
        public RecordChunkCollection(IEnumerable<TChunk> chunks, RecordKit<TChunk, TModel> kit)
            : base(chunks, kit.Deserializer, kit.Serializer, kit.Differ.Diff)
        {
            this.Kit = kit;
        }

        /// <summary>
        /// Gets what reads, writes and compares a record.
        /// </summary>
        public RecordKit<TChunk, TModel> Kit { get; }

        /// <inheritdoc/>
        public override RecordChunkCollection<TChunk, TModel> WithChunk(int index, TChunk chunk) => new(this.Replace(index, chunk), this.Kit);
    }
}
