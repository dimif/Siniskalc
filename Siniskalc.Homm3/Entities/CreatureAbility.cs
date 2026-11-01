namespace Siniskalc.Homm3.Entities;

using Siniskalc.Homm3.Data;

public sealed class CreatureAbility : CatalogEntity, Siniskalc.Heroes.Entities.ICreatureAbility
{
    internal CreatureAbility(EntityCatalog catalog, AbilityDefinition definition)
        : base(catalog, definition)
    {
        this.IsRanged = definition.Id == "Ranged";
    }

    public bool IsRanged { get; }
}
