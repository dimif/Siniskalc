namespace Siniskalc.Homm2.Tests.ViewModels
{
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// Tests what every page about one record of a HoMM2 savegame does: it finds its own pending changes, reads again after every change
    /// of the history and puts its own changes back, on a history that is a fake.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class PageViewModelTests
    {
        private static readonly ChangeArea Area = new("Creatures");

        private Mock<IPendingChangeHistory> history = null!;
        private List<PendingChange> pending = null!;

        [SetUp]
        public void StartWithNoPendingChanges()
        {
            this.pending = [];
            this.history = new Mock<IPendingChangeHistory>();
            this.history.Setup(h => h.GetPendingChanges()).Returns(() => this.pending);
        }

        private static PendingChange Change(ChunkKind kind, int index, Action<SaveDocumentHistory>? revert = null) =>
            new("Orrin", new Change(Area, "Slot 2", "1", "2"), kind, index, revert ?? (_ => { }));

        private SamplePage Page(ChunkKind? kind = null, int index = 4, IByteEditTargetFactory? targets = null) =>
            new(this.history.Object, kind ?? Homm2ChunkKinds.Hero, index, targets);

        [Test]
        public void ThePageHasNoPendingChangesUntilItIsRefreshed()
        {
            this.pending.Add(Change(Homm2ChunkKinds.Hero, 4));

            Assert.That(this.Page().HasPendingChanges, Is.False);
        }

        [Test]
        public void RefreshReadsThePageAndMarksTheChangesMadeToItsOwnRecord()
        {
            this.pending.Add(Change(Homm2ChunkKinds.Hero, 4));
            var page = this.Page();

            page.Refresh();

            Assert.That(page.Reads, Is.EqualTo(1));
            Assert.That(page.HasPendingChanges, Is.True);
        }

        [Test]
        public void ChangesToAnotherRecordOrKindOfRecordAreNotThePagesOwn()
        {
            this.pending.Add(Change(Homm2ChunkKinds.Hero, 5));
            this.pending.Add(Change(Homm2ChunkKinds.Castle, 4));
            var page = this.Page();

            page.Refresh();

            Assert.That(page.HasPendingChanges, Is.False);
        }

        [Test]
        public void RefreshRaisesEveryPropertyAsChanged()
        {
            var page = this.Page();
            var raised = new List<string?>();
            page.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            page.Refresh();

            Assert.That(raised, Does.Contain(string.Empty).Or.Contain(null));
        }

        [Test]
        public void ThePageReadsAgainWhenTheHistoryChanges()
        {
            var page = this.Page();
            this.pending.Add(Change(Homm2ChunkKinds.Hero, 4));

            this.history.Raise(h => h.Changed += null, EventArgs.Empty);

            Assert.That(page.Reads, Is.EqualTo(1));
            Assert.That(page.HasPendingChanges, Is.True);
        }

        [Test]
        public void ThePageStopsListeningToTheHistoryWhenItIsDisposed()
        {
            var page = this.Page();

            page.Dispose();
            this.history.Raise(h => h.Changed += null, EventArgs.Empty);

            Assert.That(page.Reads, Is.EqualTo(0));
        }

        [Test]
        public void ResetPutsBackEveryChangeOfThePageAndNoOtherThroughTheHistory()
        {
            var own = Change(Homm2ChunkKinds.Hero, 4);
            var otherRecord = Change(Homm2ChunkKinds.Hero, 5);
            var otherKind = Change(Homm2ChunkKinds.Player, 4);
            this.pending.AddRange([own, otherRecord, otherKind]);
            var page = this.Page();
            page.Refresh();

            page.ResetCommand.Execute(null);

            this.history.Verify(h => h.Revert(own), Times.Once);
            this.history.Verify(h => h.Revert(It.IsAny<PendingChange>()), Times.Once);
        }

        [Test]
        public void ResetCannotRunWhileThePageHasNoPendingChanges()
        {
            var page = this.Page();
            page.Refresh();

            Assert.That(page.ResetCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void ThePageWithoutTheMeansToMakeATargetHasNoByteEditTarget()
        {
            Assert.That(this.Page().CreateByteEditTarget(), Is.Null);
        }

        [Test]
        public void ThePageAsksItsFactoryForTheTargetOfItsRecord()
        {
            var target = Mock.Of<IByteEditTarget>();
            var factory = new Mock<IByteEditTargetFactory>();
            factory.Setup(f => f.ForTurn(It.IsAny<SaveDocumentHistory>())).Returns(target);
            var page = this.Page(targets: factory.Object);

            Assert.That(page.CreateByteEditTarget(), Is.SameAs(target));
            Assert.That(page.FactoryGiven, Is.SameAs(factory.Object));
        }

        private sealed class SamplePage(IPendingChangeHistory history, ChunkKind kind, int index, IByteEditTargetFactory? targets)
            : PageViewModel(history, kind, index, targets)
        {
            public int Reads { get; private set; }

            public IByteEditTargetFactory? FactoryGiven { get; private set; }

            protected override void Read() => this.Reads++;

            protected override IByteEditTarget CreateByteEditTarget(IByteEditTargetFactory factory)
            {
                this.FactoryGiven = factory;
                return factory.ForTurn(null!);
            }
        }
    }
}
