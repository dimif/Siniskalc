namespace Siniskalc.Homm3.Data;

public sealed record SkillDefinition : EntityDefinition
{
    public SkillRole Role { get; init; }
}
