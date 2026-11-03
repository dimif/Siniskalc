namespace Siniskalc.Homm3.Tests;

using Siniskalc.Homm3.Entities;

internal static class TestCatalog
{
    public static EntityCatalog For(GameVersion version) => TestServices.VersionEntities.For(version).Catalog;
    public static EntityCatalog Hota => For(GameVersion.HornOfTheAbyss);
    public static IReadOnlyDictionary<GameVersion, EntityCatalog> Catalogs => Enum.GetValues<GameVersion>().ToDictionary(v => v, For);
}
