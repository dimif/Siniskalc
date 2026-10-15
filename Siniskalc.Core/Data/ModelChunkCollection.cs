namespace Siniskalc.Core.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The chunks of one kind of record of a savegame, with what reads and writes them and the models read from them (once for each
    /// version of the collection).
    /// </summary>
    /// <typeparam name="TChunk">The chunk of a record.</typeparam>
    /// <typeparam name="TModel">The model a record is read as.</typeparam>
    /// <typeparam name="TDeserializer">What reads a chunk.</typeparam>
    /// <typeparam name="TSerializer">What writes a model into a chunk.</typeparam>
    public abstract class ModelChunkCollection<TChunk, TModel, TDeserializer, TSerializer> : ChunkCollection<TChunk>
        where TChunk : BytesChunk
        where TDeserializer : IDeserializer<TChunk, TModel>
        where TSerializer : ISerializer<TChunk, TModel>
    {
        private readonly Lazy<IReadOnlyList<TModel>> models;
        private readonly Func<int, TModel, TModel, IReadOnlyList<PendingChange>> diff;

        protected ModelChunkCollection(
            IEnumerable<TChunk> chunks,
            TDeserializer deserializer,
            TSerializer serializer,
            Func<int, TModel, TModel, IReadOnlyList<PendingChange>> diff)
            : base(chunks)
        {
            this.Deserializer = deserializer;
            this.Serializer = serializer;
            this.diff = diff;
            this.models = new Lazy<IReadOnlyList<TModel>>(() => this.Select(chunk => this.Deserializer.Deserialize(chunk)).ToList());
        }

        public TDeserializer Deserializer { get; }

        public TSerializer Serializer { get; }

        public string GetName(int index) => this[index].Name;

        public TModel GetModel(int index) => this.Deserializer.Deserialize(this[index]);

        /// <summary>
        /// Returns every record as a model, read once for each version of the collection.
        /// </summary>
        public IReadOnlyList<TModel> GetModels() => this.models.Value;

        protected override IEnumerable<PendingChange> DiffChunk(int index, TChunk original, TChunk current)
        {
            return this.diff(index, this.Deserializer.Deserialize(original), this.Deserializer.Deserialize(current));
        }
    }
}
