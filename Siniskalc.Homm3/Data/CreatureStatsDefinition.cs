namespace Siniskalc.Homm3.Data;

public sealed record CreatureStatsDefinition
{
    /// <summary>
    /// Gets the attack.
    /// </summary>
    public required short Atk { get; init; }

    /// <summary>
    /// Gets the defense.
    /// </summary>
    public required short Def { get; init; }

    /// <summary>
    /// Gets the least damage of one creature.
    /// </summary>
    public required short MinDamage { get; init; }

    /// <summary>
    /// Gets the greatest damage of one creature.
    /// </summary>
    public required short MaxDamage { get; init; }

    /// <summary>
    /// Gets the hit points of one creature.
    /// </summary>
    public required short Health { get; init; }

    /// <summary>
    /// Gets the speed.
    /// </summary>
    public required short Speed { get; init; }

    /// <summary>
    /// Gets the creatures its dwelling adds each week.
    /// </summary>
    public required short Growth { get; init; }

    /// <summary>
    /// Gets the value the computer players give the creature.
    /// </summary>
    public required int AiValue { get; init; }

    /// <summary>
    /// Gets the value of the creature for the strength of an army.
    /// </summary>
    public required int FightValue { get; init; }

    /// <summary>
    /// Gets the price in gold.
    /// </summary>
    public required int Gold { get; init; }

    /// <summary>
    /// Gets the price in wood.
    /// </summary>
    public required short Wood { get; init; }

    /// <summary>
    /// Gets the price in ore.
    /// </summary>
    public required short Ore { get; init; }

    /// <summary>
    /// Gets the price in mercury.
    /// </summary>
    public required short Mercury { get; init; }

    /// <summary>
    /// Gets the price in sulfur.
    /// </summary>
    public required short Sulfur { get; init; }

    /// <summary>
    /// Gets the price in crystal.
    /// </summary>
    public required short Crystal { get; init; }

    /// <summary>
    /// Gets the price in gems.
    /// </summary>
    public required short Gems { get; init; }

    /// <summary>
    /// Gets the shots of a ranged creature, 0 for the others.
    /// </summary>
    public required short Shots { get; init; }

    /// <summary>
    /// Gets the identifiers of the abilities of the creature.
    /// </summary>
    public required string[] Special { get; init; }
}
