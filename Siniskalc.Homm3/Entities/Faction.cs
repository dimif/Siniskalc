namespace Siniskalc.Homm3.Entities;

using Siniskalc.Homm3.Data;

public sealed class Faction : CatalogEntity, Siniskalc.Heroes.Entities.IFaction
{
    internal Faction(EntityCatalog catalog, FactionDefinition definition)
        : base(catalog, definition)
    {
        this.CompanionBits = definition.CompanionBits;
    }

    public bool IsNeutral => this.Id == "Neutral";

    public CompanionBitPolicy CompanionBits { get; }
}
