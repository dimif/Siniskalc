namespace Siniskalc.Heroes.Runtime
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// What every game knows about a loaded savefile beyond its records: the date, the map, the difficulty, whose turn it is and who plays
    /// each colour. A fact the savefile does not hold is <c>null</c>.
    /// </summary>
    /// <typeparam name="TColor">The colour of the game.</typeparam>
    public interface IGameInfo<TColor>
        where TColor : class, IPlayerColor
    {
        /// <summary>
        /// Gets the date of the game, or <c>null</c> when the savefile does not hold it.
        /// </summary>
        GameDate? Date { get; }

        /// <summary>
        /// Gets the header of the map, or <c>null</c> when the savefile does not hold it.
        /// </summary>
        MapHeader? Map { get; }

        /// <summary>
        /// Gets the difficulty the game is played at, or <c>null</c> when the savefile does not hold it.
        /// </summary>
        GameDifficulty? Difficulty { get; }

        /// <summary>
        /// Gets the colour whose turn it is, or <c>null</c> when nobody's is known.
        /// </summary>
        TColor? CurrentPlayer { get; }

        /// <summary>
        /// Returns who plays a colour, or <c>null</c> when the savefile does not say.
        /// </summary>
        /// <param name="color">The colour.</param>
        PlayerType? GetControl(TColor color);
    }
}
