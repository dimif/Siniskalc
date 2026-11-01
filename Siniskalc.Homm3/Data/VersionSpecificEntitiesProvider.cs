namespace Siniskalc.Homm3.Data;

using System.Collections.Generic;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

public sealed class VersionSpecificEntitiesProvider
{
    public VersionSpecificEntitiesProvider(EntityCatalog catalog, IGrantedSpellsResolver grantedSpellsResolver)
    {
        this.Catalog = catalog;
        this.GrantedSpellsResolver = grantedSpellsResolver;
        this.CreatureCodes = new CreatureCodes(catalog);
    }

    public EntityCatalog Catalog { get; }

    public GameVersion GameVersion => this.Catalog.Version;

    public IGrantedSpellsResolver GrantedSpellsResolver { get; }

    public CreatureCodes CreatureCodes { get; }

    public IReadOnlyList<SecondarySkill> SecondarySkills => this.Catalog.SecondarySkills.All;

    public IReadOnlyList<WarMachine> WarMachines => this.Catalog.WarMachines.All;

    public IReadOnlyList<Spell> Spells => this.Catalog.Spells.All;
}
