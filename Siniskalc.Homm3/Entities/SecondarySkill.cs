namespace Siniskalc.Homm3.Entities;

using Siniskalc.Homm3.Data;

public sealed class SecondarySkill : CatalogEntity, Siniskalc.Heroes.Entities.ISecondarySkill
{
    /// <summary>
    /// Initializes a new instance of the <see cref="None"/> class.
    /// </summary>
    /// <param name="catalog">The catalog.</param>
    /// <param name="definition">The definition.</param>
    internal SecondarySkill(EntityCatalog catalog, SkillDefinition definition)
        : base(catalog, definition)
    {
        this.Role = definition.Role;
    }

    /// <summary>
    /// Gets what the skill is for the editor.
    /// </summary>
    public SkillRole Role { get; }
}
