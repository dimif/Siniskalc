namespace Siniskalc.Homm3.Entities;

using System;
using Siniskalc.Core.Entities;
using Siniskalc.Homm3.Data;

/// <summary>An immutable entity belonging to exactly one resolved game catalog.</summary>
public abstract class CatalogEntity : Homm3Enumeration
{
    protected CatalogEntity(EntityCatalog catalog, EntityDefinition definition)
        : base(definition.Name, definition.Code, definition.AvailableFrom)
    {
        this.Catalog = catalog;
        this.Id = definition.Id;
        this.Order = definition.Order;
    }

    public EntityCatalog Catalog { get; }

    public string Id { get; }

    public int Order { get; }

    public override bool Equals(AbstractEnumeration? other) =>
        other is CatalogEntity entity && this.GetType() == entity.GetType()
        && ReferenceEquals(this.Catalog, entity.Catalog) && this.Id == entity.Id;

    public override int GetHashCode() => HashCode.Combine(this.GetType(), this.Catalog, this.Id);
}
