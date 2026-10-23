namespace Siniskalc.Tests
{
    using System.Text.Json;
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
        public void DeleteFolder() => Directory.Delete(this.folder, recursive: true);

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
