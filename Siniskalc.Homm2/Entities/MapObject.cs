namespace Siniskalc.Homm2.Entities
{
    using System.Globalization;
    using Siniskalc.Homm2.Data;

    public sealed class MapObject : CatalogEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapObject"/> class.
        /// </summary>
        /// <param name="catalog">The catalog.</param>
        /// <param name="definition">The definition.</param>
        internal MapObject(GameCatalog catalog, MapObjectDefinition definition)
            : base(catalog, definition)
        {
            this.Role = definition.Role;
            this.NameFormat = definition.NameFormat;
        }

        /// <summary>
        /// Gets what the object is for the editor.
        /// </summary>
        public MapObjectRole Role { get; }

        /// <summary>
        /// Gets the format of the name of the object, which takes its position.
        /// </summary>
        public string NameFormat { get; }

        /// <summary>
        /// Gets the name of the object at a place.
        /// </summary>
        /// <param name="arguments">The values that fill the format, such as the column and the row.</param>
        /// <returns>The name.</returns>
        public string FormatName(params object[] arguments) => string.Format(CultureInfo.CurrentCulture, this.NameFormat, arguments);
    }
}
