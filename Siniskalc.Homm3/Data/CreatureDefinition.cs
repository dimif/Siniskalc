namespace Siniskalc.Homm3.Data;

public sealed record CreatureDefinition : EntityDefinition
{
    public required int Tier { get; init; }

    public required string Faction { get; init; }

    public required CreatureStatsDefinition Info { get; init; }

    public required string? BaseCreature { get; init; }

    public required string? Dwelling { get; init; }

    public required bool Fallback { get; init; }
}
