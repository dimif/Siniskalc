namespace Siniskalc.Homm3.Tests.Integration.ViewModels
{
    using Moq;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.ViewModels;
    using Siniskalc.Wpf.ViewModels;
    using Siniskalc.Wpf.Views;

    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class SavegameFileCommandsTests
    {
        private static readonly string Save = Path.Combine("TestData", "Sod", "conflux_00_baseline.GM1");

        private string folder = string.Empty;
        private RecentFilesStore recent = null!;
        private Mock<IMessageService> messages = null!;
        private string? pathToOpen;
        private string? pathToSave;
        private string? suggestedName;
        private int asked;
        private MessageAnswer answer;
        private SavegameFileCommands commands = null!;
        private ShellViewModel shell = null!;

        [SetUp]
        public void CreateCommands()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-file-commands-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
            this.recent = TestServices.CreateRecentFiles();
            this.pathToOpen = null;
            this.pathToSave = null;
            this.suggestedName = null;
            this.asked = 0;
            this.answer = MessageAnswer.Cancel;
            var dialogs = new Mock<IFileDialogs>();
            dialogs.Setup(d => d.PickFileToOpen(It.IsAny<string>()))
                .Callback(() => this.asked++)
                .Returns(() => this.pathToOpen);
            dialogs.Setup(d => d.PickFileToSave(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .Callback<string, string?, string?>((_, name, _) =>
                {
                    this.asked++;
                    this.suggestedName = name;
                })
                .Returns(() => this.pathToSave);
            this.messages = new Mock<IMessageService>();
            this.messages.Setup(m => m.AskYesNoCancel(It.IsAny<string>(), It.IsAny<string>())).Returns(() => this.answer);
            this.commands = new SavegameFileCommands(this.recent, dialogs.Object, this.messages.Object);
            this.shell = TestServices.CreateShell();
        }

        [TearDown]
        public void DeleteFolder() => Directory.Delete(this.folder, recursive: true);

        [Test]
        public void OpeningASavegameLoadsItAndRemembersIt()
        {
            this.pathToOpen = Save;

            var status = this.commands.Open(this.shell);

            Assert.That(status, Is.EqualTo(Save));
            Assert.That(this.shell.IsGameLoaded, Is.True);
            Assert.That(this.recent.Entries, Is.EqualTo(new[] { Save }));
        }

        [Test]
        public void ACancelledDialogDoesNothing()
        {
            this.pathToOpen = null;

            Assert.That(this.commands.Open(this.shell), Is.Null);
            Assert.That(this.shell.IsGameLoaded, Is.False);
            this.messages.VerifyNoOtherCalls();
        }

        [Test]
        public void AFileThatIsNoSavegameIsToldInItsOwnWordsAndNotRemembered()
        {
            var path = Path.Combine(this.folder, "notes.GM1");
            File.WriteAllText(path, "not a savegame");
            this.pathToOpen = path;

            Assert.That(this.commands.Open(this.shell), Is.Null);

            this.messages.Verify(m => m.ShowError(It.IsAny<string>(), It.Is<string>(message => !message.StartsWith("Failed to"))), Times.Once);
            this.messages.VerifyNoOtherCalls();
            Assert.That(this.recent.Entries, Is.Empty);
        }

        [Test]
        public void ARecentFileThatIsGoneIsTakenOffTheList()
        {
            var path = Path.Combine(this.folder, "gone.GM1");
            this.recent.Add(path);

            Assert.That(this.commands.OpenRecent(this.shell, path), Is.Null);

            this.messages.Verify(m => m.ShowWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            this.messages.VerifyNoOtherCalls();
            Assert.That(this.recent.Entries, Is.Empty);
        }

        [Test]
        public void SavingAGameWithoutAFileAsksWhereAndSavingAgainDoesNot()
        {
            this.shell.LoadGameFromBytes(TestServices.SavefileCodec.Read(Save), "dump.bin");
            var path = Path.Combine(this.folder, "copy.GM1");
            this.pathToSave = path;

            Assert.That(this.commands.Save(this.shell), Is.EqualTo(path));
            Assert.That(this.asked, Is.EqualTo(1));
            Assert.That(this.commands.Save(this.shell), Is.EqualTo(path));
            Assert.That(this.asked, Is.EqualTo(1));
            Assert.That(this.recent.Entries, Is.EqualTo(new[] { path }));
        }

        [Test]
        public void ASaveThatFailsIsToldAndNothingIsRemembered()
        {
            this.shell.LoadGameFromBytes(TestServices.SavefileCodec.Read(Save), "dump.bin");
            this.pathToSave = Path.Combine(this.folder, "missing folder", "copy.GM1");

            Assert.That(this.commands.SaveAs(this.shell), Is.Null);

            this.messages.Verify(m => m.ShowError(It.IsAny<string>(), It.Is<string>(message => message.StartsWith("Failed to save game"))), Times.Once);
            this.messages.VerifyNoOtherCalls();
            Assert.That(this.recent.Entries, Is.Empty);
        }

        [Test]
        public async Task ExportedBytesCanBeLoadedAgainButNotReloaded()
        {
            this.shell.LoadGame(Save);
            var path = Path.Combine(this.folder, "bytes.bin");
            this.pathToSave = path;

            Assert.That(await this.commands.ExportBytesAsync(this.shell), Is.EqualTo(path));
            Assert.That(this.suggestedName, Does.EndWith(".bin"));

            this.pathToOpen = path;
            Assert.That(this.commands.LoadBytes(this.shell), Is.EqualTo(path));
            Assert.That(this.shell.HasFile, Is.False);
            Assert.That(this.shell.CanReload, Is.False);
        }

        [Test]
        public void ClosingWithUnsavedChangesAsksAndFollowsTheAnswer()
        {
            this.shell.LoadGame(Save);
            ((Homm3EditorViewModel)this.shell.Editor).LoadedGame().History.CreatePlayerSession(0).Update(p => p with { Resources = p.Resources.With(Resource.Gold, 4242) });

            this.answer = MessageAnswer.Cancel;
            Assert.That(this.commands.ConfirmClose(this.shell), Is.False);

            this.answer = MessageAnswer.No;
            Assert.That(this.commands.ConfirmClose(this.shell), Is.True);
            Assert.That(this.shell.HasPendingChanges, Is.True);
        }

        [Test]
        public void ClosingWithNothingUnsavedDoesNotAsk()
        {
            this.shell.LoadGame(Save);

            Assert.That(this.commands.ConfirmClose(this.shell), Is.True);
            this.messages.VerifyNoOtherCalls();
        }
    }
}
