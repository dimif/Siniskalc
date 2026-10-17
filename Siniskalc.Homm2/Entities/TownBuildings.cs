namespace Siniskalc.Homm2.Entities
{
    using System;

    /// <summary>
    /// The buildings of a town that are not dwellings (16 bits of a castle). The bits that the savegames of Broken Alliance and Threedom showed are
    /// named; the one that no save has set (0x4000) is kept as it is.
    /// A town without a castle has <see cref="TownOnly"/> and cannot build anything else; building the castle replaces it with <see cref="Castle"/>.
    /// </summary>
    [Flags]
    public enum TownBuildings : ushort
    {
        None = 0,
        MageGuild = 0x01,
        ThievesGuild = 0x02,
        Tavern = 0x04,
        Shipyard = 0x08,
        Well = 0x10,
        TownOnly = 0x20,
        Castle = 0x40,
        Statue = 0x80,

        /// <summary>
        /// The left turret, which the castle gets once it is a castle (Threedom, saves 64 and 66).
        /// </summary>
        LeftTurret = 0x100,
        RightTurret = 0x200,
        Marketplace = 0x400,

        /// <summary>
        /// The second building of the faction that is like a well: it adds creatures to the weekly growth (Knight: Farm, Barbarian: Garbage
        /// Heap, Sorceress: Crystal Garden, Warlock: Waterfall, Wizard: Orchard, Necromancer: Skull Pile; Threedom saves 39 and 62).
        /// </summary>
        SecondWell = 0x800,
        Moat = 0x1000,

        /// <summary>
        /// The special building of the faction (Knight: Fortifications, Barbarian: Coliseum, Sorceress: Rainbow, Warlock: Dungeon, Necromancer: Storm, and the Library of a Wizard, which
        /// gives each level of the Mage Guild a fourth spell: Threedom saves 47 and 64).
        /// </summary>
        Special = 0x2000,

        CaptainsQuarters = 0x8000,
    }
}
