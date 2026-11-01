namespace Siniskalc.Homm3.Data;

using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

public sealed class VersionSpecificEntitiesProviderFactory : IVersionSpecificEntitiesProviderFactory
{
    private readonly FrozenDictionary<GameVersion, VersionSpecificEntitiesProvider> providers;

    public VersionSpecificEntitiesProviderFactory(IReadOnlyDictionary<GameVersion, EntityCatalog> catalogs)
    {
        this.providers = catalogs.ToFrozenDictionary(p => p.Key, p => new VersionSpecificEntitiesProvider(p.Value, new GrantedSpellsResolver(p.Key)));
    }

    public VersionSpecificEntitiesProvider For(GameVersion gameVersion) => this.providers[gameVersion];
}
