namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Heroes.Data;

    /// <summary>
    /// Where the fields of a mine are in its record. The savegame has a table of records of 62 bytes for the objects of the map that have an
    /// owner and a guard (the mines, abandoned ones too), one after the other. Found in the saves of <c>TestData/Hota/Mines</c>, in which Red
    /// took the 14 mines of a test map one at a time and Blue took each from it the next turn.
    /// </summary>
    public static class MineLayout
    {
        /// <summary>
        /// The size of the record of a mine.
        /// </summary>
        public const int RecordSize = 62;

        /// <summary>
        /// The tile at the end of the record, which may use another anchor than the map-editor sprite or entrance.
        /// </summary>
        public const int X = 59;

        /// <summary>
        /// The row of the tile the record gives; see <see cref="X"/>.
        /// </summary>
        public const int Y = 60;

        /// <summary>
        /// The level of the map of the tile the record gives; see <see cref="X"/>.
        /// </summary>
        public const int Level = 61;

        /// <summary>
        /// The colour of the owner (Red 0, Blue 1, ...), or 0xFF for nobody.
        /// </summary>
        public const int Owner = 0;

        /// <summary>
        /// The kind of mine: 0 sawmill, 1 alchemist's lab, 2 ore, 3 sulfur, 4 crystal, 5 gems, 6 gold (the numbers of Heroes of Might and Magic II).
        /// An abandoned mine has the kind of the resource it gives once it is taken.
        /// </summary>
        public const int Kind = 1;

        /// <summary>
        /// 1 for a mine that was abandoned at the start, 0 for the others.
        /// </summary>
        public const int Abandoned = 2;

        /// <summary>
        /// The byte of the owner when nobody owns the mine.
        /// </summary>
        public const byte NoOwner = 0xFF;

        /// <summary>
        /// The number of creatures that can guard a mine.
        /// </summary>
        public const int SlotCount = 7;

        /// <summary>
        /// The kinds that are mines; the table has records of other objects too, which have other kinds.
        /// </summary>
        public const int LastMineKind = 6;

        /// <summary>
        /// The size of the table count in HotA.
        /// </summary>
        public const int HotaCountSize = sizeof(uint);

        /// <summary>The size of the table count in SoD; assumed for the other original versions.</summary>
        public const int OriginalCountSize = sizeof(byte);

        /// <summary>
        /// The most creature number a guard can hold: a number above it is not the record of a mine.
        /// </summary>
        public const uint MaxCreatureCode = 0x1FF;

        /// <summary>
        /// The most creatures a guard can have: a count above it is not the record of a mine.
        /// </summary>
        public const uint MaxCount = 1_000_000;

        /// <summary>
        /// The seven creatures that guard the mine (32 bits each, 0xFFFFFFFF for none) and then their seven counts (32 bits each).
        /// </summary>
        public static ArmyLayout Guard { get; } = new(3, 4, 31, 4, SlotCount, uint.MaxValue);
    }
}
