namespace Siniskalc.Core.Tests.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Moq;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// Tests loading, reloading and saving a game through the parser, the codec and the backup service, which are fakes that note what
    /// was done and in which order.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SavegameServiceTests
    {
        private readonly List<string> calls = [];
        private Mock<ISavegameParser<TestSavegame>> parser = null!;
        private Mock<ISavefileCodec> codec = null!;
        private Mock<IFileBackupService> backup = null!;
        private TestService service = null!;
        private string folder = null!;

        [SetUp]
        public void StartWithFakes()
        {
            this.calls.Clear();
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);

            this.parser = new Mock<ISavegameParser<TestSavegame>>();
            this.parser.Setup(p => p.Parse(It.IsAny<byte[]>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .Returns<byte[], string?, string?>((bytes, file, name) => new TestSavegame(file, name, bytes));
            this.codec = new Mock<ISavefileCodec>();
            this.codec.Setup(c => c.Read(It.IsAny<string>())).Returns(new byte[] { 0, 0, 1, 2, 3, 0, 0 }).Callback<string>(f => this.calls.Add($"read {Path.GetFileName(f)}"));
            this.codec.Setup(c => c.Write(It.IsAny<string>(), It.IsAny<byte[]>())).Callback<string, byte[]>((f, _) => this.calls.Add($"write {Path.GetFileName(f)}"));
            this.backup = new Mock<IFileBackupService>();
            this.backup.Setup(b => b.Backup(It.IsAny<string>())).Returns<string>(f => f + ".bak").Callback<string>(f => this.calls.Add($"backup {Path.GetFileName(f)}"));
            var backups = new Mock<IFileBackupServiceProvider>();
            backups.Setup(b => b.GetService()).Returns(this.backup.Object);
            this.service = new TestService(this.parser.Object, this.codec.Object, backups.Object);
        }

        [TearDown]
        public void RemoveTheFolder()
        {
            Directory.Delete(this.folder, true);
        }

        private string ExistingFile(string name)
        {
            var path = Path.Combine(this.folder, name);
            File.WriteAllBytes(path, [1]);
            return path;
        }

        [Test]
        public void LoadingAFileReadsItWithTheCodecAndGivesItToTheParserWithItsPath()
        {
            var game = this.service.LoadGameFromFile("C:\\saves\\a.GM1");

            Assert.That(game.FileName, Is.EqualTo("C:\\saves\\a.GM1"));
            Assert.That(this.calls, Is.EqualTo(new[] { "read a.GM1" }));
            this.parser.Verify(p => p.Parse(new byte[] { 0, 0, 1, 2, 3, 0, 0 }, "C:\\saves\\a.GM1", null), Times.Once);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void LoadingNoPathIsRefusedAndReadsNothing(string? path)
        {
            Assert.That(() => this.service.LoadGameFromFile(path!), Throws.ArgumentException);
            Assert.That(this.calls, Is.Empty);
        }

        [Test]
        public void LoadingBytesGivesThemToTheParserWithNoFileAndTheNameAsked()
        {
            var game = this.service.LoadGameFromBytes([1, 2, 3, 4, 5, 6, 7], "from memory");

            Assert.That(game.FileName, Is.Null);
            Assert.That(game.Name, Is.EqualTo("from memory"));
            Assert.That(this.calls, Is.Empty);
        }

        [Test]
        public void ReloadingReadsTheFileTheGameCameFromAgain()
        {
            var game = new TestSavegame("C:\\saves\\a.GM1");

            var again = this.service.ReloadGame(game);

            Assert.That(again, Is.Not.SameAs(game));
            Assert.That(again.FileName, Is.EqualTo("C:\\saves\\a.GM1"));
            Assert.That(this.calls, Is.EqualTo(new[] { "read a.GM1" }));
        }

        [Test]
        public void AGameWithNoFileCannotBeReloaded()
        {
            Assert.That(() => this.service.ReloadGame(new TestSavegame()), Throws.InvalidOperationException);
            Assert.That(() => this.service.ReloadGame(new TestSavegame("  ")), Throws.InvalidOperationException);
        }

        [Test]
        public void SavingToANewFileWritesTheBytesToSaveAndMakesBackupOfNothing()
        {
            var game = new TestSavegame();
            game.Edit(9);
            var path = Path.Combine(this.folder, "new.GM1");

            this.service.SaveGame(game, path);

            this.codec.Verify(c => c.Write(path, new byte[] { 0, 0, 9, 2, 3, 0, 0 }), Times.Once);
            this.backup.Verify(b => b.Backup(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void SavingOverAFileKeepsABackupOfItFirstAndThenWrites()
        {
            var path = this.ExistingFile("old.GM1");

            this.service.SaveGame(new TestSavegame(), path);

            Assert.That(this.calls, Is.EqualTo(new[] { "backup old.GM1", "write old.GM1" }));
        }

        [Test]
        public void AfterSavingTheGameIsTheFileSavedAndHasNothingPendingAndTheBaselineMoved()
        {
            var game = new TestSavegame("C:\\saves\\a.GM1");
            game.Edit(9);
            var path = this.ExistingFile("b.GM1");

            this.service.SaveGame(game, path);

            Assert.That(game.FileName, Is.EqualTo(path));
            Assert.That(game.History.GetPendingChanges(), Is.Empty);
            Assert.That(game.History.Original, Is.SameAs(game.History.Current));
        }

        [Test]
        public void IfTheBackupFailsNothingIsWrittenAndTheGameIsNotMarkedSaved()
        {
            var game = new TestSavegame("C:\\saves\\a.GM1");
            game.Edit(9);
            var path = this.ExistingFile("b.GM1");
            this.backup.Setup(b => b.Backup(It.IsAny<string>())).Throws(new IOException("disk full"));

            Assert.That(() => this.service.SaveGame(game, path), Throws.InstanceOf<IOException>());

            this.codec.Verify(c => c.Write(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
            Assert.That(game.FileName, Is.EqualTo("C:\\saves\\a.GM1"));
            Assert.That(game.History.Original, Is.Not.SameAs(game.History.Current));
        }

        [Test]
        public void IfTheWriteFailsTheGameIsNotMarkedSavedAndKeepsItsFile()
        {
            var game = new TestSavegame("C:\\saves\\a.GM1");
            game.Edit(9);
            this.codec.Setup(c => c.Write(It.IsAny<string>(), It.IsAny<byte[]>())).Throws(new IOException("locked"));

            Assert.That(() => this.service.SaveGame(game, Path.Combine(this.folder, "new.GM1")), Throws.InstanceOf<IOException>());

            Assert.That(game.FileName, Is.EqualTo("C:\\saves\\a.GM1"));
            Assert.That(game.History.Original, Is.Not.SameAs(game.History.Current));
        }

        [Test]
        public void IfTheEditsCannotBeAppliedTheFileIsNeitherBackedUpNorTouched()
        {
            var game = new Mock<ISavegame>();
            game.Setup(g => g.GetBytesToSave()).Throws(new ArgumentException("the edits do not fit"));
            var service = new TestServiceOf<ISavegame>(Mock.Of<ISavegameParser<ISavegame>>(), this.codec.Object, Mock.Of<IFileBackupServiceProvider>());
            var path = this.ExistingFile("b.GM1");

            Assert.That(() => service.SaveGame(game.Object, path), Throws.ArgumentException);

            Assert.That(this.calls, Is.Empty);
        }

        private sealed class TestService(ISavegameParser<TestSavegame> parser, ISavefileCodec codec, IFileBackupServiceProvider backups)
            : SavegameService<TestSavegame>(parser, codec, backups);

        private sealed class TestServiceOf<TGame>(ISavegameParser<TGame> parser, ISavefileCodec codec, IFileBackupServiceProvider backups)
            : SavegameService<TGame>(parser, codec, backups)
            where TGame : ISavegame;
    }
}
