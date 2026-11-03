namespace Siniskalc.Heroes.ViewModels
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Runtime;

    /// <summary>
    /// The words the UI uses for who plays a colour (<see cref="PlayerType"/>).
    /// </summary>
    public static class PlayerTypeText
    {
        /// <summary>
        /// Returns who controls a colour as the words the editor shows.
        /// </summary>
        /// <param name="control">Who controls the colour, or <see langword="null"/> when that is not known.</param>
        /// <returns>The text.</returns>
        public static string Of(PlayerType? control) => control switch
        {
            PlayerType.Human => "Human",
            PlayerType.Computer => "Computer",
            _ => string.Empty,
        };

        /// <summary>
        /// Returns who plays a colour, followed by "current turn" when it is that colour's turn, or only that when it is not known who plays it.
        /// </summary>
        /// <param name="control">Who plays the colour.</param>
        /// <param name="isCurrent">Whether it is the turn of the colour.</param>
        /// <param name="game">The game.</param>
        /// <param name="color">The colour.</param>
        /// <returns>The text.</returns>
        public static string Describe(PlayerType? control, bool isCurrent) => TurnText.Beside(Of(control), isCurrent);

        /// <summary>
        /// Returns who plays <paramref name="color"/> in <paramref name="game"/>, followed by "current turn" when it is that colour's turn.
        /// </summary>
        /// <typeparam name="TColor">The colour of the game.</typeparam>
        /// <param name="game">The game.</param>
        /// <param name="color">The colour.</param>
        /// <param name="control">Who plays the colour.</param>
        /// <param name="isCurrent">Whether it is the turn of the colour.</param>
        /// <returns>The text.</returns>
        public static string Describe<TColor>(IGameInfo<TColor> game, TColor color)
            where TColor : class, IPlayerColor => Describe(game.GetControl(color), game.CurrentPlayer == color);
    }
}
