namespace Siniskalc.Core.Tests.Runtime
{
    using Siniskalc.Core.Runtime;

    [TestFixture]
    internal sealed class GzipSavefileCodecTests
    {
        private readonly GzipSavefileCodec codec = new();
        private string folder = string.Empty;

        [SetUp]
        public void CreateFolder()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-codec-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
        }

        [TearDown]
        public void DeleteFolder() => Directory.Delete(this.folder, recursive: true);

        [Test]
        public void ReadsWhatWasWritten()
        {
            var path = Path.Combine(this.folder, "save.GM1");
            var bytes = Enumerable.Range(0, 5000).Select(i => (byte)(i % 251)).ToArray();

            this.codec.Write(path, bytes);

            Assert.That(this.codec.Read(path), Is.EqualTo(bytes));
        }

        [Test]
        public void WritingASmallerSavefileLeavesNothingOfTheLargerOne()
        {
            var path = Path.Combine(this.folder, "save.GM1");
            var random = new Random(7);
            var larger = new byte[100_000];
            random.NextBytes(larger);
            this.codec.Write(path, larger);
            var largerLength = new FileInfo(path).Length;

            this.codec.Write(path, [1, 2, 3]);

            Assert.That(new FileInfo(path).Length, Is.LessThan(largerLength));
            Assert.That(this.codec.Read(path), Is.EqualTo(new byte[] { 1, 2, 3 }));
        }

        [Test]
        public void AFileThatIsNotCompressedIsACorruptSavefile()
        {
            var path = Path.Combine(this.folder, "notes.GM1");
            File.WriteAllText(path, "this is not a gzip file at all");

            Assert.That(() => this.codec.Read(path), Throws.TypeOf<CorruptSavefileException>());
        }

        [Test]
        public void AWriteThatFailsLeavesNoTemporaryFileBehind()
        {
            var path = Path.Combine(this.folder, "blocked");
            Directory.CreateDirectory(path);

            Assert.That(() => this.codec.Write(path, [1, 2, 3]), Throws.Exception);

            Assert.That(Directory.GetFiles(this.folder), Is.Empty);
        }

        [Test]
        public void AWriteThatSucceedsLeavesOnlyTheSavefile()
        {
            var path = Path.Combine(this.folder, "save.GM1");

            this.codec.Write(path, [1, 2, 3]);

            Assert.That(Directory.GetFiles(this.folder), Is.EqualTo(new[] { path }));
        }
    }
}
