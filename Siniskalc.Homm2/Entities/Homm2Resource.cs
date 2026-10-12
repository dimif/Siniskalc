namespace Siniskalc.Homm2.Entities
{
    /// <summary>
    /// The seven resources, in the order of the savegame's tables (the stock of a player and what the player earns each day).
    /// </summary>
    public enum Homm2Resource : byte
    {
        Wood = 0,
        Mercury = 1,
        Ore = 2,
        Sulfur = 3,
        Crystal = 4,
        Gems = 5,
        Gold = 6,
    }
}
