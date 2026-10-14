namespace Siniskalc.Homm2.Models
{
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// A secondary skill a hero has and how well.
    /// </summary>
    public sealed record SkillEntry(SecondarySkill Skill, SkillLevel Level);
}
