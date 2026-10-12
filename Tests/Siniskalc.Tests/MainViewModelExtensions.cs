namespace Siniskalc.Tests
{
    using Siniskalc.Runtime;
    using Siniskalc.ViewModels;

    /// <summary>
    /// Lets a test read what the shell shows without a null-forgiving operator: a missing page or game fails the test.
    /// </summary>
    internal static class MainViewModelExtensions
    {
        public static T CurrentPageOf<T>(this MainViewModel main)
            where T : class
        {
            return main.CurrentPage as T ?? throw new AssertionException($"The current page is {main.CurrentPage?.GetType().Name ?? "none"}, not {typeof(T).Name}.");
        }

        public static HeroViewModel SelectedHero(this MainViewModel main)
        {
            return main.SelectedHeroViewModel ?? throw new AssertionException("No hero is selected.");
        }

        public static OverviewViewModel LoadedOverview(this MainViewModel main)
        {
            return main.Overview ?? throw new AssertionException("No overview is shown.");
        }

        public static Game LoadedGame(this MainViewModel main)
        {
            return main.CurrentGame ?? throw new AssertionException("No game is loaded.");
        }
    }
}
