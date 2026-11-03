using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Siniskalc.Core.Runtime;
using Siniskalc.Wpf.ViewModels;
using Siniskalc.Wpf.Tests.ViewModels;
using Siniskalc.Wpf.Views;

namespace Siniskalc.Wpf.Tests.Integration.ViewModels
{
    /// <summary>
    /// Tests the one window of the settings: the choices it stages, and what accepting it changes.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class SettingsViewModelTests
    {
        private string folder = string.Empty;
        private SettingsStore store = null!;
        private ShellViewModel shell = null!;

        [SetUp]
        public void CreateShell()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-settings-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
            this.store = new SettingsStore(Path.Combine(this.folder, "settings.json"));
            this.shell = new ShellViewModel([new FakeGameModule("First Game", 0x11, "*.F1")], this.store, "Editor");
        }

        [TearDown]
        public void DeleteFolder() => Directory.Delete(this.folder, recursive: true);

        private SettingsViewModel Settings(bool withLogging = true) => new(
            this.shell,
            withLogging ? new LoggingSettingsViewModel(this.store, Path.Combine(this.folder, "default"), new Mock<IFileDialogs>().Object) : null);

        [Test]
        public void TheWindowStartsFromWhatTheShellHasAndChangesNothingUntilItIsAccepted()
        {
            var settings = this.Settings();
            Assert.That((settings.BackupOnSave, settings.AdvancedMode), Is.EqualTo((true, false)));

            settings.BackupOnSave = false;
            settings.AdvancedMode = true;
            settings.Logging!.Level = LogLevel.Debug;

            Assert.That((this.shell.BackupOnSave, this.shell.IsAdvancedMode, this.store.Settings.LogLevel), Is.EqualTo((true, false, (LogLevel?)null)), "cancelling keeps everything");
        }

        [Test]
        public void AcceptingTheWindowAppliesTheChoicesAtOnceAndKeepsTheLoggingForTheNextStart()
        {
            var settings = this.Settings();
            settings.BackupOnSave = false;
            settings.AdvancedMode = true;
            settings.Logging!.Level = LogLevel.Debug;

            Assert.That(settings.TrySave(), Is.True);

            Assert.That((this.shell.BackupOnSave, this.shell.IsAdvancedMode, this.store.Settings.LogLevel), Is.EqualTo((false, true, LogLevel.Debug)));
        }

        [Test]
        public void AFolderThatCannotBeUsedKeepsTheWholeWindowOpenAndChangesNothing()
        {
            var settings = this.Settings();
            settings.AdvancedMode = true;
            settings.Logging!.Folder = "not a full path";

            Assert.That(settings.TrySave(), Is.False);

            Assert.That(settings.Logging.HasError, Is.True);
            Assert.That(this.shell.IsAdvancedMode, Is.False);
        }

        [Test]
        public void AnApplicationWithoutLoggingSettingsHasNoLoggingSection()
        {
            var settings = this.Settings(withLogging: false);
            settings.BackupOnSave = false;

            Assert.That(settings.HasLogging, Is.False);
            Assert.That(settings.TrySave(), Is.True);
            Assert.That(this.shell.BackupOnSave, Is.False);
        }
    }
}
