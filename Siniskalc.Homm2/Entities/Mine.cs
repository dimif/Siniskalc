namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Data;

    public sealed class Mine : CatalogEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mine"/> class.
        /// </summary>
        /// <param name="catalog">The catalog.</param>
        /// <param name="definition">The definition.</param>
        internal Mine(GameCatalog catalog, MineDefinition definition)
            : base(catalog, definition)
        {
            this.Kind = definition.Kind;
            this.Resource = definition.Resource;
        }

        /// <summary>
        /// Gets the kind of the mine.
        /// </summary>
        public MineKind Kind { get; }

        /// <summary>
        /// Gets the resource the mine produces.
        /// </summary>
        public Resource Resource { get; }
    }
}
