namespace Siniskalc.Homm3.Data;

using System.Collections.Generic;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

public sealed class VersionSpecificEntitiesProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="None"/> class.
    /// </summary>
    /// <param name="catalog">The catalog.</param>
    /// <param name="grantedSpellsResolver">The granted spells resolver.</param>
    public VersionSpecificEntitiesProvider(EntityCatalog catalog, IGrantedSpellsResolver grantedSpellsResolver)
    {
        this.Catalog = catalog;
        this.GrantedSpellsResolver = grantedSpellsResolver;
        this.CreatureCodes = new CreatureCodes(catalog);
    }

    /// <summary>
    /// Gets the catalog of the version of the game.
    /// </summary>
    public EntityCatalog Catalog { get; }

    /// <summary>
    /// Gets the version of the game.
    /// </summary>
    public GameVersion GameVersion => this.Catalog.Version;

    /// <summary>
    /// Gets what works out the spells the artifacts a hero wears grant.
    /// </summary>
    public IGrantedSpellsResolver GrantedSpellsResolver { get; }

    /// <summary>
    /// Gets what turns the code of a creature into the creature and back.
    /// </summary>
    public CreatureCodes CreatureCodes { get; }

    /// <summary>
    /// Gets the secondary skills of the version.
    /// </summary>
    public IReadOnlyList<SecondarySkill> SecondarySkills => this.Catalog.SecondarySkills.All;

    /// <summary>
    /// Gets the war machines of the version.
    /// </summary>
    public IReadOnlyList<WarMachine> WarMachines => this.Catalog.WarMachines.All;

    /// <summary>
    /// Gets the spells of the version.
    /// </summary>
    public IReadOnlyList<Spell> Spells => this.Catalog.Spells.All;
}
