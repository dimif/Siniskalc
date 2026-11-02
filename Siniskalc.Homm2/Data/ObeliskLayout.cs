namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Heroes.Data;

    /// <summary>
    /// Where the obelisks a player has visited are in the savegame.
    /// </summary>
    /// <remarks>
    /// In the map part, after the castles, are 48 records of 8 bytes, 48 bytes of numbers or 0xFF and then one byte for each of 48 possible
    /// obelisks: a bit for every colour that has visited it (bit 0 is Blue, 1 Green, and so on in the order of the colours; the AI players set
    /// theirs too). The bytes are in the order of the obelisks on the map, row by row from the top left, and the obelisks are the tiles with
    /// 0x99 in their byte 9 (<see cref="MapTileTable"/>). Found in the saves of Threedom (nine obelisks): the savegame in which Blue found
    /// the fifth obelisk, Charity standing on it, changed the byte of the fifth.
    /// </remarks>
    public static class ObeliskLayout
    {
        /// <summary>
        /// Where the bytes of the obelisks are, from the end of the table of castles.
        /// </summary>
        public const int FlagsFromCastlesEnd = 0x6E8;

        /// <summary>
        /// How many obelisks the table has room for.
        /// </summary>
        public const int Capacity = 48;

        /// <summary>
        /// The byte 9 of the tile that is an obelisk.
        /// </summary>
        public const byte TileMarker = 0x99;

        /// <summary>
        /// Gets where the visited flags are in their chunk. HoMM2 gets their count from the obelisks found on the map.
        /// </summary>
        public static ObeliskFlagsLayout Flags { get; } = new(0, null);
    }
}
