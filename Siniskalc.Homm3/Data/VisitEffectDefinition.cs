namespace Siniskalc.Homm3.Data;

using Siniskalc.Homm3.Entities;

public sealed record VisitEffectDefinition
{
    /// <summary>
    /// Gets the effect the visit leaves on a hero.
    /// </summary>
    public required VisitedBuildingEffects Effect { get; init; }

    /// <summary>
    /// Gets the name the editor shows.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets what the visit does.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets what the visit adds to the morale.
    /// </summary>
    public required int Morale { get; init; }

    /// <summary>
    /// Gets what the visit adds to the luck.
    /// </summary>
    public required int Luck { get; init; }

    /// <summary>
    /// Gets the movement points the visit adds, to the points left and to the most of the day, until the end of the week.
    /// </summary>
    public required int Movement { get; init; }

    /// <summary>
    /// Gets the movement points of the visit in the versions where they differ from <see cref="Movement"/>.
    /// </summary>
    public System.Collections.Generic.Dictionary<GameVersion, int> VersionMovement { get; init; } = [];

    /// <summary>
    /// Gets whether the editor lets you set the effect.
    /// </summary>
    public required bool Editable { get; init; }
}
