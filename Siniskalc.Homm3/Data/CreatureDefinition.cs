namespace Siniskalc.Homm3.Data;

using Siniskalc.Homm3.Entities;

public sealed record CreatureDefinition : EntityDefinition
{
    public required int Tier { get; init; }

    public required string Faction { get; init; }

    public required CreatureStatsDefinition Info { get; init; }

    public required string? BaseCreature { get; init; }

    public required string? Dwelling { get; init; }

    public System.Collections.Generic.Dictionary<GameVersion, CreatureStatsDefinition> VersionInfo { get; init; } = [];
}
