namespace Siniskalc.Homm2.Tests
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;

    internal static class TestCatalog
    {
        public static GameCatalogs Catalogs { get; } = new(new JsonGameDefinitionReader());
        public static GameCatalog Standard { get; } = Catalogs.For(GameVersion.BaseGame);
    }
}
