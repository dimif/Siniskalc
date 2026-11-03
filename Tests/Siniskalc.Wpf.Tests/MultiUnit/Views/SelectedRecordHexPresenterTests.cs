namespace Siniskalc.Wpf.Tests.MultiUnit.Views
{
    using System.ComponentModel;
    using System.Data;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// Tests selected-record binding with a real hex editor and WPF loaded and visibility events.
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category(TestKinds.MultiUnit)]
    [Apartment(ApartmentState.STA)]
    internal sealed class SelectedRecordHexPresenterTests
    {
        [TestCase(Visibility.Collapsed)]
        [TestCase(Visibility.Hidden)]
        public void ATargetIsCreatedOnlyAfterTheCardIsLoadedAndVisible(Visibility visibility)
        {
            using var view = new TestView(visibility);
            Assert.That(view.Context.FactoryCalls, Is.Zero);
            view.Context.Notifications.VerifyAdd(n => n.PropertyChanged += It.IsAny<PropertyChangedEventHandler>(), Times.Never);

            view.Show();
            view.Context.Notify();
            Assert.That(view.Context.FactoryCalls, Is.Zero);
            Assert.That(view.Grid.ItemsSource, Is.Null);

            view.Card.Visibility = Visibility.Visible;
            Pump();
            Assert.That(view.Context.FactoryCalls, Is.EqualTo(1));
            Assert.That(view.Grid.Items.Count, Is.GreaterThan(0));
        }

        [Test]
        public void EditsOfTheSameRecordUpdateTheBytesWithoutRebuildingTheGrid()
        {
            using var view = new TestView();
            view.Show();
            var source = view.Grid.ItemsSource;
            var row = view.Grid.Items[0];
            view.Context.Bytes[0] = 0xAB;
            view.Context.Notify();
            view.Context.Notify(string.Empty);
            view.Context.Notify(null);
            view.Context.Targets.Single().Raise(t => t.Changed += null, new ByteEditTargetChangedEventArgs(false));
            Pump();

            Assert.That(view.Context.FactoryCalls, Is.EqualTo(1));
            Assert.That(view.Grid.ItemsSource, Is.SameAs(source));
            Assert.That(view.Grid.Items[0], Is.SameAs(row));
            Assert.That(((DataRowView)row)["B00"], Is.EqualTo("AB"));
            view.Context.Targets.Single().Verify(t => t.Dispose(), Times.Never);
        }

        [Test]
        public void SelectingAnotherRecordDisposesTheOldTargetAndClearingTheSelectionUnbinds()
        {
            using var view = new TestView();
            view.Show();
            var first = view.Context.Targets.Single();
            view.Context.Key = 1;
            view.Context.Notify();
            var second = view.Context.Targets.Last();
            Assert.That(view.Context.FactoryCalls, Is.EqualTo(2));
            first.Verify(t => t.Dispose(), Times.Once);

            view.Context.Key = null;
            view.Context.Notify();
            Assert.That(view.Grid.ItemsSource, Is.Null);
            Assert.That(view.Context.FactoryCalls, Is.EqualTo(2), "an empty selection does not call the target factory");
            second.Verify(t => t.Dispose(), Times.Once);
        }

        [Test]
        public void ReplacingTheContextRebindsEvenWhenTheRecordKeyIsTheSameAndDetachesTheOldContext()
        {
            using var view = new TestView();
            view.Show();
            var old = view.Context;
            var replacement = new Context();
            view.Owner.DataContext = replacement;
            Pump();
            old.Targets.Single().Verify(t => t.Dispose(), Times.Once);
            old.Notifications.VerifyRemove(n => n.PropertyChanged -= It.IsAny<PropertyChangedEventHandler>(), Times.Once);
            replacement.Notifications.VerifyAdd(n => n.PropertyChanged += It.IsAny<PropertyChangedEventHandler>(), Times.Once);
            Assert.That(replacement.FactoryCalls, Is.EqualTo(1));

            var reads = replacement.SelectionReads;
            old.Notify();
            Assert.That(replacement.SelectionReads, Is.EqualTo(reads), "the previous context cannot refresh the new editor");
            view.Owner.DataContext = new object();
            Assert.That(view.Grid.ItemsSource, Is.Null);
            replacement.Targets.Single().Verify(t => t.Dispose(), Times.Once);
            replacement.Notifications.VerifyRemove(n => n.PropertyChanged -= It.IsAny<PropertyChangedEventHandler>(), Times.Once);
        }

        [Test]
        public void HidingTheCardReleasesItsTargetAndDefersTheLatestSelectionUntilItIsShown()
        {
            using var view = new TestView();
            view.Show();
            var old = view.Context;
            view.Card.Visibility = Visibility.Collapsed;
            Pump();
            old.Targets.Single().Verify(t => t.Dispose(), Times.Once);
            Assert.That(view.Grid.ItemsSource, Is.Null);

            var replacement = new Context { Key = 3 };
            view.Owner.DataContext = replacement;
            replacement.Key = 4;
            replacement.Notify();
            Assert.That(replacement.FactoryCalls, Is.Zero);
            view.Card.Visibility = Visibility.Visible;
            Pump();
            Assert.That(replacement.FactoryCalls, Is.EqualTo(1));
            Assert.That(replacement.CreatedKeys, Is.EqualTo(new[] { 4 }));
        }

        [Test]
        public void UnloadingDetachesTheContextAndReloadingUsesTheCurrentContextExactlyOnce()
        {
            using var view = new TestView();
            view.Show();
            view.Window.Content = null;
            Pump();
            view.Context.Targets.Single().Verify(t => t.Dispose(), Times.Once);
            view.Context.Notifications.VerifyRemove(n => n.PropertyChanged -= It.IsAny<PropertyChangedEventHandler>(), Times.Once);
            var reads = view.Context.SelectionReads;
            view.Context.Notify();
            Assert.That(view.Context.SelectionReads, Is.EqualTo(reads));
            Assert.That(view.Grid.ItemsSource, Is.Null);

            var replacement = new Context();
            view.Owner.DataContext = replacement;
            Assert.That(replacement.FactoryCalls, Is.Zero);
            view.Window.Content = view.Owner;
            Pump();
            Assert.That(replacement.FactoryCalls, Is.EqualTo(1));
            replacement.Notifications.VerifyAdd(n => n.PropertyChanged += It.IsAny<PropertyChangedEventHandler>(), Times.Once);
            replacement.Notify();
            Assert.That(replacement.FactoryCalls, Is.EqualTo(1));
        }

        [Test]
        public void DisposingThePresenterReleasesTheTargetAndStopsFollowingAllControlEvents()
        {
            using var view = new TestView();
            view.Show();
            view.Presenter.Dispose();
            view.Presenter.Dispose();
            view.Context.Targets.Single().Verify(t => t.Dispose(), Times.Once);
            view.Context.Notifications.VerifyRemove(n => n.PropertyChanged -= It.IsAny<PropertyChangedEventHandler>(), Times.Once);
            Assert.That(view.Grid.ItemsSource, Is.Null);

            var replacement = new Context();
            view.Owner.DataContext = replacement;
            view.Card.Visibility = Visibility.Collapsed;
            view.Card.Visibility = Visibility.Visible;
            view.Window.Content = null;
            Pump();
            view.Window.Content = view.Owner;
            Pump();
            replacement.Notify();
            Assert.That(replacement.FactoryCalls, Is.Zero);
            replacement.Notifications.VerifyAdd(n => n.PropertyChanged += It.IsAny<PropertyChangedEventHandler>(), Times.Never);
        }

        [Test]
        public void AnUnavailableTargetLeavesTheGridEmptyAndANewSelectionCanStillBind()
        {
            using var view = new TestView();
            view.Context.HasTarget = false;
            view.Show();
            Assert.That(view.Grid.ItemsSource, Is.Null);
            view.Context.Notify();
            Assert.That(view.Context.FactoryCalls, Is.EqualTo(1));

            view.Context.HasTarget = true;
            view.Context.Key = 1;
            view.Context.Notify();
            Assert.That(view.Grid.Items.Count, Is.GreaterThan(0));
            Assert.That(view.Context.FactoryCalls, Is.EqualTo(2));
            view.Owner.DataContext = null;
            Assert.That(view.Grid.ItemsSource, Is.Null);
            view.Context.Targets.Single().Verify(t => t.Dispose(), Times.Once);
        }

        [Test]
        public void APresenterCreatedForAnAlreadyLoadedControlBindsItsCurrentSelectionImmediately()
        {
            using var view = new TestView();
            view.Presenter.Dispose();
            view.Show();
            Assert.That(view.Context.FactoryCalls, Is.Zero);
            using var presenter = new SelectedRecordHexPresenter<Context>(
                view.Owner,
                view.Card,
                view.Editor,
                context => context.Notifications.Object,
                context => context.GetKey(),
                context => context.CreateTarget());
            Assert.That(view.Context.FactoryCalls, Is.EqualTo(1));
            Assert.That(view.Grid.Items.Count, Is.GreaterThan(0));
            view.Context.Notify();
            Assert.That(view.Context.FactoryCalls, Is.EqualTo(1));
        }

        private static void Pump() => Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);

        private sealed class Context
        {
            public Mock<INotifyPropertyChanged> Notifications { get; } = new();

            public List<Mock<IByteEditTarget>> Targets { get; } = [];

            public List<int> CreatedKeys { get; } = [];

            public byte[] Bytes { get; } = new byte[32];

            public int? Key { get; set; } = 0;

            public int SelectionReads { get; private set; }

            public int FactoryCalls { get; private set; }

            public bool HasTarget { get; set; } = true;

            public object? GetKey()
            {
                this.SelectionReads++;
                return this.Key;
            }

            public IByteEditTarget? CreateTarget()
            {
                this.FactoryCalls++;
                if (!this.HasTarget)
                {
                    return null;
                }

                this.CreatedKeys.Add(this.Key!.Value);
                var target = new Mock<IByteEditTarget>();
                target.SetupGet(t => t.Title).Returns("Record");
                target.SetupGet(t => t.Regions).Returns(Array.Empty<ByteRegion>());
                target.Setup(t => t.GetBytes()).Returns(() => this.Bytes.ToArray());
                target.Setup(t => t.GetOriginalBytes()).Returns(() => new byte[32]);
                this.Targets.Add(target);
                return target.Object;
            }

            public void Notify(string? propertyName = "Selection") =>
                this.Notifications.Raise(n => n.PropertyChanged += null, new PropertyChangedEventArgs(propertyName));
        }

        private sealed class TestView : IDisposable
        {
            public TestView(Visibility visibility = Visibility.Visible)
            {
                if (Application.Current == null)
                {
                    var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/Siniskalc.Wpf;component/Styles/SiniskalcStyles.xaml") });
                }

                this.Editor = new HexEditor();
                this.Card = new Border { Child = this.Editor, Visibility = visibility };
                this.Owner = new Border { Child = this.Card, DataContext = this.Context };
                this.Window = new Window { Content = this.Owner, Width = 1000, Height = 700, ShowInTaskbar = false };
                this.Presenter = new SelectedRecordHexPresenter<Context>(
                    this.Owner,
                    this.Card,
                    this.Editor,
                    context => context.Notifications.Object,
                    context => context.GetKey(),
                    context => context.CreateTarget());
            }

            public Context Context { get; } = new();

            public HexEditor Editor { get; }

            public Border Card { get; }

            public Border Owner { get; }

            public Window Window { get; }

            public SelectedRecordHexPresenter<Context> Presenter { get; }

            public DataGrid Grid => (DataGrid)this.Editor.FindName("bytesGrid");

            public void Show()
            {
                this.Window.Show();
                Pump();
                this.Window.UpdateLayout();
                Pump();
            }

            public void Dispose()
            {
                this.Window.Close();
                Pump();
                this.Presenter.Dispose();
            }
        }
    }
}
