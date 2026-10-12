using System.IO;
using System.Linq;
using Siniskalc.Core.Runtime;
using Siniskalc.Wpf.ViewModels;

namespace Siniskalc.Wpf.Tests.ViewModels
{
    [TestFixture]
    internal sealed class RecentFilesMenuViewModelTests
    {
        private string folder = string.Empty;
        private RecentFilesStore store = null!;
        private RecentFilesMenuViewModel menu = null!;

        [SetUp]
        public void CreateMenu()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-recent-menu-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
            this.store = new RecentFilesStore(new SettingsStore(Path.Combine(this.folder, "settings.json")));
            this.menu = new RecentFilesMenuViewModel(this.store);
        }

        [TearDown]
        public void DeleteFolder()
        {
            this.menu.Dispose();
            Directory.Delete(this.folder, recursive: true);
        }

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
            this.store.Add(@"C:\saves\old.GM1");
            this.store.Add(@"C:\saves\new.GM1");

            Assert.That(this.menu.Items.Select(i => i.Kind), Is.EqualTo(new[] { RecentMenuItemKind.File, RecentMenuItemKind.File, RecentMenuItemKind.Separator, RecentMenuItemKind.Clear }));
            Assert.That(this.menu.Items[0].Header, Is.EqualTo(@"_1 C:\saves\new.GM1"));
            Assert.That(this.menu.Items[0].ToolTip, Is.EqualTo(@"C:\saves\new.GM1"));
            Assert.That(this.menu.Items[1].Header, Is.EqualTo(@"_2 C:\saves\old.GM1"));
            Assert.That(this.menu.Items[2].IsSeparator, Is.True);
        }

        [Test]
        public void UnderscoresOfAPathAreDoubledSoTheyAreNotTakenForAccessKeys()
        {
            this.store.Add(@"C:\my_saves\a_b.GM1");

            Assert.That(this.menu.Items[0].Header, Is.EqualTo(@"_1 C:\my__saves\a__b.GM1"));
        }

        [Test]
        public void PickingAFileAsksToOpenIt()
        {
            this.store.Add(@"C:\saves\a.GM1");
            this.store.Add(@"C:\saves\b.GM1");
            var opened = new List<string>();
            this.menu.OpenRequested += opened.Add;

            this.menu.Items[1].Command!.Execute(null);

            Assert.That(opened, Is.EqualTo(new[] { @"C:\saves\a.GM1" }));
        }

        [Test]
        public void TheClearCommandEmptiesTheListAndTheMenu()
        {
            this.store.Add(@"C:\saves\a.GM1");

            this.menu.Items.Last().Command!.Execute(null);

            Assert.That(this.store.Entries, Is.Empty);
            Assert.That(this.menu.Items.Single().Kind, Is.EqualTo(RecentMenuItemKind.Empty));
        }

        [Test]
        public void ARemovedFileLeavesTheMenu()
        {
            this.store.Add(@"C:\saves\a.GM1");
            this.store.Remove(@"C:\saves\a.GM1");

            Assert.That(this.menu.Items.Single().Kind, Is.EqualTo(RecentMenuItemKind.Empty));
        }
    }
}
