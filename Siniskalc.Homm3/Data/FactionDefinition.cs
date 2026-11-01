namespace Siniskalc.Homm3.Data;

public sealed record FactionDefinition : EntityDefinition
{
    public required CompanionBitPolicy CompanionBits { get; init; }
}
