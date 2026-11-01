namespace Siniskalc.Homm3.Data;

public sealed record CreatureStatsDefinition
{
    public required short Atk { get; init; }

    public required short Def { get; init; }

    public required short MinDamage { get; init; }

    public required short MaxDamage { get; init; }

    public required short Health { get; init; }

    public required short Speed { get; init; }

    public required short Growth { get; init; }

    public required int AiValue { get; init; }

    public required int FightValue { get; init; }

    public required int Gold { get; init; }

    public required short Wood { get; init; }

    public required short Ore { get; init; }

    public required short Mercury { get; init; }

    public required short Sulfur { get; init; }

    public required short Crystal { get; init; }

    public required short Gems { get; init; }

    public required short Shots { get; init; }

    public required string[] Special { get; init; }
}
