namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Core.Entities;
    using Siniskalc.Homm2.Data;

    /// <summary>A value owned by one immutable game catalog, never a global enumeration.</summary>
    public abstract class CatalogEntity : Homm2Enumeration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogEntity"/> class.
        /// </summary>
        /// <param name="catalog">The catalog that owns the entity.</param>
        /// <param name="definition">The definition the entity is made from.</param>
        /// <param name="availableFrom">The first version of the game that has the entity.</param>
        protected CatalogEntity(GameCatalog catalog, EntityDefinition definition, GameVersion availableFrom = GameVersion.BaseGame)
            : base(definition.Name, definition.Code, availableFrom)
        {
            this.Catalog = catalog;
            this.Id = definition.Id;
        }

        /// <summary>
        /// Gets the catalog that owns the entity.
        /// </summary>
        public GameCatalog Catalog { get; }

        /// <summary>
        /// Gets the identifier of the entity, which the other definitions refer to.
        /// </summary>
        public string Id { get; }

        /// <inheritdoc/>
        public override bool Equals(AbstractEnumeration? other) => ReferenceEquals(this, other);

        /// <inheritdoc/>
        public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
    }
}
