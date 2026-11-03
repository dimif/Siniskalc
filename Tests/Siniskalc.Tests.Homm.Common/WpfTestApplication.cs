namespace Siniskalc.Tests.Homm.Common
{
    using System.Windows;

    /// <summary>
    /// Makes the application the windows need: one, with the styles of the shell and the styles of the game merged in, as the exe does.
    /// </summary>
    public static class WpfTestApplication
    {
        private const string ShellStyles = "pack://application:,,,/Siniskalc.Wpf;component/Styles/SiniskalcStyles.xaml";

        /// <summary>
        /// Makes the application if there is none.
        /// </summary>
        /// <param name="gameStyles">The pack address of the resources of the game, such as <c>pack://application:,,,/Siniskalc.Homm3;component/Views/Generic.xaml</c>.</param>
        public static void Ensure(string gameStyles)
        {
            if (Application.Current != null)
            {
                return;
            }

            var application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            application.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(ShellStyles) });
            application.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(gameStyles) });
        }
    }
}
