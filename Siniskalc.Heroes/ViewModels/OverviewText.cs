namespace Siniskalc.Heroes.ViewModels
{
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// The sentences the overview pages of the games share.
    /// </summary>
    public static class OverviewText
    {
        /// <summary>
        /// Returns the difficulty the game was started at, e.g. "Difficulty: Queen (160%)".
        /// </summary>
        public static string Difficulty(GameDifficulty difficulty) => $"Difficulty: {difficulty} ({(int)difficulty}%)";

        /// <summary>
        /// Returns the difficulty the map was made with, e.g. "Rating: Rook (130%)".
        /// </summary>
        public static string MapRating(GameDifficulty difficulty) => $"Rating: {difficulty} ({(int)difficulty}%)";

        /// <summary>
        /// Returns a count with its noun in the right number, e.g. "1 hero" or "2 heroes".
        /// </summary>
        public static string Count(int count, string singular, string plural) => $"{count} {(count == 1 ? singular : plural)}";
    }
}
