namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Homm2.Data;

    public sealed class SpellGroup : CatalogEntity
    {
        internal SpellGroup(GameCatalog catalog, GroupDefinition definition)
            : base(catalog, definition)
        {
            this.Hidden = definition.Hidden;
        }

        public bool Hidden { get; }
    }
}
