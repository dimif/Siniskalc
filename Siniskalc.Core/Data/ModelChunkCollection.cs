namespace Siniskalc.Core.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// The chunks of one kind of record of a savegame, with what reads and writes them and the models read from them (once for each
    /// version of the collection).
    /// </summary>
    /// <typeparam name="TChunk">The chunk of a record.</typeparam>
    /// <typeparam name="TModel">The model a record is read as.</typeparam>
    /// <typeparam name="TDeserializer">What reads a chunk.</typeparam>
    /// <typeparam name="TSerializer">What writes a model into a chunk.</typeparam>
    public abstract class ModelChunkCollection<TChunk, TModel, TDeserializer, TSerializer> : ChunkCollection<TChunk>, IModelChunkCollection<TChunk, TModel>
        where TChunk : BytesChunk
        where TDeserializer : IDeserializer<TChunk, TModel>
        where TSerializer : ISerializer<TChunk, TModel>
    {
        private static readonly ILogger Logger = LogFactory.For(typeof(ModelChunkCollection<,,,>));

        private readonly Lazy<IReadOnlyList<TModel>> models;
        private readonly Func<int, TModel, TModel, IReadOnlyList<PendingChange>> diff;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelChunkCollection{TChunk, TModel, TDeserializer, TSerializer}"/> class.
        /// </summary>
        /// <param name="chunks">The chunks, in the order of the savefile.</param>
        /// <param name="deserializer">Reads a chunk into its model.</param>
        /// <param name="serializer">Writes a model into a chunk.</param>
        /// <param name="diff">Lists what changed in a record, from its index and its original and current models.</param>
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
            this.models = new Lazy<IReadOnlyList<TModel>>(() =>
            {
                Logger.LogDebug("Reading the models of {Count} {Kind} chunks", this.Count, typeof(TChunk).Name);
                return this.Select(chunk => this.Deserializer.Deserialize(chunk)).ToList();
            });
        }

        /// <summary>
        /// Gets what reads a chunk into its model.
        /// </summary>
        public TDeserializer Deserializer { get; }

        /// <summary>
        /// Gets what writes a model into a chunk.
        /// </summary>
        public TSerializer Serializer { get; }

        IDeserializer<TChunk, TModel> IModelChunkCollection<TChunk, TModel>.ChunkDeserializer => this.Deserializer;

        ISerializer<TChunk, TModel> IModelChunkCollection<TChunk, TModel>.ChunkSerializer => this.Serializer;

        /// <summary>
        /// Returns the name of the record at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The place of the record.</param>
        /// <returns>The name of the chunk.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a place in the collection.</exception>
        public string GetName(int index) => this[index].Name;

        /// <summary>
        /// Reads the record at <paramref name="index"/> as a model. It is read again on every call; <see cref="GetModels"/> reads them once.
        /// </summary>
        /// <param name="index">The place of the record.</param>
        /// <returns>The model of the record.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a place in the collection.</exception>
        public TModel GetModel(int index) => this.Deserializer.Deserialize(this[index]);

        /// <summary>
        /// Returns every record as a model, read once for each version of the collection.
        /// </summary>
        /// <returns>The models, in the order of the chunks.</returns>
        public IReadOnlyList<TModel> GetModels() => this.models.Value;

        /// <inheritdoc/>
        protected override IEnumerable<PendingChange> DiffChunk(int index, TChunk original, TChunk current)
        {
            return this.diff(index, this.Deserializer.Deserialize(original), this.Deserializer.Deserialize(current));
        }
    }
}
