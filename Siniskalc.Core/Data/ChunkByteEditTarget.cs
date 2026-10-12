namespace Siniskalc.Core.Data
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The raw bytes of one struct (a hero, a castle...), edited through that struct's <see cref="EditSession{TChunk, TModel}"/>.
    /// </summary>
    public class ChunkByteEditTarget<TChunk, TModel> : IByteEditTarget
        where TChunk : BytesChunk
        where TModel : class
    {
        private readonly EditSession<TChunk, TModel> session;
        private readonly Func<string> title;
        private TChunk lastChunk;
        private TChunk lastSavedChunk;
        private bool isUpdating;
        private bool isDisposed;

        /// <param name="regions">The known regions of the chunk.</param>
        /// <param name="title">Gives the title; the name of the chunk when omitted.</param>
        public ChunkByteEditTarget(EditSession<TChunk, TModel> session, IReadOnlyList<ByteRegion> regions, Func<string>? title = null)
        {
            this.session = session;
            this.Regions = regions;
            this.title = title ?? (() => session.Chunk.Name);
            this.lastChunk = session.Chunk;
            this.lastSavedChunk = session.OriginalChunk;
            this.session.History.Changed += this.OnHistoryChanged;
        }

        /// <inheritdoc />
        public event EventHandler<ByteEditTargetChangedEventArgs>? Changed;

        /// <inheritdoc />
        public string Title => this.title();

        /// <inheritdoc />
        public IReadOnlyList<ByteRegion> Regions { get; }

        /// <inheritdoc />
        public byte[] GetBytes() => this.session.Chunk.ToArray();

        /// <inheritdoc />
        public byte[] GetOriginalBytes() => this.session.OriginalChunk.ToArray();

        /// <inheritdoc />
        public bool Update(Func<byte[], byte[]> edit)
        {
            var before = this.session.History.Current;
            this.isUpdating = true;
            try
            {
                this.session.UpdateBytes(edit);
            }
            finally
            {
                this.isUpdating = false;
            }

            return !ReferenceEquals(before, this.session.History.Current);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;
                this.session.History.Changed -= this.OnHistoryChanged;
            }
        }

        private void OnHistoryChanged(object? sender, EventArgs e)
        {
            var chunk = this.session.Chunk;
            var saved = this.session.OriginalChunk;

            // Edits to other structs share the history but leave this chunk as it was.
            // Saving moves the baseline the edited bytes are compared with, which is a change to show as well.
            if (ReferenceEquals(chunk, this.lastChunk) && ReferenceEquals(saved, this.lastSavedChunk))
            {
                return;
            }

            this.lastChunk = chunk;
            this.lastSavedChunk = saved;
            this.Changed?.Invoke(this, new ByteEditTargetChangedEventArgs(this.isUpdating));
        }
    }
}
