namespace Siniskalc.Homm2.Runtime
{
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// What a savegame says about the game besides its heroes, players and castles.
    /// </summary>
    /// <param name="Date">The date in the game.</param>
    /// <param name="Map">The map the game is played on.</param>
    public sealed record Homm2SaveMetadata(Homm2GameDate Date, Homm2MapHeader Map);
}
