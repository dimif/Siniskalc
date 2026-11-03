namespace Siniskalc.Homm3.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using Siniskalc.Homm3.Data;

/// <summary>A validated, bounded spell-grant behaviour, resolved against its immutable catalog.</summary>
public sealed class SpellGrantProvider
{
    private readonly IReadOnlyList<Spell> spells;

    /// <summary>
    /// Initializes a new instance of the <see cref="None"/> class.
    /// </summary>
    /// <param name="catalog">The catalog.</param>
    /// <param name="definition">The definition.</param>
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

    /// <summary>
    /// Gets the text that says what the artifact grants.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Makes a grant of named spells.
    /// </summary>
    /// <param name="spells">The spells.</param>
    /// <returns>The grant.</returns>
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

    /// <summary>
    /// Makes a grant of every spell of a school.
    /// </summary>
    /// <param name="school">The school.</param>
    /// <returns>The grant.</returns>
    public static SpellGrantProvider OfSchool(SpellSchool school) => new(school.Catalog, new SpellGrantDefinition
    {
        Kind = SpellGrantKind.School, Spells = [], School = school.Id, Level = null, Description = $"all {school.Name} spells",
    });

    /// <summary>
    /// Makes a grant of every spell of a level.
    /// </summary>
    /// <param name="catalog">The catalog of the spells.</param>
    /// <param name="level">The level.</param>
    /// <returns>The grant.</returns>
    public static SpellGrantProvider OfLevel(EntityCatalog catalog, int level) => new(catalog, new SpellGrantDefinition
    {
        Kind = SpellGrantKind.Level, Spells = [], School = null, Level = level, Description = $"all level {level} spells",
    });

    /// <summary>
    /// Gets the spells the grant gives.
    /// </summary>
    /// <param name="version">The version of the game.</param>
    /// <returns>The spells that exist in the version.</returns>
    public IReadOnlyCollection<Spell> Resolve(GameVersion version) => this.spells.Where(s => s.IsAvailableIn(version)).ToArray().AsReadOnly();
}
