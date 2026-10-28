namespace Siniskalc.Heroes.Wpf.Tests.MultiUnit.Views
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Moq;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Heroes.Wpf.Views;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The blocks of the pages bind to what <see cref="IPlayerPage"/> and <see cref="ICastlePage"/> say a page has: a game whose page
    /// view model has only that gets the same page. These tests build a page of nothing else and put the blocks on it.
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category(TestKinds.MultiUnit)]
    [Apartment(ApartmentState.STA)]
    internal sealed class PageContractTests
    {
        [OneTimeSetUp]
        public void MakeTheApplication() => WpfTestApplication.Ensure();

        [Test]
        public void ThePlayerBlocksNeedNothingMoreThanAPlayerPageHas()
        {
            var page = new FakePlayerPage();
            var grid = new Grid { DataContext = page };
            grid.Children.Add(new StackPanel
            {
                Children =
                {
                    new PageHeader { Title = page.Name, ColorHex = page.ColorHex },
                    new ResourcesPanel { Note = "Limits." },
                    new ItemsCard { Heading = "Heroes", ItemsSource = page.Heroes, ItemTemplate = (DataTemplate)Application.Current.Resources["LinkTemplate"], HasItems = page.HasHeroes, EmptyText = "No heroes" },
                    new ItemsCard { Heading = "Towns", ItemsSource = page.Castles, ItemTemplate = (DataTemplate)Application.Current.Resources["LinkTemplate"], HasItems = page.HasCastles, EmptyText = "No towns" },
                },
            });

            using var hosted = WpfTestApplication.Show(grid);

            var texts = WpfTestApplication.Descendants<TextBlock>(grid).Select(t => t.Text).ToList();
            Assert.That(texts, Is.SupersetOf(new[] { "Blue", "Gold", "Quick add", "Ariel", "Knight, level 3", "Limits.", "No towns" }));
            Assert.That(WpfTestApplication.Descendants<Button>(grid).Select(b => b.Content), Is.SupersetOf(new object[] { "All +99", "Gold +100k", "Reset", "Ariel" }));
        }

        [Test]
        public void TheCastleBlocksNeedNothingMoreThanACastlePageHas()
        {
            var page = new FakeCastlePage();
            var grid = new Grid { DataContext = page };
            grid.Children.Add(new StackPanel
            {
                Children =
                {
                    new PageHeader { Title = page.Name, ColorHex = page.ColorHex },
                    new OwnerPicker(),
                },
            });

            using var hosted = WpfTestApplication.Show(grid);

            Assert.That(WpfTestApplication.Descendants<ComboBox>(grid).Single().SelectedItem, Is.EqualTo("Red"));
            Assert.That(WpfTestApplication.Descendants<TextBlock>(grid).Select(t => t.Text), Does.Contain("Castle Haret"));
        }

        [Test]
        public void TheHeroBlocksNeedNothingMoreThanAHeroPageHas()
        {
            var page = new Mock<IHeroPage>();
            page.Setup(p => p.Name).Returns("Ariel");
            page.Setup(p => p.ColorHex).Returns("#C0392B");
            page.Setup(p => p.SubtitleText).Returns("Knight, level 3");
            page.Setup(p => p.OwnerChoices).Returns(["Neutral", "Red", "Blue"]);
            page.Setup(p => p.SelectedOwner).Returns("Red");
            page.Setup(p => p.ResetCommand).Returns(new NoCommand());
            var grid = new Grid { DataContext = page.Object };
            grid.Children.Add(new StackPanel
            {
                Children =
                {
                    new PageHeader { Title = page.Object.Name, ColorHex = page.Object.ColorHex },
                    new OwnerPicker(),
                },
            });

            using var hosted = WpfTestApplication.Show(grid);

            Assert.That(WpfTestApplication.Descendants<ComboBox>(grid).Single().SelectedItem, Is.EqualTo("Red"));
            Assert.That(WpfTestApplication.Descendants<TextBlock>(grid).Select(t => t.Text), Does.Contain("Ariel"));
        }

        private sealed class FakeLink(string name, string detail) : ILink
        {
            public string Name { get; } = name;

            public string Detail { get; } = detail;

            public ICommand OpenCommand { get; } = new NoCommand();
        }

        private sealed class NoCommand : ICommand
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

        private sealed class FakePlayerPage : IPlayerPage, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged
            {
                add { }
                remove { }
            }

            public bool HasPendingChanges => true;

            public ICommand ResetCommand { get; } = new NoCommand();

            public IReadOnlyList<AmountRowViewModel> Resources { get; } = [new("Gold", "#D4A91C", 0, 999, 1, () => 5, () => 5, _ => { })];

            public ICommand SetAllResourcesCommand { get; } = new NoCommand();

            public ICommand SetRichCommand { get; } = new NoCommand();

            public string Name => "Blue";

            public string ColorHex => "#2E6FD0";

            public int SelectedTab { get; set; }

            public IReadOnlyList<ILink> Heroes { get; } = [new FakeLink("Ariel", "Knight, level 3")];

            public bool HasHeroes => true;

            public IReadOnlyList<ILink> Castles { get; } = [];

            public bool HasCastles => false;
        }

        private sealed class FakeCastlePage : ICastlePage, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged
            {
                add { }
                remove { }
            }

            public bool HasPendingChanges => false;

            public ICommand ResetCommand { get; } = new NoCommand();

            public IReadOnlyList<string> OwnerChoices { get; } = ["Neutral", "Red", "Blue"];

            public string SelectedOwner { get; set; } = "Red";

            public string Name => "Castle Haret";

            public string ColorHex => "#C0392B";

            public int SelectedTab { get; set; }

            public IReadOnlyList<ILink> Heroes { get; } = [];

            public string Notice => string.Empty;

            public bool HasNotice => false;
        }
    }
}
