namespace Siniskalc.Homm2.Entities
{
    /// <summary>
    /// The mines of the map (the objects that give their owner a resource every day), in the order of their kind in the savegame.
    /// </summary>
    public enum MineKind : byte
    {
        Sawmill = 0,
        AlchemistLab = 1,
        OreMine = 2,
        SulfurMine = 3,
        CrystalMine = 4,
        GemMine = 5,
        GoldMine = 6,
    }
}
