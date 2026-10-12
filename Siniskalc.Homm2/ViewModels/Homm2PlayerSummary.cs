namespace Siniskalc.Homm2.ViewModels
{
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// One line of the overview of a savegame: what a player has.
    /// </summary>
    public sealed record Homm2PlayerSummary(Homm2Color Color, long Gold, int Heroes, int Castles, string Status);
}
