namespace Siniskalc.Homm3.Tests
{
    using System.Windows;

    /// <summary>
    /// Makes the application the windows need: one, with the styles of the shell and of the game merged in, as the exe does.
    /// </summary>
    internal static class TestApplication
    {
        public static void Ensure()
        {
            if (Application.Current != null)
            {
                return;
            }

            var application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            application.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/Siniskalc.Wpf;component/Styles/SiniskalcStyles.xaml") });
            application.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/Siniskalc.Homm3;component/Views/Generic.xaml") });
        }
    }
}
