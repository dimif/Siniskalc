namespace Siniskalc.Homm3.Data;

public sealed record AbilityDefinition : EntityDefinition
{
    public required bool IsRanged { get; init; }
}
