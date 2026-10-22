namespace Siniskalc.Homm3.Data
{
    /// <summary>
    /// Where the fields of a dwelling (a creature generator of the map) are in its record. The savegame has a table of records of 150 bytes, one
    /// for each dwelling of the map, after the table of the mines. Found in the saves of <c>TestData/Hota/Dwellings</c>, in which Tazar flagged
    /// the dwellings of the Castle and the Rampart one by one and Orrin flagged them to Blue and hired their creatures.
    /// </summary>
    public static class DwellingLayout
    {
        /// <summary>
        /// The size of a record. The first record starts right after the table of the mines (a record found 3 bytes earlier would overlap its last
        /// mine), so the fields are 3 bytes nearer the start than the numbers the first notes gave.
        /// </summary>
        public const int RecordSize = 150;

        /// <summary>
        /// The colour of the owner (Red 0, Blue 1, ...), or 0xFF for nobody.
        /// </summary>
        public const int Owner = 5;

        /// <summary>
        /// The number of the type of the object, 17 for a creature generator (what a hero standing on one reads at +11 of its own record).
        /// </summary>
        public const int ObjectType = 6;

        public const byte CreatureGeneratorType = 17;

        /// <summary>
        /// The number of the creature the dwelling gives (0 pikeman, 2 archer, 4 griffin, ... 14 centaur ...).
        /// </summary>
        public const int Creature = 8;

        /// <summary>
        /// The creatures that can be hired now, 32 bits. It is the weekly growth after the first week and adds up when it is not hired
        /// (28 pikemen after two weeks), and is 0 once all are hired.
        /// </summary>
        public const int Stock = 12;

        /// <summary>
        /// The tile of the dwelling, one byte each: x, y and the level. A hero stood on it to flag the dwellings of the Castle and the Rampart in the
        /// test map, but 3 tiles lower to visit the neutral dwellings of the row above (the entrance of the object is not always the tile of its record,
        /// as for the mines).
        /// </summary>
        public const int X = 20;
        public const int Y = 21;
        public const int Level = 22;

        public const byte NoOwner = 0xFF;

        /// <summary>
        /// How many records in a row make a table: one alone can be a coincidence.
        /// </summary>
        public const int MinimumRun = 3;
    }
}
