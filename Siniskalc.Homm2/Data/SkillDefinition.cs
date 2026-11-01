namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;

    public sealed record SkillDefinition : EntityDefinition
    {
        public required SkillRole Role { get; init; }
    }
}
