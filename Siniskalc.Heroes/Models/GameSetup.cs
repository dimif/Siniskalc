namespace Siniskalc.Heroes.Models
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// How a game was set up when it was started, in any game of the series: who plays each colour, the difficulty, and whose turn it is.
    /// </summary>
    /// <typeparam name="TColor">The colour of the game.</typeparam>
    /// <param name="PlayerControls">Who plays each colour, indexed by the code of the colour; empty when the savefile does not say.</param>
    /// <param name="Difficulty">The difficulty the game is played at.</param>
    /// <param name="CurrentPlayer">The colour whose turn it is, or <c>null</c> when the savefile does not tell.</param>
    public sealed record GameSetup<TColor>(IReadOnlyList<PlayerType> PlayerControls, GameDifficulty Difficulty, TColor? CurrentPlayer = null)
        where TColor : class, IPlayerColor
    {
        /// <summary>
        /// Returns who plays <paramref name="color"/>, or <c>null</c> when the setup does not say.
        /// </summary>
        /// <param name="color">The colour.</param>
        public PlayerType? GetControl(TColor color) => color.Code < this.PlayerControls.Count ? this.PlayerControls[color.Code] : null;
    }
}
