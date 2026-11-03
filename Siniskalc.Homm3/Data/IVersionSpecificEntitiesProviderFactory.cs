namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Gives the <see cref="VersionSpecificEntitiesProvider"/> for a game version.
    /// </summary>
    public interface IVersionSpecificEntitiesProviderFactory
    {
        VersionSpecificEntitiesProvider For(GameVersion gameVersion);
    }
}
