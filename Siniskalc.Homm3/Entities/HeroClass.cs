namespace Siniskalc.Homm3.Entities;

using Siniskalc.Homm3.Data;

public sealed class HeroClass : CatalogEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="None"/> class.
    /// </summary>
    /// <param name="catalog">The catalog.</param>
    /// <param name="definition">The definition.</param>
    internal HeroClass(EntityCatalog catalog, HeroClassDefinition definition)
        : base(catalog, definition)
    {
        this.Faction = catalog.Factions.ById(definition.Faction);
        this.Type = definition.Type;
    }

    /// <summary>
    /// Gets the faction of the class.
    /// </summary>
    public Faction Faction { get; }

    /// <summary>
    /// Gets whether the class is a might class or a magic class.
    /// </summary>
    public HeroClassType Type { get; }
}
