namespace Siniskalc.Homm3.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using Siniskalc.Homm3.Data;

/// <summary>A validated, bounded spell-grant behaviour, resolved against its immutable catalog.</summary>
public sealed class SpellGrantProvider
{
    private readonly IReadOnlyList<Spell> spells;

    internal SpellGrantProvider(EntityCatalog catalog, SpellGrantDefinition definition)
    {
        this.Description = definition.Description;
        this.spells = catalog.Spells.All.Where(s => definition.Kind switch
        {
            SpellGrantKind.Named => definition.Spells.Contains(s.Id),
            SpellGrantKind.School => !s.HeroOnly && (s.School.Universal || s.School.Id == definition.School),
            SpellGrantKind.Level => !s.HeroOnly && s.Level == definition.Level,
            _ => false,
        }).ToArray().AsReadOnly();
    }

    public string Description { get; }

    public IReadOnlyCollection<Spell> Resolve(GameVersion version) => this.spells.Where(s => s.IsAvailableIn(version)).ToArray().AsReadOnly();

    public static SpellGrantProvider Of(params Spell[] spells)
    {
        if (spells.Length == 0)
        {
            throw new ArgumentException("A named grant requires at least one spell.", nameof(spells));
        }

        return new SpellGrantProvider(spells[0].Catalog, new SpellGrantDefinition
        {
            Kind = SpellGrantKind.Named, Spells = spells.Select(s => s.Id).ToArray(), School = null, Level = null,
            Description = string.Join(", ", spells.Select(s => s.Name)),
        });
    }

    public static SpellGrantProvider OfSchool(SpellSchool school) => new(school.Catalog, new SpellGrantDefinition
    {
        Kind = SpellGrantKind.School, Spells = [], School = school.Id, Level = null, Description = $"all {school.Name} spells",
    });

    public static SpellGrantProvider OfLevel(EntityCatalog catalog, int level) => new(catalog, new SpellGrantDefinition
    {
        Kind = SpellGrantKind.Level, Spells = [], School = null, Level = level, Description = $"all level {level} spells",
    });
}
