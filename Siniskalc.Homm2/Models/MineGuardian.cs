namespace Siniskalc.Homm2.Models
{
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The guardian of a mine. An unrecognized encoding keeps its spell and count without pretending it is unguarded.
    /// </summary>
    /// <param name="SpellCode">The guardian spell saved in the record.</param>
    /// <param name="Count">The saved number of guardians.</param>
    /// <param name="Creature">The elemental, or <see langword="null"/> for an unrecognized encoding.</param>
    public sealed record MineGuardian(int SpellCode, int Count, Creature? Creature);
}
