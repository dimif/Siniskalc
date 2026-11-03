namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Heroes.Data;

    /// <summary>
    /// Where the fields of a dwelling (a creature generator of the map) are in its record. The savegame has a table of records of 75 bytes, one
    /// for each dwelling of the map, after the table of the mines. Found in the saves of <c>TestData/Hota/Dwellings</c>, in which Tazar flagged
    /// the dwellings of the Castle and the Rampart one by one and Orrin flagged them to Blue and hired their creatures.
    /// </summary>
    public static class DwellingLayout
    {
        /// <summary>
        /// The distance between owner bytes, verified by separate captures of three dwellings.
        /// </summary>
        public const int RecordSize = 75;

        /// <summary>
        /// The known payload through the last guard count. The following byte may belong to another block's marker
        /// (the obelisks on the Arts map), so it is never included in an editable dwelling chunk.
        /// </summary>
        public const int EditableSize = 74;

        /// <summary>The two-byte count preceding the records.</summary>
        public const int CountSize = sizeof(ushort);

        /// <summary>The last creature-generator object type, including generators with several creatures.</summary>
        public const byte LastGeneratorType = 20;

        /// <summary>
        /// The colour of the owner (Red 0, Blue 1, ...), or 0xFF for nobody.
        /// </summary>
        public const int Owner = 0;

        /// <summary>
        /// The number of the type of the object, 17 for a creature generator (what a hero standing on one reads at +11 of its own record).
        /// </summary>
        public const int ObjectType = 1;

        /// <summary>
        /// The number of the type of the objects of the map that are dwellings; see <see cref="ObjectType"/>.
        /// </summary>
        public const byte CreatureGeneratorType = 17;

        /// <summary>
        /// The number of the creature the dwelling gives (0 pikeman, 2 archer, 4 griffin, ... 14 centaur ...).
        /// </summary>
        public const int Creature = 3;

        /// <summary>
        /// The creatures that can be hired now, 32 bits. It is the weekly growth after the first week and adds up when it is not hired
        /// (28 pikemen after two weeks), and is 0 once all are hired.
        /// </summary>
        public const int Stock = 7;

        /// <summary>
        /// The tile of the dwelling, one byte each: x, y and the level. A hero stood on it to flag the dwellings of the Castle and the Rampart in the
        /// test map, but 3 tiles lower to visit the neutral dwellings of the row above (the entrance of the object is not always the tile of its record,
        /// as for the mines).
        /// </summary>
        public const int X = 15;

        /// <summary>
        /// The row of the tile of the dwelling; see <see cref="X"/>.
        /// </summary>
        public const int Y = 16;

        /// <summary>
        /// The level of the map the dwelling is on; see <see cref="X"/>.
        /// </summary>
        public const int Level = 17;

        /// <summary>
        /// The byte of the owner when nobody owns the dwelling.
        /// </summary>
        public const byte NoOwner = 0xFF;

        /// <summary>
        /// The seven guard creature codes followed by their counts; validated for location, not exposed for editing.
        /// </summary>
        public static ArmyLayout Guard { get; } = new(18, 4, 46, 4, MineLayout.SlotCount, uint.MaxValue);
    }
}
