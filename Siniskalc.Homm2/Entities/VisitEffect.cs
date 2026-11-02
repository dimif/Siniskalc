namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Homm2.Data;

    public sealed class VisitEffect : CatalogEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VisitEffect"/> class.
        /// </summary>
        /// <param name="catalog">The catalog.</param>
        /// <param name="definition">The definition.</param>
        internal VisitEffect(GameCatalog catalog, VisitEffectDefinition definition)
            : base(catalog, definition)
        {
            this.Flag = definition.Flag;
            this.Behaviour = definition.Behaviour;
            this.Description = definition.Description;
            this.Morale = definition.Morale;
            this.Luck = definition.Luck;
        }

        /// <summary>
        /// Gets the bit of the effects of a hero that says the hero remembers the visit.
        /// </summary>
        public HeroEffects Flag { get; }

        /// <summary>
        /// Gets how long the effect lasts.
        /// </summary>
        public VisitBehaviour Behaviour { get; }

        /// <summary>
        /// Gets what the visit does.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets what the visit adds to the morale.
        /// </summary>
        public int Morale { get; }

        /// <summary>
        /// Gets what the visit adds to the luck.
        /// </summary>
        public int Luck { get; }
    }
}
