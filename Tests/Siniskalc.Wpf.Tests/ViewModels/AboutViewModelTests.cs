using Siniskalc.Core.Runtime;
using Siniskalc.Wpf.ViewModels;

namespace Siniskalc.Wpf.Tests.ViewModels
{
    [TestFixture]
    internal sealed class AboutViewModelTests
    {
        private sealed class RecordingLauncher : IUrlLauncher
        {
            public List<string> Opened { get; } = [];

            public bool TryOpen(string url)
            {
                this.Opened.Add(url);
                return true;
            }
        }

        private static readonly AboutInfo Info = new(
            "Sample",
            "2.3.4",
            "Edits samples.",
            "https://example.com/repo",
            "https://example.com/repo/releases",
            "MIT",
            ["A notice."]);

        [Test]
        public void TheNameVersionAndNoticesComeFromTheInfo()
        {
            var viewModel = new AboutViewModel(Info, new RecordingLauncher());

            Assert.That(viewModel.Name, Is.EqualTo("Sample"));
            Assert.That(viewModel.VersionText, Is.EqualTo("Version 2.3.4"));
            Assert.That(viewModel.Notices, Is.EqualTo(new[] { "A notice." }));
        }

        [Test]
        public void TheLinksOpenTheRepositoryAndTheReleasesThroughTheLauncher()
        {
            var launcher = new RecordingLauncher();
            var viewModel = new AboutViewModel(Info, launcher);

            viewModel.OpenRepositoryCommand.Execute(null);
            viewModel.OpenReleasesCommand.Execute(null);

            Assert.That(launcher.Opened, Is.EqualTo(new[] { "https://example.com/repo", "https://example.com/repo/releases" }));
        }
    }
}
