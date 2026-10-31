namespace Siniskalc.Core.Data
{
    using System;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// A view onto a single struct (a hero, a player, a castle) in a <see cref="SaveDocumentHistory"/>. Edits made through it
    /// are pushed onto the shared history, so undo/redo and <see cref="SaveDocumentHistory.Changed"/> cover every struct.
    /// A kind of struct only says how to read its model from a chunk and how to write it back.
    /// </summary>
    /// <typeparam name="TChunk">The chunk of the struct.</typeparam>
    /// <typeparam name="TModel">The model of the struct that is edited, which the session reads from the chunk and writes back.</typeparam>
    public class EditSession<TChunk, TModel> : IEditSession<TModel>
        where TChunk : BytesChunk
        where TModel : class
    {
        private static readonly ILogger Logger = LogFactory.For(typeof(EditSession<,>));

        private readonly IDeserializer<TChunk, TModel> deserializer;
        private readonly ISerializer<TChunk, TModel> serializer;
        private Cache? cache;

        /// <param name="deserializer">Reads the model from a chunk.</param>
        /// <param name="serializer">Writes a model to a chunk, as it is.</param>
        public EditSession(SaveDocumentHistory history, ChunkKind kind, int index, IDeserializer<TChunk, TModel> deserializer, ISerializer<TChunk, TModel> serializer)
        {
            this.History = history;
            this.deserializer = deserializer;
            this.serializer = serializer;

            RangeGuard.ThrowIfNotIndex(index, history.Current.GetChunks<TChunk>(kind).Count, nameof(index));

            this.Kind = kind;
            this.Index = index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditSession{TChunk, TModel}"/> class on a record whose chunks are a
        /// <see cref="IModelChunkCollection{TChunk, TModel}"/>, which gives the deserializer and the serializer.
        /// </summary>
        public EditSession(SaveDocumentHistory history, ChunkKind kind, int index)
            : this(history, kind, index, ModelsOf(history, kind).ChunkDeserializer, ModelsOf(history, kind).ChunkSerializer)
        {
        }

        public SaveDocumentHistory History { get; }

        /// <inheritdoc/>
        public IHistoryEvents Events => this.History;

        public ChunkKind Kind { get; }

        /// <inheritdoc/>
        public int Index { get; }

        public TChunk Chunk => this.History.Current.GetChunks<TChunk>(this.Kind)[this.Index];

        /// <summary>
        /// Gets the chunk as it was when the savefile was loaded or last saved.
        /// </summary>
        public TChunk OriginalChunk => this.History.Original.GetChunks<TChunk>(this.Kind)[this.Index];

        /// <summary>
        /// Gets the model of the chunk as it was loaded or last saved.
        /// </summary>
        public TModel Original => this.deserializer.Deserialize(this.OriginalChunk);

        /// <summary>
        /// Gets whether the struct differs from the savefile as it was loaded or last saved.
        /// </summary>
        public virtual bool IsDirty => !this.Chunk.HasSameBytes(this.OriginalChunk);

        /// <summary>
        /// Gets a token that changes whenever this struct is edited, and stays the same while other structs are. Compare it with
        /// <see cref="object.ReferenceEquals(object, object)"/>.
        /// </summary>
        public object Revision => this.Chunk;

        /// <summary>
        /// Gets the model as it is in the current document. Only read again after the document changes.
        /// </summary>
        public TModel Current
        {
            get
            {
                var document = this.History.Current;
                var cached = this.cache;
                if (cached == null || !ReferenceEquals(document, cached.Document))
                {
                    cached = new Cache(document, this.deserializer.Deserialize(document.GetChunks<TChunk>(this.Kind)[this.Index]));
                    this.cache = cached;
                }

                return cached.Model;
            }
        }

        /// <summary>
        /// Applies <paramref name="edit"/> to the current model and records the result in the history.
        /// An edit that leaves the bytes unchanged is not recorded.
        /// </summary>
        public void Update(Func<TModel, TModel> edit)
        {
            var document = this.History.Current;
            var chunk = document.GetChunks<TChunk>(this.Kind)[this.Index];
            this.Apply(document, chunk, this.serializer.Serialize(chunk, edit(this.Current)), "Edited", asIs: false);
        }

        /// <summary>
        /// Replaces the raw bytes of the struct and records the result in the history as it is, without the reconciler of the savefile
        /// (<see cref="SaveDocumentHistory.ApplyAsIs"/>). Used by the hex editor.
        /// A replacement that leaves the bytes unchanged is not recorded.
        /// </summary>
        public void UpdateBytes(Func<byte[], byte[]> edit)
        {
            var document = this.History.Current;
            var chunk = document.GetChunks<TChunk>(this.Kind)[this.Index];

            this.Apply(document, chunk, (TChunk)chunk.WithBytes(edit(chunk.ToArray())), "Replaced the bytes of", asIs: true);
        }

        private static IModelChunkCollection<TChunk, TModel> ModelsOf(SaveDocumentHistory history, ChunkKind kind) =>
            history.Current.GetCollection(kind) as IModelChunkCollection<TChunk, TModel>
            ?? throw new ArgumentException($"The chunks of kind {kind} are not a collection of models.", nameof(kind));

        // An edit of the model goes through the reconciler of the savefile; an edit of the raw bytes is recorded as it is.
        private void Apply(SaveDocument document, TChunk chunk, TChunk newChunk, string what, bool asIs)
        {
            if (newChunk.HasSameBytes(chunk))
            {
                return;
            }

            Logger.LogDebug("{What} {Kind} {Index} ({Name})", what, this.Kind, this.Index, chunk.Name);
            var next = document.WithChunk(this.Kind, this.Index, newChunk);
            if (asIs)
            {
                this.History.ApplyAsIs(next);
            }
            else
            {
                this.History.Apply(next);
            }
        }

        private sealed record Cache(SaveDocument Document, TModel Model);
    }
}
