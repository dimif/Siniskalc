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
using Siniskalc.Wpf.Views;

namespace Siniskalc.Wpf.Tests.Views
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal sealed class LoggingSettingsWindowTests
    {
        [Test]
        public void TheWindowShowsTheLevelsTheFolderAndTheDefaultWithoutBindingErrors()
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
                var window = new LoggingSettingsWindow(new LoggingSettingsViewModel(store, Path.Combine(folder, "default"), new Mock<IFileDialogs>().Object));
                window.Show();

                try
                {
                    Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
                    window.UpdateLayout();

                    var levels = Descendants<ComboBox>(window).Single();
                    Assert.That(levels.Items, Has.Count.EqualTo(6));
                    Assert.That(levels.SelectedItem, Is.EqualTo(LogLevel.Debug));
                    Assert.That(Descendants<TextBox>(window).Single().Text, Is.EqualTo(Path.Combine(folder, "logs")));
                    Assert.That(Descendants<Button>(window).Select(b => b.Content), Is.EquivalentTo(new object[] { "Browse...", "Use default", "OK", "Cancel" }));
                    Assert.That(Descendants<TextBlock>(window).Any(t => t.Text.Contains(Path.Combine(folder, "default"))), Is.True);
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
