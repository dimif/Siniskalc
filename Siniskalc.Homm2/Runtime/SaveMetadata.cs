namespace Siniskalc.Homm2.Runtime
{
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
    /// <param name="CurrentPlayer">The colour whose turn it was, or <c>null</c>: the savegames do not say yet where the game keeps it (see HOMM2_PLAN.md, test 2).</param>
    public sealed record SaveMetadata(GameDate Date, MapHeader Map, GameDifficulty Difficulty, Color? CurrentPlayer = null);
}
