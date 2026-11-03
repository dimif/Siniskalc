namespace Siniskalc.Core.Tests.Integration.Runtime
{
    using Siniskalc.Core.Runtime;

    [TestFixture]
    [Category(TestKinds.Integration)]
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
        public void StartsEmptyWhenThereIsNoSettingsFileYet()
        {
            var store = new RecentFilesStore(new SettingsStore(Path.Combine(this.folder, "settings.json")));

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
