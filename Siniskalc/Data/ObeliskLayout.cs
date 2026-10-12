namespace Siniskalc.Data
{
    /// <summary>
    /// Where the obelisks the player has visited are in the savefile.
    /// </summary>
    /// <remarks>
    /// In front of the struct of the first colour (Red, in play or not) the savefile has a marker, FF 00 00 and then the number of obelisks the
    /// map has, followed by one byte for each obelisk with a bit for every colour that has visited it (bit 0 red, 1 blue, ... in the order of the colours; the game sets the bits of the whole team of the player that visits). The bytes are in the order of the obelisks
    /// of the map, not of the visits; the hero standing on an obelisk holds that number too (the byte at 16 of its chunk).
    /// Found in the Restoration of Erathia saves 67 to 77, in which Red visited the five obelisks of the map one at a time, in the order 1, 2, 5,
    /// 4, 3. Other maps (the Horn of the Abyss saves with 4, 5 and 7 obelisks) have the same marker with a count that agrees with the number
    /// of obelisks that can be found in their map objects, but no obelisk was visited there. Found for several players in the saves of <c>TestData/Sod/Hotseat</c> (Red and Blue human, five teams). Not every savefile has the marker: when other data sits in its place
    /// the obelisks are not read.
    /// </remarks>
    public static class ObeliskLayout
    {
        /// <summary>
        /// How many bytes the marker is in front of the struct of the first colour.
        /// </summary>
        public const int BeforeFirstPlayer = 53;

        /// <summary>
        /// The marker: FF, 00, 00 and then the number of obelisks.
        /// </summary>
        public const int MarkerLength = 4;

        public const int CountOffset = 3;

        public const int FlagsOffset = MarkerLength;
    }
}
