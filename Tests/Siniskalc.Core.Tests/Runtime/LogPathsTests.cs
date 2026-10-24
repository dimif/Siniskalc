namespace Siniskalc.Core.Tests.Runtime
{
    using Siniskalc.Core.Runtime;

    [TestFixture]
    internal sealed class LogPathsTests
    {
        [Test]
        public void TheDefaultFolderIsUnderTheLocalApplicationDataOfThePlatform()
        {
            var folder = LogPaths.DefaultFolderFor("Siniskalc");

            Assert.That(folder, Is.EqualTo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Siniskalc", "logs")));
        }

        [Test]
        public void TheFolderTheUserChoseWinsOverTheDefault()
        {
            var settings = new UserSettings { LogFolder = Path.Combine(Path.GetTempPath(), "mylogs") };

            Assert.That(LogPaths.Resolve(settings, "Siniskalc"), Is.EqualTo(settings.LogFolder));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void NoFolderChosenMeansTheDefault(string? chosen)
        {
            var settings = new UserSettings { LogFolder = chosen };

            Assert.That(LogPaths.Resolve(settings, "Siniskalc"), Is.EqualTo(LogPaths.DefaultFolderFor("Siniskalc")));
        }

        [Test]
        public void AFolderIsUsableWhenAFileCanBeWrittenToItAndIsCreatedIfMissing()
        {
            var folder = Path.Combine(Path.GetTempPath(), "siniskalc-logpaths-" + Guid.NewGuid().ToString("N"), "logs");
            try
            {
                Assert.That(LogPaths.TryUse(folder, out var problem), Is.True);
                Assert.That(problem, Is.Null);
                Assert.That(Directory.Exists(folder), Is.True);
                Assert.That(Directory.GetFiles(folder), Is.Empty);
            }
            finally
            {
                Directory.Delete(Path.GetDirectoryName(folder)!, recursive: true);
            }
        }

        [Test]
        public void AFileWhereTheFolderShouldBeIsNotUsableAndSaysWhy()
        {
            var file = Path.Combine(Path.GetTempPath(), "siniskalc-logpaths-" + Guid.NewGuid().ToString("N"));
            File.WriteAllText(file, string.Empty);
            try
            {
                Assert.That(LogPaths.TryUse(file, out var problem), Is.False);
                Assert.That(problem, Is.Not.Null);
            }
            finally
            {
                File.Delete(file);
            }
        }

        [Test]
        public void ASavegameIsNamedByItsFileNameNeverByItsPath()
        {
            var file = Path.Combine(Path.GetTempPath(), "user", "slot1.GM1");

            Assert.That(LogPaths.Label(file, "Loaded from bytes"), Is.EqualTo("slot1.GM1"));
            Assert.That(LogPaths.Label(null, "dump"), Is.EqualTo("dump"));
        }
    }
}
