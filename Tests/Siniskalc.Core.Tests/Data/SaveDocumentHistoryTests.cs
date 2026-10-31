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

        [Test]
        public void ApplyMakesWhatTheReconcilerOfTheEditedDocumentReturnsCurrent()
        {
            var original = Document();
            var reconciled = Document();
            var reconciler = new Mock<IDocumentReconciler>();
            var next = new SaveDocument([1, 2, 3], new Dictionary<ChunkKind, IChunkCollection>(), Mock.Of<ISavefileBytesPatcher>(), reconciler.Object);
            reconciler.Setup(r => r.Reconcile(original, original, next)).Returns(reconciled);
            var history = new SaveDocumentHistory(original);

            history.Apply(next);

            Assert.That(history.Current, Is.SameAs(reconciled));
            history.Undo();
            Assert.That(history.Current, Is.SameAs(original), "the edit and what followed it are one step");
        }

        [Test]
        public void ApplyAsIsMakesTheDocumentCurrentWithoutAskingItsReconciler()
        {
            var reconciler = new Mock<IDocumentReconciler>(MockBehavior.Strict);
            var next = new SaveDocument([1, 2, 3], new Dictionary<ChunkKind, IChunkCollection>(), Mock.Of<ISavefileBytesPatcher>(), reconciler.Object);
            var history = new SaveDocumentHistory(Document());

            history.ApplyAsIs(next);

            Assert.That(history.Current, Is.SameAs(next));
        }

        [Test]
        public void AnEditTheReconcilerRefusesLeavesTheHistoryAsItWasAndRaisesNothing()
        {
            var original = Document();
            var reconciler = new Mock<IDocumentReconciler>();
            reconciler.Setup(r => r.Reconcile(It.IsAny<SaveDocument>(), It.IsAny<SaveDocument>(), It.IsAny<SaveDocument>())).Throws(new InvalidOperationException("full"));
            var next = new SaveDocument([1, 2, 3], new Dictionary<ChunkKind, IChunkCollection>(), Mock.Of<ISavefileBytesPatcher>(), reconciler.Object);
            var history = new SaveDocumentHistory(original);
            var raised = 0;
            history.Changed += (_, _) => raised++;

            Assert.Throws<InvalidOperationException>(() => history.Apply(next));

            Assert.That((history.Current, history.CanUndo, raised), Is.EqualTo((original, false, 0)));
        }

        [Test]
        public void ADocumentWithoutAReconcilerIsAppliedAsItIs()
        {
            var next = Document();
            var history = new SaveDocumentHistory(Document());

            history.Apply(next);

            Assert.That(history.Current, Is.SameAs(next));
        }
    }
}
