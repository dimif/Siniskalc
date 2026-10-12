namespace Siniskalc.Entities
{
    using System;

    /// <summary>
    /// The effects of adventure map buildings that a hero visited and that are still on it: they last until the next battle, or until the
    /// end of the week for the stables. The game keeps them as bits of three bytes of the hero (see <c>docs/KNOWLEDGE_BASE.md</c>).
    /// </summary>
    [Flags]
    public enum VisitedBuildingEffects
    {
        None = 0,
        Stables = 1,
        SwanPond = 2,
        WateringHole = 4,
        Crypt = 8,
        FaerieRing = 16,
        FountainOfFortune = 32,
        IdolOfFortune = 64,
    }
}
