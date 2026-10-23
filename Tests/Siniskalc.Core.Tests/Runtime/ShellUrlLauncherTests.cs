using Microsoft.Extensions.Logging;
using Siniskalc.Core.Runtime;

namespace Siniskalc.Core.Tests.Runtime
{
    [TestFixture]
    internal sealed class ShellUrlLauncherTests
    {
        [Test]
        public void AWebAddressIsHandedToTheShell()
        {
            var started = new List<string>();
            var launcher = new ShellUrlLauncher(started.Add, NullLogger<ShellUrlLauncher>.Instance);

            Assert.That(launcher.TryOpen("https://github.com/dimif/Siniskalc"), Is.True);
            Assert.That(started, Is.EqualTo(new[] { "https://github.com/dimif/Siniskalc" }));
        }

        [TestCase(@"C:\Windows\System32\calc.exe")]
        [TestCase("file:///C:/Windows/System32/calc.exe")]
        [TestCase("not an address")]
        [TestCase("")]
        public void AnythingButAWebAddressIsNeverStarted(string address)
        {
            var started = new List<string>();
            var launcher = new ShellUrlLauncher(started.Add, NullLogger<ShellUrlLauncher>.Instance);

            Assert.That(launcher.TryOpen(address), Is.False);
            Assert.That(started, Is.Empty);
        }

        [Test]
        public void ABrowserThatFailsToStartIsReportedNotThrown()
        {
            var launcher = new ShellUrlLauncher(_ => throw new InvalidOperationException("no browser"), NullLogger<ShellUrlLauncher>.Instance);

            Assert.That(launcher.TryOpen("https://github.com/dimif/Siniskalc"), Is.False);
        }

        [Test]
        public void ABrowserThatFailsToStartIsLoggedAsAWarningWithItsException()
        {
            var logger = new RecordingLogger<ShellUrlLauncher>();
            var launcher = new ShellUrlLauncher(_ => throw new InvalidOperationException("no browser"), logger);

            launcher.TryOpen("https://github.com/dimif/Siniskalc");

            var entry = logger.Entries.Single();
            Assert.That(entry.Level, Is.EqualTo(LogLevel.Warning));
            Assert.That(entry.Message, Does.Contain("https://github.com/dimif/Siniskalc"));
            Assert.That(entry.Exception, Is.TypeOf<InvalidOperationException>());
        }
    }
}
