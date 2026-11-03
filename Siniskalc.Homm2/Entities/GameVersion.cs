namespace Siniskalc.Homm2.Entities
{
    /// <summary>
    /// The versions of Heroes of Might and Magic II a savegame can come from. Also the availability tier of an entity.
    /// </summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(Data.GameVersionJsonConverter))]
    public enum GameVersion
    {
        Unknown,
        BaseGame,
        PriceOfLoyalty,
    }
}
