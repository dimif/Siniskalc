namespace Siniskalc.Core.Tests.Runtime
{
    using Siniskalc.Core.Runtime;

    [TestFixture]
    internal sealed class SettingsStoreTests
    {
        private string folder = string.Empty;

        [SetUp]
        public void CreateFolder()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-settings-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
        }

        [TearDown]
        public void DeleteFolder() => Directory.Delete(this.folder, recursive: true);

        [Test]
        public void BackupOnSaveIsOnAndAdvancedModeIsOffUntilTheUserChangesThem()
        {
            var store = new SettingsStore(Path.Combine(this.folder, "settings.json"));

            Assert.That(store.BackupOnSave, Is.True);
            Assert.That(store.AdvancedMode, Is.False);
        }

        [Test]
        public void ASettingMissingFromTheFileKeepsItsDefault()
        {
            var path = Path.Combine(this.folder, "settings.json");
            File.WriteAllText(path, "{ \"RecentFiles\": [ \"C:\\\\saves\\\\a.GM1\" ] }");

            var store = new SettingsStore(path);

            Assert.That(store.BackupOnSave, Is.True);
            Assert.That(store.AdvancedMode, Is.False);
            Assert.That(store.Settings.RecentFiles, Is.EqualTo(new[] { @"C:\saves\a.GM1" }));
        }

        [Test]
        public void ACorruptSettingsFileFallsBackToTheDefaults()
        {
            var path = Path.Combine(this.folder, "settings.json");
            File.WriteAllText(path, "{ this is not json");

            var store = new SettingsStore(path);

            Assert.That(store.BackupOnSave, Is.True);
            Assert.That(store.Settings.RecentFiles, Is.Empty);
        }

        [Test]
        public void SettingsThatCannotBeWrittenNeverInterruptTheUser()
        {
            // A folder where the file should be: writing it fails.
            var path = Path.Combine(this.folder, "settings.json");
            Directory.CreateDirectory(path);
            var store = new SettingsStore(path);

            Assert.That(() => store.AdvancedMode = true, Throws.Nothing);
            Assert.That(store.AdvancedMode, Is.True);
        }

        [Test]
        public void ChangedSettingsAreKeptForTheNextTime()
        {
            var path = Path.Combine(this.folder, "settings.json");
            var store = new SettingsStore(path);

            store.BackupOnSave = false;
            store.AdvancedMode = true;

            var again = new SettingsStore(path);
            Assert.That(again.BackupOnSave, Is.False);
            Assert.That(again.AdvancedMode, Is.True);
        }

        [Test]
        public void RecentFilesAndOtherSettingsDoNotOverwriteEachOther()
        {
            var path = Path.Combine(this.folder, "settings.json");
            var store = new SettingsStore(path);
            var recent = new RecentFilesStore(store);

            store.AdvancedMode = true;
            recent.Add(@"C:\saves\a.GM1");

            var again = new SettingsStore(path);
            Assert.That(again.AdvancedMode, Is.True);
            Assert.That(new RecentFilesStore(again).Entries, Is.EqualTo(new[] { @"C:\saves\a.GM1" }));
        }
    }
}
