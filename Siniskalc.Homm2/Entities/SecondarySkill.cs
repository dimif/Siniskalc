namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Data;

    public sealed class SecondarySkill : CatalogEntity, ISecondarySkill
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecondarySkill"/> class.
        /// </summary>
        /// <param name="catalog">The catalog.</param>
        /// <param name="definition">The definition.</param>
        internal SecondarySkill(GameCatalog catalog, SkillDefinition definition)
            : base(catalog, definition)
        {
        }
    }
}
