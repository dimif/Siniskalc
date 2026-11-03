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

        /// <summary>
        /// The size of the set of castles that have built today.
        /// </summary>
        public const int BuiltTodaySize = 9;

        /// <summary>
        /// Where the list of mines begins, from the end of the table of castles. A record is 7 bytes: the owner (a colour, 0xFF for nobody),
        /// the kind, guardian spell and count, the column, the row and a byte of 0xFF. No guardian is FF with count zero or FF.
        /// </summary>
        public const int MineTable = 5;

        /// <summary>
        /// The size of a record of the list of mines.
        /// </summary>
        public const int MineRecordSize = 7;

        /// <summary>
        /// Where the guardian spell is stored in a mine record.
        /// </summary>
        public const int MineGuardianSpell = 2;

        /// <summary>
        /// Where the guardian count is stored in a mine record.
        /// </summary>
        public const int MineGuardianCount = 3;

        /// <summary>
        /// The first guardian spell code stored in a mine record (Set Earth Guardian).
        /// </summary>
        public const byte FirstGuardianSpell = 62;

        /// <summary>
        /// The last guardian spell code stored in a mine record (Set Water Guardian).
        /// </summary>
        public const byte LastGuardianSpell = 65;

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

        /// <summary>
        /// The byte at 7 of a tile (and the second of an addon) when nothing is there; see <see cref="NoClass"/>.
        /// </summary>
        public const byte NoPicture = 0xFF;

        /// <summary>
        /// Where the first object of a tile is kept (its class and its picture, bytes 6 and 7), from the start of the tile.
        /// </summary>
        public const int TileSlot = 6;

        /// <summary>
        /// Where the 16 bit number that says which addon follows the tile is, from the start of the tile.
        /// </summary>
        public const int TileLink = 10;

        /// <summary>
        /// Where the addons begin, from the end of the table of tiles: a 32 bit count and a byte, then 7 bytes each.
        /// </summary>
        public const int AddonsFromTilesEnd = 9;

        /// <summary>
        /// The size of an addon.
        /// </summary>
        public const int AddonSize = 7;

        /// <summary>
        /// The byte 9 of a tile for each circle of shrine, and of the sphinx.
        /// </summary>
        public const byte ShrineFirstCircle = 0x9F;

        /// <summary>
        /// The byte 9 of a tile with a shrine of the second circle.
        /// </summary>
        public const byte ShrineSecondCircle = 0xCA;

        /// <summary>
        /// The byte 9 of a tile with a shrine of the third circle.
        /// </summary>
        public const byte ShrineThirdCircle = 0xCB;

        /// <summary>
        /// The byte 9 of a tile with a sphinx.
        /// </summary>
        public const byte Sphinx = 0xCF;

        /// <summary>
        /// The byte 9 of a tile with a hero on it, which hides the spell of a shrine.
        /// </summary>
        public const byte HeroMarker = 0xAA;

        /// <summary>
        /// The spell of a shrine is the 16 bit number at bytes 4 and 5 of its tile, shifted right by this many bits.
        /// </summary>
        public const int ShrineSpellShift = 3;

        /// <summary>
        /// Where the 16 bit number of a tile that holds the spell of a shrine is (bytes 4 and 5), from the start of the tile.
        /// </summary>
        public const int TileQuantity = 4;

        /// <summary>
        /// The size of the header of the block of an object with state (the marker and the 16 bit length of the payload), which comes before the payload.
        /// </summary>
        public const int BlockHeader = 6;

        // The payload of a sphinx: the riddle is answered when the first byte is 0, then the 7 resources, the artifact, the answers and the riddle.
        /// <summary>
        /// Where the byte that says whether the riddle can still be answered is, in the payload of a sphinx.
        /// </summary>
        public const int SphinxActive = 0;

        /// <summary>
        /// Where the seven resources the sphinx gives are, in the payload of a sphinx.
        /// </summary>
        public const int SphinxReward = 1;

        /// <summary>
        /// Where the artifact the sphinx gives is, in the payload of a sphinx.
        /// </summary>
        public const int SphinxArtifact = 29;

        /// <summary>
        /// Where the number of answers is, in the payload of a sphinx.
        /// </summary>
        public const int SphinxAnswerCount = 31;

        /// <summary>
        /// Where the answers begin, in the payload of a sphinx.
        /// </summary>
        public const int SphinxAnswers = 32;

        /// <summary>
        /// The size of an answer of a sphinx.
        /// </summary>
        public const int SphinxAnswerSize = 13;

        /// <summary>
        /// Where the text of the riddle is, in the payload of a sphinx.
        /// </summary>
        public const int SphinxRiddle = 136;

        /// <summary>
        /// The block that starts the data of an object with state: 1234 as 32 bits, then the length of the payload in 16 bits.
        /// </summary>
        public static byte[] BlockMarker { get; } = [0xD2, 0x04, 0x00, 0x00];
    }
}
