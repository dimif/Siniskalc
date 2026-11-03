namespace Siniskalc.Homm3.Data;

public static class VisitedPlaceOffsets
{
    /// <summary>
    /// Gets where the visits to the places of a slot are kept in the chunk of a hero.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <returns>The offset from the start of the chunk.</returns>
    public static int For(VisitSlot slot) => slot switch
    {
        VisitSlot.HotaPrimarySkills => 1098,
        VisitSlot.HotaExperience => 1114,
        _ => 79 + ((int)slot * sizeof(uint)),
    };
}
