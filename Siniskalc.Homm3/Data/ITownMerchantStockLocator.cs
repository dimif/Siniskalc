namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Homm3.Entities;

    /// <summary>Finds the single stock shared by town Artifact Merchants, independently of Black Markets.</summary>
    public interface ITownMerchantStockLocator
    {
        /// <summary>
        /// Returns the first stock slot's offset, or -1 for an unsupported, ambiguous or incomplete block.
        /// </summary>
        /// <param name="bytes">The decompressed savefile.</param>
        /// <param name="version">The detected game version.</param>
        /// <param name="searchFrom">Where to start, normally after the player records.</param>
        /// <returns>The offset, or -1. No artifact-value scan or unmarked-date fallback is used.</returns>
        int FindOffset(byte[] bytes, GameVersion version, int searchFrom = 0);
    }
}
