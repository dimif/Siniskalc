namespace Siniskalc.Homm2.Entities
{
    using System.Globalization;
    using Siniskalc.Homm2.Data;

    public sealed class MapObject : CatalogEntity
    {
        internal MapObject(GameCatalog catalog, MapObjectDefinition definition)
            : base(catalog, definition)
        {
            this.Role = definition.Role;
            this.NameFormat = definition.NameFormat;
        }

        public MapObjectRole Role { get; }

        public string NameFormat { get; }

        public string FormatName(params object[] arguments) => string.Format(CultureInfo.CurrentCulture, this.NameFormat, arguments);
    }
}
