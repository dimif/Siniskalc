namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using System;
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the page that can be put back to what the savefile had: its Reset button, and that it follows the history until disposed.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ResettablePageViewModelBaseTests
    {
        private sealed class Page(string title, IHistoryEvents history) : ResettablePageViewModelBase(title, history)
        {
            public int Changes { get; private set; }

            public int Resets { get; private set; }

            protected override void OnHistoryChanged() => this.Changes++;

            protected override void Reset() => this.Resets++;
        }

        private sealed class FakeHistory : IHistoryEvents
        {
            public event EventHandler? Changed;

            public bool HasListeners => this.Changed != null;

            public void Raise() => this.Changed?.Invoke(this, EventArgs.Empty);
        }

        [Test]
        public void APageKeepsItsTitleAndStartsWithTheResetButtonOff()
        {
            var page = new Page("Hero", new FakeHistory());

            Assert.That(page.Title, Is.EqualTo("Hero"));
            Assert.That(page.ResetCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void ResetIsOfferedOnlyWhileThePageHasPendingChangesAndRunsTheGamesReset()
        {
            var page = new Page("Hero", new FakeHistory());
            page.HasPendingChanges = true;
            Assert.That(page.ResetCommand.CanExecute(null), Is.True);

            page.ResetCommand.Execute(null);

            Assert.That(page.Resets, Is.EqualTo(1));
        }

        [Test]
        public void EveryChangeOfTheHistoryTellsThePage()
        {
            var history = new FakeHistory();
            var page = new Page("Hero", history);

            history.Raise();
            history.Raise();

            Assert.That(page.Changes, Is.EqualTo(2));
        }

        [Test]
        public void ADisposedPageNoLongerFollowsTheHistory()
        {
            var history = new FakeHistory();
            var page = new Page("Hero", history);

            page.Dispose();
            history.Raise();

            Assert.That(page.Changes, Is.Zero);
            Assert.That(history.HasListeners, Is.False);
        }

        [Test]
        public void ThePageIsATabbedPageSoItKeepsTheTabThatIsShown()
        {
            var page = new Page("Hero", Mock.Of<IHistoryEvents>());

            page.SelectedTab = 3;

            Assert.That(page.SelectedTab, Is.EqualTo(3));
        }
    }
}
