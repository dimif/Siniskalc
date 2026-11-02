namespace Siniskalc.Homm3.Data;

public sealed record SkillDefinition : EntityDefinition
{
    /// <summary>
    /// Gets what the skill is for the editor.
    /// </summary>
    public SkillRole Role { get; init; }
}
