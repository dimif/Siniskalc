namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The definition of what visiting a place leaves on a hero, as it is read from <c>visitEffects.json</c>.
    /// </summary>
    public sealed record VisitEffectDefinition : EntityDefinition
    {
        /// <summary>
        /// Gets the bit of the effects of a hero that says the hero remembers the visit.
        /// </summary>
        public required HeroEffects Flag { get; init; }

        /// <summary>
        /// Gets how long the effect lasts.
        /// </summary>
        public required VisitBehaviour Behaviour { get; init; }

        /// <summary>
        /// Gets what the visit does.
        /// </summary>
        public required string Description { get; init; }

        /// <summary>
        /// Gets what the visit adds to the morale.
        /// </summary>
        public required int Morale { get; init; }

        /// <summary>
        /// Gets what the visit adds to the luck.
        /// </summary>
        public required int Luck { get; init; }
    }
}
