namespace Siniskalc.Homm3.Data;

public sealed record SpellGrantDefinition
{
    /// <summary>
    /// Gets how the spells are chosen: by name, by school or by level.
    /// </summary>
    public required SpellGrantKind Kind { get; init; }

    /// <summary>
    /// Gets the identifiers of the spells, for a grant by name.
    /// </summary>
    public required string[] Spells { get; init; }

    /// <summary>
    /// Gets the identifier of the school, for a grant by school.
    /// </summary>
    public required string? School { get; init; }

    /// <summary>
    /// Gets the level, for a grant by level.
    /// </summary>
    public required int? Level { get; init; }

    /// <summary>
    /// Gets the text that says what the artifact grants.
    /// </summary>
    public required string Description { get; init; }
}
