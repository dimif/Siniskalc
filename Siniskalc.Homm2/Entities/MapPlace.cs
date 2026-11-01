namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Homm2.Data;

    public sealed class MapPlace : CatalogEntity
    {
        internal MapPlace(GameCatalog catalog, PlaceDefinition definition)
            : base(catalog, definition)
        {
            this.Kind = definition.Kind;
            this.Description = definition.Description;
        }

        public PlaceKind Kind { get; }

        public string Description { get; }
    }
}
