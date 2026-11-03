namespace Siniskalc.Core.Tests.Runtime
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Core.Tests.Data;

    /// <summary>A game made of one chunk of three bytes at the middle of a savefile of seven, for the tests of what a game does.</summary>
    public sealed class TestSavegame : AbstractSavegame
    {
        public TestSavegame(string? fileName = null, string? name = null, byte[]? bytes = null)
            : base(fileName, bytes ?? new byte[] { 0, 0, 1, 2, 3, 0, 0 }, Create(bytes ?? new byte[] { 0, 0, 1, 2, 3, 0, 0 }), name)
        {
        }

        public override string VersionName => "Test";

        /// <summary>Edits the middle chunk, so the game has something to save.</summary>
        public void Edit(byte first)
        {
            var chunks = this.History.Current.GetChunks<RawBytesChunk>(TestChunkKinds.Unit);
            var bytes = chunks[0].ToArray();
            bytes[0] = first;
            this.History.Apply(this.History.Current.WithChunk(TestChunkKinds.Unit, 0, chunks[0].WithBytes(bytes)));
        }

        private static SaveDocumentHistory Create(byte[] bytes)
        {
            var collections = new Dictionary<ChunkKind, IChunkCollection>
            {
                [TestChunkKinds.Unit] = new TestModelCollection([new RawBytesChunk("middle", [bytes[2], bytes[3], bytes[4]], 2)]),
            };
            return new SaveDocumentHistory(new SaveDocument(bytes, collections, new SavefileBytesPatcher()));
        }
    }
}
