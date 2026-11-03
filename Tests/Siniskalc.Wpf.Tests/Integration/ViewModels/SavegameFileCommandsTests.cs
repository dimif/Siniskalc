namespace Siniskalc.Wpf.Tests.Integration.ViewModels
{
    using System.IO;
    using Moq;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Wpf.Tests.ViewModels;
    using Siniskalc.Wpf.ViewModels;
    using Siniskalc.Wpf.Views;

    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class SavegameFileCommandsTests
    {
        private string folder = string.Empty;
        private FakeGameModule first = null!;
        private FakeGameModule second = null!;
        private ShellViewModel shell = null!;
        private Mock<IRecentFilesStore> recentFiles = null!;
        private Mock<IFileDialogs> dialogs = null!;
        private Mock<IMessageService> messages = null!;
        private SavegameFileCommands commands = null!;

        [SetUp]
        public void CreateShellAndCommands()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-startup-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
            this.first = new FakeGameModule("First Game", 0x11, "*.F1");
            this.second = new FakeGameModule("Second Game", 0x22, "*.S1");
            this.shell = new ShellViewModel([this.first, this.second], Mock.Of<ISettingsStore>(), "Editor");
            this.recentFiles = new Mock<IRecentFilesStore>();
            this.dialogs = new Mock<IFileDialogs>(MockBehavior.Strict);
            this.messages = new Mock<IMessageService>(MockBehavior.Strict);
            this.commands = new SavegameFileCommands(this.recentFiles.Object, this.dialogs.Object, this.messages.Object);
        }

        [TearDown]
        public void DeleteFolder() => Directory.Delete(this.folder, recursive: true);

        [TestCase(0x11)]
        [TestCase(0x22)]
        public void ASuppliedPathWithSpacesLoadsTheMatchingGameAndIsAddedToRecentFiles(int marker)
        {
            var path = Path.Combine(this.folder, "My save.GM1");
            File.WriteAllBytes(path, [(byte)marker]);
            var module = marker == 0x11 ? this.first : this.second;

            var opened = this.commands.Open(this.shell, path);

            Assert.That(opened, Is.EqualTo(path));
            Assert.That(this.shell.CurrentModule, Is.SameAs(module));
            Assert.That(module.Editor.Loaded, Is.EqualTo(new[] { path }));
            Assert.That(this.shell.IsGameLoaded, Is.True);
            this.recentFiles.Verify(r => r.Add(path), Times.Once);
            this.dialogs.VerifyNoOtherCalls();
            this.messages.VerifyNoOtherCalls();
        }

        [Test]
        public void AFailedStartupLoadReportsTheErrorAndDoesNotAddARecentFile()
        {
            var path = Path.Combine(this.folder, "missing.GM1");
            this.first.Editor.FailToLoad = true;
            this.messages.Setup(m => m.ShowError("Load Game Error", "Failed to load game: not a savegame"));

            var opened = this.commands.Open(this.shell, path);

            Assert.That(opened, Is.Null);
            Assert.That(this.shell.IsGameLoaded, Is.False);
            this.messages.Verify(m => m.ShowError("Load Game Error", "Failed to load game: not a savegame"), Times.Once);
            this.recentFiles.Verify(r => r.Add(It.IsAny<string>()), Times.Never);
            this.dialogs.VerifyNoOtherCalls();
        }
    }
}
