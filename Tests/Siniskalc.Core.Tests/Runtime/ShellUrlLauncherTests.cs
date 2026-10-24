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
            var launcher = new ShellUrlLauncher(started.Add);

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
            var launcher = new ShellUrlLauncher(started.Add);

            Assert.That(launcher.TryOpen(address), Is.False);
            Assert.That(started, Is.Empty);
        }

        [Test]
        public void ABrowserThatFailsToStartIsReportedNotThrown()
        {
            var launcher = new ShellUrlLauncher(_ => throw new InvalidOperationException("no browser"));

            Assert.That(launcher.TryOpen("https://github.com/dimif/Siniskalc"), Is.False);
        }
    }
}
