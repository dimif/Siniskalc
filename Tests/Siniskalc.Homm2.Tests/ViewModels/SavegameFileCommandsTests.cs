namespace Siniskalc.Homm2.Tests.ViewModels
{
    using Moq;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Wpf.ViewModels;
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// The game lists a savegame only when its name has at most 8 characters before the extension; the editor refuses to save under another name.
    /// </summary>
    [TestFixture]
    internal sealed class SavegameFileCommandsTests
    {
        private string folder = string.Empty;
        private TestApplicationServices services = null!;
        private ShellViewModel shell = null!;
        private readonly List<string> warnings = [];
        private string? nextPath;
        private SavegameFileCommands commands = null!;

        [SetUp]
        public void LoadASavegame()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-save-names-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
            this.services = new TestApplicationServices(settingsPath: Path.Combine(this.folder, "settings.json"));
            this.shell = this.services.CreateShell();
            this.warnings.Clear();
            var dialogs = new Mock<IFileDialogs>();
            dialogs.Setup(d => d.PickFileToSave(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>())).Returns(() => this.nextPath);
            var messages = new Mock<IMessageService>();
            messages.Setup(m => m.ShowWarning(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((_, message) => this.warnings.Add(message));
            messages.Setup(m => m.AskYesNoCancel(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageAnswer.No);
            this.commands = new SavegameFileCommands(this.services.RecentFiles, dialogs.Object, messages.Object);
            this.shell.LoadGame(Saves.BrokenAllianceSave(5));
        }

        [TearDown]
        public void DeleteFolder() => Directory.Delete(this.folder, recursive: true);

        [TestCase("E01GOLD.GM1")]
        [TestCase("12345678.GM1")]
        [TestCase("A.GX1")]
        [TestCase("my_save.gm1")]
        [TestCase("SAVE-1.GM1")]
        public void NamesTheGameListsAreAccepted(string name) => Assert.That(this.services.Module.ValidateSavegameName(name), Is.Null);

        [TestCase("123456789.GM1")]
        [TestCase("broken_alliance_05.GM1")]
        [TestCase("my save.GM1")]
        [TestCase(".GM1")]
        [TestCase("sa+e.GM1")]
        public void NamesTheGameDoesNotListAreRefusedWithAReason(string name) =>
            Assert.That(this.services.Module.ValidateSavegameName(name), Does.Contain("would not show in the game"));

        [Test]
        public void TheExtensionIsNotCountedInTheLength() =>
            Assert.That(this.services.Module.ValidateSavegameName("ABCDEFGH.GM1"), Is.Null);

        [Test]
        public void SaveAsWithALongNameWarnsAndWritesNothing()
        {
            this.nextPath = Path.Combine(this.folder, "far_too_long_a_name.GM1");

            var saved = this.commands.SaveAs(this.shell);

            Assert.That(saved, Is.Null);
            Assert.That(File.Exists(this.nextPath), Is.False);
            Assert.That(this.warnings, Has.Count.EqualTo(1));
            Assert.That(this.warnings[0], Does.Contain("8"));
        }

        [Test]
        public void SaveAsWithAShortNameSaves()
        {
            this.nextPath = Path.Combine(this.folder, "OK.GM1");

            var saved = this.commands.SaveAs(this.shell);

            Assert.That(saved, Is.EqualTo(this.nextPath));
            Assert.That(File.Exists(this.nextPath), Is.True);
            Assert.That(this.warnings, Is.Empty);
        }
    }
}
