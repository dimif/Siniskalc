namespace Siniskalc.Core.Tests.Runtime
{
    using Siniskalc.Core.Runtime;

    [TestFixture]
    internal sealed class FileBackupServiceProviderTests
    {
        private string folder = string.Empty;
        private SettingsStore settings = null!;
        private IFileBackupService enabled = null!;
        private IFileBackupService disabled = null!;

        [SetUp]
        public void CreateSettings()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-backup-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
            this.settings = new SettingsStore(Path.Combine(this.folder, "settings.json"));
            this.enabled = new NumberedSavefileBackupService();
            this.disabled = new DisabledFileBackupService();
        }

        [TearDown]
        public void DeleteFolder() => Directory.Delete(this.folder, recursive: true);

        [Test]
        public void TheNumberedServiceIsServedWhileBackupOnSaveIsOn()
        {
            this.settings.BackupOnSave = true;

            Assert.That(this.Provider().GetService(), Is.SameAs(this.enabled));
        }

        [Test]
        public void TheDisabledServiceIsServedWhileBackupOnSaveIsOff()
        {
            this.settings.BackupOnSave = false;

            Assert.That(this.Provider().GetService(), Is.SameAs(this.disabled));
        }

        [Test]
        public void ChangingTheSettingChangesTheServiceOfTheNextCall()
        {
            var provider = this.Provider();
            this.settings.BackupOnSave = false;
            var before = provider.GetService();
            this.settings.BackupOnSave = true;

            Assert.That(before, Is.SameAs(this.disabled));
            Assert.That(provider.GetService(), Is.SameAs(this.enabled));
        }

        private FileBackupServiceProvider Provider() => new(this.settings, this.enabled, this.disabled);
    }
}
