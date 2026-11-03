using Moq;
using Siniskalc.Core.Data;
using Siniskalc.Core.Runtime;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Tests the cache of parsed savegames that both game test projects share (<c>SaveCache</c> of <c>Siniskalc.Tests.Homm.Common</c>), with a
    /// game that is only bytes. The library has no test project of its own, so its tests are here.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SaveCacheTests
    {
        private static readonly byte[] FileBytes = [1, 2, 3, 4];

        private int loads;
        private SaveCache<FakeGame> cache = null!;

        [SetUp]
        public void StartWithAnEmptyCache()
        {
            this.loads = 0;
            this.cache = new SaveCache<FakeGame>(
                path =>
                {
                    Interlocked.Increment(ref this.loads);
                    return FakeGame.Of(path, FileBytes);
                },
                (game, file, name) => new FakeGame(file, (byte[])game.Bytes.Clone(), new SaveDocumentHistory(game.History.Original), name));
        }

        private static string Path(string name) => System.IO.Path.Combine(System.IO.Path.GetTempPath(), "siniskalc-cache-tests", name);

        [Test]
        public void AFileIsParsedOnceHoweverOftenItIsAskedFor()
        {
            this.cache.Get(Path("a.GM1"));
            this.cache.Get(Path("a.GM1"));
            this.cache.Get(Path("a.GM1"));

            Assert.That(this.loads, Is.EqualTo(1));
        }

        [Test]
        public void EveryCallGivesAGameOfItsOwnWithTheFileNameItWasAskedWith()
        {
            var first = this.cache.Get(Path("a.GM1"));
            var second = this.cache.Get(Path("a.GM1"));

            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(second.History, Is.Not.SameAs(first.History));
            Assert.That(second.Bytes, Is.Not.SameAs(first.Bytes));
            Assert.That((first.FileName, first.Name), Is.EqualTo((Path("a.GM1"), Path("a.GM1"))));
        }

        [Test]
        public void AnEditOfOneCopyIsNotSeenByAnotherCopyNorByTheNextCall()
        {
            var first = this.cache.Get(Path("a.GM1"));
            var other = this.cache.Get(Path("a.GM1"));
            var edited = FakeGame.NewDocument([9, 9, 9, 9]);

            first.History.Apply(edited);
            first.SavedTo(Path("saved.GM1"));
            first.Bytes[0] = 99;

            Assert.That(first.History.Current, Is.SameAs(edited));
            Assert.That(other.History.Current, Is.SameAs(other.History.Original));
            Assert.That(other.FileName, Is.EqualTo(Path("a.GM1")));
            var next = this.cache.Get(Path("a.GM1"));
            Assert.That(next.History.Current, Is.SameAs(next.History.Original));
            Assert.That(next.Bytes, Is.EqualTo(FileBytes));
        }

        [Test]
        public void TheCopiesShareTheParsedDocumentSoNothingIsParsedAgain()
        {
            var first = this.cache.Get(Path("a.GM1"));
            var second = this.cache.Get(Path("a.GM1"));

            Assert.That(second.History.Original, Is.SameAs(first.History.Original));
        }

        [Test]
        public void TwoSpellingsOfOnePathAreOneFile()
        {
            this.cache.Get(Path("a.GM1"));
            this.cache.Get(Path("sub/../a.GM1"));

            Assert.That(this.loads, Is.EqualTo(1));
        }

        [Test]
        public void TheSameBytesGiveAGameWithNoFileNameAndTheGivenName()
        {
            var copy = this.cache.GetFromBytes(Path("a.GM1"), [1, 2, 3, 4], "my name");

            Assert.That(copy, Is.Not.Null);
            Assert.That((copy!.FileName, copy.Name), Is.EqualTo(((string?)null, "my name")));
            Assert.That(this.loads, Is.EqualTo(1));
        }

        [Test]
        public void OtherBytesAreLeftToTheCallerToParse()
        {
            Assert.That(this.cache.GetFromBytes(Path("a.GM1"), [1, 2, 3, 5], null), Is.Null);
            Assert.That(this.cache.GetFromBytes(Path("a.GM1"), [1, 2, 3], null), Is.Null);
        }

        [Test]
        public void ManyFilesCanBeLoadedAtOnceAndEachIsParsedOnce()
        {
            var paths = Enumerable.Range(0, 40).Select(i => Path($"{i}.GM1")).ToList();

            this.cache.LoadAll(paths.Concat(paths));
            foreach (var path in paths)
            {
                this.cache.Get(path);
            }

            Assert.That(this.loads, Is.EqualTo(40));
        }

        [Test]
        public void AFileThatWasRemovedOrClearedIsParsedAgain()
        {
            this.cache.Get(Path("a.GM1"));
            this.cache.Get(Path("b.GM1"));

            this.cache.Remove(Path("a.GM1"));
            this.cache.Get(Path("a.GM1"));
            Assert.That(this.loads, Is.EqualTo(3));

            this.cache.Clear();
            this.cache.Get(Path("a.GM1"));
            this.cache.Get(Path("b.GM1"));
            Assert.That(this.loads, Is.EqualTo(5));
        }

        [Test]
        public void AFileThatCouldNotBeParsedIsTriedAgainTheNextTime()
        {
            var attempts = 0;
            var flaky = new SaveCache<FakeGame>(
                path => ++attempts == 1 ? throw new IOException("not there yet") : FakeGame.Of(path, FileBytes),
                (game, file, name) => new FakeGame(file, game.Bytes, new SaveDocumentHistory(game.History.Original), name));

            Assert.Throws<IOException>(() => flaky.Get(Path("a.GM1")));
            Assert.That(flaky.Get(Path("a.GM1")).Bytes, Is.EqualTo(FileBytes));
            Assert.That(attempts, Is.EqualTo(2));
        }

        private sealed class FakeGame(string? file, byte[] bytes, SaveDocumentHistory history, string? name) : AbstractSavegame(file, bytes, history, name)
        {
            public override string VersionName => "fake";

            public static FakeGame Of(string path, byte[] bytes) => new(path, (byte[])bytes.Clone(), new SaveDocumentHistory(NewDocument(bytes)), null);

            public static SaveDocument NewDocument(byte[] bytes) => new(bytes, new Dictionary<ChunkKind, IChunkCollection>(), Mock.Of<ISavefileBytesPatcher>());
        }
    }
}
