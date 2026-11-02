namespace Siniskalc.Homm3.Data;

using Siniskalc.Homm3.Entities;

public sealed record BuildingDefinition : EntityDefinition
{
    /// <summary>
    /// Gets the type of the building.
    /// </summary>
    public required BuildingType Type { get; init; }

    /// <summary>
    /// Gets the identifier of the faction that has the building, or <see langword="null"/> for a building every faction has but the ones in <see cref="Except"/>.
    /// </summary>
    public required string? Faction { get; init; }

    /// <summary>
    /// Gets the identifier of the building this one replaces when it is built, or <see langword="null"/> when it replaces none.
    /// </summary>
    public required string? Upgrades { get; init; }

    /// <summary>
    /// Gets the identifiers of the buildings that must be built first.
    /// </summary>
    public required string[] Requires { get; init; }

    /// <summary>
    /// Gets the identifiers of the factions that do not have the building, for a building of no faction.
    /// </summary>
    public required string[] Except { get; init; }

    /// <summary>
    /// Gets whether the game builds the building once its prerequisites are met.
    /// </summary>
    public required bool Automatic { get; init; }

    /// <summary>
    /// Gets the gold the building gives each day.
    /// </summary>
    public required int GoldIncome { get; init; }
}
