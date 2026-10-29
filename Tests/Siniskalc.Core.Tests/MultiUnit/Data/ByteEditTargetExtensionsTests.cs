namespace Siniskalc.Core.Tests.MultiUnit.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Tests.Data;

    /// <summary>
    /// Tests the helpers that write bytes through a target of the hex editor and find where two arrays differ.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class ByteEditTargetExtensionsTests
    {
        private ChunkByteEditTarget<RawBytesChunk, TestModel> target = null!;

        [SetUp]
        public void StartWithAChunkOfThreeBytes()
        {
            var collections = new Dictionary<ChunkKind, IChunkCollection>
            {
                [TestChunkKinds.Unit] = new TestModelCollection([new RawBytesChunk("first", [1, 2, 3], 0)]),
            };
            var history = new SaveDocumentHistory(new SaveDocument(new byte[3], collections, new SavefileBytesPatcher()));
            this.target = new ChunkByteEditTarget<RawBytesChunk, TestModel>(new EditSession<RawBytesChunk, TestModel>(history, TestChunkKinds.Unit, 0), []);
        }

        [TearDown]
        public void CloseTheTarget()
        {
            this.target.Dispose();
        }

        [Test]
        public void WriteBytesPutsValuesAtAnOffsetThroughTheTarget()
        {
            var changed = this.target.WriteBytes(1, [8, 9]);

            Assert.That(changed, Is.True);
            Assert.That(this.target.GetBytes(), Is.EqualTo(new byte[] { 1, 8, 9 }));
        }

        [Test]
        public void WriteBytesStopsAtTheEndOfTheChunkAndWritesNothingForANegativeStartOrNoValues()
        {
            Assert.That(this.target.WriteBytes(2, [8, 9, 10]), Is.True);
            Assert.That(this.target.GetBytes(), Is.EqualTo(new byte[] { 1, 2, 8 }));
            Assert.That(this.target.WriteBytes(-1, [5]), Is.False);
            Assert.That(this.target.WriteBytes(0, []), Is.False);
            Assert.That(this.target.GetBytes(), Is.EqualTo(new byte[] { 1, 2, 8 }));
        }

        [Test]
        public void FindDifferencesListsTheOffsetsWhereTwoArraysDifferUpToTheShorterOne()
        {
            Assert.That(ByteEditTargetExtensions.FindDifferences([1, 2, 3, 4], [1, 9, 3, 8]), Is.EqualTo(new[] { 1, 3 }));
            Assert.That(ByteEditTargetExtensions.FindDifferences([1, 2, 3], [1, 2]), Is.Empty);
            Assert.That(ByteEditTargetExtensions.FindDifferences([], []), Is.Empty);
        }
    }
}
