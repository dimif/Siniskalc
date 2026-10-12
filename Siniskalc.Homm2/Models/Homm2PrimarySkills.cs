namespace Siniskalc.Homm2.Models
{
    /// <summary>
    /// The attack, the defense, the spell power and the knowledge of a hero, as the savegame has them (what artifacts add is not in it).
    /// </summary>
    public sealed record Homm2PrimarySkills(int Attack, int Defense, int SpellPower, int Knowledge);
}
