namespace Siniskalc.Wpf.Tests.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// A game made of two records of one byte each, in a savefile of two bytes, whose edits show as pending changes.
    /// </summary>
    public sealed class TestGame : AbstractSavegame
    {
        public static readonly ChunkKind RecordKind = new("Record", 0);

        public TestGame(string? fileName = null, string? name = null)
            : base(fileName, [10, 20], Create(), name)
        {
        }

        public override string VersionName => "Test version";

        /// <summary>Gives record <paramref name="index"/> another value, as an edit of the page would.</summary>
        public void Edit(int index, byte value)
        {
            var chunk = this.History.Current.GetChunks<RawBytesChunk>(RecordKind)[index];
            this.History.Apply(this.History.Current.WithChunk(RecordKind, index, chunk.WithBytes([value])));
        }

        private static SaveDocumentHistory Create()
        {
            var collections = new Dictionary<ChunkKind, IChunkCollection>
            {
                [RecordKind] = new Records([new RawBytesChunk("first", [10], 0), new RawBytesChunk("second", [20], 1)]),
            };
            return new SaveDocumentHistory(new SaveDocument([10, 20], collections, new SavefileBytesPatcher()));
        }

        private sealed class Records : ModelChunkCollection<RawBytesChunk, byte, Reader, Writer>
        {
            public Records(IEnumerable<RawBytesChunk> chunks)
                : base(chunks, new Reader(), new Writer(), Diff)
            {
            }

            public override Records WithChunk(int index, RawBytesChunk chunk) => new(this.Replace(index, chunk));

            private static IReadOnlyList<PendingChange> Diff(int index, byte before, byte after) =>
                before == after
                    ? []
                    : [new PendingChange($"record {index}", new Change(new ChangeArea("Value"), "Value", before.ToString(), after.ToString()), RecordKind, index, _ => { })];
        }

        private sealed class Reader : IDeserializer<RawBytesChunk, byte>
        {
            public byte Deserialize(RawBytesChunk chunk) => chunk.GetByteAt(0);
        }

        private sealed class Writer : ISerializer<RawBytesChunk, byte>
        {
            public RawBytesChunk Serialize(RawBytesChunk chunk, byte model) => chunk.WithBytes([model]);
        }
    }
}
