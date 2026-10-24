namespace Siniskalc.Core.Tests.Runtime
{
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Runtime;

    [TestFixture]
    internal sealed class LogFactoryTests
    {
        [TearDown]
        public void ResetFactory() => LogFactory.Reset();

        [Test]
        public void ALoggerWritesNowhereUntilAFactoryIsInstalled()
        {
            var logger = LogFactory.For<LogFactoryTests>();

            Assert.That(() => logger.LogError("Nobody listens"), Throws.Nothing);
            Assert.That(logger.IsEnabled(LogLevel.Critical), Is.False);
        }

        [Test]
        public void ALoggerCreatedBeforeTheInstallUsesTheFactoryInstalledAfterwards()
        {
            var logger = LogFactory.For<LogFactoryTests>();
            var factory = new RecordingFactory();

            LogFactory.Install(factory);
            logger.LogWarning("Now it is heard");

            Assert.That(factory.Lines, Is.EqualTo(new[] { "Siniskalc.Core.Tests.Runtime.LogFactoryTests: Now it is heard" }));
        }

        [Test]
        public void TheCategoryOfAGenericTypeLeavesOutTheArity()
        {
            var factory = new RecordingFactory();
            LogFactory.Install(factory);

            LogFactory.For(typeof(List<>)).LogInformation("Listed");

            Assert.That(factory.Lines.Single(), Does.StartWith("System.Collections.Generic.List: "));
        }

        private sealed class RecordingFactory : ILoggerFactory
        {
            public List<string> Lines { get; } = [];

            public void AddProvider(ILoggerProvider provider)
            {
            }

            public ILogger CreateLogger(string categoryName) => new RecordingLogger(categoryName, this.Lines);

            public void Dispose()
            {
            }
        }

        private sealed class RecordingLogger(string category, List<string> lines) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
                => lines.Add(category + ": " + formatter(state, exception));
        }
    }
}
