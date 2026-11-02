namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Globalization;

    /// <summary>
    /// The words for what a dwelling of a castle gains each week, the same in every game.
    /// </summary>
    public static class WeeklyGrowthText
    {
        /// <summary>
        /// Returns what a dwelling gains each week, such as "+20 a week", or nothing when it gains nothing (a dwelling that is not built).
        /// </summary>
        /// <param name="growth">The creatures gained each week.</param>
        /// <returns>The text.</returns>
        public static string Of(int growth) => growth > 0 ? $"+{growth.ToString(CultureInfo.CurrentCulture)} a week" : string.Empty;
    }
}
