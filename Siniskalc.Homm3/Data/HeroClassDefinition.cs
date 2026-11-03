namespace Siniskalc.Homm3.Data;

using Siniskalc.Homm3.Entities;

public sealed record HeroClassDefinition : EntityDefinition
{
    /// <summary>
    /// Gets the identifier of the faction of the class.
    /// </summary>
    public required string Faction { get; init; }

    /// <summary>
    /// Gets whether the class is a might class or a magic class.
    /// </summary>
    public required HeroClassType Type { get; init; }
}
