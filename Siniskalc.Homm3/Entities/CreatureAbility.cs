namespace Siniskalc.Homm3.Entities;

using Siniskalc.Homm3.Data;

public sealed class CreatureAbility : CatalogEntity, Siniskalc.Heroes.Entities.ICreatureAbility
{
    /// <summary>
    /// Initializes a new instance of the <see cref="None"/> class.
    /// </summary>
    /// <param name="catalog">The catalog.</param>
    /// <param name="definition">The definition.</param>
    internal CreatureAbility(EntityCatalog catalog, AbilityDefinition definition)
        : base(catalog, definition)
    {
        this.IsRanged = definition.Id == "Ranged";
    }

    /// <summary>
    /// Gets whether the ability makes the creature shoot from a distance.
    /// </summary>
    public bool IsRanged { get; }
}
