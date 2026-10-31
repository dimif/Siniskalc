namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the page of a record that is edited through a session, with a session and a history that are fakes.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SessionPageViewModelBaseTests
    {
        private sealed class Page : SessionPageViewModelBase<string>
        {
            public Page(IEditSession<string> session)
                : base("Page", session)
            {
            }

            public int HistoryChanges { get; private set; }

            public void CheckDirty() => this.UpdateIsDirty();

            public bool Edit(Func<string, string> edit) => this.TryUpdate(edit);

            protected override void OnHistoryChanged() => this.HistoryChanges++;
        }

        private Mock<IHistoryEvents> events = null!;
        private Mock<IEditSession<string>> session = null!;

        [SetUp]
        public void StartWithASessionOverAHistoryThatIsAFake()
        {
            this.events = new Mock<IHistoryEvents>();
            this.session = new Mock<IEditSession<string>>();
            this.session.Setup(s => s.Events).Returns(this.events.Object);
            this.session.Setup(s => s.IsDirty).Returns(true);
        }

        [Test]
        public void ThePageHasPendingChangesWhileTheSessionIsDirty()
        {
            var page = new Page(this.session.Object);

            page.CheckDirty();

            Assert.That(page.HasPendingChanges, Is.True);
        }

        [Test]
        public void ResettingPutsBackTheModelAsLoaded()
        {
            this.session.Setup(s => s.Original).Returns("as loaded");
            string? result = null;
            this.session.Setup(s => s.Update(It.IsAny<Func<string, string>>())).Callback<Func<string, string>>(edit => result = edit("edited"));
            var page = new Page(this.session.Object);
            page.CheckDirty();

            page.ResetCommand.Execute(null);

            Assert.That(result, Is.EqualTo("as loaded"));
        }

        [Test]
        public void AResetTheRulesOfTheGameRefuseIsNotMadeAndTheNoticeSaysWhy()
        {
            this.session.Setup(s => s.Update(It.IsAny<Func<string, string>>())).Throws(new GameRuleException("Blue has the most heroes a player can have."));
            var page = new Page(this.session.Object);
            page.CheckDirty();
            var raised = new List<string?>();
            page.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            Assert.That(() => page.ResetCommand.Execute(null), Throws.Nothing);

            Assert.That((page.HasNotice, page.Notice), Is.EqualTo((true, "Blue has the most heroes a player can have.")));
            Assert.That(raised, Is.SupersetOf(new[] { nameof(page.Notice), nameof(page.HasNotice) }));
        }

        [Test]
        public void AnEditThatIsMadeClearsTheNoticeOfAnEditThatWasRefused()
        {
            var page = new Page(this.session.Object);
            this.session.Setup(s => s.Update(It.IsAny<Func<string, string>>())).Throws(new GameRuleException("Refused."));
            Assert.That(page.Edit(s => s), Is.False);

            this.session.Reset();
            this.session.Setup(s => s.Events).Returns(this.events.Object);

            Assert.That(page.Edit(s => s), Is.True);
            Assert.That((page.HasNotice, page.Notice), Is.EqualTo((false, string.Empty)));
        }

        [Test]
        public void AFaultOfTheSessionIsNotMadeToLookLikeARuleOfTheGame()
        {
            this.session.Setup(s => s.Update(It.IsAny<Func<string, string>>())).Throws(new InvalidOperationException("broken"));
            var page = new Page(this.session.Object);

            Assert.That(() => page.Edit(s => s), Throws.InvalidOperationException.With.Message.EqualTo("broken"));
            Assert.That(page.HasNotice, Is.False);
        }

        [Test]
        public void ThePageIsShownAgainWhenTheHistoryChangesAndNoLongerAfterItIsDisposed()
        {
            var page = new Page(this.session.Object);

            this.events.Raise(e => e.Changed += null, EventArgs.Empty);
            Assert.That(page.HistoryChanges, Is.EqualTo(1));

            page.Dispose();
            this.events.Raise(e => e.Changed += null, EventArgs.Empty);

            Assert.That(page.HistoryChanges, Is.EqualTo(1));
            this.events.VerifyRemove(e => e.Changed -= It.IsAny<EventHandler>(), Times.Once);
        }
    }
}
