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

    public ArtifactRole Role { get; init; }

    public bool Wearable { get; init; } = true;

    public bool Backpack { get; init; } = true;
}
