namespace Siniskalc.Homm3.Data
{
    /// <summary>
    /// Where the stock of the merchants of a game is. Near the start of the savefile (in front of the player data, after the text of the
    /// rumours) there is a 32 bit number of merchants and, one after the other, that many lists of seven artifacts, 32 bits each: the stock
    /// of each merchant of the map (the game calls every one of them a black market). A slot that was bought is 0xFFFFFFFF. The merchants
    /// are in the order of their number among the objects of the type 7, which the cells of the map give. Found in the saves of
    /// <c>TestData/Hota/VisitedPlaces</c>, in which Tazar and Orrin visited the two merchants of the map and bought from them, and in
    /// <c>TestData/Hota/multi_merchants</c>, a map with four of them.
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
        /// The most merchants the number in front of the first list may name for the bytes to be taken for the lists.
        /// </summary>
        public const int MaxMerchants = 32;

        /// <summary>
        /// The number of the type of the objects of the map that are merchants.
        /// </summary>
        public const int ObjectType = 7;

        /// <summary>
        /// How far into the savefile the lists are looked for: they are in the header, before the table of the players and the map.
        /// </summary>
        public const int SearchLength = 0x6000;

        /// <summary>
        /// How many of the slots must name an artifact for the bytes to be taken for the lists (the merchants of a game that bought
        /// everything are not found).
        /// </summary>
        public const int MinimumStocked = 4;

        public static int SlotAt(int slot) => slot * SlotSize;
    }
}
