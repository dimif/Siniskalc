namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Data;

    public sealed class CreatureAbility : CatalogEntity, ICreatureAbility
    {
        internal CreatureAbility(GameCatalog catalog, AbilityDefinition definition)
            : base(catalog, definition)
        {
            this.IsRanged = definition.IsRanged;
        }

        public bool IsRanged { get; }
    }
}
