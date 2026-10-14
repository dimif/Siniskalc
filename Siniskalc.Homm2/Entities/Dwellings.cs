namespace Siniskalc.Homm2.Entities
{
    using System;

    /// <summary>
    /// The dwellings of a town and the level of its Mage Guild (32 bits of a castle). The six dwellings have a bit each, and when one is
    /// upgraded its bit is replaced by the bit of the upgrade (0x200 for the second dwelling and so on; the sixth can be upgraded twice,
    /// as the dragons of a Warlock): a town of the savegames never has both. The level of the Mage Guild is not a flag: it is the number
    /// in the bits 16 to 18 (0 to 5), so level 3 is 0x30000 and level 5 is 0x50000.
    /// </summary>
    [Flags]
    public enum Dwellings : uint
    {
        None = 0,
        Dwelling1 = 0x08,
        Dwelling2 = 0x10,
        Dwelling3 = 0x20,
        Dwelling4 = 0x40,
        Dwelling5 = 0x80,
        Dwelling6 = 0x100,
        Dwelling2Upgraded = 0x200,
        Dwelling3Upgraded = 0x400,
        Dwelling4Upgraded = 0x800,
        Dwelling5Upgraded = 0x1000,
        Dwelling6Upgraded = 0x2000,
        Dwelling6UpgradedTwice = 0x4000,
        MageGuild1 = 0x10000,
        MageGuild2 = 0x20000,
        MageGuildLevelMask = 0x70000,
    }
}
