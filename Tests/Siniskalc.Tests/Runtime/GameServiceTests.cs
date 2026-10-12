namespace Siniskalc.Tests.Runtime
{
    using Siniskalc.Core.Runtime;
    using Siniskalc.Data;
    using Siniskalc.Runtime;

    [TestFixture]
    internal sealed class GameServiceTests
    {
        private static readonly string Save = Path.Combine("TestData", "Sod", "conflux_00_baseline.GM1");

        private string folder = string.Empty;

        [SetUp]
        public void CreateFolder()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-games-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
        }

        [TearDown]
        public void DeleteFolder() => Directory.Delete(this.folder, recursive: true);

        [Test]
        public void AGameLoadedFromBytesHasNoFileToReload()
        {
            var bytes = TestServices.SavefileCodec.Read(Save);

            var game = TestServices.GameService.LoadGameFromBytes(bytes, "dump.bin");

            Assert.That(game.File, Is.Null);
            Assert.That(game.Name, Is.EqualTo("dump.bin"));
            Assert.That(() => TestServices.GameService.ReloadGame(game), Throws.InvalidOperationException);
        }

        [Test]
        public void AGameLoadedFromAFileIsNamedAfterIt()
        {
            var game = TestServices.GameService.LoadGameFromFile(Save);

            Assert.That(game.File, Is.EqualTo(Save));
            Assert.That(game.Name, Is.EqualTo(Save));
        }

        [Test]
        public void SavingToAnotherFileMakesThatFileTheOneToReload()
        {
            var game = TestServices.GameService.LoadGameFromBytes(TestServices.SavefileCodec.Read(Save), "dump.bin");
            var path = Path.Combine(this.folder, "copy.GM1");

            TestServices.GameService.SaveGame(game, path);

            Assert.That(game.File, Is.EqualTo(path));
            Assert.That(TestServices.GameService.ReloadGame(game).File, Is.EqualTo(path));
        }

        [Test]
        public void ABlankPathIsRefused()
        {
            Assert.That(() => TestServices.GameService.LoadGameFromFile(" "), Throws.ArgumentException);
        }

        [Test]
        public void ABackupThatFailsStopsTheSaveAndLeavesTheGameUnsaved()
        {
            var game = this.EditedGame(out var path);
            var service = new GameService(new NoParser(), TestServices.SavefileCodec, new FailingBackup());
            var before = File.ReadAllBytes(path);

            Assert.That(() => service.SaveGame(game, path), Throws.TypeOf<IOException>());

            Assert.That(File.ReadAllBytes(path), Is.EqualTo(before));
            Assert.That(game.File, Is.EqualTo(Save));
            Assert.That(game.History.GetPendingChanges(), Is.Not.Empty);
        }

        [Test]
        public void AWriteThatFailsLeavesTheGameUnsavedAndTheFileAsItWas()
        {
            var game = this.EditedGame(out var path);
            var service = new GameService(new NoParser(), new FailingCodec(), new NoBackup());
            var before = File.ReadAllBytes(path);

            Assert.That(() => service.SaveGame(game, path), Throws.TypeOf<IOException>());

            Assert.That(File.ReadAllBytes(path), Is.EqualTo(before));
            Assert.That(game.File, Is.EqualTo(Save));
            Assert.That(game.History.GetPendingChanges(), Is.Not.Empty);
        }

        private Game EditedGame(out string path)
        {
            var game = TestServices.GameService.LoadGameFromFile(Save);
            game.History.CreatePlayerSession(0).Update(player => player with { Resources = player.Resources.With(Siniskalc.Entities.Resource.Gold, 12345) });
            path = Path.Combine(this.folder, "target.GM1");
            File.WriteAllBytes(path, [1, 2, 3]);
            return game;
        }

        private sealed class NoParser : ISavegameParser
        {
            public Game Parse(byte[] bytes, string? file, string? name = null) => throw new NotSupportedException();
        }

        private sealed class NoBackup : ISavefileBackup
        {
            public string? Backup(string filePath) => null;
        }

        private sealed class FailingBackup : ISavefileBackup
        {
            public string? Backup(string filePath) => throw new IOException("The disk is full.");
        }

        private sealed class FailingCodec : ISavefileCodec
        {
            public byte[] Read(string filePath) => throw new NotSupportedException();

            public void Write(string filePath, byte[] bytes) => throw new IOException("The disk is full.");
        }
    }
}
