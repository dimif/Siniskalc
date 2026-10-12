namespace Siniskalc.Core.Tests.Data
{
    using Siniskalc.Core.Data;

    [TestFixture]
    internal sealed class SavefileBytesPatcherTests
    {
        private readonly SavefileBytesPatcher patcher = new();

        [Test]
        public void PatchReplacesOnlyChunkRange()
        {
            var save = new byte[20];
            var chunk = new RawBytesChunk("c", [1, 2, 3], 5);

            var result = this.patcher.Patch(save, [chunk]);

            Assert.That(result[4], Is.EqualTo(0));
            Assert.That(result[5..8], Is.EqualTo(new byte[] { 1, 2, 3 }));
            Assert.That(result[8], Is.EqualTo(0));
            Assert.That(save, Is.All.EqualTo(0));
        }

        [Test]
        public void PatchRejectsOutOfRangeChunk()
        {
            var chunk = new RawBytesChunk("c", new byte[4], 18);

            Assert.Throws<ArgumentException>(() => this.patcher.Patch(new byte[20], [chunk]));
        }

        [Test]
        public void PatchRejectsOverlappingChunks()
        {
            var a = new RawBytesChunk("a", new byte[4], 0);
            var b = new RawBytesChunk("b", new byte[4], 2);

            Assert.Throws<ArgumentException>(() => this.patcher.Patch(new byte[20], [a, b]));
        }
    }
}
