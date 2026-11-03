using Siniskalc.Homm3.Entities;
namespace Siniskalc.Homm3.Tests
{
    /// <summary>
    /// Makes the application the windows need: one, with the styles of the shell and of the game merged in, as the exe does.
    /// </summary>
    internal static class TestApplication
    {
        public static void Ensure() => WpfTestApplication.Ensure("pack://application:,,,/Siniskalc.Homm3;component/Views/Generic.xaml");
    }
}
