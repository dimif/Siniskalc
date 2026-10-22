namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// What the merchants of a savegame have in store: the artifact merchants, then the black market.
    /// </summary>
    public sealed class MerchantStockChunkCollection : ModelChunkCollection<MerchantStockChunk, MerchantStockModel, IMerchantStockDeserializer, IMerchantStockSerializer>
    {
        private readonly IMerchantStockDiffer differ;

        public MerchantStockChunkCollection(IEnumerable<MerchantStockChunk> chunks, IMerchantStockDeserializer deserializer, IMerchantStockSerializer serializer, IMerchantStockDiffer differ)
            : base(chunks, deserializer, serializer, differ.Diff)
        {
            this.differ = differ;
        }

        public override MerchantStockChunkCollection WithChunk(int index, MerchantStockChunk chunk) => new(this.Replace(index, chunk), this.Deserializer, this.Serializer, this.differ);
    }
}
