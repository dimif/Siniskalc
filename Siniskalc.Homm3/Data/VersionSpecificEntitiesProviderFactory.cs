namespace Siniskalc.Homm3.Data;

using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

public sealed class VersionSpecificEntitiesProviderFactory : IVersionSpecificEntitiesProviderFactory
{
    private readonly FrozenDictionary<GameVersion, VersionSpecificEntitiesProvider> providers;

    /// <summary>
    /// Initializes a new instance of the <see cref="None"/> class.
    /// </summary>
    /// <param name="catalogs">The catalogs.</param>
    public VersionSpecificEntitiesProviderFactory(IReadOnlyDictionary<GameVersion, EntityCatalog> catalogs)
    {
        this.providers = catalogs.ToFrozenDictionary(p => p.Key, p => new VersionSpecificEntitiesProvider(p.Value, new GrantedSpellsResolver(p.Key)));
    }

    /// <summary>
    /// Gets the entities of a version of the game.
    /// </summary>
    /// <param name="gameVersion">The version.</param>
    /// <returns>The provider.</returns>
    public VersionSpecificEntitiesProvider For(GameVersion gameVersion) => this.providers[gameVersion];
}
