namespace Siniskalc.Homm3.Data;

public sealed record ArtifactEffectDefinition
{
    public required short Atk { get; init; }

    public required short Def { get; init; }

    public required short Pow { get; init; }

    public required short Kno { get; init; }

    public required short Luck { get; init; }

    public required short Morale { get; init; }

    public required short LandMovement { get; init; }

    public required short WaterMovement { get; init; }

    public required SpellGrantDefinition[] SpellGrants { get; init; }

    public required bool GrantsSpellBook { get; init; }

    public required short Gold { get; init; }

    public required short Ore { get; init; }

    public required short Wood { get; init; }

    public required short Crystal { get; init; }

    public required short Gems { get; init; }

    public required short Mercury { get; init; }

    public required short Sulfur { get; init; }

    public required string Other { get; init; }
}
