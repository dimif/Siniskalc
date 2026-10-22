namespace Siniskalc.Homm3.Data
{
    /// <summary>
    /// Where the stock of the merchants of a game is. Near the start of the savefile (in front of the player data, after the text of the
    /// rumours) there are two lists of seven artifacts, 32 bits each, one after the other: the stock of the artifact merchants and the stock of
    /// the black market. A slot that was bought is 0xFFFFFFFF. A 32 bit 2 comes in front of the first list. Found in the saves of
    /// <c>TestData/Hota/VisitedPlaces</c>, in which Tazar and Orrin visited both and bought from them.
    /// </summary>
    public static class MerchantLayout
    {
        public const int SlotCount = 7;

        public const int SlotSize = 4;

        /// <summary>
        /// The size of the list of one merchant.
        /// </summary>
        public const int ListSize = SlotCount * SlotSize;

        public const uint Empty = 0xFFFFFFFF;

        /// <summary>
        /// The number that comes in front of the first list.
        /// </summary>
        public const uint MarkerBefore = 2;

        /// <summary>
        /// How far into the savefile the lists are looked for: they are in the header, before the table of the players and the map.
        /// </summary>
        public const int SearchLength = 0x6000;

        /// <summary>
        /// How many of the fourteen slots must name an artifact for the bytes to be taken for the lists (the merchants of a game that bought
        /// everything are not found).
        /// </summary>
        public const int MinimumStocked = 4;

        public static int SlotAt(int slot) => slot * SlotSize;
    }
}
