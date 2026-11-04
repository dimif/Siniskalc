namespace Siniskalc.Homm3.Data
{
    /// <summary>
    /// The shared town Artifact Merchant stock follows the six-byte date and a 32-byte zero block.
    /// Verified by the HotA purchase controls and the SoD/HotA corpus; not established for RoE, AB or Chronicles.
    /// </summary>
    public static class TownMerchantStockLayout
    {
        /// <summary>The distance from the uint16 day to the first of seven uint32 artifact slots.</summary>
        public const int OffsetFromDate = 38;

        /// <summary>The length of the recognized prefix immediately before the date.</summary>
        public const int DatePrefixLength = 4;

        /// <summary>The size of the shared stock, with the same seven-slot encoding as a Black Market.</summary>
        public const int Length = MerchantLayout.ListSize;
    }
}
