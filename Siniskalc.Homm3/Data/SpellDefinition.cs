namespace Siniskalc.Homm3.Data;

using Siniskalc.Heroes.Entities;

public sealed record SpellDefinition : EntityDefinition
{
    /// <summary>
    /// Gets the identifier of the school of the spell.
    /// </summary>
    public required string School { get; init; }

    /// <summary>
    /// Gets the level of the spell.
    /// </summary>
    public required int Level { get; init; }

    /// <summary>
    /// Gets whether the spell is cast in a battle, on the map or both.
    /// </summary>
    public required SpellType Type { get; init; }

    /// <summary>
    /// Gets whether only a hero can learn the spell: a Mage Guild does not offer it.
    /// </summary>
    public bool HeroOnly { get; init; }
}
