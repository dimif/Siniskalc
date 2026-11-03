namespace Siniskalc.Homm3.Entities;

using System;
using Siniskalc.Core.Entities;
using Siniskalc.Homm3.Data;

/// <summary>An immutable entity belonging to exactly one resolved game catalog.</summary>
public abstract class CatalogEntity : Homm3Enumeration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="None"/> class.
    /// </summary>
    /// <param name="catalog">The catalog.</param>
    /// <param name="definition">The definition.</param>
    protected CatalogEntity(EntityCatalog catalog, EntityDefinition definition)
        : base(definition.Name, definition.Code, definition.AvailableFrom)
    {
        this.Catalog = catalog;
        this.Id = definition.Id;
    }

    /// <summary>
    /// Gets the catalog that owns the entity.
    /// </summary>
    public EntityCatalog Catalog { get; }

    /// <summary>
    /// Gets the identifier of the entity, which the other definitions refer to.
    /// </summary>
    public string Id { get; }

    /// <inheritdoc/>
    public override bool Equals(AbstractEnumeration? other) =>
        other is CatalogEntity entity && this.GetType() == entity.GetType()
        && ReferenceEquals(this.Catalog, entity.Catalog) && this.Id == entity.Id;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(this.GetType(), this.Catalog, this.Id);
}
