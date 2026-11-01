namespace Siniskalc.Homm3.Data;

using Siniskalc.Homm3.Entities;

public sealed record VisitEffectDefinition
{
    public required VisitedBuildingEffects Effect { get; init; }
    public required int Order { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required int Morale { get; init; }
    public required int Luck { get; init; }
    public required bool Editable { get; init; }
}
