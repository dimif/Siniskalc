namespace Siniskalc.Tests.ViewModels
{
    using Siniskalc.Core.Runtime;
    using Siniskalc.Data;
    using Siniskalc.Entities;
    using Siniskalc.ViewModels;
    using Siniskalc.Wpf.Views;

    [TestFixture]
    internal sealed class SavegameFileCommandsTests
    {
        private static readonly string Save = Path.Combine("TestData", "Sod", "conflux_00_baseline.GM1");

        private string folder = string.Empty;
        private RecentFilesStore recent = null!;
        private ScriptedFileDialogs dialogs = null!;
        private RecordingMessageService messages = null!;
        private SavegameFileCommands commands = null!;
        private MainViewModel main = null!;

        [SetUp]
        public void CreateCommands()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-file-commands-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
            this.recent = TestServices.CreateRecentFiles();
            this.dialogs = new ScriptedFileDialogs();
            this.messages = new RecordingMessageService();
            this.commands = new SavegameFileCommands(this.recent, this.dialogs, this.messages);
            this.main = TestServices.CreateMainViewModel();
        }

        [TearDown]
        public void DeleteFolder() => Directory.Delete(this.folder, recursive: true);

        [Test]
        public void OpeningASavegameLoadsItAndRemembersIt()
        {
            this.dialogs.PathToOpen = Save;

            var status = this.commands.Open(this.main);

            Assert.That(status, Is.EqualTo(Save));
            Assert.That(this.main.IsGameLoaded, Is.True);
            Assert.That(this.recent.Entries, Is.EqualTo(new[] { Save }));
        }

        [Test]
        public void ACancelledDialogDoesNothing()
        {
            this.dialogs.PathToOpen = null;

            Assert.That(this.commands.Open(this.main), Is.Null);
            Assert.That(this.main.IsGameLoaded, Is.False);
            Assert.That(this.messages.Shown, Is.Empty);
        }

        [Test]
        public void AFileThatIsNoSavegameIsToldInItsOwnWordsAndNotRemembered()
        {
            var path = Path.Combine(this.folder, "notes.GM1");
            File.WriteAllText(path, "not a savegame");
            this.dialogs.PathToOpen = path;

            Assert.That(this.commands.Open(this.main), Is.Null);

            Assert.That(this.messages.Shown, Has.Count.EqualTo(1));
            Assert.That(this.messages.Shown[0].Kind, Is.EqualTo("Error"));
            Assert.That(this.messages.Shown[0].Message, Does.Not.StartWith("Failed to"));
            Assert.That(this.recent.Entries, Is.Empty);
        }

        [Test]
        public void ARecentFileThatIsGoneIsTakenOffTheList()
        {
            var path = Path.Combine(this.folder, "gone.GM1");
            this.recent.Add(path);

            Assert.That(this.commands.OpenRecent(this.main, path), Is.Null);

            Assert.That(this.messages.Shown.Single().Kind, Is.EqualTo("Warning"));
            Assert.That(this.recent.Entries, Is.Empty);
        }

        [Test]
        public void SavingAGameWithoutAFileAsksWhereAndSavingAgainDoesNot()
        {
            this.main.LoadGameFromBytes(TestServices.SavefileCodec.Read(Save), "dump.bin");
            var path = Path.Combine(this.folder, "copy.GM1");
            this.dialogs.PathToSave = path;

            Assert.That(this.commands.Save(this.main), Is.EqualTo(path));
            Assert.That(this.dialogs.Asked, Is.EqualTo(1));
            Assert.That(this.commands.Save(this.main), Is.EqualTo(path));
            Assert.That(this.dialogs.Asked, Is.EqualTo(1));
            Assert.That(this.recent.Entries, Is.EqualTo(new[] { path }));
        }

        [Test]
        public void ASaveThatFailsIsToldAndNothingIsRemembered()
        {
            this.main.LoadGameFromBytes(TestServices.SavefileCodec.Read(Save), "dump.bin");
            this.dialogs.PathToSave = Path.Combine(this.folder, "missing folder", "copy.GM1");

            Assert.That(this.commands.SaveAs(this.main), Is.Null);

            Assert.That(this.messages.Shown.Single().Message, Does.StartWith("Failed to save game"));
            Assert.That(this.recent.Entries, Is.Empty);
        }

        [Test]
        public async Task ExportedBytesCanBeLoadedAgainButNotReloaded()
        {
            this.main.LoadGame(Save);
            var path = Path.Combine(this.folder, "bytes.bin");
            this.dialogs.PathToSave = path;

            Assert.That(await this.commands.ExportBytesAsync(this.main), Is.EqualTo(path));
            Assert.That(this.dialogs.SuggestedName, Does.EndWith(".bin"));

            this.dialogs.PathToOpen = path;
            Assert.That(this.commands.LoadBytes(this.main), Is.EqualTo(path));
            Assert.That(this.main.HasFile, Is.False);
            Assert.That(this.main.CanReload, Is.False);
        }

        [Test]
        public void ClosingWithUnsavedChangesAsksAndFollowsTheAnswer()
        {
            this.main.LoadGame(Save);
            this.main.LoadedGame().History.CreatePlayerSession(0).Update(p => p with { Resources = p.Resources.With(Resource.Gold, 4242) });

            this.messages.Answer = MessageAnswer.Cancel;
            Assert.That(this.commands.ConfirmClose(this.main), Is.False);

            this.messages.Answer = MessageAnswer.No;
            Assert.That(this.commands.ConfirmClose(this.main), Is.True);
            Assert.That(this.main.HasPendingChanges, Is.True);
        }

        [Test]
        public void ClosingWithNothingUnsavedDoesNotAsk()
        {
            this.main.LoadGame(Save);

            Assert.That(this.commands.ConfirmClose(this.main), Is.True);
            Assert.That(this.messages.Shown, Is.Empty);
        }
    }
}
