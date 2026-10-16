using Moq;
using Siniskalc.Core.Runtime;
using Siniskalc.Wpf.ViewModels;

namespace Siniskalc.Wpf.Tests.ViewModels
{
    [TestFixture]
    internal sealed class AboutViewModelTests
    {
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
            var viewModel = new AboutViewModel(Info, new Mock<IUrlLauncher>().Object);

            Assert.That(viewModel.Name, Is.EqualTo("Sample"));
            Assert.That(viewModel.VersionText, Is.EqualTo("Version 2.3.4"));
            Assert.That(viewModel.Notices, Is.EqualTo(new[] { "A notice." }));
        }

        [Test]
        public void TheLinksOpenTheRepositoryAndTheReleasesThroughTheLauncher()
        {
            var launcher = new Mock<IUrlLauncher>();
            launcher.Setup(l => l.TryOpen(It.IsAny<string>())).Returns(true);
            var viewModel = new AboutViewModel(Info, launcher.Object);

            viewModel.OpenRepositoryCommand.Execute(null);
            viewModel.OpenReleasesCommand.Execute(null);

            launcher.Verify(l => l.TryOpen("https://example.com/repo"), Times.Once);
            launcher.Verify(l => l.TryOpen("https://example.com/repo/releases"), Times.Once);
        }
    }
}
