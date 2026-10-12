namespace Siniskalc.Core.Tests.Runtime
{
    using Siniskalc.Core.Runtime;

    [TestFixture]
    internal sealed class RecentFilesStoreTests
    {
        private string folder = string.Empty;

        [SetUp]
        public void CreateFolder()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-recent-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
        }

        [TearDown]
        public void DeleteFolder() => Directory.Delete(this.folder, recursive: true);

        [Test]
        public void TheListTellsWhenItChanges()
        {
            var store = new RecentFilesStore(new SettingsStore(Path.Combine(this.folder, "settings.json")));
            var changes = 0;
            store.Changed += (_, _) => changes++;

            store.Add(@"C:\saves\a.GM1");
            store.Remove(@"C:\saves\other.GM1");
            store.Remove(@"C:\saves\a.GM1");
            store.Clear();

            Assert.That(changes, Is.EqualTo(3));
        }

        [Test]
        public void ReadsTheListOfAnEarlierVersionWhenThereIsNoSettingsFileYet()
        {
            var legacy = this.WriteSettings("legacy.json", @"C:\saves\old.GM1");

            var store = new RecentFilesStore(new SettingsStore(Path.Combine(this.folder, "settings.json"), legacy));

            Assert.That(store.Entries, Is.EqualTo(new[] { @"C:\saves\old.GM1" }));
        }

        [Test]
        public void WritesTheMigratedListToTheNewLocationAndLeavesTheOldFileAlone()
        {
            var legacy = this.WriteSettings("legacy.json", @"C:\saves\old.GM1");
            var current = Path.Combine(this.folder, "settings.json");

            new RecentFilesStore(new SettingsStore(current, legacy)).Add(@"C:\saves\new.GM1");

            Assert.That(new RecentFilesStore(new SettingsStore(current, legacy)).Entries, Is.EqualTo(new[] { @"C:\saves\new.GM1", @"C:\saves\old.GM1" }));
            Assert.That(File.ReadAllText(legacy), Does.Contain("old.GM1").And.Not.Contain("new.GM1"));
        }

        [Test]
        public void PrefersTheSettingsFileOverTheOneOfAnEarlierVersion()
        {
            var legacy = this.WriteSettings("legacy.json", @"C:\saves\old.GM1");
            var current = this.WriteSettings("settings.json", @"C:\saves\current.GM1");

            var store = new RecentFilesStore(new SettingsStore(current, legacy));

            Assert.That(store.Entries, Is.EqualTo(new[] { @"C:\saves\current.GM1" }));
        }

        [Test]
        public void StartsEmptyWhenNeitherFileExists()
        {
            var store = new RecentFilesStore(new SettingsStore(Path.Combine(this.folder, "settings.json"), Path.Combine(this.folder, "legacy.json")));

            Assert.That(store.Entries, Is.Empty);
        }

        private string WriteSettings(string name, string recentFile)
        {
            var path = Path.Combine(this.folder, name);
            new UserSettings { RecentFiles = { recentFile } }.Save(path);
            return path;
        }
    }
}
