namespace Siniskalc.Heroes.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// The colours with player data or ownership of a hero or a used castle.
    /// </summary>
    public static class PlayersInGame
    {
        /// <summary>
        /// Returns each colour once, in code order, including Neutral when it owns something.
        /// </summary>
        /// <typeparam name="TColor">The colour of the game.</typeparam>
        /// <param name="players">The colours with player data.</param>
        /// <param name="heroOwners">The owners of the heroes.</param>
        /// <param name="castleOwners">The owners of the used castles.</param>
        /// <returns>The colours.</returns>
        public static IReadOnlyList<TColor> Of<TColor>(IEnumerable<TColor> players, IEnumerable<TColor> heroOwners, IEnumerable<TColor> castleOwners)
            where TColor : class, IPlayerColor =>
            players.Concat(heroOwners).Concat(castleOwners).Distinct().OrderBy(color => color.Code).ToList();
    }
}
