namespace Siniskalc.Homm3.Entities;

using Siniskalc.Homm3.Data;

public sealed class HeroClass : CatalogEntity
{
    internal HeroClass(EntityCatalog catalog, HeroClassDefinition definition)
        : base(catalog, definition)
    {
        this.Faction = catalog.Factions.ById(definition.Faction);
        this.Type = definition.Type;
    }

    public Faction Faction { get; }

    public HeroClassType Type { get; }
}
