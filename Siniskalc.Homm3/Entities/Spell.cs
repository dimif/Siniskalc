namespace Siniskalc.Homm3.Entities;

using Siniskalc.Heroes.Entities;
using Siniskalc.Homm3.Data;

public sealed class Spell : CatalogEntity, ISpell
{
    /// <summary>
    /// Initializes a new instance of the <see cref="None"/> class.
    /// </summary>
    /// <param name="catalog">The catalog.</param>
    /// <param name="definition">The definition.</param>
    internal Spell(EntityCatalog catalog, SpellDefinition definition)
        : base(catalog, definition)
    {
        this.School = catalog.SpellSchools.ById(definition.School);
        this.Level = definition.Level;
        this.Type = definition.Type;
        this.HeroOnly = definition.HeroOnly;
    }

    /// <summary>
    /// Gets the school of magic of the spell.
    /// </summary>
    public SpellSchool School { get; }

    /// <summary>
    /// Gets the level of the spell.
    /// </summary>
    public int Level { get; }

    /// <summary>
    /// Gets whether the spell is cast in a battle, on the map or both.
    /// </summary>
    public SpellType Type { get; }

    /// <summary>
    /// Gets whether only a hero can learn the spell.
    /// </summary>
    public bool HeroOnly { get; }

    /// <summary>
    /// Gets whether a Mage Guild can offer the spell.
    /// </summary>
    public bool CanBeInMageGuild => !this.HeroOnly;
}
