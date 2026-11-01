namespace Siniskalc.Homm3.Data;

public sealed record FactionDefinition : EntityDefinition
{
    public required bool IsNeutral { get; init; }

    public required CompanionBitPolicy CompanionBits { get; init; }
}
