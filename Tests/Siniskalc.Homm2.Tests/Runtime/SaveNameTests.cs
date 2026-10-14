namespace Siniskalc.Homm2.Tests.Runtime
{
    using Siniskalc.Core.Runtime;
    using Siniskalc.Wpf.ViewModels;
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// The game lists a savegame only when its name has at most 8 characters before the extension; the editor refuses to save under another name.
    /// </summary>
    [TestFixture]
    internal sealed class SaveNameTests
    {
        private string folder = string.Empty;
        private AppServices services = null!;
        private ShellViewModel shell = null!;
        private Dialogs dialogs = null!;
        private Messages messages = null!;
        private SavegameFileCommands commands = null!;

        [SetUp]
        public void LoadASavegame()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-save-names-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
            this.services = new AppServices(settingsPath: Path.Combine(this.folder, "settings.json"));
            this.shell = this.services.CreateShellViewModel();
            this.dialogs = new Dialogs();
            this.messages = new Messages();
            this.commands = new SavegameFileCommands(this.services.RecentFiles, this.dialogs, this.messages);
            this.shell.LoadGame(Saves.BrokenAllianceSave(5));
        }

        [TearDown]
        public void DeleteFolder() => Directory.Delete(this.folder, recursive: true);

        [TestCase("E01GOLD.GM1")]
        [TestCase("12345678.GM1")]
        [TestCase("A.GX1")]
        [TestCase("my_save.gm1")]
        [TestCase("SAVE-1.GM1")]
        public void NamesTheGameListsAreAccepted(string name) => Assert.That(this.services.Homm2.ValidateSavegameName(name), Is.Null);

        [TestCase("123456789.GM1")]
        [TestCase("broken_alliance_05.GM1")]
        [TestCase("my save.GM1")]
        [TestCase(".GM1")]
        [TestCase("sa+e.GM1")]
        public void NamesTheGameDoesNotListAreRefusedWithAReason(string name) =>
            Assert.That(this.services.Homm2.ValidateSavegameName(name), Does.Contain("would not show in the game"));

        [Test]
        public void TheExtensionIsNotCountedInTheLength() =>
            Assert.That(this.services.Homm2.ValidateSavegameName("ABCDEFGH.GM1"), Is.Null);

        [Test]
        public void HeroesOfMightAndMagicIIIAcceptsAnyName() =>
            Assert.That(this.services.Homm3.ValidateSavegameName("a very long name for a savegame.GM1"), Is.Null);

        [Test]
        public void SaveAsWithALongNameWarnsAndWritesNothing()
        {
            this.dialogs.Next = Path.Combine(this.folder, "far_too_long_a_name.GM1");

            var saved = this.commands.SaveAs(this.shell);

            Assert.That(saved, Is.Null);
            Assert.That(File.Exists(this.dialogs.Next), Is.False);
            Assert.That(this.messages.Warnings, Has.Count.EqualTo(1));
            Assert.That(this.messages.Warnings[0], Does.Contain("8"));
        }

        [Test]
        public void SaveAsWithAShortNameSaves()
        {
            this.dialogs.Next = Path.Combine(this.folder, "OK.GM1");

            var saved = this.commands.SaveAs(this.shell);

            Assert.That(saved, Is.EqualTo(this.dialogs.Next));
            Assert.That(File.Exists(this.dialogs.Next), Is.True);
            Assert.That(this.messages.Warnings, Is.Empty);
        }

        private sealed class Dialogs : IFileDialogs
        {
            public string? Next { get; set; }

            public string? PickFileToOpen(string filter) => null;

            public string? PickFileToSave(string filter, string? fileName = null, string? defaultExtension = null) => this.Next;
        }

        private sealed class Messages : IMessageService
        {
            public List<string> Warnings { get; } = [];

            public void ShowError(string title, string message)
            {
            }

            public void ShowUnexpectedError(string title, string message, string details)
            {
            }

            public void ShowWarning(string title, string message) => this.Warnings.Add(message);

            public void ShowInformation(string title, string message)
            {
            }

            public MessageAnswer AskYesNoCancel(string title, string message) => MessageAnswer.No;
        }
    }
}
