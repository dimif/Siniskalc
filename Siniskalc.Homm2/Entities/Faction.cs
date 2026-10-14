namespace Siniskalc.Homm2.Entities
{
    /// <summary>
    /// The six towns (and the classes of the heroes that belong to them), in the order of the savegame: 0 for a Knight.
    /// </summary>
    public enum Faction : byte
    {
        Knight = 0,
        Barbarian = 1,
        Sorceress = 2,
        Warlock = 3,
        Wizard = 4,
        Necromancer = 5,
    }
}
