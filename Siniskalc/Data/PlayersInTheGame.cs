namespace Siniskalc.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Entities;

    /// <summary>
    /// Tells which of the players found in a savefile are in the game, from the setup of the game. The scan that finds the players looks at
    /// their bytes only; this is the one place where the setup has a say.
    /// </summary>
    public static class PlayersInTheGame
    {
        /// <summary>
        /// Returns <paramref name="players"/> without the colours the setup says are not played, unless that leaves none.
        /// </summary>
        /// <param name="players">The players the loader found.</param>
        /// <param name="controls">Who plays each colour, or <c>null</c> when the setup of the game is not known: then every player stays.</param>
        public static IReadOnlyList<PlayerChunk> Of(IReadOnlyList<PlayerChunk> players, IReadOnlyList<PlayerType>? controls)
        {
            var kept = players.Where(player => !IsOutOfTheGame(player, controls)).ToList();
            return kept.Count > 0 ? kept.AsReadOnly() : players;
        }

        /// <summary>
        /// Gets whether the setup says the colour of the chunk is not played and the chunk has no heroes and no towns. Such a colour can still
        /// have gold (the colour Tan of TestData/Hota/tan_not_parsing has 10000), and is not a player.
        /// </summary>
        private static bool IsOutOfTheGame(PlayerChunk player, IReadOnlyList<PlayerType>? controls)
        {
            return controls != null
                && (int)player.Color < controls.Count
                && controls[(int)player.Color] == PlayerType.NotInGame
                && player.GetByteAt(PlayerLayout.HeroCount) == 0
                && player.GetByteAt(PlayerLayout.TownCount) == 0;
        }
    }
}
