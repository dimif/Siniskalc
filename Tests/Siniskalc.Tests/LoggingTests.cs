namespace Siniskalc.Tests
{
    using System.Text.Json;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Runtime;

    [TestFixture]
    internal sealed class LoggingTests
    {
        private string folder = string.Empty;

        [SetUp]
        public void CreateFolder()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-logging-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
        }

        [TearDown]
        public void DeleteFolder()
        {
            LogFactory.Reset();
            Directory.Delete(this.folder, recursive: true);
        }

        [Test]
        public void TheApplicationWritesAPlainTextLogToTheFolderTheUserChose()
        {
            var logs = Path.Combine(this.folder, "mylogs");
            var settingsPath = this.WriteSettings(new { LogFolder = logs });

            var services = new AppServices(settingsPath: settingsPath);
            services.LoggerFactory.Dispose();

            var file = Directory.GetFiles(logs, "siniskalc-*.log").Single();
            var text = File.ReadAllText(file);
            Assert.That(text, Does.Contain("[INF] Siniskalc.AppServices: Started Siniskalc"));
        }

        [Test]
        public void TheLevelOfTheSettingsDecidesWhatIsWritten()
        {
            var logs = Path.Combine(this.folder, "quiet");
            var settingsPath = this.WriteSettings(new { LogFolder = logs, LogLevel = "Error" });

            var services = new AppServices(settingsPath: settingsPath);
            services.LoggerFactory.Dispose();

            Assert.That(Directory.GetFiles(logs).SelectMany(File.ReadAllLines), Is.Empty);
        }

        [Test]
        public void ALineShowsWhatItIsAboutWhenItIsLoggedInsideASubject()
        {
            var logs = Path.Combine(this.folder, "subject");
            var settingsPath = this.WriteSettings(new { LogFolder = logs, LogLevel = "Trace" });

            var services = new AppServices(settingsPath: settingsPath);
            var logger = LogFactory.For(typeof(LoggingTests));
            using (logger.BeginSubject("Hero", "Sandro"))
            {
                logger.LogTrace("Class byte {Code:X2} read as Knight", 0xFE);
            }

            logger.LogTrace("Outside any subject");
            services.LoggerFactory.Dispose();

            var lines = File.ReadAllLines(Directory.GetFiles(logs).Single());
            Assert.That(lines, Has.One.Matches<string>(l => l.Contains("[VRB] Siniskalc.Tests.LoggingTests (Hero Sandro): Class byte FE read as Knight")));
            Assert.That(lines, Has.One.Matches<string>(l => l.EndsWith("[VRB] Siniskalc.Tests.LoggingTests: Outside any subject")));
        }

        [Test]
        public void ALogFolderThatCannotBeUsedStopsTheStartWithTheFolderNamed()
        {
            // A file where the folder should be.
            var blocked = Path.Combine(this.folder, "blocked");
            File.WriteAllText(blocked, string.Empty);
            var settingsPath = this.WriteSettings(new { LogFolder = blocked });

            var failure = Assert.Throws<LoggingException>(() => new AppServices(settingsPath: settingsPath));

            Assert.That(failure!.Message, Does.Contain(blocked));
        }

        [Test]
        public void ACorruptSettingsFileStopsTheStartWithItsPathNamed()
        {
            var settingsPath = Path.Combine(this.folder, "settings.json");
            File.WriteAllText(settingsPath, "{ not json");

            var failure = Assert.Throws<SettingsException>(() => new AppServices(settingsPath: settingsPath));

            Assert.That(failure!.Path, Is.EqualTo(settingsPath));
        }

        private string WriteSettings(object settings)
        {
            var path = Path.Combine(this.folder, "settings.json");
            File.WriteAllText(path, JsonSerializer.Serialize(settings));
            return path;
        }
    }
}
