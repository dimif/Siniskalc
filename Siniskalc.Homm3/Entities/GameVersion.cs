namespace Siniskalc.Homm3.Entities
{
    [System.Text.Json.Serialization.JsonConverter(typeof(Data.GameVersionJsonConverter))]
    public enum GameVersion
    {
        Unknown,
        RestorationOfErathia,
        ArmageddonsBlade,
        ShadowOfDeath,
        HeroesChronicles,
        HornOfTheAbyss,
    }
}
