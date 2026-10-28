namespace Siniskalc.Heroes.Wpf.Tests.MultiUnit.Views
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Siniskalc.Heroes.Wpf.Views;

    /// <summary>
    /// Tests the header of a page.
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category(TestKinds.MultiUnit)]
    [Apartment(ApartmentState.STA)]
    internal sealed class PageHeaderTests
    {
        [OneTimeSetUp]
        public void MakeTheApplication() => WpfTestApplication.Ensure();

        [Test]
        public void TheHeaderShowsTheNameAndWhatThePageAdds()
        {
            var header = new PageHeader { Title = "Maximus", ColorHex = "#2E6FD0", Content = new TextBlock { Text = "Knight, level 3" }, DataContext = new FakePage() };

            using var hosted = WpfTestApplication.Show(header);

            var texts = WpfTestApplication.Descendants<TextBlock>(header).Select(t => t.Text).ToList();
            Assert.That(texts, Does.Contain("Maximus"));
            Assert.That(texts, Does.Contain("Knight, level 3"));
        }

        [Test]
        public void TheEditedMarkAndResetShowOnlyWhileThePageHasUnsavedEdits()
        {
            var page = new FakePage();
            var header = new PageHeader { Title = "Maximus", DataContext = page };
            using var hosted = WpfTestApplication.Show(header);
            var reset = WpfTestApplication.Descendants<Button>(header).Single(b => Equals(b.Content, "Reset"));
            Assert.That(reset.Visibility, Is.EqualTo(Visibility.Collapsed));

            page.HasPendingChanges = true;

            Assert.That(reset.Visibility, Is.EqualTo(Visibility.Visible));
            Assert.That(reset.Command, Is.SameAs(page.ResetCommand));
        }

        private sealed class FakePage : INotifyPropertyChanged
        {
            private bool hasPendingChanges;

            public event PropertyChangedEventHandler? PropertyChanged;

            public ICommand ResetCommand { get; } = new FakeCommand();

            public bool HasPendingChanges
            {
                get => this.hasPendingChanges;
                set
                {
                    this.hasPendingChanges = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.HasPendingChanges)));
                }
            }
        }

        private sealed class FakeCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged
            {
                add { }
                remove { }
            }

            public bool CanExecute(object? parameter) => true;

            public void Execute(object? parameter)
            {
            }
        }
    }
}
