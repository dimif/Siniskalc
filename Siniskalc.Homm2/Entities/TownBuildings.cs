namespace Siniskalc.Homm2.Entities
{
    using System;

    /// <summary>
    /// The buildings of a town that are not dwellings (16 bits of a castle). The bits that the savegames of Broken Alliance showed are named;
    /// the others (the marketplace, the turrets, the captain and the rest of the high byte) are kept as they are.
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
        /// The special building of the faction (the Library of a Wizard, which gives each level of the Mage Guild a fourth spell). Found by
        /// comparing the castles of the savegames: only the bit tells a Wizard castle with the Library from one without.
        /// </summary>
        Special = 0x2000,
    }
}
