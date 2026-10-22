namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// A view onto the stock of one merchant in a <see cref="SaveDocumentHistory"/>.
    /// </summary>
    public sealed class MerchantStockEditSession : EditSession<MerchantStockChunk, MerchantStockModel>
    {
        public MerchantStockEditSession(SaveDocumentHistory history, int index)
            : base(history, Homm3ChunkKinds.Merchants, index)
        {
        }
    }
}
