namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Data;

    public sealed class SecondarySkill : CatalogEntity, ISecondarySkill
    {
        internal SecondarySkill(GameCatalog catalog, SkillDefinition definition)
            : base(catalog, definition)
        {
        }
    }
}
