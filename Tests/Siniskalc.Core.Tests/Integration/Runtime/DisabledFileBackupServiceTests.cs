namespace Siniskalc.Core.Tests.Integration.Runtime
{
    using Siniskalc.Core.Runtime;

    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class DisabledFileBackupServiceTests
    {
        [Test]
        public void NothingIsCopiedAndNoPathIsReturned()
        {
            var folder = Path.Combine(Path.GetTempPath(), "siniskalc-backup-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            try
            {
                var save = Path.Combine(folder, "save1.gm1");
                File.WriteAllText(save, "one");

                var copy = new DisabledFileBackupService().Backup(save);

                Assert.That(copy, Is.Null);
                Assert.That(Directory.GetFiles(folder), Is.EqualTo(new[] { save }));
            }
            finally
            {
                Directory.Delete(folder, recursive: true);
            }
        }

        [Test]
        public void AFileThatDoesNotExistIsNotAnError()
        {
            Assert.That(new DisabledFileBackupService().Backup("missing.gm1"), Is.Null);
        }
    }
}
