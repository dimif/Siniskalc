namespace Siniskalc.Core.Tests.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;

    /// <summary>
    /// Tests the records of one kind of a savefile read, written and compared with the kit of their kind.
    /// </summary>
    [TestFixture]
    internal sealed class RecordChunkCollectionTests
    {
        private static readonly RecordKit<RecordChunk, TestModel> Kit = new(new Reader(), new Writer(), new Differ());

        private static SaveDocumentHistory Create(params byte[] values)
        {
            var chunks = values.Select((value, i) => new RecordChunk($"r{i}", [value], i, i));
            var collections = new Dictionary<ChunkKind, IChunkCollection>
            {
                [TestChunkKinds.Unit] = new RecordChunkCollection<RecordChunk, TestModel>(chunks, Kit),
            };

            return new SaveDocumentHistory(new SaveDocument(new byte[values.Length], collections, new SavefileBytesPatcher()));
        }

        [Test]
        public void EachRecordIsReadWithTheDeserializerOfTheKit()
        {
            var collection = new RecordChunkCollection<RecordChunk, TestModel>([new RecordChunk("a", [4], 0, 0), new RecordChunk("b", [6], 1, 1)], Kit);

            Assert.That(collection.GetModels().Select(m => m.Value), Is.EqualTo(new byte[] { 4, 6 }));
        }

        [Test]
        public void ReplacingARecordKeepsTheKit()
        {
            var collection = new RecordChunkCollection<RecordChunk, TestModel>([new RecordChunk("a", [4], 0, 0)], Kit);

            var replaced = collection.WithChunk(0, new RecordChunk("a", [9], 0, 0));

            Assert.That(replaced.Kit, Is.SameAs(Kit));
            Assert.That(replaced.GetModel(0).Value, Is.EqualTo(9));
        }

        [Test]
        public void ASessionBuiltFromTheKindEditsARecordWithTheSerializerOfTheKit()
        {
            var history = Create(4, 6);
            var session = new EditSession<RecordChunk, TestModel>(history, TestChunkKinds.Unit, 1);

            session.Update(m => m with { Value = 8 });

            Assert.That(session.Current.Value, Is.EqualTo(8));
            Assert.That(session.Original.Value, Is.EqualTo(6));
            Assert.That(session.Index, Is.EqualTo(1));
        }

        [Test]
        public void TheChangesOfARecordAreListedByTheDifferOfTheKit()
        {
            var history = Create(4, 6);
            new EditSession<RecordChunk, TestModel>(history, TestChunkKinds.Unit, 1).Update(m => m with { Value = 8 });

            var changes = history.Current.GetCollection(TestChunkKinds.Unit).GetPendingChanges(history.Original.GetCollection(TestChunkKinds.Unit));

            Assert.That(changes.Select(c => (c.Index, c.Change.After)), Is.EqualTo(new[] { (1, "8") }));
        }

        private sealed class Reader : IDeserializer<RecordChunk, TestModel>
        {
            public TestModel Deserialize(RecordChunk chunk) => new(chunk.GetByteAt(0));
        }

        private sealed class Writer : ISerializer<RecordChunk, TestModel>
        {
            public RecordChunk Serialize(RecordChunk chunk, TestModel model) => chunk.WithBytes([model.Value]);
        }

        private sealed class Differ : IDiffer<TestModel>
        {
            public IReadOnlyList<PendingChange> Diff(int index, TestModel original, TestModel current) =>
                [new PendingChange($"r{index}", new Change(new ChangeArea("General"), "Value", original.Value.ToString(), current.Value.ToString()), TestChunkKinds.Unit, index, _ => { })];
        }
    }
}
