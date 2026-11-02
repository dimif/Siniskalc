namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Data;

    public sealed class CreatureAbility : CatalogEntity, ICreatureAbility
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreatureAbility"/> class.
        /// </summary>
        /// <param name="catalog">The catalog.</param>
        /// <param name="definition">The definition.</param>
        internal CreatureAbility(GameCatalog catalog, AbilityDefinition definition)
            : base(catalog, definition)
        {
            this.IsRanged = definition.Id == "Ranged";
        }

        /// <summary>
        /// Gets whether the ability makes the creature shoot from a distance.
        /// </summary>
        public bool IsRanged { get; }
    }
}
