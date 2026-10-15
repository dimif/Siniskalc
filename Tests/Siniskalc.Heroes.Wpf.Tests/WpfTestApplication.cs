namespace Siniskalc.Heroes.Wpf.Tests
{
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// Makes the application the controls need: one, with the styles of the pages merged in, the way a game's <c>Generic.xaml</c> does it.
    /// </summary>
    internal static class WpfTestApplication
    {
        private static readonly Uri PageStyles = new("pack://application:,,,/Siniskalc.Heroes.Wpf;component/Styles/PageStyles.xaml");

        public static void Ensure()
        {
            var application = Application.Current ?? new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            if (!application.Resources.MergedDictionaries.Any(d => d.Source == PageStyles))
            {
                application.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = PageStyles });
            }
        }

        /// <summary>
        /// Shows <paramref name="element"/> in a window, so its template and its triggers are applied; dispose the result to close it.
        /// </summary>
        public static IDisposable Show(FrameworkElement element)
        {
            var window = new Window { Content = element };
            window.Show();
            window.UpdateLayout();
            return new Hosted(window);
        }

        private sealed class Hosted(Window window) : IDisposable
        {
            public void Dispose() => window.Close();
        }

        public static IEnumerable<T> Descendants<T>(DependencyObject root)
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
    }
}
