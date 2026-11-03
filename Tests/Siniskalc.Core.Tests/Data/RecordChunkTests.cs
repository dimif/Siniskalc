namespace Siniskalc.Core.Tests.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// Tests the chunk of one record of a list of records of the same kind.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class RecordChunkTests
    {
        [Test]
        public void ReplacingTheBytesKeepsTheNameThePlaceInTheSavefileAndTheIndex()
        {
            var chunk = new RecordChunk("Gold mine", [1, 2], 40, 3);

            var replaced = chunk.WithBytes([7, 8]);

            Assert.That(replaced.Name, Is.EqualTo("Gold mine"));
            Assert.That(replaced.StartIndex, Is.EqualTo(40));
            Assert.That(replaced.Index, Is.EqualTo(3));
            Assert.That(replaced.ToArray(), Is.EqualTo(new byte[] { 7, 8 }));
            Assert.That(chunk.ToArray(), Is.EqualTo(new byte[] { 1, 2 }), "the chunk itself is not changed");
        }

        [Test]
        public void BytesOfAnotherLengthCannotReplaceTheRecord()
        {
            var chunk = new RecordChunk("Gold mine", [1, 2], 40, 3);

            Assert.That(() => chunk.WithBytes([1]), Throws.ArgumentException);
        }
    }
}
