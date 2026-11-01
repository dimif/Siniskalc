namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Core.Entities;
    using Siniskalc.Homm2.Data;

    /// <summary>A value owned by one immutable game catalog, never a global enumeration.</summary>
    public abstract class CatalogEntity : Homm2Enumeration
    {
        protected CatalogEntity(GameCatalog catalog, EntityDefinition definition, GameVersion availableFrom = GameVersion.BaseGame)
            : base(definition.Name, definition.Code, availableFrom)
        {
            this.Catalog = catalog;
            this.Id = definition.Id;
            this.Order = definition.Order;
        }

        public GameCatalog Catalog { get; }

        public string Id { get; }

        public int Order { get; }

        public override bool Equals(AbstractEnumeration? other) => ReferenceEquals(this, other);

        public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
    }
}
