using Moq;
using Siniskalc.Core.Data;
using Siniskalc.Core.Models;

namespace Siniskalc.Core.Tests.Data
{
    /// <summary>
    /// Tests the history of a savefile on documents that hold no chunks.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SaveDocumentHistoryTests
    {
        private static SaveDocument Document() => new([1, 2, 3], new Dictionary<ChunkKind, IChunkCollection>(), Mock.Of<ISavefileBytesPatcher>());

        [Test]
        public void RevertGivesTheHistoryToTheRevertActionOfTheChange()
        {
            var history = new SaveDocumentHistory(Document());
            SaveDocumentHistory? given = null;
            var change = new PendingChange("Orrin", new Change(new ChangeArea("Creatures"), "Slot 2", "1", "2"), new ChunkKind("Hero", 0), 4, h => given = h);

            history.Revert(change);

            Assert.That(given, Is.SameAs(history));
        }

        [Test]
        public void ApplyingADocumentMakesItCurrentKeepsTheOriginalAndRaisesChanged()
        {
            var original = Document();
            var next = Document();
            var history = new SaveDocumentHistory(original);
            var raised = 0;
            history.Changed += (_, _) => raised++;

            history.Apply(next);

            Assert.That((history.Current, history.Original, raised), Is.EqualTo((next, original, 1)));
            Assert.That(history.CanUndo, Is.True);
        }

        [Test]
        public void UndoAndRedoMoveBetweenTheDocumentsAndRaiseChanged()
        {
            var original = Document();
            var next = Document();
            var history = new SaveDocumentHistory(original);
            history.Apply(next);
            var raised = 0;
            history.Changed += (_, _) => raised++;

            history.Undo();
            Assert.That(history.Current, Is.SameAs(original));
            history.Redo();

            Assert.That(history.Current, Is.SameAs(next));
            Assert.That(raised, Is.EqualTo(2));
        }

        [Test]
        public void MarkSavedMovesTheOriginalToTheCurrentDocumentAndSaysSoOnlyWhenTheyDiffered()
        {
            var history = new SaveDocumentHistory(Document());
            var next = Document();
            history.Apply(next);
            var raised = 0;
            history.Changed += (_, _) => raised++;

            history.MarkSaved();
            history.MarkSaved();

            Assert.That(history.Original, Is.SameAs(next));
            Assert.That(raised, Is.EqualTo(1));
        }
    }
}
