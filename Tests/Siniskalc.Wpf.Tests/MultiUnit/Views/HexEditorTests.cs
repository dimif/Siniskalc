using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Siniskalc.Core.Data;
using Siniskalc.Wpf.Views;

namespace Siniskalc.Wpf.Tests.MultiUnit.Views
{
    /// <summary>
    /// Tests what clicking a known field of the hex editor does to the bytes that are selected, and the colours of the fields.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    [Apartment(ApartmentState.STA)]
    internal sealed class HexEditorTests
    {
        [Test]
        public void ClickingAFieldSelectsAllItsBytesEveryTimeAndAGroupSelectsTheBytesOfAllItsParts()
        {
            EnsureApplication();
            var parts = new[]
            {
                new ByteRegion("Slot 1", 0, 4, ByteRegionKind.Creatures, "Army"),
                new ByteRegion("Slot 2", 4, 4, ByteRegionKind.Creatures, "Army"),
            };
            var target = new FakeTarget(
                new byte[40],
                [new ByteRegion("Army", 0, 8, ByteRegionKind.Creatures, Parts: parts), new ByteRegion("Id", 20, 2, ByteRegionKind.Identity)]);
            var editor = new HexEditor();
            var window = new Window { Content = editor, Width = 1200, Height = 700 };
            window.Show();
            try
            {
                editor.Bind(target);
                Pump();
                window.UpdateLayout();
                Pump();
                var grid = Descendants<DataGrid>(editor).First(g => g.Name == "bytesGrid");
                var items = Descendants<TreeViewItem>(editor).ToList();
                var army = items.First(i => ((dynamic)i.DataContext).Label == "Army");
                var id = items.First(i => ((dynamic)i.DataContext).Label == "Id");

                Click(army);
                Assert.That(grid.SelectedCells, Has.Count.EqualTo(8), "the group selects its parts together");

                Click(id);
                Assert.That(grid.SelectedCells, Has.Count.EqualTo(2));

                Click(id);
                Assert.That(grid.SelectedCells, Has.Count.EqualTo(2), "clicking the same field again selects it again");

                Click(army);
                Click(army);
                Assert.That(grid.SelectedCells, Has.Count.EqualTo(8));
            }
            finally
            {
                window.Close();
            }
        }

        [Test]
        public void TheShadesOfAKindAreFarApartAndTheSelectionIsAColourNoKindUses()
        {
            foreach (var kind in Enum.GetValues<ByteRegionKind>())
            {
                var light = ((SolidColorBrush)HexPalette.GetBrush(kind, 0)).Color;
                var dark = ((SolidColorBrush)HexPalette.GetBrush(kind, 1)).Color;

                Assert.That(Luminance(light) - Luminance(dark), Is.GreaterThan(0.15), $"the two shades of {kind} are too close");
                Assert.That(Contrast(Colors.Black, dark), Is.GreaterThan(6), $"black text on the deeper shade of {kind}");
                Assert.That(light, Is.Not.EqualTo(((SolidColorBrush)HexPalette.SelectedBackground).Color));
            }

            Assert.That(Contrast(((SolidColorBrush)HexPalette.SelectedForeground).Color, ((SolidColorBrush)HexPalette.SelectedBackground).Color), Is.GreaterThan(5), "selected text");
        }

        private static double Luminance(Color color)
        {
            static double Channel(byte value)
            {
                var c = value / 255.0;
                return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
            }

            return (0.2126 * Channel(color.R)) + (0.7152 * Channel(color.G)) + (0.0722 * Channel(color.B));
        }

        private static double Contrast(Color first, Color second)
        {
            var (a, b) = (Luminance(first), Luminance(second));
            return (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05);
        }

        private static void Click(TreeViewItem item)
        {
            item.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonUpEvent,
                Source = item,
            });
            Pump();
        }

        private static void EnsureApplication()
        {
            if (Application.Current == null)
            {
                var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/Siniskalc.Wpf;component/Styles/SiniskalcStyles.xaml") });
            }
        }

        private static void Pump() => Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);

        private static IEnumerable<T> Descendants<T>(DependencyObject root)
            where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T match)
                {
                    yield return match;
                }

                foreach (var nested in Descendants<T>(child))
                {
                    yield return nested;
                }
            }
        }

        private sealed class FakeTarget(byte[] bytes, IReadOnlyList<ByteRegion> regions) : IByteEditTarget
        {
            public event EventHandler<ByteEditTargetChangedEventArgs>? Changed
            {
                add { }
                remove { }
            }

            public string Title => "Fake";

            public IReadOnlyList<ByteRegion> Regions => regions;

            public byte[] GetBytes() => (byte[])bytes.Clone();

            public byte[] GetOriginalBytes() => (byte[])bytes.Clone();

            public bool Update(Func<byte[], byte[]> edit) => false;

            public void Dispose()
            {
            }
        }
    }
}
