namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Data;

    public sealed class Spell : CatalogEntity, ISpell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Spell"/> class.
        /// </summary>
        /// <param name="catalog">The catalog.</param>
        /// <param name="definition">The definition.</param>
        internal Spell(GameCatalog catalog, SpellDefinition definition)
            : base(catalog, definition)
        {
            this.SpellPoints = definition.SpellPoints;
            this.Type = definition.Type;
            this.Level = definition.Level;
            this.Group = catalog.SpellTypes[definition.Group];
        }

        /// <summary>
        /// Gets the spell points the spell costs.
        /// </summary>
        public int SpellPoints { get; }

        /// <summary>
        /// Gets whether the spell is cast in a battle, on the map or both.
        /// </summary>
        public SpellType Type { get; }

        /// <summary>
        /// Gets the level of the spell.
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// Gets the group the spell belongs to.
        /// </summary>
        public SpellGroup Group { get; }
    }
}
