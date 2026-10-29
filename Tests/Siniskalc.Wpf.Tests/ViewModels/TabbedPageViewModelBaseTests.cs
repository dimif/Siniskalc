namespace Siniskalc.Wpf.Tests.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests the page that has tabs: the tab that is shown and the title and pending changes it has like every page.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class TabbedPageViewModelBaseTests
    {
        private sealed class Page(string title) : TabbedPageViewModelBase(title);

        [Test]
        public void ATabbedPageStartsOnTheFirstTabAndKeepsItsTitle()
        {
            var page = new Page("Castle");

            Assert.That((page.SelectedTab, page.Title), Is.EqualTo((0, "Castle")));
        }

        [Test]
        public void ChangingTheTabTellsOnceAndTheSameTabTellsNothing()
        {
            var page = new Page("Castle");
            var raised = new List<string?>();
            page.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            page.SelectedTab = 2;
            Assert.That(page.SelectedTab, Is.EqualTo(2));
            page.SelectedTab = 2;
            page.SelectedTab = 0;

            Assert.That(raised, Is.EqualTo(new[] { nameof(TabbedPageViewModelBase.SelectedTab), nameof(TabbedPageViewModelBase.SelectedTab) }));
            Assert.That(page.SelectedTab, Is.EqualTo(0));
        }

        [Test]
        public void ATabbedPageIsAPageSoItHasPendingChangesToo()
        {
            PageViewModelBase page = new Page("Castle");

            page.HasPendingChanges = true;

            Assert.That(page.HasPendingChanges, Is.True);
        }
    }
}
