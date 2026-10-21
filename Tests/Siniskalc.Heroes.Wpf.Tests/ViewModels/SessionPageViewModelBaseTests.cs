namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the page of a record that is edited through a session.
    /// </summary>
    [TestFixture]
    internal sealed class SessionPageViewModelBaseTests
    {
        private sealed class Page : SessionPageViewModelBase<string>
        {
            public Page(IEditSession<string> session)
                : base("Page", session)
            {
            }

            public void CheckDirty() => this.UpdateIsDirty();

            protected override void OnHistoryChanged()
            {
            }
        }

        private static SaveDocumentHistory History() =>
            new(new SaveDocument(new byte[2], new Dictionary<ChunkKind, IChunkCollection>(), new SavefileBytesPatcher()));

        [Test]
        public void ThePageHasPendingChangesWhileTheSessionIsDirty()
        {
            var session = new Mock<IEditSession<string>>();
            session.Setup(s => s.History).Returns(History());
            session.Setup(s => s.IsDirty).Returns(true);
            var page = new Page(session.Object);

            page.CheckDirty();

            Assert.That(page.HasPendingChanges, Is.True);
        }

        [Test]
        public void ResettingPutsBackTheModelAsLoaded()
        {
            var session = new Mock<IEditSession<string>>();
            session.Setup(s => s.History).Returns(History());
            session.Setup(s => s.IsDirty).Returns(true);
            session.Setup(s => s.Original).Returns("as loaded");
            string? result = null;
            session.Setup(s => s.Update(It.IsAny<Func<string, string>>())).Callback<Func<string, string>>(edit => result = edit("edited"));
            var page = new Page(session.Object);
            page.CheckDirty();

            page.ResetCommand.Execute(null);

            Assert.That(result, Is.EqualTo("as loaded"));
        }
    }
}
