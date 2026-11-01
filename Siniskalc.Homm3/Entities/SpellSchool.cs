namespace Siniskalc.Homm3.Entities;

using Siniskalc.Homm3.Data;

public sealed class SpellSchool : CatalogEntity
{
    internal SpellSchool(EntityCatalog catalog, SchoolDefinition definition) : base(catalog, definition)
    {
        this.Description = definition.Description;
        this.Universal = definition.Universal;
    }

    public string Description { get; }
    public bool Universal { get; }
}
