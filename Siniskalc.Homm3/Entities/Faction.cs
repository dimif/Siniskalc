namespace Siniskalc.Homm3.Entities;

using Siniskalc.Homm3.Data;

public sealed class Faction : CatalogEntity, Siniskalc.Heroes.Entities.IFaction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="None"/> class.
    /// </summary>
    /// <param name="catalog">The catalog.</param>
    /// <param name="definition">The definition.</param>
    internal Faction(EntityCatalog catalog, FactionDefinition definition)
        : base(catalog, definition)
    {
        this.CompanionBits = definition.CompanionBits;
    }

    /// <summary>
    /// Gets whether this is the faction of a castle that has no known faction.
    /// </summary>
    public bool IsNeutral => this.Id == "Neutral";

    /// <summary>
    /// Gets which extra bits of the building masks the game sets together with a building of the faction.
    /// </summary>
    public CompanionBitPolicy CompanionBits { get; }
}
