using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Siniskalc.Core.Runtime;
using Siniskalc.Wpf.ViewModels;
using Siniskalc.Wpf.Tests.ViewModels;
using Siniskalc.Wpf.Views;

namespace Siniskalc.Wpf.Tests.Views
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal sealed class SettingsWindowTests
    {
        [Test]
        public void TheWindowShowsTheSavingTheToolsAndTheLoggingWithoutBindingErrors()
        {
            var folder = Path.Combine(Path.GetTempPath(), "siniskalc-logging-window-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            var errors = new BindingErrors();
            PresentationTraceSources.Refresh();
            PresentationTraceSources.DataBindingSource.Listeners.Add(errors);
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;

            try
            {
                var store = new SettingsStore(Path.Combine(folder, "settings.json"));
                store.Settings.LogLevel = LogLevel.Debug;
                store.Settings.LogFolder = Path.Combine(folder, "logs");
                var shell = new ShellViewModel([new FakeGameModule("First Game", 0x11, "*.F1")], store, "Editor");
                var window = new SettingsWindow(new SettingsViewModel(shell, new LoggingSettingsViewModel(store, Path.Combine(folder, "default"), new Mock<IFileDialogs>().Object)));
                window.Show();

                try
                {
                    Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
                    window.UpdateLayout();

                    var levels = Descendants<ComboBox>(window).Single();
                    Assert.That(levels.Items, Has.Count.EqualTo(6));
                    Assert.That(levels.SelectedItem, Is.EqualTo(LogLevel.Debug));
                    Assert.That(Descendants<TextBox>(window).Single(t => t.TemplatedParent is not ComboBox).Text, Is.EqualTo(Path.Combine(folder, "logs")));
                    Assert.That(Descendants<Button>(window).Select(b => b.Content), Is.EquivalentTo(new object[] { "Browse...", "Use default", "OK", "Cancel" }));
                    Assert.That(Descendants<CheckBox>(window).Select(c => (c.Content, c.IsChecked)), Is.EqualTo(new[] { ((object)"Back up on save", (bool?)true), ("Advanced mode", false) }));
                    Assert.That(Descendants<TextBlock>(window).Select(t => t.Text), Is.SupersetOf(new[] { "Saving", "Tools", "Logging" }));
                    Assert.That(Descendants<TextBlock>(window).Any(t => t.Text.Contains(Path.Combine(folder, "default"))), Is.True);
                    var box = Descendants<TextBox>(window).Single(t => t.TemplatedParent is not ComboBox);
                    var label = Descendants<TextBlock>(window).Single(t => t.Text == "Log folder");
                    var browse = Descendants<Button>(window).Single(b => Equals(b.Content, "Browse..."));
                    var useDefault = Descendants<Button>(window).Single(b => Equals(b.Content, "Use default"));
                    Point TopLeft(FrameworkElement element) => element.TranslatePoint(new Point(0, 0), window);
                    Assert.That(box.ActualWidth, Is.GreaterThan(250), "the folder box takes the room of the row");
                    Assert.That(box.HorizontalContentAlignment, Is.EqualTo(HorizontalAlignment.Left));
                    Assert.That(TopLeft(label).X, Is.LessThan(TopLeft(box).X), "the label comes first");
                    Assert.That(TopLeft(box).X, Is.LessThan(TopLeft(browse).X));
                    Assert.That(TopLeft(browse).X, Is.LessThan(TopLeft(useDefault).X));
                    Assert.That(Math.Abs((TopLeft(box).Y + (box.ActualHeight / 2)) - (TopLeft(browse).Y + (browse.ActualHeight / 2))), Is.LessThan(3), "label, box and buttons are one row");
                    Assert.That(errors.Messages, Is.Empty);
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
        }

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

        private sealed class BindingErrors : TraceListener
        {
            public List<string> Messages { get; } = [];

            public override void Write(string? message)
            {
            }

            public override void WriteLine(string? message) => this.Messages.Add(message ?? string.Empty);
        }
    }
}
