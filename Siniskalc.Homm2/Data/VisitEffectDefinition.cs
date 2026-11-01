namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;

    public sealed record VisitEffectDefinition : EntityDefinition
    {
        public required HeroEffects Flag { get; init; }

        public required VisitBehaviour Behaviour { get; init; }

        public required string Description { get; init; }

        public required int Morale { get; init; }

        public required int Luck { get; init; }
    }
}
