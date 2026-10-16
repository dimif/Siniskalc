namespace Siniskalc.Homm2.Runtime
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// What a savegame says about the game besides its heroes, players and castles.
    /// </summary>
    /// <param name="Date">The date in the game.</param>
    /// <param name="Map">The map the game is played on.</param>
    /// <param name="Difficulty">The difficulty the game was started with.</param>
    public sealed record SaveMetadata(GameDate Date, MapHeader Map, GameDifficulty Difficulty);
}
