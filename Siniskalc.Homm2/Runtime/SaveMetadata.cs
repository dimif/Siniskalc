namespace Siniskalc.Homm2.Runtime
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// What a savegame says about the game besides its heroes, players and castles.
    /// </summary>
    /// <param name="Date">The date in the game.</param>
    /// <param name="Map">The map the game is played on.</param>
    /// <param name="Difficulty">The difficulty the game was started with.</param>
    /// <param name="CurrentPlayer">The colour whose turn it was, or <c>null</c> when the savegame does not say.</param>
    /// <param name="PlayerControls">Who plays each colour, Blue first, or <c>null</c> when not known.</param>
    public sealed record SaveMetadata(GameDate Date, MapHeader Map, GameDifficulty Difficulty, Color? CurrentPlayer = null, IReadOnlyList<PlayerType>? PlayerControls = null);
}
