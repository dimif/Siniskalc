namespace Siniskalc.Heroes.Wpf.Tests.MultiUnit.ViewModels
{
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the page that edits the records of some kinds of the savefile of a made-up game, whose record is one byte.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class RecordsPageViewModelBaseTests
    {
        private static readonly ChunkKind Mines = new("Mines", 0);

        private static readonly ChunkKind Shrines = new("Shrines", 1);

        [Test]
        public void ThePageHasPendingChangesOnlyWhenARecordOfItsKindsWasEdited()
        {
            var history = History();
            using var page = new Page(history, Mines);

            Session(history, Shrines).Update(_ => 5);
            Assert.That(page.HasPendingChanges, Is.False, "the shrines are not on the page");

            Session(history, Mines).Update(_ => 7);
            Assert.That(page.HasPendingChanges, Is.True);
            Assert.That(page.Refreshes, Is.EqualTo(3), "the page shows its records again when it is made and after each edit");
        }

        [Test]
        public void AResetPutsBackTheRecordsOfItsKindsAndLeavesTheOthers()
        {
            var history = History();
            using var page = new Page(history, Mines);
            Session(history, Mines).Update(_ => 7);
            Session(history, Shrines).Update(_ => 5);

            page.ResetCommand.Execute(null);

            Assert.That(Session(history, Mines).Current, Is.EqualTo(1));
            Assert.That(Session(history, Shrines).Current, Is.EqualTo(5));
            Assert.That(page.HasPendingChanges, Is.False);
        }

        private static SaveDocumentHistory History()
        {
            var collections = new Dictionary<ChunkKind, IChunkCollection>
            {
                [Mines] = Collection(Mines, 1),
                [Shrines] = Collection(Shrines, 2),
            };

            return new SaveDocumentHistory(new SaveDocument(new byte[3], collections, new SavefileBytesPatcher()));
        }

        private static RecordChunkCollection<RecordChunk, Box> Collection(ChunkKind kind, int start)
        {
            var deserializer = new Mock<IDeserializer<RecordChunk, Box>>();
            deserializer.Setup(d => d.Deserialize(It.IsAny<RecordChunk>())).Returns((RecordChunk chunk) => new Box(chunk.GetByteAt(0)));
            var serializer = new Mock<ISerializer<RecordChunk, Box>>();
            serializer.Setup(s => s.Serialize(It.IsAny<RecordChunk>(), It.IsAny<Box>())).Returns((RecordChunk chunk, Box box) => chunk.WithBytes([box.Value]));
            var differ = new Mock<IDiffer<Box>>();
            differ.Setup(d => d.Diff(It.IsAny<int>(), It.IsAny<Box>(), It.IsAny<Box>())).Returns((int index, Box original, Box current) =>
                [new PendingChange(kind.ToString(), new Change(new ChangeArea("General"), "Value", $"{original.Value}", $"{current.Value}"), kind, index, h => new EditSession<RecordChunk, Box>(h, kind, index).Update(_ => original))]);
            return new RecordChunkCollection<RecordChunk, Box>([new RecordChunk(kind.ToString(), [(byte)start], start, 0)], new RecordKit<RecordChunk, Box>(deserializer.Object, serializer.Object, differ.Object));
        }

        private static SessionOfOneByte Session(SaveDocumentHistory history, ChunkKind kind) => new(new EditSession<RecordChunk, Box>(history, kind, 0));

        /// <summary>
        /// The model of a record of the made-up game: its byte.
        /// </summary>
        internal sealed record Box(byte Value);

        private sealed class SessionOfOneByte(EditSession<RecordChunk, Box> session)
        {
            public byte Current => session.Current.Value;

            public void Update(Func<byte, byte> edit) => session.Update(box => new Box(edit(box.Value)));
        }

        private sealed class Page : RecordsPageViewModelBase
        {
            public Page(SaveDocumentHistory history, params ChunkKind[] kinds)
                : base("Page", history, kinds)
            {
                this.Refresh();
            }

            public int Refreshes { get; private set; }

            protected override void RefreshRecords() => this.Refreshes++;
        }
    }
}
