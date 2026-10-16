using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Moq;
using Siniskalc.Core.Runtime;
using Siniskalc.Wpf.Tests.ViewModels;
using Siniskalc.Wpf.ViewModels;
using Siniskalc.Wpf.Views;

namespace Siniskalc.Wpf.Tests.Views
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal sealed class ShellWindowTests
    {
        [Test]
        public void TheWindowShowsTheTitleOfTheGameAndEveryIconOfItsToolbarAndMenuLoads()
        {
            var folder = Path.Combine(Path.GetTempPath(), "siniskalc-shell-window-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            var errors = new BindingErrorListener();
            PresentationTraceSources.Refresh();
            PresentationTraceSources.DataBindingSource.Listeners.Add(errors);
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;

            try
            {
                EnsureApplication();
                var settings = new SettingsStore(Path.Combine(folder, "settings.json"));
                var recent = new RecentFilesStore(settings);
                var shell = new ShellViewModel([new FakeGameModule("First Game", 0x11, "*.F1")], settings, "Editor");
                var window = new ShellWindow(
                    new RecentFilesMenuViewModel(recent),
                    new ShellUrlLauncher(_ => { }),
                    new AboutInfo("Editor", "1.0", "A test.", "https://example.org", "https://example.org/releases", "MIT", []),
                    new HelpLinks("https://example.org/guide", "https://example.org/issues"),
                    SilentMessages(),
                    new SavegameFileCommands(recent, new Mock<IFileDialogs>().Object, SilentMessages())) { DataContext = shell };
                window.Show();

                try
                {
                    Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
                    window.UpdateLayout();

                    Assert.That(window.Title, Is.EqualTo("Editor - First Game"));

                    var images = Descendants<Image>(window).ToList();
                    Assert.That(images, Is.Not.Empty);
                    foreach (var image in images)
                    {
                        Assert.That(image.Source, Is.InstanceOf<BitmapSource>());
                        Assert.That(((BitmapSource)image.Source).PixelWidth, Is.GreaterThan(0));
                    }

                    // The icons of the menus are not in the visual tree until a menu opens: they are in the items.
                    var menuIcons = Descendants<Menu>(window).Single().Items.OfType<MenuItem>().SelectMany(Subitems).Select(m => m.Icon).OfType<Image>().ToList();
                    Assert.That(menuIcons, Is.Not.Empty);
                    foreach (var image in menuIcons)
                    {
                        Assert.That(((BitmapSource)image.Source).PixelWidth, Is.GreaterThan(0));
                    }
                }
                finally
                {
                    window.Close();
                }
            }
            finally
            {
                PresentationTraceSources.DataBindingSource.Listeners.Remove(errors);
                Directory.Delete(folder, recursive: true);
            }

            Assert.That(errors.Messages, Is.Empty, string.Join(Environment.NewLine, errors.Messages));
        }

        private static void EnsureApplication()
        {
            if (Application.Current == null)
            {
                var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/Siniskalc.Wpf;component/Styles/SiniskalcStyles.xaml") });
            }
        }

        private static IEnumerable<MenuItem> Subitems(MenuItem item) => item.Items.OfType<MenuItem>().SelectMany(Subitems).Prepend(item);

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

        private sealed class BindingErrorListener : TraceListener
        {
            public List<string> Messages { get; } = [];

            public override void Write(string? message)
            {
            }

            public override void WriteLine(string? message) => this.Messages.Add(message ?? string.Empty);
        }

        private static IMessageService SilentMessages()
        {
            var messages = new Mock<IMessageService>();
            messages.Setup(m => m.AskYesNoCancel(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageAnswer.No);
            return messages.Object;
        }
    }
}
