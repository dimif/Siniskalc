namespace Siniskalc.Homm3.Data;

using Siniskalc.Heroes.Entities;

public sealed record SpellDefinition : EntityDefinition
{
    public required string School { get; init; }

    public required int Level { get; init; }

    public required SpellType Type { get; init; }

    public bool HeroOnly { get; init; }
}
