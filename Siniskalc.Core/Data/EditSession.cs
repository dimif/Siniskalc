namespace Siniskalc.Core.Data
{
    using System;
    using Siniskalc.Core;

    /// <summary>
    /// A view onto a single struct (a hero, a player, a castle) in a <see cref="SaveDocumentHistory"/>. Edits made through it
    /// are pushed onto the shared history, so undo/redo and <see cref="SaveDocumentHistory.Changed"/> cover every struct.
    /// A kind of struct only says how to read its model from a chunk and how to write it back.
    /// </summary>
    /// <typeparam name="TChunk">The chunk of the struct.</typeparam>
    /// <typeparam name="TModel">The model of the struct that is edited, which the session reads from the chunk and writes back.</typeparam>
    public class EditSession<TChunk, TModel>
        where TChunk : BytesChunk
        where TModel : class
    {
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

        public SaveDocumentHistory History { get; }

        public ChunkKind Kind { get; }

        /// <summary>
        /// Gets the index of the struct among the savefile's structs of the same kind.
        /// </summary>
        public int Index { get; }

        public TChunk Chunk => this.History.Current.GetChunks<TChunk>(this.Kind)[this.Index];

        /// <summary>
        /// Gets the chunk as it was when the savefile was loaded or last saved.
        /// </summary>
        public TChunk OriginalChunk => this.History.Original.GetChunks<TChunk>(this.Kind)[this.Index];

        /// <summary>
        /// Gets whether the struct differs from the savefile as it was loaded or last saved.
        /// </summary>
        public bool IsDirty => !this.Chunk.HasSameBytes(this.OriginalChunk);

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
            this.Apply(document, chunk, this.serializer.Serialize(chunk, edit(this.Current)));
        }

        /// <summary>
        /// Replaces the raw bytes of the struct and records the result in the history. Used by the hex editor.
        /// A replacement that leaves the bytes unchanged is not recorded.
        /// </summary>
        public void UpdateBytes(Func<byte[], byte[]> edit)
        {
            var document = this.History.Current;
            var chunk = document.GetChunks<TChunk>(this.Kind)[this.Index];

            this.Apply(document, chunk, (TChunk)chunk.WithBytes(edit(chunk.ToArray())));
        }

        private void Apply(SaveDocument document, TChunk chunk, TChunk newChunk)
        {
            if (newChunk.HasSameBytes(chunk))
            {
                return;
            }

            this.History.Apply(document.WithChunk(this.Kind, this.Index, newChunk));
        }

        private sealed record Cache(SaveDocument Document, TModel Model);
    }
}
