namespace Siniskalc.Heroes.ViewModels
{
    /// <summary>
    /// The words for whose turn it is in a game, on the overview and next to the name of a player.
    /// </summary>
    public static class TurnText
    {
        /// <summary>
        /// Returns the line of the overview, such as "Current turn: Red", or nothing when the savegame does not say whose turn it is.
        /// </summary>
        public static string Overview(string? player) => string.IsNullOrEmpty(player) ? string.Empty : $"Current turn: {player}";

        /// <summary>
        /// Returns what is told next to the name of a player: who plays it (such as "Human"), followed by "current turn" when it is that player's turn,
        /// or only that when it is not known who plays it.
        /// </summary>
        public static string Beside(string control, bool isCurrent) =>
            control + (isCurrent ? (control.Length == 0 ? "Current turn" : " · current turn") : string.Empty);
    }
}
