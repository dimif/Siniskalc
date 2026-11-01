namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Homm2.Data;

    public sealed class VisitEffect : CatalogEntity
    {
        internal VisitEffect(GameCatalog catalog, VisitEffectDefinition definition)
            : base(catalog, definition)
        {
            this.Flag = definition.Flag;
            this.Behaviour = definition.Behaviour;
            this.Description = definition.Description;
            this.Morale = definition.Morale;
            this.Luck = definition.Luck;
        }

        public HeroEffects Flag { get; }

        public VisitBehaviour Behaviour { get; }

        public string Description { get; }

        public int Morale { get; }

        public int Luck { get; }
    }
}
