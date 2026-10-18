namespace Siniskalc.Core.Tests.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Tests the lookups every collection of chunks has.
    /// </summary>
    [TestFixture]
    internal sealed class ChunkCollectionTests
    {
        private static readonly RawBytesChunk[] Chunks = [new("Ann", [0], 0), new("", [0], 1), new("Bob", [0], 2)];

        [Test]
        public void AChunkIsFoundByItsNameWhateverTheCase()
        {
            var collection = new TestModelCollection(Chunks);

            Assert.That(collection.FindByName("bob"), Is.EqualTo(2));
            Assert.That(collection.FindByName("Nobody"), Is.EqualTo(-1));
        }

        [Test]
        public void ACollectionCanLeaveOutTheChunksThatHaveNoName()
        {
            var collection = new NamedOnlyCollection(Chunks);

            Assert.That(collection.FindByName(string.Empty), Is.EqualTo(-1));
            Assert.That(new TestModelCollection(Chunks).FindByName(string.Empty), Is.EqualTo(1));
        }

        private sealed class NamedOnlyCollection : ChunkCollection<RawBytesChunk>
        {
            public NamedOnlyCollection(IEnumerable<RawBytesChunk> chunks)
                : base(chunks)
            {
            }

            public override ChunkCollection<RawBytesChunk> WithChunk(int index, RawBytesChunk chunk) => new NamedOnlyCollection(this.Replace(index, chunk));

            protected override bool IsNamed(RawBytesChunk chunk) => chunk.Name.Length > 0;

            protected override IEnumerable<PendingChange> DiffChunk(int index, RawBytesChunk original, RawBytesChunk current) => [];
        }
    }
}
