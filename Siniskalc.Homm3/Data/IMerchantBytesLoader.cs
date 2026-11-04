namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Cuts the lists of what the merchants have in store out of a savefile.
    /// </summary>
    public interface IMerchantBytesLoader
    {
        /// <summary>
        /// Returns one stock list per adventure-map Black Market, in type-7 object-number order, or none when they are not found.
        /// The shared town Artifact Merchant stock is separate and is not loaded here.
        /// </summary>
        IReadOnlyList<RecordChunk> LoadMerchantChunks(byte[] bytes);
    }
}
