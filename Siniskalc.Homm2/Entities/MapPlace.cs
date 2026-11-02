namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Homm2.Data;

    public sealed class MapPlace : CatalogEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapPlace"/> class.
        /// </summary>
        /// <param name="catalog">The catalog.</param>
        /// <param name="definition">The definition.</param>
        internal MapPlace(GameCatalog catalog, PlaceDefinition definition)
            : base(catalog, definition)
        {
            this.Kind = definition.Kind;
            this.Description = definition.Description;
        }

        /// <summary>
        /// Gets the kind of the place.
        /// </summary>
        public PlaceKind Kind { get; }

        /// <summary>
        /// Gets what the place gives.
        /// </summary>
        public string Description { get; }
    }
}
