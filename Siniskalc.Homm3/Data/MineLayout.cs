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
        public const int RecordSize = 62;

        /// <summary>
        /// The tile the record gives, one byte each: x, y and the level. It is not the tile where the mine is entered (the hero that visits
        /// it stands somewhere else, 4 tiles to the right and 3 up in one row of the test map, 0 and 3 down in the other).
        /// </summary>
        public const int X = 0;
        public const int Y = 1;
        public const int Level = 2;

        /// <summary>
        /// The colour of the owner (Red 0, Blue 1, ...), or 0xFF for nobody.
        /// </summary>
        public const int Owner = 3;

        /// <summary>
        /// The kind of mine: 0 sawmill, 1 alchemist's lab, 2 ore, 3 sulfur, 4 crystal, 5 gems, 6 gold (the numbers of Heroes of Might and Magic II).
        /// An abandoned mine has the kind of the resource it gives once it is taken.
        /// </summary>
        public const int Kind = 4;

        /// <summary>
        /// 1 for a mine that was abandoned at the start, 0 for the others.
        /// </summary>
        public const int Abandoned = 5;

        public const byte NoOwner = 0xFF;

        public const int SlotCount = 7;

        /// <summary>
        /// The kinds that are mines; the table has records of other objects too, which have other kinds.
        /// </summary>
        public const int LastMineKind = 6;

        /// <summary>
        /// How many records in a row make a table: one alone can be a coincidence.
        /// </summary>
        public const int MinimumRun = 3;

        /// <summary>
        /// The most creature number a guard can hold: a number above it is not the record of a mine.
        /// </summary>
        public const uint MaxCreatureCode = 0x1FF;

        public const uint MaxCount = 1_000_000;

        /// <summary>
        /// The seven creatures that guard the mine (32 bits each, 0xFFFFFFFF for none) and then their seven counts (32 bits each).
        /// </summary>
        public static ArmyLayout Guard { get; } = new(6, 4, 34, 4, SlotCount, uint.MaxValue);
    }
}
