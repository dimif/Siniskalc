namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;

    public sealed class ArtifactEffect : CatalogEntity
    {
        internal ArtifactEffect(GameCatalog catalog, EffectDefinition definition)
            : base(catalog, definition)
        {
            this.Bonus = new PrimarySkillBonus(definition.Attack, definition.Defense, definition.Power, definition.Knowledge);
        }

        public PrimarySkillBonus Bonus { get; }
    }
}
