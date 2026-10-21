namespace Siniskalc.Homm2.Data
{
    /// <summary>
    /// Where the objects with state of the map are in the map part of a savegame (the part after the table of castles), seen in the saves of
    /// Threedom and checked in the other test maps.
    /// </summary>
    public static class MapObjectLayout
    {
        /// <summary>
        /// Where the set of the castles that have built today begins, from the end of the table of castles: a bit for each of the 72 castle
        /// slots, 9 bytes (seen in the base game and in the Price of Loyalty, at the same distance).
        /// </summary>
        public const int BuiltToday = 0x48;

        public const int BuiltTodaySize = 9;

        /// <summary>
        /// Where the list of mines begins, from the end of the table of castles. A record is 7 bytes: the owner (a colour, 0xFF for nobody),
        /// the kind, two bytes of 0xFF, the column, the row and a byte of 0xFF; records that are not mines have other bytes in those places.
        /// </summary>
        public const int MineTable = 5;

        public const int MineRecordSize = 7;

        /// <summary>
        /// Where the room for the list of mines ends, from the end of the table of castles.
        /// </summary>
        public const int MineTableEnd = 0x538;

        /// <summary>
        /// The kind of an object that is a flag, as the second object of a tile or in an addon of it.
        /// </summary>
        public const byte FlagClass = 0x38;

        /// <summary>
        /// The bytes at 6 and 7 of a tile (and the first two of an addon) when nothing is there.
        /// </summary>
        public const byte NoClass = 0x00;

        public const byte NoPicture = 0xFF;

        public const int TileSlot = 6;

        public const int TileLink = 10;

        /// <summary>
        /// Where the addons begin, from the end of the table of tiles: a 32 bit count and a byte, then 7 bytes each.
        /// </summary>
        public const int AddonsFromTilesEnd = 9;

        public const int AddonSize = 7;

        /// <summary>
        /// The byte 9 of a tile for each circle of shrine, and of the sphinx.
        /// </summary>
        public const byte ShrineFirstCircle = 0x9F;

        public const byte ShrineSecondCircle = 0xCA;

        public const byte ShrineThirdCircle = 0xCB;

        public const byte Sphinx = 0xCF;

        /// <summary>
        /// The byte 9 of a tile with a hero on it, which hides the spell of a shrine.
        /// </summary>
        public const byte HeroMarker = 0xAA;

        /// <summary>
        /// The spell of a shrine is the 16 bit number at bytes 4 and 5 of its tile, shifted right by this many bits.
        /// </summary>
        public const int ShrineSpellShift = 3;

        public const int TileQuantity = 4;

        /// <summary>
        /// The block that starts the data of an object with state: 1234 as 32 bits, then the length of the payload in 16 bits.
        /// </summary>
        public static readonly byte[] BlockMarker = [0xD2, 0x04, 0x00, 0x00];

        public const int BlockHeader = 6;

        // The payload of a sphinx: the riddle is answered when the first byte is 0, then the 7 resources, the artifact, the answers and the riddle.
        public const int SphinxActive = 0;

        public const int SphinxReward = 1;

        public const int SphinxArtifact = 29;

        public const int SphinxAnswerCount = 31;

        public const int SphinxAnswers = 32;

        public const int SphinxMaxAnswers = 8;

        public const int SphinxAnswerSize = 13;

        public const int SphinxRiddle = 136;
    }
}
