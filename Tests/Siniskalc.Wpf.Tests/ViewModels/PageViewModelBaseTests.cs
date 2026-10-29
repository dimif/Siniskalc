namespace Siniskalc.Wpf.Tests.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests what every page of the editor has: its title and whether it has pending changes.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class PageViewModelBaseTests
    {
        private sealed class Page(string title) : PageViewModelBase(title);

        [Test]
        public void APageKeepsItsTitleAndStartsWithNoPendingChanges()
        {
            var page = new Page("Orrin");

            Assert.That((page.Title, page.HasPendingChanges), Is.EqualTo(("Orrin", false)));
        }

        [Test]
        public void SettingPendingChangesTellsOnceAndTheSameValueTellsNothing()
        {
            var page = new Page("Orrin");
            var raised = new List<string?>();
            page.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            page.HasPendingChanges = true;
            page.HasPendingChanges = true;
            page.HasPendingChanges = false;
            page.HasPendingChanges = false;

            Assert.That(raised, Is.EqualTo(new[] { nameof(PageViewModelBase.HasPendingChanges), nameof(PageViewModelBase.HasPendingChanges) }));
            Assert.That(page.HasPendingChanges, Is.False);
        }
    }
}
