namespace Siniskalc.Core.Tests.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;

    /// <summary>
    /// Tests the pending change of a record that is a chunk with a model, and the edit session that puts it back.
    /// </summary>
    [TestFixture]
    internal sealed class ChunkPendingChangeTests
    {
        private static SaveDocumentHistory Create(byte value)
        {
            var collections = new Dictionary<ChunkKind, IChunkCollection>
            {
                [TestChunkKinds.Unit] = new TestModelCollection([new RawBytesChunk("u", [value, 0], 0)]),
            };

            return new SaveDocumentHistory(new SaveDocument(new byte[2], collections, new SavefileBytesPatcher()));
        }

        [Test]
        public void PuttingAChangeBackEditsTheModelOfTheRecord()
        {
            var history = Create(5);
            var change = ChunkPendingChange.For<RawBytesChunk, TestModel>(TestChunkKinds.Unit, "u", 0, new Change(new ChangeArea("General"), "Value", "0", "5"), m => m with { Value = 0 });

            change.Revert(history);

            Assert.That(history.Current.GetChunks<RawBytesChunk>(TestChunkKinds.Unit)[0].GetByteAt(0), Is.EqualTo(0));
        }

        [Test]
        public void ASessionBuiltFromTheKindTakesTheReaderAndWriterOfTheCollection()
        {
            var history = Create(5);
            var session = new EditSession<RawBytesChunk, TestModel>(history, TestChunkKinds.Unit, 0);

            Assert.That(session.Current.Value, Is.EqualTo(5));
            session.Update(m => m with { Value = 9 });

            Assert.That(session.Current.Value, Is.EqualTo(9));
        }

        [Test]
        public void ASessionOnChunksThatAreNotModelsIsRefused()
        {
            var collections = new Dictionary<ChunkKind, IChunkCollection>
            {
                [TestChunkKinds.Unit] = new TestChunkCollection(TestChunkKinds.Unit, [new RawBytesChunk("u", [0, 0], 0)]),
            };
            var history = new SaveDocumentHistory(new SaveDocument(new byte[2], collections, new SavefileBytesPatcher()));

            Assert.That(() => new EditSession<RawBytesChunk, TestModel>(history, TestChunkKinds.Unit, 0), Throws.ArgumentException);
        }
    }
}
