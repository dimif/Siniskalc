namespace Siniskalc.Core.Tests.Integration.Runtime
{
    using Siniskalc.Core.Runtime;

    [TestFixture]
    [Category(TestKinds.Integration)]
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
        public void TheFirstStartWritesTheDefaultSettingsFileAndLaterStartsLeaveItAlone()
        {
            var path = Path.Combine(this.folder, "settings.json");
            var store = new SettingsStore(path);

            store.EnsureCreated();

            Assert.That(File.ReadAllText(path), Is.EqualTo(DefaultSettings.Json));
            File.WriteAllText(path, "{ \"AdvancedMode\": true }");
            new SettingsStore(path).EnsureCreated();
            Assert.That(File.ReadAllText(path), Is.EqualTo("{ \"AdvancedMode\": true }"));
        }

        [Test]
        public void TheSettingsOfAnEarlierVersionAreWrittenInsteadOfTheDefaultsTheFirstTime()
        {
            var path = Path.Combine(this.folder, "settings.json");
            var legacy = Path.Combine(this.folder, "legacy.json");
            File.WriteAllText(legacy, "{ \"AdvancedMode\": true }");

            new SettingsStore(path, legacy).EnsureCreated();

            Assert.That(new SettingsStore(path).AdvancedMode, Is.True);
        }

        [Test]
        public void ASettingsFileThatCannotBeWrittenTheFirstTimeIsReported()
        {
            // A folder where the file should be.
            var path = Path.Combine(this.folder, "settings.json");
            Directory.CreateDirectory(path);

            Assert.That(() => new SettingsStore(path).EnsureCreated(), Throws.TypeOf<SettingsException>());
        }

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
        public void ACorruptSettingsFileIsReportedWithItsPath()
        {
            var path = Path.Combine(this.folder, "settings.json");
            File.WriteAllText(path, "{ this is not json");

            var failure = Assert.Throws<SettingsException>(() => new SettingsStore(path));

            Assert.That(failure!.Path, Is.EqualTo(path));
            Assert.That(failure.Message, Does.Contain(path));
        }

        [Test]
        public void ALogLevelThatMeansNothingIsReportedLikeACorruptFile()
        {
            var path = Path.Combine(this.folder, "settings.json");
            File.WriteAllText(path, "{ \"LogLevel\": \"Loud\" }");

            Assert.That(() => new SettingsStore(path), Throws.TypeOf<SettingsException>());
        }

        [Test]
        public void SettingsThatCannotBeWrittenAreReportedButKeptForTheSession()
        {
            // A folder where the file should be: writing it fails.
            var path = Path.Combine(this.folder, "settings.json");
            Directory.CreateDirectory(path);
            var store = new SettingsStore(path);

            Assert.That(() => store.AdvancedMode = true, Throws.TypeOf<SettingsException>());
            Assert.That(store.AdvancedMode, Is.True);
        }

        [Test]
        public void TheLogFolderAndLevelAreKeptForTheNextTime()
        {
            var path = Path.Combine(this.folder, "settings.json");
            var store = new SettingsStore(path);
            store.Settings.LogFolder = Path.Combine(this.folder, "mylogs");
            store.Settings.LogLevel = Microsoft.Extensions.Logging.LogLevel.Warning;
            store.Save();

            var again = new SettingsStore(path);

            Assert.That(again.Settings.LogFolder, Is.EqualTo(Path.Combine(this.folder, "mylogs")));
            Assert.That(again.Settings.LogLevel, Is.EqualTo(Microsoft.Extensions.Logging.LogLevel.Warning));
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
