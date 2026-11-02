namespace Siniskalc.Tests.Integration
{
    using System.Text;
    using System.Text.Json;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;

    [TestFixture]
    [Category(TestKinds.Integration)]
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

            var services = Start(settingsPath);
            services.LoggerFactory.Dispose();

            var file = Directory.GetFiles(logs, "siniskalc-*.log").Single();
            var text = File.ReadAllText(file);
            Assert.That(text, Does.Contain("[INF] Siniskalc.AppCompositionRoot: Started Siniskalc"));
        }

        [Test]
        public void TheLevelOfTheSettingsDecidesWhatIsWritten()
        {
            var logs = Path.Combine(this.folder, "quiet");
            var settingsPath = this.WriteSettings(new { LogFolder = logs, LogLevel = "Error" });

            var services = Start(settingsPath);
            services.LoggerFactory.Dispose();

            Assert.That(Directory.GetFiles(logs).SelectMany(File.ReadAllLines), Is.Empty);
        }

        [Test]
        public void ALineShowsWhatItIsAboutWhenItIsLoggedInsideASubject()
        {
            var logs = Path.Combine(this.folder, "subject");
            var settingsPath = this.WriteSettings(new { LogFolder = logs, LogLevel = "Trace" });

            var services = Start(settingsPath);
            var logger = LogFactory.For(typeof(LoggingTests));
            using (logger.BeginSubject("Hero", "Sandro"))
            {
                logger.LogTrace("Class byte {Code:X2} read as Knight", 0xFE);
            }

            logger.LogTrace("Outside any subject");
            services.LoggerFactory.Dispose();

            var lines = File.ReadAllLines(Directory.GetFiles(logs).Single());
            Assert.That(lines, Has.One.Matches<string>(l => l.Contains("[VRB] Siniskalc.Tests.Integration.LoggingTests (Hero Sandro): Class byte FE read as Knight")));
            Assert.That(lines, Has.One.Matches<string>(l => l.EndsWith("[VRB] Siniskalc.Tests.Integration.LoggingTests: Outside any subject")));
        }

        [Test]
        public void TheFirstStartWritesTheDefaultSettingsAndTheSerilogConfigurationBesideEachOther()
        {
            var logs = Path.Combine(this.folder, "first");
            var settingsPath = this.WriteSettings(new { LogFolder = logs });

            var services = Start(settingsPath);
            services.LoggerFactory.Dispose();

            var written = JsonDocument.Parse(File.ReadAllText(Path.Combine(this.folder, "serilog.json"))).RootElement.GetProperty("Serilog");
            var file = written.GetProperty("WriteTo").EnumerateArray().Single(sink => sink.GetProperty("Name").GetString() == "File").GetProperty("Args");
            Assert.That(file.GetProperty("path").GetString(), Is.EqualTo("{LogFolder}/siniskalc-.log"));
            Assert.That(file.GetProperty("rollingInterval").GetString(), Is.EqualTo("Day"));
            Assert.That(file.GetProperty("retainedFileCountLimit").GetInt32(), Is.EqualTo(7));
            Assert.That(file.GetProperty("shared").GetBoolean(), Is.True);
            Assert.That(written.GetProperty("MinimumLevel").GetProperty("Default").GetString(), Is.EqualTo("Information"));
        }

        [Test]
        public void TheFirstStartWritesTheDefaultSettingsFile()
        {
            var settingsPath = Path.Combine(this.folder, "settings.json");

            var settings = new SettingsStore(settingsPath);
            settings.EnsureCreated();
            AppCompositionRoot.Compose(settings, NullLoggerFactory.Instance, Encoding.Latin1, new JsonGameDefinitionReader());

            Assert.That(File.ReadAllText(settingsPath), Is.EqualTo(DefaultSettings.Json));
        }

        [Test]
        public void TheTemplateAndTheRollingIntervalOfTheSerilogConfigurationAreHonoured()
        {
            var logs = Path.Combine(this.folder, "edited");
            var settingsPath = this.WriteSettings(new { LogFolder = logs });
            this.WriteSerilogConfiguration("""
                { "Serilog": { "WriteTo": [ { "Name": "File", "Args": {
                    "path": "{LogFolder}/edited-.log", "rollingInterval": "Hour", "shared": true,
                    "formatter": { "type": "Serilog.Templates.ExpressionTemplate, Serilog.Expressions", "template": "TEMPLATE {@l:u3} {@m}\n" } } } ] } }
                """);

            var services = Start(settingsPath);
            services.LoggerFactory.Dispose();

            var file = Directory.GetFiles(logs).Single();
            Assert.That(Path.GetFileName(file), Does.Match(@"^edited-\d{10}\.log$"));
            Assert.That(File.ReadAllLines(file).Single(), Does.StartWith("TEMPLATE INF Started Siniskalc"));
        }

        [Test]
        public void TheMinimumLevelOfTheSerilogConfigurationAppliesWhenTheSettingsLeaveTheLevelOpen()
        {
            var logs = Path.Combine(this.folder, "warnings");
            var settingsPath = this.WriteSettings(new { LogFolder = logs });
            this.WriteSerilogConfiguration("""
                { "Serilog": { "MinimumLevel": { "Default": "Warning" }, "WriteTo": [ { "Name": "File", "Args": {
                    "path": "{LogFolder}/warnings-.log", "shared": true } } ] } }
                """);

            var services = Start(settingsPath);
            services.LoggerFactory.Dispose();

            Assert.That(Directory.GetFiles(logs).SelectMany(File.ReadAllLines), Is.Empty);
        }

        [Test]
        public void TheLevelOfTheSettingsReplacesTheMinimumLevelOfTheSerilogConfiguration()
        {
            var logs = Path.Combine(this.folder, "replaced");
            var settingsPath = this.WriteSettings(new { LogFolder = logs, LogLevel = "Information" });
            this.WriteSerilogConfiguration("""
                { "Serilog": { "MinimumLevel": { "Default": "Error" }, "WriteTo": [ { "Name": "File", "Args": {
                    "path": "{LogFolder}/replaced-.log", "shared": true } } ] } }
                """);

            var services = Start(settingsPath);
            services.LoggerFactory.Dispose();

            Assert.That(File.ReadAllText(Directory.GetFiles(logs).Single()), Does.Contain("Started Siniskalc"));
        }

        [Test]
        public void ASerilogConfigurationThatCannotBeReadStopsTheStartWithItsPath()
        {
            var settingsPath = this.WriteSettings(new { LogFolder = Path.Combine(this.folder, "logs") });
            this.WriteSerilogConfiguration("{ this is not json");

            var failure = Assert.Throws<LoggingException>(() => Start(settingsPath));

            Assert.That(failure!.Path, Is.EqualTo(Path.Combine(this.folder, "serilog.json")));
            Assert.That(failure.Message, Does.Contain("serilog.json"));
        }

        [Test]
        public void ASerilogConfigurationWithASinkThatDoesNotExistStopsTheStartWithItsPath()
        {
            var settingsPath = this.WriteSettings(new { LogFolder = Path.Combine(this.folder, "logs") });
            this.WriteSerilogConfiguration("""{ "Serilog": { "WriteTo": [ { "Name": "NoSuchSink" } ] } }""");

            var failure = Assert.Throws<LoggingException>(() => Start(settingsPath));

            Assert.That(failure!.Path, Is.EqualTo(Path.Combine(this.folder, "serilog.json")));
        }

        [Test]
        public void ALogFolderThatCannotBeUsedStopsTheStartWithTheFolderNamed()
        {
            // A file where the folder should be.
            var blocked = Path.Combine(this.folder, "blocked");
            File.WriteAllText(blocked, string.Empty);
            var settingsPath = this.WriteSettings(new { LogFolder = blocked });

            var failure = Assert.Throws<LoggingException>(() => Start(settingsPath));

            Assert.That(failure!.Message, Does.Contain(blocked));
        }

        [Test]
        public void ACorruptSettingsFileStopsTheStartWithItsPathNamed()
        {
            var settingsPath = Path.Combine(this.folder, "settings.json");
            File.WriteAllText(settingsPath, "{ not json");

            var failure = Assert.Throws<SettingsException>(() => Start(settingsPath));

            Assert.That(failure!.Path, Is.EqualTo(settingsPath));
        }

        private static AppServices Start(string settingsPath) => AppCompositionRoot.Start(settingsPath, Encoding.Latin1, new JsonGameDefinitionReader());

        private void WriteSerilogConfiguration(string json) => File.WriteAllText(Path.Combine(this.folder, "serilog.json"), json);

        private string WriteSettings(object settings)
        {
            var path = Path.Combine(this.folder, "settings.json");
            File.WriteAllText(path, JsonSerializer.Serialize(settings));
            return path;
        }
    }
}
