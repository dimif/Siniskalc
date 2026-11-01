namespace Siniskalc.Homm3.Data;

public sealed record SkillDefinition : EntityDefinition
{
    public required SkillRole Role { get; init; }
}
