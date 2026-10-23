namespace Siniskalc.Homm3.Tests.Runtime
{
    using Microsoft.Extensions.Logging;
    using Moq;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm3.Runtime;

    [TestFixture]
    internal sealed class GzipSavefileCodecTests
    {
        private readonly GzipSavefileCodec codec = new(NullLogger<GzipSavefileCodec>.Instance);
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
        public void ACorruptSavefileIsLoggedByItsFileNameAsAWarning()
        {
            var logger = new Mock<ILogger<GzipSavefileCodec>>();
            var path = Path.Combine(this.folder, "notes.GM1");
            File.WriteAllText(path, "this is not a gzip file at all");

            Assert.That(() => new GzipSavefileCodec(logger.Object).Read(path), Throws.TypeOf<CorruptSavefileException>());

            logger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("notes.GM1") && !state.ToString()!.Contains(this.folder)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
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
