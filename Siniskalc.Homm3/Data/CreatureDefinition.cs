namespace Siniskalc.Homm3.Data;

using Siniskalc.Homm3.Entities;

public sealed record CreatureDefinition : EntityDefinition
{
    /// <summary>
    /// Gets the level of the creature in its faction.
    /// </summary>
    public required int Tier { get; init; }

    /// <summary>
    /// Gets the identifier of the faction of the creature.
    /// </summary>
    public required string Faction { get; init; }

    /// <summary>
    /// Gets the numbers the game keeps for the creature.
    /// </summary>
    public required CreatureStatsDefinition Info { get; init; }

    /// <summary>
    /// Gets the identifier of the creature this one is the upgrade of, or <see langword="null"/> when it is not an upgrade.
    /// </summary>
    public required string? BaseCreature { get; init; }

    /// <summary>
    /// Gets the identifier of the building that recruits the creature, or <see langword="null"/> for a creature no dwelling offers.
    /// </summary>
    public required string? Dwelling { get; init; }

    /// <summary>
    /// Gets the numbers of the versions of the game that differ from <see cref="Info"/>.
    /// </summary>
    public System.Collections.Generic.Dictionary<GameVersion, CreatureStatsDefinition> VersionInfo { get; init; } = [];
}
