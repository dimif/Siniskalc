namespace Siniskalc.Core.Tests.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;

    [TestFixture]
    internal sealed class SaveDocumentTests
    {
        private static SaveDocument Create()
        {
            var collections = new Dictionary<ChunkKind, IChunkCollection>
            {
                [TestChunkKinds.Building] = new TestChunkCollection(TestChunkKinds.Building, [new RawBytesChunk("b", [0, 0], 8)]),
                [TestChunkKinds.Unit] = new TestChunkCollection(TestChunkKinds.Unit, [new RawBytesChunk("u", [0, 0], 4)]),
            };

            return new SaveDocument(new byte[10], collections, new SavefileBytesPatcher());
        }

        [Test]
        public void ToBytesAppliesChunkEdits()
        {
            var document = Create().WithChunk(TestChunkKinds.Unit, 0, new RawBytesChunk("u", [7, 8], 4));

            Assert.That(document.ToBytes()[4..6], Is.EqualTo(new byte[] { 7, 8 }));
        }

        [Test]
        public void EditingADocumentLeavesTheOneItWasMadeFromAlone()
        {
            var original = Create();

            _ = original.WithChunk(TestChunkKinds.Unit, 0, new RawBytesChunk("u", [7, 8], 4));

            Assert.That(original.ToBytes(), Is.All.EqualTo(0));
        }

        [Test]
        public void UndoAndRedoRestoreSnapshots()
        {
            var history = new SaveDocumentHistory(Create());
            var edited = history.Current.WithChunk(TestChunkKinds.Unit, 0, new RawBytesChunk("u", [1, 1], 4));

            history.Apply(edited);
            history.Undo();
            Assert.That(history.Current.ToBytes()[4], Is.EqualTo(0));

            history.Redo();
            Assert.That(history.Current.ToBytes()[4], Is.EqualTo(1));
            Assert.That(history.CanRedo, Is.False);
        }

        [Test]
        public void ApplyChangesToKeepsEditsMadeElsewhere()
        {
            var document = Create().WithChunk(TestChunkKinds.Unit, 0, new RawBytesChunk("u", [7, 0], 4));
            var target = new byte[10];
            target[0] = 1;
            target[5] = 2;

            var result = document.ApplyChangesTo(target);

            Assert.That(result[0], Is.EqualTo(1));
            Assert.That(result[4], Is.EqualTo(7));
            Assert.That(result[5], Is.EqualTo(2));
            Assert.That(target[4], Is.EqualTo(0));
        }

        [Test]
        public void HistoryRaisesChangedOnlyWhenSomethingHappens()
        {
            var history = new SaveDocumentHistory(Create());
            var changes = 0;
            history.Changed += (_, _) => changes++;

            history.Undo();
            history.Redo();
            Assert.That(changes, Is.EqualTo(0));

            history.Apply(history.Current.WithChunk(TestChunkKinds.Unit, 0, new RawBytesChunk("u", [1, 1], 4)));
            history.Undo();
            history.Redo();
            Assert.That(changes, Is.EqualTo(3));
        }

        [Test]
        public void PendingChangesListTheKindsInTheOrderTheyAreDefinedIn()
        {
            var original = Create();
            var edited = original
                .WithChunk(TestChunkKinds.Building, 0, new RawBytesChunk("b", [1, 0], 8))
                .WithChunk(TestChunkKinds.Unit, 0, new RawBytesChunk("u", [1, 0], 4));

            var changes = edited.GetPendingChanges(original);

            Assert.That(changes.Select(c => c.Kind), Is.EqualTo(new[] { TestChunkKinds.Unit, TestChunkKinds.Building }));
            Assert.That(changes.Select(c => c.Path), Is.EqualTo(new[] { "u › Bytes", "b › Bytes" }));
        }

        [Test]
        public void MarkSavedMakesTheCurrentDocumentTheOneChangesAreMeasuredAgainst()
        {
            var history = new SaveDocumentHistory(Create());
            history.Apply(history.Current.WithChunk(TestChunkKinds.Unit, 0, new RawBytesChunk("u", [1, 0], 4)));
            Assert.That(history.GetPendingChanges(), Has.Count.EqualTo(1));

            history.MarkSaved();

            Assert.That(history.GetPendingChanges(), Is.Empty);
        }
    }
}
