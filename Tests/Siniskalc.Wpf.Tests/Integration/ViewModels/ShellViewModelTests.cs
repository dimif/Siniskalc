using System.IO;
using Moq;
using Siniskalc.Core.Runtime;
using Siniskalc.Wpf.Tests.ViewModels;
using Siniskalc.Wpf.ViewModels;

namespace Siniskalc.Wpf.Tests.Integration.ViewModels
{
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class ShellViewModelTests
    {
        private string folder = string.Empty;
        private Mock<ISettingsStore> settings = null!;
        private FakeGameModule first = null!;
        private FakeGameModule second = null!;
        private ShellViewModel shell = null!;

        [SetUp]
        public void CreateShell()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-shell-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
            this.settings = new Mock<ISettingsStore>();
            this.settings.SetupAllProperties();
            this.settings.Object.BackupOnSave = true;
            this.first = new FakeGameModule("First Game", 0x11, "*.F1;*.F2");
            this.second = new FakeGameModule("Second Game", 0x22, "*.S1");
            this.shell = new ShellViewModel([this.first, this.second], this.settings.Object, "Editor");
        }

        [TearDown]
        public void DeleteFolder() => Directory.Delete(this.folder, recursive: true);

        [Test]
        public void ANewShellShowsTheEditorOfTheFirstGame()
        {
            Assert.That(this.shell.CurrentModule, Is.SameAs(this.first));
            Assert.That(this.shell.Editor, Is.SameAs(this.first.Editor));
            Assert.That(this.shell.Title, Is.EqualTo("Editor - Save game editor"), "no game is named before a savegame is open");
            Assert.That(this.shell.IsGameLoaded, Is.False);
            Assert.That(this.second.Created, Is.Zero);
        }

        [Test]
        public void AShellWithoutGamesCannotBeMade()
        {
            Assert.That(() => new ShellViewModel([], this.settings.Object, "Editor"), Throws.ArgumentException);
        }

        [Test]
        public void AFileIsOpenedWithTheEditorOfTheGameThatRecognisesIt()
        {
            var path = this.WriteFile("save.S1", 0x22);

            this.shell.LoadGame(path);

            Assert.That(this.shell.CurrentModule, Is.SameAs(this.second));
            Assert.That(this.shell.Editor, Is.SameAs(this.second.Editor));
            Assert.That(this.second.Editor.Loaded, Is.EqualTo(new[] { path }));
            Assert.That(this.first.Editor.Loaded, Is.Empty);
            Assert.That(this.shell.Title, Is.EqualTo("Editor - Second Game (v1)"));
            Assert.That(this.shell.IsGameLoaded, Is.True);
            Assert.That(this.shell.File, Is.EqualTo(path));
        }

        [Test]
        public void AFileOfTheGameThatIsShownKeepsItsEditor()
        {
            var path = this.WriteFile("save.F1", 0x11);

            this.shell.LoadGame(path);

            Assert.That(this.shell.Editor, Is.SameAs(this.first.Editor));
            Assert.That(this.first.Created, Is.EqualTo(1));
            Assert.That(this.second.Created, Is.Zero);
        }

        [Test]
        public void AFileNoGameRecognisesIsGivenToTheEditorThatIsShown()
        {
            this.shell.LoadGame(this.WriteFile("save.S1", 0x22));
            var unknown = this.WriteFile("notes.txt", 0x99);

            this.shell.LoadGame(unknown);

            Assert.That(this.second.Editor.Loaded, Does.Contain(unknown));
            Assert.That(this.first.Editor.Loaded, Is.Empty);
        }

        [Test]
        public void AFileThatCannotBeReadIsGivenToTheEditorThatIsShownToTellWhy()
        {
            var missing = Path.Combine(this.folder, "missing.F1");

            this.shell.LoadGame(missing);

            Assert.That(this.first.Editor.Loaded, Is.EqualTo(new[] { missing }));
        }

        [Test]
        public void ALoadThatFailsLeavesTheEditorThatWasShown()
        {
            this.shell.LoadGame(this.WriteFile("first.F1", 0x11));
            this.second.Editor.FailToLoad = true;

            Assert.That(() => this.shell.LoadGame(this.WriteFile("broken.S1", 0x22)), Throws.InstanceOf<InvalidDataException>());

            Assert.That(this.shell.Editor, Is.SameAs(this.first.Editor));
            Assert.That(this.shell.IsGameLoaded, Is.True);
            Assert.That(this.shell.Title, Is.EqualTo("Editor - First Game (v1)"));
        }

        [Test]
        public void TheShellTellsWhenWhatItMirrorsFromTheEditorChanges()
        {
            var changed = new List<string?>();
            this.shell.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            this.first.Editor.LoadGame(this.WriteFile("save.F1", 0x11));
            this.first.Editor.Edit();

            Assert.That(changed, Does.Contain(nameof(ShellViewModel.IsGameLoaded)));
            Assert.That(changed, Does.Contain(nameof(ShellViewModel.Name)));
            Assert.That(changed, Does.Contain(nameof(ShellViewModel.HasPendingChanges)));
            Assert.That(changed, Does.Contain(nameof(ShellViewModel.CanUndo)));
            Assert.That(this.shell.HasPendingChanges, Is.True);
            Assert.That(this.shell.PendingChanges, Is.SameAs(this.first.Editor.PendingChanges));
        }

        [Test]
        public void ChangingTheEditorMakesEveryBindingReadAgain()
        {
            var changed = new List<string?>();
            this.shell.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            this.shell.LoadGame(this.WriteFile("save.S1", 0x22));

            Assert.That(changed, Does.Contain(string.Empty));
        }

        [Test]
        public void TheEditorThatWasLeftNoLongerReachesTheShell()
        {
            this.shell.LoadGame(this.WriteFile("save.S1", 0x22));
            var changed = new List<string?>();
            this.shell.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            this.first.Editor.Edit();

            Assert.That(changed, Is.Empty);
        }

        [Test]
        public void RevertAllCanBeRunOnlyWhileThereAreChanges()
        {
            Assert.That(this.shell.RevertAllCommand.CanExecute(null), Is.False);

            this.first.Editor.Edit();

            Assert.That(this.shell.RevertAllCommand.CanExecute(null), Is.True);
            this.shell.RevertAllCommand.Execute(null);
            Assert.That(this.first.Editor.Reverted, Is.EqualTo(1));
        }

        [Test]
        public void TheOpenFilterOffersEveryGameAndThenEachOfThem()
        {
            Assert.That(this.shell.OpenFilter, Is.EqualTo("Savegames |*.F1;*.F2;*.S1|First Game savegames |*.F1;*.F2|Second Game savegames |*.S1"));
        }

        [Test]
        public void TheOpenFilterOfASingleGameOffersItsFilesOnly()
        {
            var single = new ShellViewModel([this.first], this.settings.Object, "Editor");

            Assert.That(single.OpenFilter, Is.EqualTo("First Game savegames |*.F1;*.F2"));
        }

        [Test]
        public void TheSaveFilterIsThatOfTheGameThatIsShown()
        {
            Assert.That(this.shell.SaveFilter, Is.EqualTo("First Game savegames |*.F1;*.F2"));

            this.shell.LoadGame(this.WriteFile("save.S1", 0x22));

            Assert.That(this.shell.SaveFilter, Is.EqualTo("Second Game savegames |*.S1"));
        }

        [Test]
        public void ThePreferencesAreKeptInTheSettingsOfTheUser()
        {
            this.shell.BackupOnSave = false;
            this.shell.IsAdvancedMode = true;

            this.settings.VerifySet(s => s.BackupOnSave = false, Times.Once);
            this.settings.VerifySet(s => s.AdvancedMode = true, Times.Once);
        }

        [Test]
        public void ChangingAPreferenceTellsTheBindingsOnceAndASameValueTellsNothing()
        {
            var changed = new List<string?>();
            this.shell.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            this.shell.IsAdvancedMode = true;
            this.shell.IsAdvancedMode = true;
            this.shell.IsPendingChangesOpen = true;

            Assert.That(changed, Is.EqualTo(new[] { nameof(ShellViewModel.IsAdvancedMode), nameof(ShellViewModel.IsPendingChangesOpen) }));
        }

        [Test]
        public void UndoingAndSavingGoToTheEditorThatIsShown()
        {
            this.shell.LoadGame(this.WriteFile("save.S1", 0x22));
            var path = Path.Combine(this.folder, "copy.S1");

            this.shell.Undo();
            this.shell.SaveGame(path);

            Assert.That(this.second.Editor.Undone, Is.EqualTo(1));
            Assert.That(this.second.Editor.Savegame?.FileName, Is.EqualTo(path));
            Assert.That(this.first.Editor.Undone, Is.Zero);
            Assert.That(this.shell.GetBytesToSave(), Is.EqualTo(new byte[] { 1, 2, 3 }));
        }

        private string WriteFile(string name, byte marker)
        {
            var path = Path.Combine(this.folder, name);
            File.WriteAllBytes(path, [marker, 0, 0, 0]);
            return path;
        }
    }
}
