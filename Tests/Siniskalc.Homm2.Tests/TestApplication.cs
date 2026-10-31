namespace Siniskalc.Homm2.Tests
{
    /// <summary>
    /// Makes the application the windows need: one, with the styles of the shell and of the game merged in, as the exe does.
    /// </summary>
    internal static class TestApplication
    {
        public static void Ensure() => WpfTestApplication.Ensure("pack://application:,,,/Siniskalc.Homm2;component/Views/Generic.xaml");
    }
}
