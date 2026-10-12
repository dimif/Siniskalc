namespace Siniskalc.Models
{
    /// <summary>
    /// What a hero's equipped artifacts add to the four primary skills. It is worked out by the hero and is never saved.
    /// </summary>
    public readonly record struct PrimarySkillBonus(short Atk, short Def, short Pow, short Kno);
}
