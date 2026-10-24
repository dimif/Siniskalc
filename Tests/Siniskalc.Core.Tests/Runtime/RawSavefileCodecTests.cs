namespace Siniskalc.Core.Tests.Runtime
{
    using Siniskalc.Core.Runtime;

    [TestFixture]
    internal sealed class RawSavefileCodecTests
    {
        private readonly RawSavefileCodec codec = new();
        private string folder = string.Empty;

        [SetUp]
        public void CreateFolder()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-raw-codec-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
        }

        [TearDown]
        public void DeleteFolder() => Directory.Delete(this.folder, recursive: true);

        [Test]
        public void ReadsWhatWasWrittenAndTheFileHoldsTheBytesAsTheyAre()
        {
            var path = Path.Combine(this.folder, "save.GM1");
            var bytes = Enumerable.Range(0, 5000).Select(i => (byte)(i % 251)).ToArray();

            this.codec.Write(path, bytes);

            Assert.That(File.ReadAllBytes(path), Is.EqualTo(bytes));
            Assert.That(this.codec.Read(path), Is.EqualTo(bytes));
        }

        [Test]
        public void WritingASmallerSavefileLeavesNothingOfTheLargerOne()
        {
            var path = Path.Combine(this.folder, "save.GM1");
            this.codec.Write(path, new byte[100_000]);

            this.codec.Write(path, [1, 2, 3]);

            Assert.That(File.ReadAllBytes(path), Is.EqualTo(new byte[] { 1, 2, 3 }));
            Assert.That(Directory.GetFiles(this.folder), Is.EqualTo(new[] { path }));
        }

        [Test]
        public void AWriteThatFailsLeavesTheFileThatWasThereAndNoTemporaryFileBehind()
        {
            var path = Path.Combine(this.folder, "save.GM1");
            this.codec.Write(path, [9, 9, 9]);

            Assert.That(() => this.codec.Write(Path.Combine(this.folder, "missing folder", "save.GM1"), [1]), Throws.InstanceOf<IOException>());

            Assert.That(File.ReadAllBytes(path), Is.EqualTo(new byte[] { 9, 9, 9 }));
            Assert.That(Directory.GetFiles(this.folder), Is.EqualTo(new[] { path }));
        }
    }
}
