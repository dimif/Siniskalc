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
        public void ASavegameIsNamedByItsFileNameNeverByItsPath()
        {
            var file = Path.Combine(Path.GetTempPath(), "user", "slot1.GM1");

            Assert.That(LogPaths.Label(file, "Loaded from bytes"), Is.EqualTo("slot1.GM1"));
            Assert.That(LogPaths.Label(null, "dump"), Is.EqualTo("dump"));
        }
    }
}
