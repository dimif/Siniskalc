namespace Siniskalc.Wpf.Tests.Integration.ViewModels
{
    using System;
    using System.IO;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Wpf.ViewModels;
    using Siniskalc.Wpf.Views;

    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class LoggingSettingsViewModelTests
    {
        private string folder = string.Empty;
        private string settingsPath = string.Empty;
        private SettingsStore store = null!;
        private Mock<IFileDialogs> dialogs = null!;

        [SetUp]
        public void CreateStore()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-logging-settings-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
            this.settingsPath = Path.Combine(this.folder, "settings.json");
            this.store = new SettingsStore(this.settingsPath);
            this.dialogs = new Mock<IFileDialogs>();
        }

        [TearDown]
        public void DeleteFolder() => Directory.Delete(this.folder, recursive: true);

        [Test]
        public void ThePageStartsWithTheLevelAndFolderOfTheSettings()
        {
            this.store.Settings.LogLevel = LogLevel.Warning;
            this.store.Settings.LogFolder = Path.Combine(this.folder, "logs");

            var page = this.CreatePage();

            Assert.That(page.Level, Is.EqualTo(LogLevel.Warning));
            Assert.That(page.Folder, Is.EqualTo(Path.Combine(this.folder, "logs")));
        }

        [Test]
        public void TheLevelsAreOfferedFromTheMostToTheLeastWritten()
        {
            Assert.That(
                this.CreatePage().Levels,
                Is.EqualTo(new[] { LogLevel.Trace, LogLevel.Debug, LogLevel.Information, LogLevel.Warning, LogLevel.Error, LogLevel.Critical }));
        }

        [Test]
        public void SavingKeepsTheLevelAndFolderForTheNextStart()
        {
            var chosen = Path.Combine(this.folder, "mylogs");
            var page = this.CreatePage();
            page.Level = LogLevel.Trace;
            page.Folder = "  " + chosen + "  ";

            Assert.That(page.TrySave(), Is.True);

            var again = new SettingsStore(this.settingsPath);
            Assert.That(again.Settings.LogLevel, Is.EqualTo(LogLevel.Trace));
            Assert.That(again.Settings.LogFolder, Is.EqualTo(chosen));
            Assert.That(Directory.Exists(chosen), Is.True);
        }

        [Test]
        public void AnEmptyFolderMeansTheDefaultOne()
        {
            this.store.Settings.LogFolder = Path.Combine(this.folder, "old");
            var page = this.CreatePage();

            page.UseDefaultCommand.Execute(null);
            Assert.That(page.TrySave(), Is.True);

            Assert.That(new SettingsStore(this.settingsPath).Settings.LogFolder, Is.Null);
        }

        [Test]
        public void AFolderThatCannotBeUsedIsNotKeptAndTheReasonIsShown()
        {
            // A file where the folder should be.
            var blocked = Path.Combine(this.folder, "blocked");
            File.WriteAllText(blocked, string.Empty);
            var page = this.CreatePage();
            page.Level = LogLevel.Trace;
            page.Folder = blocked;

            Assert.That(page.TrySave(), Is.False);

            Assert.That(page.HasError, Is.True);
            Assert.That(page.Error, Does.Contain(blocked));
            Assert.That(new SettingsStore(this.settingsPath).Settings.LogLevel, Is.EqualTo(LogLevel.Information));
        }

        [Test]
        public void AFolderThatIsNotAFullPathIsNotKept()
        {
            var page = this.CreatePage();
            page.Folder = Path.Combine("relative", "logs");

            Assert.That(page.TrySave(), Is.False);

            Assert.That(page.Error, Does.Contain("full path"));
        }

        [Test]
        public void SettingsThatCannotBeWrittenAreReportedInThePage()
        {
            // A folder where the settings file should be: writing it fails.
            Directory.Delete(this.folder, recursive: true);
            Directory.CreateDirectory(this.settingsPath);
            var page = this.CreatePage();

            Assert.That(page.TrySave(), Is.False);

            Assert.That(page.Error, Does.Contain(this.settingsPath));
        }

        [Test]
        public void ChoosingAFolderWithTheDialogFillsTheFolder()
        {
            var chosen = Path.Combine(this.folder, "picked");
            this.dialogs.Setup(d => d.PickFolder(It.IsAny<string?>(), It.IsAny<string>())).Returns(chosen);
            var page = this.CreatePage();

            page.BrowseCommand.Execute(null);

            Assert.That(page.Folder, Is.EqualTo(chosen));
        }

        [Test]
        public void CancellingTheFolderDialogChangesNothing()
        {
            this.dialogs.Setup(d => d.PickFolder(It.IsAny<string?>(), It.IsAny<string>())).Returns((string?)null);
            var page = this.CreatePage();
            page.Folder = Path.Combine(this.folder, "kept");

            page.BrowseCommand.Execute(null);

            Assert.That(page.Folder, Is.EqualTo(Path.Combine(this.folder, "kept")));
        }

        private LoggingSettingsViewModel CreatePage() => new(this.store, Path.Combine(this.folder, "default"), this.dialogs.Object);
    }
}
