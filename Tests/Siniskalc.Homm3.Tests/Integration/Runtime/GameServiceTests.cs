namespace Siniskalc.Homm3.Tests.Integration.Runtime
{
    using Moq;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Runtime;

    [TestFixture]
    [Category(TestKinds.Integration)]
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

            Assert.That(game.FileName, Is.Null);
            Assert.That(game.Name, Is.EqualTo("dump.bin"));
            Assert.That(() => TestServices.GameService.ReloadGame(game), Throws.InvalidOperationException);
        }

        [Test]
        public void AGameLoadedFromAFileIsNamedAfterIt()
        {
            var game = TestServices.GameService.LoadGameFromFile(Save);

            Assert.That(game.FileName, Is.EqualTo(Save));
            Assert.That(game.Name, Is.EqualTo(Save));
        }

        [Test]
        public void SavingToAnotherFileMakesThatFileTheOneToReload()
        {
            var game = TestServices.GameService.LoadGameFromBytes(TestServices.SavefileCodec.Read(Save), "dump.bin");
            var path = Path.Combine(this.folder, "copy.GM1");

            TestServices.GameService.SaveGame(game, path);

            Assert.That(game.FileName, Is.EqualTo(path));
            Assert.That(TestServices.GameService.ReloadGame(game).FileName, Is.EqualTo(path));
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
            var service = new GameService(NoParser(), TestServices.SavefileCodec, BackupsOf(FailingBackup()));
            var before = File.ReadAllBytes(path);

            Assert.That(() => service.SaveGame(game, path), Throws.TypeOf<IOException>());

            Assert.That(File.ReadAllBytes(path), Is.EqualTo(before));
            Assert.That(game.FileName, Is.EqualTo(Save));
            Assert.That(game.History.GetPendingChanges(), Is.Not.Empty);
        }

        [Test]
        public void SavingToAFileThatDoesNotExistYetMakesNoBackup()
        {
            var game = TestServices.GameService.LoadGameFromFile(Save);
            var backup = new Mock<IFileBackupService>(MockBehavior.Strict);
            var service = new GameService(NoParser(), TestServices.SavefileCodec, BackupsOf(backup.Object));
            var path = Path.Combine(this.folder, "new.GM1");

            service.SaveGame(game, path);

            backup.Verify(b => b.Backup(It.IsAny<string>()), Times.Never);
            Assert.That(File.Exists(path), Is.True);
        }

        [Test]
        public void SavingOverAnExistingFileBacksItUpFirst()
        {
            var game = this.EditedGame(out var path);
            var backup = new Mock<IFileBackupService>();
            var service = new GameService(NoParser(), TestServices.SavefileCodec, BackupsOf(backup.Object));

            service.SaveGame(game, path);

            backup.Verify(b => b.Backup(path), Times.Once);
        }

        [Test]
        public void AWriteThatFailsLeavesTheGameUnsavedAndTheFileAsItWas()
        {
            var game = this.EditedGame(out var path);
            var service = new GameService(NoParser(), FailingCodec(), BackupsOf(new DisabledFileBackupService()));
            var before = File.ReadAllBytes(path);

            Assert.That(() => service.SaveGame(game, path), Throws.TypeOf<IOException>());

            Assert.That(File.ReadAllBytes(path), Is.EqualTo(before));
            Assert.That(game.FileName, Is.EqualTo(Save));
            Assert.That(game.History.GetPendingChanges(), Is.Not.Empty);
        }

        private Game EditedGame(out string path)
        {
            var game = TestServices.GameService.LoadGameFromFile(Save);
            game.History.CreatePlayerSession(0).Update(player => player with { Resources = player.Resources.With(Siniskalc.Heroes.Entities.Resource.Gold, 12345) });
            path = Path.Combine(this.folder, "target.GM1");
            File.WriteAllBytes(path, [1, 2, 3]);
            return game;
        }

        private static ISavegameParser NoParser() => new Mock<ISavegameParser>(MockBehavior.Strict).Object;

        private static IFileBackupServiceProvider BackupsOf(IFileBackupService service)
        {
            var provider = new Mock<IFileBackupServiceProvider>();
            provider.Setup(p => p.GetService()).Returns(service);
            return provider.Object;
        }

        private static IFileBackupService FailingBackup()
        {
            var backup = new Mock<IFileBackupService>();
            backup.Setup(b => b.Backup(It.IsAny<string>())).Throws(new IOException("The disk is full."));
            return backup.Object;
        }

        private static ISavefileCodec FailingCodec()
        {
            var codec = new Mock<ISavefileCodec>();
            codec.Setup(c => c.Write(It.IsAny<string>(), It.IsAny<byte[]>())).Throws(new IOException("The disk is full."));
            return codec.Object;
        }
    }
}
