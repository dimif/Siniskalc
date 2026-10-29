namespace Siniskalc.Wpf.Tests.ViewModels
{
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests an entry of the menu of recent files: what it shows and whether it is a separator or can be chosen.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class RecentMenuItemViewModelTests
    {
        [Test]
        public void AnEntryShowsItsKindItsHeaderItsToolTipAndItsCommand()
        {
            var command = new DelegateCommand(_ => { });

            var item = new RecentMenuItemViewModel(RecentMenuItemKind.File, "_1 a.GM1", "C:\\saves\\a.GM1", command);

            Assert.That((item.Kind, item.Header, item.ToolTip), Is.EqualTo((RecentMenuItemKind.File, "_1 a.GM1", "C:\\saves\\a.GM1")));
            Assert.That(item.Command, Is.SameAs(command));
        }

        [Test]
        public void AnEntryWithNoToolTipOrCommandHasNone()
        {
            var item = new RecentMenuItemViewModel(RecentMenuItemKind.Clear, "_Clear list");

            Assert.That((item.ToolTip, item.Command), Is.EqualTo(((string?)null, (System.Windows.Input.ICommand?)null)));
        }

        [TestCase(RecentMenuItemKind.Separator, true)]
        [TestCase(RecentMenuItemKind.File, false)]
        [TestCase(RecentMenuItemKind.Empty, false)]
        [TestCase(RecentMenuItemKind.Clear, false)]
        public void OnlyTheSeparatorIsASeparator(RecentMenuItemKind kind, bool expected)
        {
            Assert.That(new RecentMenuItemViewModel(kind, string.Empty).IsSeparator, Is.EqualTo(expected));
        }

        [TestCase(RecentMenuItemKind.Empty, false)]
        [TestCase(RecentMenuItemKind.File, true)]
        [TestCase(RecentMenuItemKind.Clear, true)]
        [TestCase(RecentMenuItemKind.Separator, true)]
        public void EveryEntryCanBeChosenExceptTheOneThatSaysTheListIsEmpty(RecentMenuItemKind kind, bool expected)
        {
            Assert.That(new RecentMenuItemViewModel(kind, string.Empty).IsEnabled, Is.EqualTo(expected));
        }
    }
}
