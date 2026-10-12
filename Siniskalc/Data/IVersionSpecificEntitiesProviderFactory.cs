namespace Siniskalc.Data
{
    using Siniskalc.Entities;

    /// <summary>
    /// Gives the <see cref="VersionSpecificEntitiesProvider"/> for a game version.
    /// </summary>
    public interface IVersionSpecificEntitiesProviderFactory
    {
        VersionSpecificEntitiesProvider For(GameVersion gameVersion);
    }
}
