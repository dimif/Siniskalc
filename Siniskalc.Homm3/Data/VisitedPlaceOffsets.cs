namespace Siniskalc.Homm3.Data;

public static class VisitedPlaceOffsets
{
    public static int For(VisitSlot slot) => slot switch
    {
        VisitSlot.HotaPrimarySkills => 1098,
        VisitSlot.HotaExperience => 1114,
        _ => 79 + ((int)slot * sizeof(uint)),
    };
}
