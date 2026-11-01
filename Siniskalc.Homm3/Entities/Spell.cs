namespace Siniskalc.Homm3.Entities;

using Siniskalc.Heroes.Entities;
using Siniskalc.Homm3.Data;

public sealed class Spell : CatalogEntity, ISpell
{
    internal Spell(EntityCatalog catalog, SpellDefinition definition)
        : base(catalog, definition)
    {
        this.School = catalog.SpellSchools.ById(definition.School);
        this.Level = definition.Level;
        this.Type = definition.Type;
        this.HeroOnly = definition.HeroOnly;
    }

    public SpellSchool School { get; }

    public int Level { get; }

    public SpellType Type { get; }

    public bool HeroOnly { get; }

    public bool CanBeInMageGuild => !this.HeroOnly;
}
