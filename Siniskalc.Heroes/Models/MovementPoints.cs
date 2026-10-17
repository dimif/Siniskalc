namespace Siniskalc.Heroes.Models
{
    /// <summary>
    /// The movement points of a hero: what it has left today, and what a full day gives it.
    /// </summary>
    /// <param name="Current">The movement points left.</param>
    /// <param name="Max">The movement points of a full day, which the game works out from the army, the skills and the artifacts.</param>
    public readonly record struct MovementPoints(long Current, long Max);
}
