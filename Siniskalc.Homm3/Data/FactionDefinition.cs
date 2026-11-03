namespace Siniskalc.Homm3.Data;

public sealed record FactionDefinition : EntityDefinition
{
    /// <summary>
    /// Gets which extra bits of the building masks the game sets together with a building of the faction.
    /// </summary>
    public required CompanionBitPolicy CompanionBits { get; init; }
}
