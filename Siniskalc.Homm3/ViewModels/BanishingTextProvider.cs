namespace Siniskalc.Homm3.ViewModels
{
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// The words the UI uses for a player who owns no town and is counting down to being banished.
    /// </summary>
    public sealed class BanishingTextProvider
    {
        /// <summary>
        /// Returns the pill of the player, such as "No towns: 6 days left" or "Banished", or nothing for a player who owns a town.
        /// </summary>
        public string Pill(PlayerModel player) => player switch
        {
            { IsBanished: true } => "Banished",
            { DaysLeft: { } days } => $"No towns: {Days(days)} left",
            _ => string.Empty,
        };

        /// <summary>
        /// Returns the short text beside the player in the navigation tree, such as "6 days left" or "banished", or nothing for a player who owns a town.
        /// </summary>
        public string Nav(PlayerModel player) => player switch
        {
            { IsBanished: true } => "banished",
            { DaysLeft: { } days } => $"{Days(days)} left",
            _ => string.Empty,
        };

        private static string Days(int days) => days == 1 ? "1 day" : $"{days} days";
    }
}
