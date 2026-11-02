namespace Siniskalc.Homm3.Models
{
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Whose turn it is in a game: the savegame keeps the colour of that player after the human flags of the setup (see <c>TurnLayout</c>).
    /// </summary>
    /// <param name="Index">The place in the list of turns, which has only one.</param>
    /// <param name="Color">The colour whose turn it is, or <see cref="PlayerColor.Neutral"/> when the savegame holds no colour there.</param>
    public sealed record TurnModel(int Index, PlayerColor Color)
    {
        /// <summary>
        /// Gets the name of the turn as the pending changes show it.
        /// </summary>
        public string Name => "Current turn";
    }
}
