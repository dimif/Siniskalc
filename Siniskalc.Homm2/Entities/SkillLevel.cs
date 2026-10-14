namespace Siniskalc.Homm2.Entities
{
    /// <summary>
    /// How well a hero has a secondary skill. The savegame keeps it as a number from 0 (none) to 3.
    /// </summary>
    public enum SkillLevel : byte
    {
        None = 0,
        Basic = 1,
        Advanced = 2,
        Expert = 3,
    }
}
