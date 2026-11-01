namespace Siniskalc.Homm3.Data;

using Siniskalc.Homm3.Entities;

public sealed record HeroClassDefinition : EntityDefinition
{
    public required string Faction { get; init; }
    public required HeroClassType Type { get; init; }
    public required bool Fallback { get; init; }
}
