namespace Siniskalc.Data
{
    using System;
    using System.Collections.Concurrent;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <inheritdoc />
    public sealed class VersionSpecificEntitiesProviderFactory : IVersionSpecificEntitiesProviderFactory
    {
        private readonly Func<GameVersion, IGrantedSpellsResolver> resolvers;
        private readonly ConcurrentDictionary<GameVersion, VersionSpecificEntitiesProvider> providers = new();

        /// <param name="resolvers">Makes the resolver of the spells artifacts grant in a game version.</param>
        public VersionSpecificEntitiesProviderFactory(Func<GameVersion, IGrantedSpellsResolver> resolvers)
        {
            this.resolvers = resolvers;
        }

        public VersionSpecificEntitiesProvider For(GameVersion gameVersion)
        {
            return this.providers.GetOrAdd(gameVersion, version => new VersionSpecificEntitiesProvider(version, this.resolvers(version)));
        }
    }
}
