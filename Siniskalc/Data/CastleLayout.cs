namespace Siniskalc.Data
{
    /// <summary>
    /// Where the fields of a castle (a town) are in a <see cref="CastleChunk"/>.
    /// </summary>
    /// <remarks>
    /// A castle chunk starts at the owner byte and ends after the lists of the Mage Guild's spells, which follow the last building mask. The name is in the middle of it and
    /// has a different length for every castle, so what comes before the name is at a fixed offset from the start of the
    /// chunk, and what comes after it at a fixed offset from the end of the name (<see cref="CastleChunk.NameEnd"/>).
    /// The positions were found by comparing savefiles of one map in which a blacksmith and a cloud temple were built,
    /// a garrison was set and a castle changed owner. The layout was then checked against Shadow of Death savefiles
    /// (<c>TestData/Roe</c>, 51 saves of eight factions built up step by step): every field here, including the three
    /// building masks, is at the same offset in both versions. Restoration of Erathia, Armageddon's Blade and Chronicles
    /// savefiles have not been seen. See the "Castles" section of docs/KNOWLEDGE_BASE.md for what differs between factions.
    /// </remarks>
    public static class CastleLayout
    {
        /// <summary>The colour of the owning player, 0xFF when no player owns the castle.</summary>
        public const int Owner = 0;

        /// <summary>
        /// 1 when something was built in the castle this turn (the game allows one building a day), otherwise 0.
        /// </summary>
        public const int BuiltThisTurn = 1;

        /// <summary>The faction (town type) of the castle.</summary>
        public const int Faction = 3;

        /// <summary>The map coordinates, one byte each: X, Y and the level.</summary>
        public const int PositionX = 4;

        public const int PositionY = 5;

        public const int PositionZ = 6;

        /// <summary>
        /// The map coordinates of the tile where the boats of the castle appear, X then Y, one byte each. Both are FF for
        /// a castle that is not on the coast, so they are in every castle but only mean something for the coastal ones.
        /// </summary>
        public const int ShipyardX = 7;

        public const int ShipyardY = 8;

        /// <summary>
        /// The creature types of the garrison: seven 32-bit integers, -1 for an empty slot. The counts follow
        /// at <see cref="GarrisonCounts"/>.
        /// </summary>
        public const int GarrisonTypes = 9;

        public const int GarrisonCounts = 37;

        /// <summary>The length of the name in bytes, as a 16-bit integer. The name follows at <see cref="Name"/>.</summary>
        public const int NameLength = 69;

        public const int Name = 71;

        /// <summary>
        /// The creatures waiting in the dwellings: seven 16-bit integers for the dwellings of the creatures and, from
        /// <see cref="UpgradedCreaturesAvailableFromNameEnd"/>, seven for the upgraded dwellings. When a dwelling is
        /// upgraded its count moves to the second list. Offsets are from the end of the name.
        /// </summary>
        public const int CreaturesAvailableFromNameEnd = 0;

        public const int UpgradedCreaturesAvailableFromNameEnd = 14;

        public const int CreatureLevelCount = 7;

        /// <summary>
        /// The number of spells in the Mage Guild for each of the five spell levels, one byte each, from the end of the name.
        /// </summary>
        public const int SpellCountsFromNameEnd = 84;

        public const int SpellLevelCount = 5;

        /// <summary>
        /// The offset of the build mask of the buildings that are in the castle now, from the end of the name:
        /// a 64-bit integer with one bit per <see cref="Siniskalc.Entities.Building.Code"/>. A building that was replaced by
        /// an upgrade (the village hall by the town hall, a dwelling by its upgrade) is not in this one.
        /// </summary>
        public const int CurrentBuildingsFromNameEnd = 89;

        /// <summary>
        /// The offset of the mask of every building that has been built, including the ones that were replaced by an upgrade.
        /// </summary>
        public const int BuiltBuildingsFromNameEnd = 97;

        /// <summary>
        /// The offset of the mask of the buildings that the map allows to build. It is what the map editor lets the author
        /// disable, so a bit can be set for a building that has not been built and cleared for one that has.
        /// </summary>
        public const int EnabledBuildingsFromNameEnd = 105;

        /// <summary>The distance from the end of the name to the end of the last building mask.</summary>
        public const int ChunkTailFromNameEnd = 113;

        /// <summary>
        /// The offset of the spells of the Mage Guild, from the end of the name: it follows the last building mask. Five lists, one
        /// for each spell level, each <see cref="SpellListStride"/> bytes apart and holding spell codes (<see cref="Siniskalc.Entities.Spell.Code"/>)
        /// as 32-bit integers. The game picks them when the map starts, so they are the same in every savefile of a game, whether
        /// the Mage Guild is built or not. Only the first <see cref="SpellCountsFromNameEnd"/> of a level are in the guild: in Restoration of
        /// Erathia and Shadow of Death the entry after them is the spell a Library would add (Tower only), and in Horn of the Abyss
        /// the slots after them are empty (0, which is also Summon Boat) or hold other data. A level that the faction cannot build
        /// holds -1. Confirmed against the guilds read in the game: Tower and Dungeon (Shadow of Death, <c>TestData/Roe</c> 46),
        /// Tower with and without its Library (11 and 12) and Bulwark (Horn of the Abyss, <c>TestData/Hota/Bulwark</c> 33).
        /// </summary>
        public const int SpellListsFromNameEnd = ChunkTailFromNameEnd;

        /// <summary>The distance between the lists of two spell levels.</summary>
        public const int SpellListStride = 24;

        /// <summary>The spells of one level that the guild can hold (the most it ever shows, Tower's level 1 with a Library).</summary>
        public const int SpellsPerLevelMaximum = 6;

        /// <summary>The length of the five lists of spells.</summary>
        public const int SpellListsLength = SpellLevelCount * SpellListStride;

        /// <summary>The distance from the end of the name to the end of the chunk, which is the end of the lists of spells.</summary>
        public const int ChunkEndFromNameEnd = SpellListsFromNameEnd + SpellListsLength;

        /// <summary>
        /// Four bytes in front of the name length. The third is the level of the Mage Guild (0 to 5), the last is 00 and
        /// the first two are the garrisoned and the visiting hero (<see cref="GarrisonedHero"/>, <see cref="VisitingHero"/>).
        /// </summary>
        public const int NamePrefixLength = 4;

        /// <summary>
        /// The index in the savefile's table of heroes of the hero garrisoned in the castle, FF when there is none. A garrisoned
        /// hero is inside the castle and not on the map. The first two bytes of the prefix of the name length.
        /// </summary>
        public const int GarrisonedHero = NameLength - 4;

        /// <summary>The index of the hero standing at the gate of the castle, FF when there is none.</summary>
        public const int VisitingHero = NameLength - 3;

        /// <summary>The level of the Mage Guild (0 when there is none), the third byte of the prefix, from the start of the chunk.</summary>
        public const int MageGuildLevel = NameLength - 2;

        /// <summary>The bytes in front of the length of the name, from the start of the chunk.</summary>
        public const int NamePrefix = NameLength - NamePrefixLength;

        /// <summary>The number of bytes from the start of a chunk to the name, and from the end of the name to the end.</summary>
        public const int FixedLength = Name + ChunkEndFromNameEnd;
    }
}
