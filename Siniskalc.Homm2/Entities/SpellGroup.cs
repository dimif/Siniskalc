namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Homm2.Data;

    public sealed class SpellGroup : CatalogEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpellGroup"/> class.
        /// </summary>
        /// <param name="catalog">The catalog.</param>
        /// <param name="definition">The definition.</param>
        internal SpellGroup(GameCatalog catalog, SpellTypeDefinition definition)
            : base(catalog, definition)
        {
            this.Hidden = definition.Hidden;
        }

        /// <summary>
        /// Gets whether the group is the one of the spells the editor does not list.
        /// </summary>
        public bool Hidden { get; }
    }
}
