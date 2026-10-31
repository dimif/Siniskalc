using System.Linq;
using Moq;
using Siniskalc.Core.Runtime;
using Siniskalc.Wpf.ViewModels;

namespace Siniskalc.Wpf.Tests.ViewModels
{
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class RecentFilesMenuViewModelTests
    {
        private readonly List<string> entries = [];
        private Mock<IRecentFilesStore> store = null!;
        private RecentFilesMenuViewModel menu = null!;

        [SetUp]
        public void CreateMenu()
        {
            this.entries.Clear();
            this.store = new Mock<IRecentFilesStore>();
            this.store.SetupGet(s => s.Entries).Returns(this.entries);
            this.menu = new RecentFilesMenuViewModel(this.store.Object);
        }

        [TearDown]
        public void DisposeMenu() => this.menu.Dispose();

        [Test]
        public void AMenuWithoutFilesSaysThereAreNone()
        {
            var item = this.menu.Items.Single();

            Assert.That(item.Kind, Is.EqualTo(RecentMenuItemKind.Empty));
            Assert.That(item.Header, Is.EqualTo("(empty)"));
            Assert.That(item.IsEnabled, Is.False);
        }

        [Test]
        public void TheNewestFileIsFirstAndTheListEndsWithAClearCommand()
        {
            this.Change(@"C:\saves\new.GM1", @"C:\saves\old.GM1");

            Assert.That(this.menu.Items.Select(i => i.Kind), Is.EqualTo(new[] { RecentMenuItemKind.File, RecentMenuItemKind.File, RecentMenuItemKind.Separator, RecentMenuItemKind.Clear }));
            Assert.That(this.menu.Items[0].Header, Is.EqualTo(@"_1 C:\saves\new.GM1"));
            Assert.That(this.menu.Items[0].ToolTip, Is.EqualTo(@"C:\saves\new.GM1"));
            Assert.That(this.menu.Items[1].Header, Is.EqualTo(@"_2 C:\saves\old.GM1"));
            Assert.That(this.menu.Items[2].IsSeparator, Is.True);
        }

        [Test]
        public void UnderscoresOfAPathAreDoubledSoTheyAreNotTakenForAccessKeys()
        {
            this.Change(@"C:\my_saves\a_b.GM1");

            Assert.That(this.menu.Items[0].Header, Is.EqualTo(@"_1 C:\my__saves\a__b.GM1"));
        }

        [Test]
        public void PickingAFileAsksToOpenIt()
        {
            this.Change(@"C:\saves\a.GM1", @"C:\saves\b.GM1");
            var opened = new List<string>();
            this.menu.OpenRequested += opened.Add;

            this.menu.Items[1].Command!.Execute(null);

            Assert.That(opened, Is.EqualTo(new[] { @"C:\saves\b.GM1" }));
        }

        [Test]
        public void TheClearCommandClearsTheStore()
        {
            this.Change(@"C:\saves\a.GM1");

            this.menu.Items.Last().Command!.Execute(null);

            this.store.Verify(s => s.Clear(), Times.Once);
        }

        [Test]
        public void TheMenuIsBuiltAgainWhenTheStoreChangesAndNoLongerAfterTheMenuIsDisposed()
        {
            this.Change(@"C:\saves\a.GM1");
            this.Change();
            Assert.That(this.menu.Items.Single().Kind, Is.EqualTo(RecentMenuItemKind.Empty));

            this.menu.Dispose();
            this.Change(@"C:\saves\b.GM1");

            Assert.That(this.menu.Items.Single().Kind, Is.EqualTo(RecentMenuItemKind.Empty));
        }

        private void Change(params string[] paths)
        {
            this.entries.Clear();
            this.entries.AddRange(paths);
            this.store.Raise(s => s.Changed += null, EventArgs.Empty);
        }
    }
}
