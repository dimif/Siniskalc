namespace Siniskalc.Homm2.Entities
{
    using System;

    /// <summary>
    /// The dwellings of a town and the level of its Mage Guild (32 bits of a castle). The bits that the savegames showed are named: the six
    /// dwellings, and the Mage Guild whose level is one bit (level 1 is replaced by level 2, not added to it). The bits of the upgraded
    /// dwellings (probably 0x200 and up) are kept as they are.
    /// </summary>
    [Flags]
    public enum Homm2Dwellings : uint
    {
        None = 0,
        Dwelling1 = 0x08,
        Dwelling2 = 0x10,
        Dwelling3 = 0x20,
        Dwelling4 = 0x40,
        Dwelling5 = 0x80,
        Dwelling6 = 0x100,
        MageGuild1 = 0x10000,
        MageGuild2 = 0x20000,
    }
}
