namespace Siniskalc.Homm3.Data;

using Siniskalc.Homm3.Entities;

public sealed record BuildingDefinition : EntityDefinition
{
    public required BuildingType Type { get; init; }

    public required string? Faction { get; init; }

    public required string? Upgrades { get; init; }

    public required string[] Requires { get; init; }

    public required string[] Except { get; init; }

    public required bool Automatic { get; init; }

    public required int GoldIncome { get; init; }
}
