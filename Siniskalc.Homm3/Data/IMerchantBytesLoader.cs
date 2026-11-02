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
        /// Returns the two lists (the artifact merchants, then the black market), or none when they are not found.
        /// </summary>
        IReadOnlyList<RecordChunk> LoadMerchantChunks(byte[] bytes);
    }
}
