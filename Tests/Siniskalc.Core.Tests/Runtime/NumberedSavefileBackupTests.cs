namespace Siniskalc.Core.Tests.Runtime
{
    using Siniskalc.Core.Runtime;

    [TestFixture]
    internal sealed class NumberedSavefileBackupTests
    {
        private string folder = string.Empty;
        private string save = string.Empty;

        [SetUp]
        public void CreateFolder()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-backup-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
            this.save = Path.Combine(this.folder, "save1.gm1");
        }

        [TearDown]
        public void DeleteFolder() => Directory.Delete(this.folder, recursive: true);

        [Test]
        public void TheFirstBackupIsNamedBak()
        {
            File.WriteAllText(this.save, "one");

            var copy = new NumberedSavefileBackupService(NullLogger<NumberedSavefileBackupService>.Instance).Backup(this.save);

            Assert.That(copy, Is.EqualTo(this.save + ".bak"));
            Assert.That(File.ReadAllText(this.save + ".bak"), Is.EqualTo("one"));
        }

        [Test]
        public void ABackupNeverReplacesAnEarlierOneAndTheCounterGoesUp()
        {
            var backup = new NumberedSavefileBackupService(NullLogger<NumberedSavefileBackupService>.Instance);
            File.WriteAllText(this.save, "one");
            backup.Backup(this.save);
            File.WriteAllText(this.save, "two");
            backup.Backup(this.save);
            File.WriteAllText(this.save, "three");
            backup.Backup(this.save);

            Assert.That(File.ReadAllText(this.save + ".bak"), Is.EqualTo("one"));
            Assert.That(File.ReadAllText(this.save + ".bak1"), Is.EqualTo("two"));
            Assert.That(File.ReadAllText(this.save + ".bak2"), Is.EqualTo("three"));
        }

        [Test]
        public void TheCounterGoesOnAfterTheHighestCopyEvenWhenALowerOneWasDeleted()
        {
            File.WriteAllText(this.save, "current");
            File.WriteAllText(this.save + ".bak", "old");
            File.WriteAllText(this.save + ".bak5", "newer");

            var copy = new NumberedSavefileBackupService(NullLogger<NumberedSavefileBackupService>.Instance).Backup(this.save);

            Assert.That(copy, Is.EqualTo(this.save + ".bak6"));
        }

        [Test]
        public void CopiesOfOtherFilesAreNotCounted()
        {
            File.WriteAllText(this.save, "current");
            File.WriteAllText(this.save + ".bakery", "not a copy");
            File.WriteAllText(Path.Combine(this.folder, "save10.gm1.bak3"), "another save");

            var copy = new NumberedSavefileBackupService(NullLogger<NumberedSavefileBackupService>.Instance).Backup(this.save);

            Assert.That(copy, Is.EqualTo(this.save + ".bak"));
        }

        [Test]
        public void AFileThatDoesNotExistCannotBeBackedUp()
        {
            Assert.That(() => new NumberedSavefileBackupService(NullLogger<NumberedSavefileBackupService>.Instance).Backup(this.save), Throws.TypeOf<FileNotFoundException>());
        }
    }
}
