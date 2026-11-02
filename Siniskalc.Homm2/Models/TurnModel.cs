namespace Siniskalc.Homm2.Models
{
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Whose turn it is in a game: the savegame keeps the number of the record of the player in the table of players (the humans come first).
    /// </summary>
    /// <param name="Index">The place in the list of turns, which has only one.</param>
    /// <param name="Record">The number of the record of the player whose turn it is, from 0.</param>
    /// <param name="Color">The colour of that player, or <c>null</c> when the number is no record.</param>
    public sealed record TurnModel(int Index, int Record, PlayerColor? Color)
    {
        /// <summary>
        /// Gets the name the editor shows for whose turn it is.
        /// </summary>
        public string Name => "Current turn";
    }
}
