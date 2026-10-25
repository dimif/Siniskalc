namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the obelisks a player visited, read from and written to the savefile of a made-up game through its own reader and writer.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal sealed class ObeliskVisitsViewModelTests
    {
        private static readonly ChunkKind Obelisks = new("Obelisks", 7);

        private static SaveDocumentHistory History(byte[] masks, params MapPosition?[] positions)
        {
            var deserializer = new Mock<IDeserializer<ObeliskChunk, ObeliskVisits<TestColor>>>();
            deserializer.Setup(d => d.Deserialize(It.IsAny<ObeliskChunk>())).Returns((ObeliskChunk chunk) => new ObeliskVisits<TestColor>(chunk.ToArray(), chunk.Positions));
            var serializer = new Mock<ISerializer<ObeliskChunk, ObeliskVisits<TestColor>>>();
            serializer.Setup(s => s.Serialize(It.IsAny<ObeliskChunk>(), It.IsAny<ObeliskVisits<TestColor>>()))
                .Returns((ObeliskChunk chunk, ObeliskVisits<TestColor> visits) => chunk.WithBytes([.. visits.Masks]));
            ObeliskChunk[] chunks = masks.Length == 0 ? [] : [new ObeliskChunk("Obelisks", masks, 0, positions)];
            var collections = new Dictionary<ChunkKind, IChunkCollection>
            {
                [Obelisks] = new ObeliskChunkCollection<TestColor>(chunks, deserializer.Object, serializer.Object, Mock.Of<IDiffer<ObeliskVisits<TestColor>>>()),
            };

            return new SaveDocumentHistory(new SaveDocument(new byte[masks.Length], collections, new SavefileBytesPatcher()));
        }

        private static ObeliskVisitsViewModel<TestColor> Visits(SaveDocumentHistory history, TestColor color) =>
            new(history, Obelisks, color, "Prize", hasLevels: true);

        [Test]
        public void EachObeliskShowsWhetherThePlayerVisitedItAndWhereItIsInTheWordsOfTheGame()
        {
            var visits = Visits(History([0b01, 0b10], new MapPosition(3, 4, 1), null), TestColor.Blue);

            Assert.That(visits.Obelisks.Select(o => (o.Number, o.Coordinates, o.IsVisited)), Is.EqualTo(new[] { (1, "Pos: 3, 4, Underground", false), (2, string.Empty, true) }));
            Assert.That(visits.Summary, Is.EqualTo("1 of 2 visited"));
        }

        [Test]
        public void TheHeadingSaysWhatThePuzzleLeadsTo()
        {
            var visits = Visits(History([0]), TestColor.Red);

            Assert.That(visits.PuzzleToolTip, Does.EndWith("the puzzle that leads to the Prize."));
        }

        [Test]
        public void MarkingAnObeliskIsAnEditOfTheSavefileThatLeavesTheOtherColoursAlone()
        {
            var history = History([0b10, 0b00]);
            var visits = Visits(history, TestColor.Red);

            visits.Obelisks[1].IsVisited = true;

            Assert.That(history.Current.GetChunks<ObeliskChunk>(Obelisks)[0].ToArray(), Is.EqualTo(new byte[] { 0b10, 0b01 }));
            Assert.That(history.CanUndo, Is.True);
        }

        [Test]
        public void VisitingAllMarksEveryObeliskForThePlayer()
        {
            var history = History([0b00, 0b10, 0b00]);
            var visits = Visits(history, TestColor.Red);

            visits.VisitAllCommand.Execute(null);
            visits.Refresh();

            Assert.That(history.Current.GetChunks<ObeliskChunk>(Obelisks)[0].ToArray(), Is.EqualTo(new byte[] { 0b01, 0b11, 0b01 }));
            Assert.That(visits.Summary, Is.EqualTo("3 of 3 visited"));
        }

        [Test]
        public void ASavefileWithoutObelisksHasNothingToShowAndNothingToEdit()
        {
            var visits = Visits(History([]), TestColor.Red);

            Assert.That((visits.HasObelisks, visits.Obelisks.Count, visits.VisitAllCommand.CanExecute(null)), Is.EqualTo((false, 0, false)));
        }
    }
}
