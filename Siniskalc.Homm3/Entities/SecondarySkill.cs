namespace Siniskalc.Homm3.Entities;

using Siniskalc.Homm3.Data;

public sealed class SecondarySkill : CatalogEntity, Siniskalc.Heroes.Entities.ISecondarySkill
{
    internal SecondarySkill(EntityCatalog catalog, SkillDefinition definition)
        : base(catalog, definition)
    {
        this.Role = definition.Role;
    }

    public SkillRole Role { get; }
}
