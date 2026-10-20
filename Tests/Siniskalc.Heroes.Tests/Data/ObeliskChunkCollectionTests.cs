namespace Siniskalc.Heroes.Tests.Data
{
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Tests.Entities;

    /// <summary>
    /// Tests the visited obelisks of a savefile, which any game reads and writes through its own deserializer, serializer and differ.
    /// </summary>
    [TestFixture]
    internal sealed class ObeliskChunkCollectionTests
    {
        private Mock<IDeserializer<ObeliskChunk, ObeliskVisits<TestColor>>> deserializer = new();
        private Mock<ISerializer<ObeliskChunk, ObeliskVisits<TestColor>>> serializer = new();
        private Mock<IDiffer<ObeliskVisits<TestColor>>> differ = new();

        [SetUp]
        public void NewCollaborators()
        {
            this.deserializer = new();
            this.serializer = new();
            this.differ = new();
        }

        private static ObeliskChunk Chunk(params byte[] bytes) => new("Obelisks", bytes, 0);

        private ObeliskChunkCollection<TestColor> Collection(params ObeliskChunk[] chunks) =>
            new(chunks, this.deserializer.Object, this.serializer.Object, this.differ.Object);

        [Test]
        public void ASavefileWithoutObelisksHasNothingToShow()
        {
            var collection = this.Collection();

            Assert.That((collection.Any, collection.GetModel()), Is.EqualTo((false, (ObeliskVisits<TestColor>?)null)));
            this.deserializer.VerifyNoOtherCalls();
        }

        [Test]
        public void TheModelIsWhatTheDeserializerReadsFromTheOnlyChunk()
        {
            var chunk = Chunk(1, 2);
            var visits = new ObeliskVisits<TestColor>(new byte[] { 1, 2 });
            this.deserializer.Setup(d => d.Deserialize(chunk)).Returns(visits);

            Assert.That(this.Collection(chunk).GetModel(), Is.SameAs(visits));
        }

        [Test]
        public void TheChunkIsFoundByTheEditSessionsAsAModelCollection()
        {
            IModelChunkCollection<ObeliskChunk, ObeliskVisits<TestColor>> models = this.Collection(Chunk(0));

            Assert.That((models.ChunkDeserializer, models.ChunkSerializer), Is.EqualTo((this.deserializer.Object, this.serializer.Object)));
        }

        [Test]
        public void ReplacingTheChunkKeepsWhatReadsWritesAndDiffsIt()
        {
            var original = Chunk(0);
            var edited = Chunk(1);
            var before = new ObeliskVisits<TestColor>(new byte[] { 0 });
            var after = new ObeliskVisits<TestColor>(new byte[] { 1 });
            this.deserializer.Setup(d => d.Deserialize(original)).Returns(before);
            this.deserializer.Setup(d => d.Deserialize(edited)).Returns(after);
            var expected = new PendingChange("Obelisks", new Change(ChangeAreas.General, "Obelisk 1, Red", "not visited", "visited"), new ChunkKind("Obelisks", 3), 0, _ => { });
            this.differ.Setup(d => d.Diff(0, before, after)).Returns([expected]);

            var changes = this.Collection(original).WithChunk(0, edited).GetPendingChanges(this.Collection(original));

            Assert.That(changes, Is.EqualTo(new[] { expected }));
            this.differ.Verify(d => d.Diff(0, before, after), Times.Once);
        }
    }
}
