namespace Siniskalc.Core.Tests.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;

    /// <summary>
    /// A collection of raw chunks of one kind, which reports one change for every chunk whose bytes differ.
    /// </summary>
    internal sealed class TestChunkCollection : ChunkCollection<RawBytesChunk>
    {
        private readonly ChunkKind kind;

        public TestChunkCollection(ChunkKind kind, IEnumerable<RawBytesChunk> chunks)
            : base(chunks)
        {
            this.kind = kind;
        }

        public override ChunkCollection<RawBytesChunk> WithChunk(int index, RawBytesChunk chunk)
        {
            return new TestChunkCollection(this.kind, this.Replace(index, chunk));
        }

        protected override IEnumerable<PendingChange> DiffChunk(int index, RawBytesChunk original, RawBytesChunk current)
        {
            var change = new Change(new ChangeArea("Bytes"), "changed", original.GetByteAt(0).ToString(), current.GetByteAt(0).ToString());
            yield return new PendingChange(current.Name, change, this.kind, index, _ => { });
        }
    }
}
