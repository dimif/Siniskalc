namespace Siniskalc.Homm3.Entities;

using Siniskalc.Homm3.Data;

public sealed class SpellSchool : CatalogEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="None"/> class.
    /// </summary>
    /// <param name="catalog">The catalog.</param>
    /// <param name="definition">The definition.</param>
    internal SpellSchool(EntityCatalog catalog, SchoolDefinition definition)
        : base(catalog, definition)
    {
        this.Description = definition.Description;
    }

    /// <summary>
    /// Gets the text that says what the school is.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets whether this is the universal spell school.
    /// </summary>
    public bool Universal => this.Id == "all";
}
