namespace Siniskalc.Homm3.Data;

using Siniskalc.Homm3.Entities;

public sealed record ArtifactDefinition : EntityDefinition
{
    public required ArtifactSlots Slot { get; init; }
    public required ArtifactSlots BlockedSlots { get; init; }
    public required ArtifactEffectDefinition EffectInfo { get; init; }
    public required string Description { get; init; }
    public required bool IsCombination { get; init; }
    public required ArtifactClass Class { get; init; }
    public required int Cost { get; init; }
    public required string? CombinesInto { get; init; }
    public required ArtifactRole Role { get; init; }
    public required bool Wearable { get; init; }
    public required bool Backpack { get; init; }
}
