namespace Siniskalc.Models
{
    using System.Collections.Generic;
    using Siniskalc.Entities;

    /// <summary>
    /// How the game was set up when it was started: who plays each colour and the difficulty.
    /// </summary>
    /// <param name="PlayerControls">Who plays each colour, indexed by <see cref="PlayerColor"/>.</param>
    /// <param name="Difficulty">The difficulty level.</param>
    /// <param name="CurrentPlayer">The colour whose turn it is, or <c>null</c> when the savefile does not tell.</param>
    public sealed record GameSetup(IReadOnlyList<PlayerType> PlayerControls, GameDifficulty Difficulty, PlayerColor? CurrentPlayer = null);
}
