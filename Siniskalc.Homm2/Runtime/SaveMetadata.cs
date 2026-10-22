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
    /// <param name="PlayerControls">Who plays each colour, Blue first, or <c>null</c> when not known. Whose turn it is is not here but in the
    /// turn record (<see cref="Game.CurrentPlayer"/>), which is the same byte and can be edited.</param>
    public sealed record SaveMetadata(GameDate Date, MapHeader Map, GameDifficulty Difficulty, IReadOnlyList<PlayerType>? PlayerControls = null);
}
