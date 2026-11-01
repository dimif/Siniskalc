namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Data;

    public sealed class Mine : CatalogEntity
    {
        internal Mine(GameCatalog catalog, MineDefinition definition)
            : base(catalog, definition)
        {
            this.Kind = definition.Kind;
            this.Resource = definition.Resource;
        }

        public MineKind Kind { get; }

        public Resource Resource { get; }
    }
}
