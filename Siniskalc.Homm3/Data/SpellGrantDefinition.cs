namespace Siniskalc.Homm3.Data;

public sealed record SpellGrantDefinition
{
    public required SpellGrantKind Kind { get; init; }
    public required string[] Spells { get; init; }
    public required string? School { get; init; }
    public required int? Level { get; init; }
    public required string Description { get; init; }
}
